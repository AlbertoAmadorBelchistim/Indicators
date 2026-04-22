namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OFT.Rendering.Heatmap;

public sealed class HeatmapVwapIndicator
	: IHeatmapIndicator<HeatmapVwapSettings, HeatmapIndicatorSeriesSnapshot<HeatmapPriceLineSample>>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	private readonly object _sync = new();
	private readonly HeatmapSeriesBuffer<HeatmapPriceLineSample> _samples = new();
	private HeatmapIndicatorContext _context = CreateDefaultContext();
	private HeatmapVwapSettings _settings = HeatmapVwapSettings.Default;
	private VwapAccumulator _current = new(HeatmapPeriodKey.Empty);
	private VwapAccumulator? _previous;

	public string Id => "heatmap.vwap";

	public void Configure(HeatmapVwapSettings settings)
	{
		lock (_sync)
			_settings = settings;
	}

	public void Reset(HeatmapIndicatorContext context)
	{
		lock (_sync)
		{
			_context = context;
			_current = new VwapAccumulator(HeatmapPeriodKey.Empty);
			_previous = null;
			_samples.Clear();
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
		var orderedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(tick => tick.TimestampNanos)
			.ToList();

		lock (_sync)
		{
			_current = new VwapAccumulator(HeatmapPeriodKey.Empty);
			_previous = null;
			_samples.Clear();

			foreach (var tick in orderedTicks)
				ProcessTickCore(tick);
		}
	}

	public void ProcessTick(HeatmapTradeTick tick)
	{
		if (!IsEligibleTick(tick))
			return;

		lock (_sync)
			ProcessTickCore(tick);
	}

	public HeatmapIndicatorSeriesSnapshot<HeatmapPriceLineSample> GetSnapshot(HeatmapIndicatorSnapshotRequest request) =>
		_samples.GetSnapshot(request);

	public void ProcessTimer(HeatmapIndicatorTimerTick tick)
	{
	}

	public decimal GetValueAtOrBefore(long cutoffTimeNanos) =>
		_samples.GetLatestAtOrBefore(cutoffTimeNanos)?.Value.Price ?? 0;

	private void ProcessTickCore(HeatmapTradeTick tick)
	{
		var key = HeatmapPeriodResolver.Resolve(tick.Time, _context, _settings.Scope);
		if (_current.Key != key)
		{
			if (_current.HasValue)
				_previous = _current;

			_current = new VwapAccumulator(key);
		}

		_current.Add(tick.Price, tick.Volume);

		var value = _settings.Scope switch
		{
			HeatmapProfileScope.LastDay or HeatmapProfileScope.LastWeek or HeatmapProfileScope.LastMonth =>
				_previous?.Value ?? 0m,
			_ => _current.Value
		};

		if (value > 0)
			_samples.Append(tick.TimestampNanos, new HeatmapPriceLineSample(value));
	}

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.TimestampNanos > 0 && tick.Price > 0 && tick.Volume > 0;

	private static HeatmapIndicatorContext CreateDefaultContext() =>
		new(string.Empty, 0, 1, TimeZoneInfo.Utc);

	private sealed class VwapAccumulator(HeatmapPeriodKey key)
	{
		private decimal _cumulativePriceVolume;
		private decimal _cumulativeVolume;

		public HeatmapPeriodKey Key { get; } = key;

		public bool HasValue => _cumulativeVolume > 0;

		public decimal Value => _cumulativeVolume > 0
			? _cumulativePriceVolume / _cumulativeVolume
			: 0m;

		public void Add(decimal price, decimal volume)
		{
			_cumulativePriceVolume += price * volume;
			_cumulativeVolume += volume;
		}
	}
}

public sealed class HeatmapValueAreaIndicator
	: IHeatmapIndicator<HeatmapValueAreaSettings, HeatmapIndicatorSeriesSnapshot<HeatmapValueAreaSample>>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	private const decimal ValueAreaFraction = 0.70m;

	private readonly object _sync = new();
	private readonly HeatmapSeriesBuffer<HeatmapValueAreaSample> _samples = new();
	private HeatmapIndicatorContext _context = CreateDefaultContext();
	private HeatmapValueAreaSettings _settings = HeatmapValueAreaSettings.Default;
	private ValueAreaAccumulator _current = new(HeatmapPeriodKey.Empty);
	private ValueAreaAccumulator? _previous;

	public string Id => "heatmap.value-area";

	public void Configure(HeatmapValueAreaSettings settings)
	{
		lock (_sync)
			_settings = settings;
	}

	public void Reset(HeatmapIndicatorContext context)
	{
		lock (_sync)
		{
			_context = context;
			_current = new ValueAreaAccumulator(HeatmapPeriodKey.Empty);
			_previous = null;
			_samples.Clear();
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
		var orderedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(tick => tick.TimestampNanos)
			.ToList();

		lock (_sync)
		{
			_current = new ValueAreaAccumulator(HeatmapPeriodKey.Empty);
			_previous = null;
			_samples.Clear();

			foreach (var tick in orderedTicks)
				ProcessTickCore(tick);
		}
	}

	public void ProcessTick(HeatmapTradeTick tick)
	{
		if (!IsEligibleTick(tick))
			return;

		lock (_sync)
			ProcessTickCore(tick);
	}

	public HeatmapIndicatorSeriesSnapshot<HeatmapValueAreaSample> GetSnapshot(HeatmapIndicatorSnapshotRequest request) =>
		_samples.GetSnapshot(request);

	public void ProcessTimer(HeatmapIndicatorTimerTick tick)
	{
	}

	public HeatmapValueAreaSample GetValueAtOrBefore(long cutoffTimeNanos) =>
		_samples.GetLatestAtOrBefore(cutoffTimeNanos)?.Value ?? default;

	private void ProcessTickCore(HeatmapTradeTick tick)
	{
		var key = HeatmapPeriodResolver.Resolve(tick.Time, _context, _settings.Scope);
		if (_current.Key != key)
		{
			if (_current.HasValue)
				_previous = _current;

			_current = new ValueAreaAccumulator(key);
		}

		_current.Add(tick.Price, tick.Volume);

		var value = _settings.Scope switch
		{
			HeatmapProfileScope.LastDay or HeatmapProfileScope.LastWeek or HeatmapProfileScope.LastMonth =>
				_previous?.Value ?? default,
			_ => _current.Value
		};

		if (value.Poc > 0)
			_samples.Append(tick.TimestampNanos, value);
	}

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.TimestampNanos > 0 && tick.Price > 0 && tick.Volume > 0;

	private static HeatmapIndicatorContext CreateDefaultContext() =>
		new(string.Empty, 0, 1, TimeZoneInfo.Utc);

	private sealed class ValueAreaAccumulator(HeatmapPeriodKey key)
	{
		private readonly SortedDictionary<decimal, decimal> _volumeByPrice = new();
		private decimal _totalVolume;
		private decimal _pocPrice;
		private decimal _pocVolume;

		public HeatmapPeriodKey Key { get; } = key;

		public bool HasValue => _pocPrice > 0 && _totalVolume > 0;

		public HeatmapValueAreaSample Value
		{
			get
			{
				if (!HasValue)
					return default;

				var (valueAreaLow, valueAreaHigh) = CalculateValueArea();
				return new HeatmapValueAreaSample(_pocPrice, valueAreaHigh, valueAreaLow);
			}
		}

		public void Add(decimal price, decimal volume)
		{
			var updatedVolume = volume;
			if (_volumeByPrice.TryGetValue(price, out var currentVolume))
				updatedVolume += currentVolume;

			_volumeByPrice[price] = updatedVolume;
			_totalVolume += volume;

			if (updatedVolume > _pocVolume || _pocPrice <= 0)
			{
				_pocPrice = price;
				_pocVolume = updatedVolume;
			}
		}

		private (decimal Low, decimal High) CalculateValueArea()
		{
			var levels = _volumeByPrice.ToArray();
			if (levels.Length == 0)
				return (0, 0);

			var pocIndex = Array.FindIndex(levels, level => level.Key == _pocPrice);
			if (pocIndex < 0)
				return (_pocPrice, _pocPrice);

			var lowIndex = pocIndex;
			var highIndex = pocIndex;
			var includedVolume = levels[pocIndex].Value;
			var targetVolume = _totalVolume * ValueAreaFraction;

			while (includedVolume < targetVolume && (lowIndex > 0 || highIndex < levels.Length - 1))
			{
				var lowerVolume = lowIndex > 0 ? levels[lowIndex - 1].Value : -1m;
				var upperVolume = highIndex < levels.Length - 1 ? levels[highIndex + 1].Value : -1m;

				if (upperVolume > lowerVolume)
				{
					highIndex++;
					includedVolume += upperVolume;
				}
				else if (lowerVolume >= 0)
				{
					lowIndex--;
					includedVolume += lowerVolume;
				}
				else
				{
					break;
				}
			}

			return (levels[lowIndex].Key, levels[highIndex].Key);
		}
	}
}

internal readonly record struct HeatmapPeriodKey(int Year, int Period, HeatmapProfileScope Scope)
{
	public static HeatmapPeriodKey Empty { get; } = new(0, 0, HeatmapProfileScope.DataStart);
}

internal static class HeatmapPeriodResolver
{
	public static HeatmapPeriodKey Resolve(
		DateTime timestamp,
		HeatmapIndicatorContext context,
		HeatmapProfileScope scope)
	{
		var local = timestamp.Kind == DateTimeKind.Unspecified
			? timestamp
			: TimeZoneInfo.ConvertTime(timestamp, context.TimeZone);

		return scope switch
		{
			HeatmapProfileScope.CurrentDay or HeatmapProfileScope.LastDay =>
				new HeatmapPeriodKey(local.Year, local.DayOfYear, scope),
			HeatmapProfileScope.CurrentWeek or HeatmapProfileScope.LastWeek =>
				new HeatmapPeriodKey(IsoWeekYear(local), IsoWeek(local), scope),
			HeatmapProfileScope.CurrentMonth or HeatmapProfileScope.LastMonth =>
				new HeatmapPeriodKey(local.Year, local.Month, scope),
			_ => new HeatmapPeriodKey(0, 0, scope)
		};
	}

	private static int IsoWeek(DateTime value)
	{
		var day = (int)value.DayOfWeek;
		if (day == 0)
			day = 7;

		var thursday = value.AddDays(4 - day);
		return (thursday.DayOfYear - 1) / 7 + 1;
	}

	private static int IsoWeekYear(DateTime value)
	{
		var day = (int)value.DayOfWeek;
		if (day == 0)
			day = 7;

		return value.AddDays(4 - day).Year;
	}
}
