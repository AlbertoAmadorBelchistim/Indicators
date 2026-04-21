namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OFT.Rendering.Heatmap;

public sealed class HeatmapPriceChangeIndicator
	: IHeatmapIndicator<HeatmapPriceChangeSettings, HeatmapIndicatorSeriesSnapshot<HeatmapPriceChangeSample>>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	private const decimal MinimumMaxValue = 0.01m;

	private readonly object _sync = new();
	private readonly List<PricePoint> _priceHistory = new();
	private readonly HeatmapSeriesBuffer<HeatmapPriceChangeSample> _samples = new();

	private HeatmapPriceChangeSettings _settings = HeatmapPriceChangeSettings.Default;
	private DateTime _trainingStartTime = DateTime.MinValue;
	private DateTime _lastTickTime = DateTime.MinValue;
	private DateTime _virtualCurrentTime = DateTime.MinValue;
	private DateTime _lastCleanupTime = DateTime.MinValue;
	private decimal _maxStandardDeviation = MinimumMaxValue;
	private decimal _maxRateOfChange = MinimumMaxValue;
	private decimal _currentValue;
	private decimal _currentPrice;
	private bool _isTrainingComplete;
	private decimal _cachedValue;
	private DateTime _cacheTime = DateTime.MinValue;
	private HeatmapPriceChangeMode _cachedMode = HeatmapPriceChangeMode.StandardDeviation;
	private HeatmapPriceChangePeriod _cachedPeriod = HeatmapPriceChangePeriod.OneMinute;
	private readonly TimeSpan _cacheValidityPeriod = TimeSpan.FromMilliseconds(50);
	private decimal _currentPeriodStartPrice;
	private DateTime _currentPeriodStartTime = DateTime.MinValue;
	private HeatmapPriceChangePeriod _currentPeriod = HeatmapPriceChangePeriod.OneMinute;

	public string Id => "heatmap.price-change";

	public float CurrentValue { get; private set; }

	public bool IsTraining
	{
		get
		{
			lock (_sync)
				return !_isTrainingComplete;
		}
	}

	public void Configure(HeatmapPriceChangeSettings settings)
	{
		lock (_sync)
		{
			_settings = settings with
			{
				TrainingPeriod = ValidateTrainingPeriod(settings.Period, settings.TrainingPeriod)
			};

			_cacheTime = DateTime.MinValue;

			if (_currentPeriod != _settings.Period)
			{
				_currentPeriod = _settings.Period;
				_currentPeriodStartPrice = 0;
				_currentPeriodStartTime = DateTime.MinValue;

				if (_virtualCurrentTime != DateTime.MinValue && _currentPrice > 0)
					UpdateCurrentPeriodWindow(_virtualCurrentTime, _currentPrice, _settings.Period);
			}

			if (_lastTickTime != DateTime.MinValue)
			{
				_currentValue = CalculateValue(_virtualCurrentTime, _settings.Mode, _settings.Period);
				CurrentValue = (float)NormalizeValue(_currentValue, _settings.Mode);
			}
		}
	}

	public void Reset(HeatmapIndicatorContext context)
	{
		lock (_sync)
		{
			_priceHistory.Clear();
			_samples.Clear();
			ResetState();
		}
	}

	public async ValueTask WarmUpAsync(
		HeatmapIndicatorWarmupRequest request,
		IHeatmapIndicatorDataSources dataSources,
		CancellationToken cancellationToken)
	{
		var ticks = await dataSources.Trades.GetTradeTicksAsync(request, cancellationToken).ConfigureAwait(false);
		cancellationToken.ThrowIfCancellationRequested();
		ProcessHistoricalTicks(ticks);
	}

	public void ProcessHistoricalTicks(IEnumerable<HeatmapTradeTick> ticks)
	{
		var trainingPeriod = ValidateTrainingPeriod(_settings.Period, _settings.TrainingPeriod);
		var requiredSeconds = (int)trainingPeriod + (int)_settings.Period;
		var bufferSeconds = requiredSeconds * 0.1;
		var maxRetention = TimeSpan.FromSeconds(requiredSeconds + bufferSeconds);
		var tempHistory = new List<PricePoint>();
		var lastTickTime = DateTime.MinValue;

		foreach (var tick in ticks)
		{
			if (tick.Price <= 0)
				continue;

			lastTickTime = tick.Time;

			if (tempHistory.Count == 0 || tempHistory[^1].Price != tick.Price)
			{
				tempHistory.Add(new PricePoint(tick.Time, tick.TimestampNanos, tick.Price, 1));
			}
			else
			{
				var lastIndex = tempHistory.Count - 1;
				var point = tempHistory[lastIndex];
				tempHistory[lastIndex] = point with { Time = tick.Time, TimestampNanos = tick.TimestampNanos, TickCount = point.TickCount + 1 };
			}
		}

		if (tempHistory.Count == 0)
			return;

		var cutoffTime = lastTickTime - maxRetention;
		var startIndex = 0;
		for (var i = 0; i < tempHistory.Count; i++)
		{
			if (tempHistory[i].Time >= cutoffTime)
			{
				startIndex = i;
				break;
			}
		}

		lock (_sync)
		{
			_priceHistory.Clear();
			for (var i = startIndex; i < tempHistory.Count; i++)
				_priceHistory.Add(tempHistory[i]);

			_lastTickTime = lastTickTime;
			_virtualCurrentTime = lastTickTime;
			_trainingStartTime = lastTickTime;

			CalculateTrainingMaximums(lastTickTime, _settings.Mode, _settings.Period, trainingPeriod);

			for (var i = 0; i < _priceHistory.Count; i++)
			{
				var point = _priceHistory[i];
				CalculateAndRecord(point.Time, point.TimestampNanos);
			}
		}
	}

	public void ProcessTick(HeatmapTradeTick tick)
	{
		if (tick.Price <= 0)
			return;

		var settings = _settings;
		var trainingPeriod = ValidateTrainingPeriod(settings.Period, settings.TrainingPeriod);

		lock (_sync)
		{
			if (_trainingStartTime == DateTime.MinValue)
				_trainingStartTime = tick.Time;

			_lastTickTime = tick.Time;
			_virtualCurrentTime = tick.Time;
			_currentPrice = tick.Price;

			var lastIndex = _priceHistory.Count - 1;
			if (lastIndex < 0 || _priceHistory[lastIndex].Price != tick.Price)
				_priceHistory.Add(new PricePoint(tick.Time, tick.TimestampNanos, tick.Price, 1));
			else
			{
				var point = _priceHistory[lastIndex];
				_priceHistory[lastIndex] = point with { Time = tick.Time, TimestampNanos = tick.TimestampNanos, TickCount = point.TickCount + 1 };
			}

			UpdateCurrentPeriodWindow(tick.Time, tick.Price, settings.Period);

			if (tick.Time - _lastCleanupTime > TimeSpan.FromMinutes(1))
			{
				CleanupOldData(tick.Time, trainingPeriod);
				_lastCleanupTime = tick.Time;
			}

			_cacheTime = DateTime.MinValue;

			if (!_isTrainingComplete && tick.Time - _trainingStartTime >= TimeSpan.FromSeconds((int)trainingPeriod))
			{
				CalculateTrainingMaximums(tick.Time, settings.Mode, settings.Period, trainingPeriod);
				_isTrainingComplete = true;
			}
			else if (!_isTrainingComplete)
			{
				UpdateTrainingMaximums(tick.Time, settings.Mode, settings.Period);
			}

			_currentValue = CalculateValue(tick.Time, settings.Mode, settings.Period);

			if (_isTrainingComplete)
				AdaptMaximums(_currentValue, settings.Mode);

			RecordCurrentValue(tick.TimestampNanos, settings.Mode);
		}
	}

	public void ProcessTimer(long timestampNanos)
	{
		lock (_sync)
		{
			if (_lastTickTime == DateTime.MinValue)
				return;

			if (_cacheTime != DateTime.MinValue &&
			    DateTime.UtcNow - _cacheTime < _cacheValidityPeriod &&
			    _cachedMode == _settings.Mode &&
			    _cachedPeriod == _settings.Period)
			{
				RecordCurrentValue(timestampNanos, _settings.Mode, _cachedValue);
				return;
			}

			_currentValue = CalculateValue(_virtualCurrentTime, _settings.Mode, _settings.Period);
			_cachedValue = _currentValue;
			_cacheTime = DateTime.UtcNow;
			_cachedMode = _settings.Mode;
			_cachedPeriod = _settings.Period;

			RecordCurrentValue(timestampNanos, _settings.Mode);
		}
	}

	public void ProcessTimer(HeatmapIndicatorTimerTick tick) => ProcessTimer(tick.TimestampNanos);

	public HeatmapIndicatorSeriesSnapshot<HeatmapPriceChangeSample> GetSnapshot(HeatmapIndicatorSnapshotRequest request) =>
		_samples.GetSnapshot(request, IsTraining);

	public HeatmapPriceChangeSample GetValueAtOrBefore(long cutoffTimeNanos) =>
		_samples.GetLatestAtOrBefore(cutoffTimeNanos)?.Value ?? default;

	private void CalculateAndRecord(DateTime referenceTime, long timestampNanos)
	{
		_currentValue = CalculateValue(referenceTime, _settings.Mode, _settings.Period);
		_currentPrice = _priceHistory.Count > 0 ? _priceHistory[^1].Price : 0;
		RecordCurrentValue(timestampNanos, _settings.Mode);
	}

	private void RecordCurrentValue(long timestampNanos, HeatmapPriceChangeMode mode, decimal? rawValue = null)
	{
		var value = NormalizeValue(rawValue ?? _currentValue, mode);
		CurrentValue = (float)value;

		if (timestampNanos > 0)
			_samples.Append(timestampNanos, new HeatmapPriceChangeSample(CurrentValue));
	}

	private static HeatmapTrainingPeriod ValidateTrainingPeriod(HeatmapPriceChangePeriod period, HeatmapTrainingPeriod trainingPeriod)
	{
		if ((int)period <= (int)trainingPeriod)
			return trainingPeriod;

		if ((int)period <= 300)
			return HeatmapTrainingPeriod.FiveMinutes;

		if ((int)period <= 900)
			return HeatmapTrainingPeriod.FifteenMinutes;

		return HeatmapTrainingPeriod.OneHour;
	}

	private void CleanupOldData(DateTime currentTime, HeatmapTrainingPeriod trainingPeriod)
	{
		var requiredSeconds = (int)trainingPeriod + 300;
		var bufferSeconds = requiredSeconds * 0.1;
		var maxRetention = TimeSpan.FromSeconds(requiredSeconds + bufferSeconds);
		var cutoffTime = currentTime - maxRetention;
		_priceHistory.RemoveAll(p => p.Time < cutoffTime);
	}

	private void CalculateTrainingMaximums(DateTime referenceTime, HeatmapPriceChangeMode mode, HeatmapPriceChangePeriod period, HeatmapTrainingPeriod trainingPeriod)
	{
		var trainingStart = referenceTime.AddSeconds(-(int)trainingPeriod);
		var startIndex = -1;
		var endIndex = -1;

		for (var i = 0; i < _priceHistory.Count; i++)
		{
			var point = _priceHistory[i];
			if (point.Time < trainingStart || point.Time > referenceTime)
				continue;

			if (startIndex < 0)
				startIndex = i;

			endIndex = i;
		}

		var trainingDataCount = endIndex - startIndex + 1;
		if (startIndex < 0 || trainingDataCount < 10)
			return;

		var maxValue = MinimumMaxValue;
		var step = Math.Max(1, trainingDataCount / 100);

		for (var i = startIndex; i <= endIndex; i += step)
		{
			var value = Math.Abs(CalculateValueAt(_priceHistory[i].Time, mode, period, _priceHistory));
			maxValue = Math.Max(maxValue, value);
		}

		if ((endIndex - startIndex) % step != 0)
		{
			var value = Math.Abs(CalculateValueAt(_priceHistory[endIndex].Time, mode, period, _priceHistory));
			maxValue = Math.Max(maxValue, value);
		}

		switch (mode)
		{
			case HeatmapPriceChangeMode.StandardDeviation:
				_maxStandardDeviation = Math.Max(_maxStandardDeviation, maxValue);
				break;
			case HeatmapPriceChangeMode.RateOfChange:
				_maxRateOfChange = Math.Max(_maxRateOfChange, maxValue);
				break;
		}
	}

	private void UpdateTrainingMaximums(DateTime currentTime, HeatmapPriceChangeMode mode, HeatmapPriceChangePeriod period)
	{
		var value = Math.Abs(CalculateValue(currentTime, mode, period));

		switch (mode)
		{
			case HeatmapPriceChangeMode.StandardDeviation:
				_maxStandardDeviation = Math.Max(_maxStandardDeviation, value);
				break;
			case HeatmapPriceChangeMode.RateOfChange:
				_maxRateOfChange = Math.Max(_maxRateOfChange, value);
				break;
		}
	}

	private void AdaptMaximums(decimal currentValue, HeatmapPriceChangeMode mode)
	{
		const decimal adaptationRate = 0.001m;
		var absValue = Math.Abs(currentValue);

		switch (mode)
		{
			case HeatmapPriceChangeMode.StandardDeviation:
				if (absValue > _maxStandardDeviation)
					_maxStandardDeviation = absValue;
				else if (absValue < _maxStandardDeviation * 0.5m)
					_maxStandardDeviation = _maxStandardDeviation * (1 - adaptationRate) + absValue * adaptationRate * 2;
				break;
			case HeatmapPriceChangeMode.RateOfChange:
				if (absValue > _maxRateOfChange)
					_maxRateOfChange = absValue;
				else if (absValue < _maxRateOfChange * 0.5m)
					_maxRateOfChange = _maxRateOfChange * (1 - adaptationRate) + absValue * adaptationRate * 2;
				break;
		}
	}

	private decimal CalculateValue(DateTime referenceTime, HeatmapPriceChangeMode mode, HeatmapPriceChangePeriod period) =>
		CalculateValueAt(referenceTime, mode, period, _priceHistory);

	private decimal CalculateValueAt(DateTime referenceTime, HeatmapPriceChangeMode mode, HeatmapPriceChangePeriod period, List<PricePoint> data)
	{
		var periodSeconds = (int)period;
		var periodStart = referenceTime.AddSeconds(-periodSeconds);
		var currentPrice = GetPriceAtOrBeforeTime(referenceTime, data);

		if (currentPrice == 0)
			return 0;

		return mode switch
		{
			HeatmapPriceChangeMode.StandardDeviation => CalculateStandardDeviationInRange(data, currentPrice, periodStart, referenceTime),
			HeatmapPriceChangeMode.RateOfChange => CalculateRateOfChange(data, currentPrice, referenceTime, periodSeconds),
			_ => 0
		};
	}

	private static decimal CalculateStandardDeviationInRange(List<PricePoint> data, decimal currentPrice, DateTime periodStart, DateTime periodEnd)
	{
		decimal weightedSum = 0;
		var totalWeight = 0;

		for (var i = 0; i < data.Count; i++)
		{
			var point = data[i];
			if (point.Time < periodStart || point.Time > periodEnd)
				continue;

			weightedSum += point.Price * point.TickCount;
			totalWeight += point.TickCount;
		}

		if (totalWeight == 0)
			return 0;

		var sma = weightedSum / totalWeight;
		return currentPrice - sma;
	}

	private decimal CalculateRateOfChange(List<PricePoint> data, decimal currentPrice, DateTime referenceTime, int periodSeconds)
	{
		var period = (HeatmapPriceChangePeriod)periodSeconds;

		if (_currentPeriod == period && _currentPeriodStartPrice > 0)
			return ((currentPrice - _currentPeriodStartPrice) / _currentPeriodStartPrice) * 100;

		var periodAgo = referenceTime.AddSeconds(-periodSeconds);
		var referenceOldPrice = GetPriceAtOrBeforeTime(periodAgo, data);

		return referenceOldPrice == 0
			? 0
			: ((currentPrice - referenceOldPrice) / referenceOldPrice) * 100;
	}

	private static decimal GetPriceAtOrBeforeTime(DateTime targetTime, List<PricePoint> data)
	{
		for (var i = data.Count - 1; i >= 0; i--)
		{
			if (data[i].Time <= targetTime)
				return data[i].Price;
		}

		return 0;
	}

	private decimal NormalizeValue(decimal value, HeatmapPriceChangeMode mode)
	{
		var maxValue = mode switch
		{
			HeatmapPriceChangeMode.StandardDeviation => _maxStandardDeviation,
			HeatmapPriceChangeMode.RateOfChange => _maxRateOfChange,
			_ => MinimumMaxValue
		};

		if (maxValue == 0)
			return 0;

		var normalized = value / maxValue * 100;
		return Math.Max(-100, Math.Min(100, normalized));
	}

	private void UpdateCurrentPeriodWindow(DateTime currentTime, decimal currentPrice, HeatmapPriceChangePeriod period)
	{
		if (_currentPeriod != period)
		{
			_currentPeriod = period;
			_currentPeriodStartPrice = 0;
			_currentPeriodStartTime = DateTime.MinValue;
		}

		var periodSeconds = (int)period;
		var windowStart = currentTime.AddSeconds(-periodSeconds);

		if (_currentPeriodStartTime != DateTime.MinValue && _currentPeriodStartTime >= windowStart)
			return;

		if (GetPricePointAtOrAfterTime(windowStart) is { } startPoint)
		{
			_currentPeriodStartPrice = startPoint.Price;
			_currentPeriodStartTime = startPoint.Time;
		}
	}

	private PricePoint? GetPricePointAtOrAfterTime(DateTime targetTime)
	{
		for (var i = 0; i < _priceHistory.Count; i++)
		{
			if (_priceHistory[i].Time >= targetTime)
				return _priceHistory[i];
		}

		return null;
	}

	private void ResetState()
	{
		_trainingStartTime = DateTime.MinValue;
		_lastTickTime = DateTime.MinValue;
		_virtualCurrentTime = DateTime.MinValue;
		_lastCleanupTime = DateTime.MinValue;
		_maxStandardDeviation = MinimumMaxValue;
		_maxRateOfChange = MinimumMaxValue;
		_currentValue = 0;
		_currentPrice = 0;
		_isTrainingComplete = false;
		_cacheTime = DateTime.MinValue;
		_currentPeriodStartPrice = 0;
		_currentPeriodStartTime = DateTime.MinValue;
		CurrentValue = 0;
	}

	private readonly record struct PricePoint(DateTime Time, long TimestampNanos, decimal Price, int TickCount);
}
