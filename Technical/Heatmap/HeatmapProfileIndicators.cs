namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATAS.Indicators.Heatmap;
using OFT.Rendering.Heatmap;

// Reference indicator: trade-tick -> price-line overlay (VWAP).
// Patterns: minimal HeatmapIndicator<T> shape, persistent State.BeginUpdate
//           lease pattern, static-ctor descriptor build with typed series
//           projection.
// Copy as a starting point for any tick-driven price-line indicator.
[HeatmapIndicator(id: "heatmap.vwap", DisplayName = "VWAP")]
public sealed class HeatmapVwapIndicator
	: HeatmapIndicator<HeatmapVwapSettings>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	#region Nested types

	private sealed class VwapAccumulator(HeatmapPeriodKey key)
	{
		#region Fields

		private decimal _cumulativePriceVolume;
		private decimal _cumulativeVolume;

		#endregion

		#region Properties

		public HeatmapPeriodKey Key { get; } = key;

		public bool HasValue => _cumulativeVolume > 0;

		public decimal Value => _cumulativeVolume > 0
			? _cumulativePriceVolume / _cumulativeVolume
			: 0m;

		#endregion

		#region Public methods

		public void Add(decimal price, decimal volume)
		{
			_cumulativePriceVolume += price * volume;
			_cumulativeVolume += volume;
		}

		#endregion
	}

	#endregion

	#region Static fields

	private static readonly HeatmapIndicatorDescriptor _descriptor;
	private static readonly HeatmapIndicatorVisualHandle _line;
	private static readonly HeatmapIndicatorSeriesHandle<HeatmapPriceLineSample> _price;

	#endregion

	#region Static constructors

	static HeatmapVwapIndicator()
	{
		var build = Describe<HeatmapVwapIndicator>();
		_line = build.PriceLine("vwap.line", "VWAP");
		_price = _line.Series<HeatmapPriceLineSample>(
			"vwap.price",
			HeatmapIndicatorSeriesRole.Price,
			HeatmapIndicatorValueKind.Price,
			sample => sample.Price);
		_descriptor = build.Done();
	}

	#endregion

	#region Fields

	private HeatmapVwapSettings _settings = new();
	private VwapAccumulator _current = new(HeatmapPeriodKey.Empty);
	private VwapAccumulator? _previous;

	#endregion

	#region Properties

	public override HeatmapIndicatorDescriptor Descriptor => _descriptor;

	#endregion

	#region Public methods

	public override ValueTask ConfigureAsync(HeatmapVwapSettings settings, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_settings = settings;
		return ValueTask.CompletedTask;
	}

	#endregion

	#region Protected methods

	protected override ValueTask OnStateResetCoreAsync(
		IHeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_current = new VwapAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;

		return ValueTask.CompletedTask;
	}

	public async ValueTask WarmUpAsync(
		HeatmapIndicatorWarmupRequest request,
		IHeatmapIndicatorDataSources dataSources,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var ticks = await dataSources.Trades.GetTradeTicksAsync(request, cancellationToken).ConfigureAwait(false);
		cancellationToken.ThrowIfCancellationRequested();

		ProcessHistoricalTicks(ticks);
	}

	public ValueTask ProcessTicksAsync(
		HeatmapTickBatch ticks,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var span = ticks.AsSpan();
		if (span.Length == 0)
			return ValueTask.CompletedTask;

		using (var lease = State.BeginUpdate())
		{
			var seriesLease = lease.Visual(_line).Series(_price);
			ApplyStyle(lease.Visual(_line));

			for (var i = 0; i < span.Length; i++)
			{
				var tick = span[i];
				if (!IsEligibleTick(tick))
					continue;

				if (TryComputeNextValue(tick, out var sample))
					seriesLease.Append(tick.TimestampNanos, sample);
			}
		}

		return ValueTask.CompletedTask;
	}

	public void ProcessHistoricalTicks(HeatmapTickBatch ticks)
	{
		var span = ticks.AsSpan();
		var ordered = new List<HeatmapTradeTick>(span.Length);
		for (var i = 0; i < span.Length; i++)
		{
			if (IsEligibleTick(span[i]))
				ordered.Add(span[i]);
		}
		ordered.Sort(static (left, right) => left.TimestampNanos.CompareTo(right.TimestampNanos));

		_current = new VwapAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;

		using var lease = State.BeginUpdate();
		var visualLease = lease.Visual(_line);
		ApplyStyle(visualLease);
		var seriesLease = visualLease.Series(_price);
		seriesLease.Clear();

		foreach (var tick in ordered)
		{
			if (TryComputeNextValue(tick, out var sample))
				seriesLease.Append(tick.TimestampNanos, sample);
		}
	}

	public void ProcessHistoricalTicks(IEnumerable<HeatmapTradeTick> ticks)
	{
		var orderedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(tick => tick.TimestampNanos)
			.ToList();

		_current = new VwapAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;

		using var lease = State.BeginUpdate();
		var visualLease = lease.Visual(_line);
		ApplyStyle(visualLease);
		var seriesLease = visualLease.Series(_price);
		seriesLease.Clear();

		foreach (var tick in orderedTicks)
		{
			if (TryComputeNextValue(tick, out var sample))
				seriesLease.Append(tick.TimestampNanos, sample);
		}
	}

	#endregion

	#region Private methods

	private bool TryComputeNextValue(HeatmapTradeTick tick, out HeatmapPriceLineSample sample)
	{
		var key = HeatmapPeriodResolver.Resolve(tick.Time, Context, _settings.Scope);
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
		{
			sample = new HeatmapPriceLineSample { Price = value };
			return true;
		}

		sample = default!;
		return false;
	}

	private void ApplyStyle(IHeatmapVisualLease visualLease)
	{
		visualLease.Style = new HeatmapIndicatorVisualStyle(
			Color: ColorToHex(_settings.Color),
			Thickness: _settings.Thickness);
	}

	#endregion

	#region Private static methods

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.TimestampNanos > 0 && tick.Price > 0 && tick.Volume > 0;

	private static string ColorToHex(CrossColor color) =>
		$"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

	#endregion
}

// Reference indicator: market-profile derived three-series visual (Value Area).
// Patterns: multiple HeatmapIndicatorSeriesHandle<T> on a single visual with
//           per-series projection delegates, persistent State.BeginUpdate
//           lease pattern, conditional series emission gated by settings.
// Copy as a starting point for any indicator that publishes several
// related series (POC + VAH + VAL, bid/ask/mid, etc.) on one visual.
[HeatmapIndicator(id: "heatmap.value-area", DisplayName = "Value Area")]
public sealed class HeatmapValueAreaIndicator
	: HeatmapIndicator<HeatmapValueAreaSettings>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	#region Nested types

	private sealed class ValueAreaAccumulator(HeatmapPeriodKey key)
	{
		#region Readonly initialized fields

		private readonly SortedDictionary<decimal, decimal> _volumeByPrice = new();

		#endregion

		#region Fields

		private decimal _totalVolume;
		private decimal _pocPrice;
		private decimal _pocVolume;

		#endregion

		#region Properties

		public HeatmapPeriodKey Key { get; } = key;

		public bool HasValue => _pocPrice > 0 && _totalVolume > 0;

		public HeatmapValueAreaSample Value
		{
			get
			{
				if (!HasValue)
					return default;

				var (valueAreaLow, valueAreaHigh) = CalculateValueArea();
				return new HeatmapValueAreaSample
			{
				Poc = _pocPrice,
				ValueAreaHigh = valueAreaHigh,
				ValueAreaLow = valueAreaLow,
			};
			}
		}

		#endregion

		#region Public methods

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

		#endregion

		#region Private methods

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

		#endregion
	}

	#endregion

	#region Const fields

	private const decimal ValueAreaFraction = 0.70m;

	#endregion

	#region Static fields

	private static readonly HeatmapIndicatorDescriptor _descriptor;
	private static readonly HeatmapIndicatorVisualHandle _lines;
	private static readonly HeatmapIndicatorSeriesHandle<HeatmapValueAreaSample> _poc;
	private static readonly HeatmapIndicatorSeriesHandle<HeatmapValueAreaSample> _high;
	private static readonly HeatmapIndicatorSeriesHandle<HeatmapValueAreaSample> _low;

	#endregion

	#region Static constructors

	static HeatmapValueAreaIndicator()
	{
		var build = Describe<HeatmapValueAreaIndicator>();
		_lines = build.ValueArea("value-area.lines", "Value Area");
		_poc = _lines.Series<HeatmapValueAreaSample>(
			"value-area.poc", HeatmapIndicatorSeriesRole.Poc, HeatmapIndicatorValueKind.Price,
			sample => sample.Poc);
		_high = _lines.Series<HeatmapValueAreaSample>(
			"value-area.high", HeatmapIndicatorSeriesRole.ValueAreaHigh, HeatmapIndicatorValueKind.Price,
			sample => sample.ValueAreaHigh);
		_low = _lines.Series<HeatmapValueAreaSample>(
			"value-area.low", HeatmapIndicatorSeriesRole.ValueAreaLow, HeatmapIndicatorValueKind.Price,
			sample => sample.ValueAreaLow);
		_descriptor = build.Done();
	}

	#endregion

	#region Fields

	private HeatmapValueAreaSettings _settings = new();
	private ValueAreaAccumulator _current = new(HeatmapPeriodKey.Empty);
	private ValueAreaAccumulator? _previous;

	#endregion

	#region Properties

	public override HeatmapIndicatorDescriptor Descriptor => _descriptor;

	#endregion

	#region Public methods

	public override ValueTask ConfigureAsync(HeatmapValueAreaSettings settings, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_settings = settings;
		return ValueTask.CompletedTask;
	}

	#endregion

	#region Protected methods

	protected override ValueTask OnStateResetCoreAsync(
		IHeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_current = new ValueAreaAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;

		return ValueTask.CompletedTask;
	}

	public async ValueTask WarmUpAsync(
		HeatmapIndicatorWarmupRequest request,
		IHeatmapIndicatorDataSources dataSources,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var ticks = await dataSources.Trades.GetTradeTicksAsync(request, cancellationToken).ConfigureAwait(false);
		cancellationToken.ThrowIfCancellationRequested();

		ProcessHistoricalTicks(ticks);
	}

	public ValueTask ProcessTicksAsync(
		HeatmapTickBatch ticks,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var span = ticks.AsSpan();
		if (span.Length == 0)
			return ValueTask.CompletedTask;

		using (var lease = State.BeginUpdate())
		{
			var visualLease = lease.Visual(_lines);
			ApplyStyles(visualLease);

			var pocLease = visualLease.Series(_poc);
			var highLease = visualLease.Series(_high);
			var lowLease = visualLease.Series(_low);
			var publishValueArea = _settings.ShowValueArea;

			for (var i = 0; i < span.Length; i++)
			{
				var tick = span[i];
				if (!IsEligibleTick(tick))
					continue;

				if (TryComputeNextValue(tick, out var sample))
				{
					pocLease.Append(tick.TimestampNanos, sample);
					if (publishValueArea)
					{
						highLease.Append(tick.TimestampNanos, sample);
						lowLease.Append(tick.TimestampNanos, sample);
					}
				}
			}
		}

		return ValueTask.CompletedTask;
	}

	public void ProcessHistoricalTicks(HeatmapTickBatch ticks)
	{
		var span = ticks.AsSpan();
		var ordered = new List<HeatmapTradeTick>(span.Length);
		for (var i = 0; i < span.Length; i++)
		{
			if (IsEligibleTick(span[i]))
				ordered.Add(span[i]);
		}
		ordered.Sort(static (left, right) => left.TimestampNanos.CompareTo(right.TimestampNanos));

		_current = new ValueAreaAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;

		using var lease = State.BeginUpdate();
		var visualLease = lease.Visual(_lines);
		ApplyStyles(visualLease);

		var pocLease = visualLease.Series(_poc);
		pocLease.Clear();
		var highLease = visualLease.Series(_high);
		highLease.Clear();
		var lowLease = visualLease.Series(_low);
		lowLease.Clear();
		var publishValueArea = _settings.ShowValueArea;

		foreach (var tick in ordered)
		{
			if (TryComputeNextValue(tick, out var sample))
			{
				pocLease.Append(tick.TimestampNanos, sample);
				if (publishValueArea)
				{
					highLease.Append(tick.TimestampNanos, sample);
					lowLease.Append(tick.TimestampNanos, sample);
				}
			}
		}
	}

	public void ProcessHistoricalTicks(IEnumerable<HeatmapTradeTick> ticks)
	{
		var orderedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(tick => tick.TimestampNanos)
			.ToList();

		_current = new ValueAreaAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;

		using var lease = State.BeginUpdate();
		var visualLease = lease.Visual(_lines);
		ApplyStyles(visualLease);

		var pocLease = visualLease.Series(_poc);
		pocLease.Clear();
		var highLease = visualLease.Series(_high);
		highLease.Clear();
		var lowLease = visualLease.Series(_low);
		lowLease.Clear();
		var publishValueArea = _settings.ShowValueArea;

		foreach (var tick in orderedTicks)
		{
			if (TryComputeNextValue(tick, out var sample))
			{
				pocLease.Append(tick.TimestampNanos, sample);
				if (publishValueArea)
				{
					highLease.Append(tick.TimestampNanos, sample);
					lowLease.Append(tick.TimestampNanos, sample);
				}
			}
		}
	}

	#endregion

	#region Private methods

	private bool TryComputeNextValue(HeatmapTradeTick tick, out HeatmapValueAreaSample sample)
	{
		var key = HeatmapPeriodResolver.Resolve(tick.Time, Context, _settings.Scope);
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
		{
			sample = value;
			return true;
		}

		sample = default!;
		return false;
	}

	private void ApplyStyles(IHeatmapVisualLease visualLease)
	{
		var pocStyle = new HeatmapIndicatorVisualStyle(
			Color: ColorToHex(_settings.PocColor),
			Thickness: _settings.PocThickness);
		var valueAreaStyle = new HeatmapIndicatorVisualStyle(
			Color: ColorToHex(_settings.ValueAreaColor),
			Thickness: _settings.PocThickness);

		visualLease.Style = pocStyle;
		visualLease.Series(_poc).Style = pocStyle;
		visualLease.Series(_high).Style = valueAreaStyle;
		visualLease.Series(_low).Style = valueAreaStyle;
	}

	#endregion

	#region Private static methods

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.TimestampNanos > 0 && tick.Price > 0 && tick.Volume > 0;

	private static string ColorToHex(CrossColor color) =>
		$"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

	#endregion
}

internal readonly record struct HeatmapPeriodKey(int Year, int Period, HeatmapProfileScope Scope)
{
	public static HeatmapPeriodKey Empty { get; } = new(0, 0, HeatmapProfileScope.DataStart);
}

internal static class HeatmapPeriodResolver
{
	public static HeatmapPeriodKey Resolve(
		DateTime timestamp,
		IHeatmapIndicatorContext? context,
		HeatmapProfileScope scope)
	{
		var timeZone = context?.TimeZone ?? TimeZoneInfo.Utc;
		var local = timestamp.Kind == DateTimeKind.Unspecified
			? timestamp
			: TimeZoneInfo.ConvertTime(timestamp, timeZone);

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
