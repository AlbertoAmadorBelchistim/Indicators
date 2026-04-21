namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OFT.Rendering.Heatmap;

public sealed class HeatmapMarketPressureIndicator
	: IHeatmapIndicator<HeatmapPressureSettings, HeatmapIndicatorSeriesSnapshot<HeatmapPressureSample>>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	private const decimal MinimumMaxPressure = 0.01m;
	private const int SnapshotIntervalSeconds = 5;

	private readonly object _sync = new();
	private readonly List<PressureEvent> _buyerEvents = new();
	private readonly List<PressureEvent> _sellerEvents = new();
	private readonly List<HeatmapTradeTick> _historicalTicks = new();
	private readonly List<PressureSnapshot> _historicalSnapshots = new();
	private readonly HeatmapSeriesBuffer<HeatmapPressureSample> _samples = new();

	private HeatmapPressureSettings _settings = HeatmapPressureSettings.Default;
	private DateTime _lastSnapshotTime = DateTime.MinValue;
	private DateTime _trainingStartTime = DateTime.MinValue;
	private DateTime _lastTickTime = DateTime.MinValue;
	private DateTime _virtualCurrentTime = DateTime.MinValue;
	private DateTime _lastMaxUpdateTime = DateTime.MinValue;
	private DateTime _lastCleanupTime = DateTime.MinValue;
	private DateTime _lastHistoricalCleanupTime = DateTime.MinValue;
	private decimal _maxPressure = MinimumMaxPressure;
	private bool _isTraining = true;
	private decimal _cachedBuyersPressure;
	private decimal _cachedSellersPressure;
	private DateTime _cacheTime = DateTime.MinValue;
	private readonly TimeSpan _cacheValidityPeriod = TimeSpan.FromMilliseconds(50);

	public string Id => "heatmap.market-pressure";

	public float CurrentBuyNormalized { get; private set; }

	public float CurrentSellNormalized { get; private set; }

	public bool IsTraining
	{
		get
		{
			lock (_sync)
				return _isTraining;
		}
	}

	public void Configure(HeatmapPressureSettings settings)
	{
		lock (_sync)
		{
			_settings = settings;
			_cacheTime = DateTime.MinValue;
		}

		RecalculateWithCurrentSettings();
	}

	public void Reset(HeatmapIndicatorContext context)
	{
		lock (_sync)
		{
			_buyerEvents.Clear();
			_sellerEvents.Clear();
			_historicalTicks.Clear();
			_historicalSnapshots.Clear();
			_samples.Clear();
			_lastSnapshotTime = DateTime.MinValue;
			_trainingStartTime = DateTime.MinValue;
			_lastTickTime = DateTime.MinValue;
			_virtualCurrentTime = DateTime.MinValue;
			_lastMaxUpdateTime = DateTime.MinValue;
			_lastCleanupTime = DateTime.MinValue;
			_lastHistoricalCleanupTime = DateTime.MinValue;
			_maxPressure = MinimumMaxPressure;
			_isTraining = true;
			_cacheTime = DateTime.MinValue;
			CurrentBuyNormalized = 0;
			CurrentSellNormalized = 0;
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
		var mode = _settings.Mode;
		var preset = _settings.Preset;
		var halfLife = GetHalfLifePeriod(preset);
		var trainingPeriod = GetTrainingPeriod(preset);
		var sortedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(t => t.Time)
			.ToList();

		if (sortedTicks.Count == 0)
			return;

		var lastTick = sortedTicks[^1];
		var lastTickTime = lastTick.Time;
		var maxRetention = GetMaximumDataRetention();
		var cutoffTime = lastTickTime - maxRetention;
		var relevantTicks = sortedTicks
			.Where(t => t.Time >= cutoffTime)
			.ToList();

		var eventsCutoffTime = lastTickTime - TimeSpan.FromSeconds(halfLife.TotalSeconds * 5);
		var buyerEvents = new List<PressureEvent>();
		var sellerEvents = new List<PressureEvent>();

		foreach (var tick in relevantTicks)
		{
			if (tick.Time < eventsCutoffTime)
				continue;

			var evt = new PressureEvent(tick.Time, GetWeight(tick, mode));
			if (tick.Direction == HeatmapTradeDirection.Buy)
				buyerEvents.Add(evt);
			else if (tick.Direction == HeatmapTradeDirection.Sell)
				sellerEvents.Add(evt);
		}

		var searchStartTime = lastTickTime - trainingPeriod;
		var ticksForMaxCalc = relevantTicks.Where(t => t.Time >= searchStartTime).ToList();
		var calculatedMax = CalculateHistoricalMaximumsOutsideLock(
			buyerEvents,
			sellerEvents,
			[],
			ticksForMaxCalc,
			lastTickTime,
			halfLife);

		lock (_sync)
		{
			_historicalTicks.Clear();
			_historicalTicks.AddRange(relevantTicks);
			_buyerEvents.Clear();
			_buyerEvents.AddRange(buyerEvents);
			_sellerEvents.Clear();
			_sellerEvents.AddRange(sellerEvents);

			_lastTickTime = lastTickTime;
			_virtualCurrentTime = lastTickTime;
			_trainingStartTime = lastTickTime;
			_maxPressure = Math.Max(calculatedMax, MinimumMaxPressure);
			_lastMaxUpdateTime = lastTickTime;
			_lastCleanupTime = lastTickTime;
		}

		foreach (var tick in relevantTicks)
			CalculateAndRecord(tick.Time, tick.TimestampNanos);
	}

	public void ProcessTick(HeatmapTradeTick tick)
	{
		if (!IsEligibleTick(tick))
			return;

		var mode = _settings.Mode;
		var evt = new PressureEvent(tick.Time, GetWeight(tick, mode));
		bool needCompress;

		lock (_sync)
		{
			if (_trainingStartTime == DateTime.MinValue)
				_trainingStartTime = tick.Time;

			_lastTickTime = tick.Time;
			_virtualCurrentTime = tick.Time;
			_historicalTicks.Add(tick);

			if ((tick.Time - _lastHistoricalCleanupTime).TotalSeconds >= 5)
			{
				var cutoffTime = tick.Time - GetMaximumDataRetention();
				_historicalTicks.RemoveAll(t => t.Time < cutoffTime);
				_lastHistoricalCleanupTime = tick.Time;
			}

			if (tick.Direction == HeatmapTradeDirection.Buy)
			{
				_buyerEvents.Add(evt);
				MaintainBufferSize(_buyerEvents, 10000);
			}
			else if (tick.Direction == HeatmapTradeDirection.Sell)
			{
				_sellerEvents.Add(evt);
				MaintainBufferSize(_sellerEvents, 10000);
			}

			_cacheTime = DateTime.MinValue;
			needCompress = (tick.Time - _lastSnapshotTime).TotalMinutes >= 1;
		}

		if (needCompress)
			CompressOldEventsToSnapshots(tick.Time, mode);

		CalculateAndRecord(tick.Time, tick.TimestampNanos);
	}

	public void ProcessTimer(DateTime targetTime, long timestampNanos)
	{
		lock (_sync)
		{
			if (_lastTickTime == DateTime.MinValue)
				return;

			_virtualCurrentTime = targetTime > _virtualCurrentTime
				? targetTime
				: _virtualCurrentTime.AddMilliseconds(50);
		}

		CalculateAndRecord(targetTime, timestampNanos);
	}

	public void ProcessTimer(HeatmapIndicatorTimerTick tick) => ProcessTimer(tick.Time, tick.TimestampNanos);

	public HeatmapIndicatorSeriesSnapshot<HeatmapPressureSample> GetSnapshot(HeatmapIndicatorSnapshotRequest request) =>
		_samples.GetSnapshot(request, IsTraining);

	public HeatmapPressureSample GetValueAtOrBefore(long cutoffTimeNanos) =>
		_samples.GetLatestAtOrBefore(cutoffTimeNanos)?.Value ?? default;

	private void RecalculateWithCurrentSettings()
	{
		var mode = _settings.Mode;
		var preset = _settings.Preset;
		var halfLife = GetHalfLifePeriod(preset);
		var trainingPeriod = GetTrainingPeriod(preset);
		DateTime lastTickTime;
		List<HeatmapTradeTick> ticksToProcess;
		List<PressureSnapshot> snapshotsCopy;

		lock (_sync)
		{
			_buyerEvents.Clear();
			_sellerEvents.Clear();
			_maxPressure = MinimumMaxPressure;
			_lastMaxUpdateTime = DateTime.MinValue;
			_isTraining = true;
			_cacheTime = DateTime.MinValue;

			if (_historicalTicks.Count == 0)
				return;

			lastTickTime = _historicalTicks.Max(t => t.Time);
			_trainingStartTime = lastTickTime;
			_virtualCurrentTime = lastTickTime;
			var cutoffTime = lastTickTime - TimeSpan.FromSeconds(halfLife.TotalSeconds * 5) - trainingPeriod;
			ticksToProcess = _historicalTicks
				.Where(t => t.Time >= cutoffTime)
				.OrderBy(t => t.Time)
				.ToList();
			snapshotsCopy = _historicalSnapshots.ToList();
		}

		if (ticksToProcess.Count == 0)
			return;

		var eventsCutoffTime = lastTickTime - TimeSpan.FromSeconds(halfLife.TotalSeconds * 5);
		var buyerEvents = new List<PressureEvent>();
		var sellerEvents = new List<PressureEvent>();

		foreach (var tick in ticksToProcess)
		{
			if (tick.Time < eventsCutoffTime)
				continue;

			var evt = new PressureEvent(tick.Time, GetWeight(tick, mode));
			if (tick.Direction == HeatmapTradeDirection.Buy)
				buyerEvents.Add(evt);
			else if (tick.Direction == HeatmapTradeDirection.Sell)
				sellerEvents.Add(evt);
		}

		var searchStartTime = lastTickTime - trainingPeriod;
		var ticksForMaxCalc = ticksToProcess.Where(t => t.Time >= searchStartTime).ToList();
		var calculatedMax = CalculateHistoricalMaximumsOutsideLock(
			buyerEvents,
			sellerEvents,
			snapshotsCopy,
			ticksForMaxCalc,
			lastTickTime,
			halfLife);

		lock (_sync)
		{
			_buyerEvents.AddRange(buyerEvents);
			_sellerEvents.AddRange(sellerEvents);
			_maxPressure = Math.Max(calculatedMax, MinimumMaxPressure);
			_lastMaxUpdateTime = lastTickTime;
			_lastCleanupTime = lastTickTime;
		}
	}

	private void CalculateAndRecord(DateTime referenceTime, long timestampNanos)
	{
		var preset = _settings.Preset;
		var halfLife = GetHalfLifePeriod(preset);
		decimal buyersPressure;
		decimal sellersPressure;
		decimal buyersNormalized;
		decimal sellersNormalized;

		lock (_sync)
		{
			if (_cacheTime != DateTime.MinValue &&
			    referenceTime - _cacheTime < _cacheValidityPeriod &&
			    _buyerEvents.Count > 0 &&
			    _sellerEvents.Count > 0)
			{
				var deltaSeconds = (referenceTime - _cacheTime).TotalSeconds;
				var decay = Math.Exp(-deltaSeconds / halfLife.TotalSeconds);
				buyersPressure = _cachedBuyersPressure * (decimal)decay;
				sellersPressure = _cachedSellersPressure * (decimal)decay;
			}
			else
			{
				buyersPressure = CalculatePressureWithSnapshots(true, referenceTime, halfLife);
				sellersPressure = CalculatePressureWithSnapshots(false, referenceTime, halfLife);
				_cachedBuyersPressure = buyersPressure;
				_cachedSellersPressure = sellersPressure;
				_cacheTime = referenceTime;
			}

			if ((referenceTime - _lastCleanupTime).TotalSeconds >= 1)
			{
				CleanupOldEvents(_buyerEvents, referenceTime, halfLife);
				CleanupOldEvents(_sellerEvents, referenceTime, halfLife);
				_lastCleanupTime = referenceTime;
			}

			UpdateMaximumValue(buyersPressure, sellersPressure, referenceTime, preset);

			buyersNormalized = Math.Min(100m, buyersPressure / _maxPressure * 100m);
			sellersNormalized = Math.Min(100m, sellersPressure / _maxPressure * 100m);
		}

		CurrentBuyNormalized = (float)buyersNormalized;
		CurrentSellNormalized = (float)sellersNormalized;

		if (timestampNanos > 0)
			_samples.Append(timestampNanos, new HeatmapPressureSample(CurrentBuyNormalized, CurrentSellNormalized));
	}

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.Volume > 0 &&
		(tick.Direction == HeatmapTradeDirection.Buy || tick.Direction == HeatmapTradeDirection.Sell);

	private static decimal GetWeight(HeatmapTradeTick tick, HeatmapPressureMode mode) =>
		mode == HeatmapPressureMode.Pace ? 1m : tick.Volume;

	private static decimal CalculateHistoricalMaximumsOutsideLock(
		List<PressureEvent> buyerEvents,
		List<PressureEvent> sellerEvents,
		List<PressureSnapshot> snapshots,
		List<HeatmapTradeTick> relevantTicks,
		DateTime referenceTime,
		TimeSpan halfLife)
	{
		var buyersPressureAtEnd = CalculatePressure(buyerEvents, referenceTime, halfLife) +
			CalculatePressureFromSnapshots(snapshots, true, referenceTime, halfLife);
		var sellersPressureAtEnd = CalculatePressure(sellerEvents, referenceTime, halfLife) +
			CalculatePressureFromSnapshots(snapshots, false, referenceTime, halfLife);
		var maxAtEnd = Math.Max(buyersPressureAtEnd, sellersPressureAtEnd);
		return Math.Max(maxAtEnd * 1.3m, MinimumMaxPressure);
	}

	private static decimal CalculatePressure(IEnumerable<PressureEvent> events, DateTime referenceTime, TimeSpan halfLife)
	{
		decimal pressure = 0;
		var halfLifeSeconds = halfLife.TotalSeconds;

		foreach (var evt in events)
		{
			var deltaSeconds = (referenceTime - evt.Time).TotalSeconds;
			if (deltaSeconds < 0)
				continue;

			var decay = Math.Exp(-deltaSeconds / halfLifeSeconds);
			pressure += evt.Weight * (decimal)decay;
		}

		return pressure;
	}

	private decimal CalculatePressureWithSnapshots(bool buyerSide, DateTime referenceTime, TimeSpan halfLife)
	{
		var pressure = CalculatePressureFromSnapshots(_historicalSnapshots, buyerSide, referenceTime, halfLife);
		var events = buyerSide ? _buyerEvents : _sellerEvents;
		pressure += CalculatePressure(events, referenceTime, halfLife);
		return pressure;
	}

	private static decimal CalculatePressureFromSnapshots(IEnumerable<PressureSnapshot> snapshots, bool buyerSide, DateTime referenceTime, TimeSpan halfLife)
	{
		decimal pressure = 0;
		var halfLifeSeconds = halfLife.TotalSeconds;

		foreach (var snapshot in snapshots)
		{
			var weight = buyerSide ? snapshot.BuyerWeight : snapshot.SellerWeight;
			if (weight <= 0)
				continue;

			var deltaSeconds = (referenceTime - snapshot.CenterTime).TotalSeconds;
			if (deltaSeconds < 0)
				continue;

			var decay = Math.Exp(-deltaSeconds / halfLifeSeconds);
			pressure += weight * (decimal)decay;
		}

		return pressure;
	}

	private void CleanupOldEvents(List<PressureEvent> events, DateTime referenceTime, TimeSpan halfLife)
	{
		var cutoffTime = referenceTime - TimeSpan.FromSeconds(halfLife.TotalSeconds * 5);
		var cutoffIndex = 0;

		for (var i = 0; i < events.Count; i++)
		{
			if (events[i].Time >= cutoffTime)
			{
				cutoffIndex = i;
				break;
			}

			cutoffIndex = i + 1;
		}

		if (cutoffIndex > 0)
			events.RemoveRange(0, cutoffIndex);
	}

	private static void MaintainBufferSize(List<PressureEvent> events, int maxSize)
	{
		if (events.Count <= maxSize)
			return;

		events.RemoveRange(0, events.Count - maxSize);
	}

	private void UpdateMaximumValue(decimal buyersPressure, decimal sellersPressure, DateTime referenceTime, HeatmapPressurePreset preset)
	{
		var currentMaxPressure = Math.Max(buyersPressure, sellersPressure);

		if (_isTraining && _trainingStartTime != DateTime.MinValue)
		{
			var trainingPeriod = GetTrainingPeriod(preset);
			if (_virtualCurrentTime - _trainingStartTime >= trainingPeriod)
				_isTraining = false;
		}

		if (_isTraining)
		{
			if (currentMaxPressure > _maxPressure)
			{
				_maxPressure = currentMaxPressure;
				_lastMaxUpdateTime = referenceTime;
			}

			return;
		}

		if (currentMaxPressure > _maxPressure)
		{
			_maxPressure = currentMaxPressure;
			_lastMaxUpdateTime = referenceTime;
			return;
		}

		var timeSinceLastMax = referenceTime - _lastMaxUpdateTime;
		var maxDecayPeriod = GetMaxDecayPeriod(preset);
		if (timeSinceLastMax <= maxDecayPeriod)
			return;

		var newMax = _maxPressure * 0.95m;
		newMax = Math.Max(newMax, Math.Max(currentMaxPressure, MinimumMaxPressure));

		if (newMax < _maxPressure)
		{
			_maxPressure = newMax;
			_lastMaxUpdateTime = referenceTime;
		}
	}

	private static TimeSpan GetMaxDecayPeriod(HeatmapPressurePreset preset) =>
		preset switch
		{
			HeatmapPressurePreset.Short => TimeSpan.FromSeconds(20),
			HeatmapPressurePreset.Medium => TimeSpan.FromMinutes(1),
			HeatmapPressurePreset.Long => TimeSpan.FromMinutes(2),
			_ => TimeSpan.FromMinutes(1)
		};

	private static TimeSpan GetHalfLifePeriod(HeatmapPressurePreset preset) =>
		preset switch
		{
			HeatmapPressurePreset.Short => TimeSpan.FromSeconds(10),
			HeatmapPressurePreset.Medium => TimeSpan.FromSeconds(30),
			HeatmapPressurePreset.Long => TimeSpan.FromMinutes(1),
			_ => TimeSpan.FromSeconds(30)
		};

	private static TimeSpan GetTrainingPeriod(HeatmapPressurePreset preset) =>
		preset switch
		{
			HeatmapPressurePreset.Short => TimeSpan.FromMinutes(5),
			HeatmapPressurePreset.Medium => TimeSpan.FromMinutes(15),
			HeatmapPressurePreset.Long => TimeSpan.FromHours(1),
			_ => TimeSpan.FromMinutes(15)
		};

	private static TimeSpan GetMaximumDataRetention()
	{
		var longestHalfLife = GetHalfLifePeriod(HeatmapPressurePreset.Long);
		var longestTraining = GetTrainingPeriod(HeatmapPressurePreset.Long);
		return TimeSpan.FromSeconds(longestHalfLife.TotalSeconds * 5) + longestTraining + TimeSpan.FromMinutes(25);
	}

	private void CompressOldEventsToSnapshots(DateTime currentTime, HeatmapPressureMode mode)
	{
		lock (_sync)
		{
			var compressionBoundary = currentTime.AddMinutes(-10);

			if (_lastSnapshotTime == DateTime.MinValue)
				_lastSnapshotTime = _historicalTicks.FirstOrDefault().Time;

			if (_lastSnapshotTime == DateTime.MinValue)
				_lastSnapshotTime = currentTime;

			var snapshotTime = _lastSnapshotTime;
			while (snapshotTime < compressionBoundary)
			{
				var snapshotEnd = snapshotTime.AddSeconds(SnapshotIntervalSeconds);
				var ticksInWindow = _historicalTicks
					.Where(t => t.Time >= snapshotTime && t.Time < snapshotEnd && IsEligibleTick(t))
					.ToList();

				if (ticksInWindow.Count > 0)
				{
					decimal buyerWeight;
					decimal sellerWeight;

					if (mode == HeatmapPressureMode.Pace)
					{
						buyerWeight = ticksInWindow.Count(t => t.Direction == HeatmapTradeDirection.Buy);
						sellerWeight = ticksInWindow.Count(t => t.Direction == HeatmapTradeDirection.Sell);
					}
					else
					{
						buyerWeight = ticksInWindow.Where(t => t.Direction == HeatmapTradeDirection.Buy).Sum(t => t.Volume);
						sellerWeight = ticksInWindow.Where(t => t.Direction == HeatmapTradeDirection.Sell).Sum(t => t.Volume);
					}

					_historicalSnapshots.Add(new PressureSnapshot(
						snapshotTime,
						snapshotEnd,
						buyerWeight,
						sellerWeight));
				}

				snapshotTime = snapshotEnd;
			}

			_historicalTicks.RemoveAll(t => t.Time < compressionBoundary);
			_lastSnapshotTime = snapshotTime;

			var cutoffTime = currentTime - GetMaximumDataRetention();
			_historicalSnapshots.RemoveAll(s => s.EndTime < cutoffTime);
		}
	}

	private readonly record struct PressureEvent(DateTime Time, decimal Weight);

	private readonly record struct PressureSnapshot(
		DateTime StartTime,
		DateTime EndTime,
		decimal BuyerWeight,
		decimal SellerWeight)
	{
		public DateTime CenterTime => StartTime.AddSeconds((EndTime - StartTime).TotalSeconds / 2);
	}
}
