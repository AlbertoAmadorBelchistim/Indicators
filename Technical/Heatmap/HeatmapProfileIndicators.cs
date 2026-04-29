namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ATAS.Indicators.Heatmap;
using OFT.Rendering.Heatmap;

[HeatmapIndicator("heatmap.vwap", DisplayName = "VWAP")]
public sealed class HeatmapVwapIndicator
	: HeatmapIndicator<HeatmapVwapSettings>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
	#region Static fields

	private static readonly HeatmapIndicatorDescriptor _descriptor;
	private static readonly HeatmapIndicatorVisualHandle _line;
	private static readonly HeatmapIndicatorSeriesHandle<HeatmapPriceLineSample> _price;

	#endregion

	#region Static constructors

	static HeatmapVwapIndicator()
	{
		var build = HeatmapIndicator.Describe("heatmap.vwap", "VWAP");
		_line = build.PriceLine("vwap.line", "VWAP");
		_price = _line.Series<HeatmapPriceLineSample>(
			"vwap.price",
			HeatmapIndicatorSeriesRole.Price,
			HeatmapIndicatorValueKind.Price);
		_descriptor = build.Done();
	}

	#endregion

	#region Readonly initialized fields

	private readonly HeatmapSeriesBuffer<HeatmapPriceLineSample> _samples = new();

	#endregion

	#region Fields

	private HeatmapIndicatorContext _context = CreateDefaultContext();
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

	public override ValueTask ResetAsync(
		HeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_context = context;
		_current = new VwapAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;
		_samples.Clear();

		return ValueTask.CompletedTask;
	}

	public override ValueTask<HeatmapIndicatorStateSnapshot?> GetSnapshotAsync(
		HeatmapIndicatorSnapshotRequest request,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var source = _samples.GetSnapshot(request);
		var style = new HeatmapIndicatorVisualStyle(
			Color: ColorToHex(_settings.Color),
			Thickness: _settings.Thickness);

		var state = _descriptor.NewState()
			.Training(source.IsTraining)
			.Visual(_line, v => v.Series(_price, source, sample => sample.Price, style))
			.Build();

		return new ValueTask<HeatmapIndicatorStateSnapshot?>(state);
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
		IReadOnlyList<HeatmapTradeTick> ticks,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		foreach (var tick in ticks)
		{
			if (!IsEligibleTick(tick))
				continue;

			ProcessTickCore(tick);
		}

		return ValueTask.CompletedTask;
	}

	public void ProcessHistoricalTicks(IEnumerable<HeatmapTradeTick> ticks)
	{
		var orderedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(tick => tick.TimestampNanos)
			.ToList();

		_current = new VwapAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;
		_samples.Clear();

		foreach (var tick in orderedTicks)
			ProcessTickCore(tick);
	}

	public void ProcessTick(HeatmapTradeTick tick)
	{
		if (!IsEligibleTick(tick))
			return;

		ProcessTickCore(tick);
	}

	public decimal GetValueAtOrBefore(long cutoffTimeNanos) =>
		_samples.GetLatestAtOrBefore(cutoffTimeNanos)?.Value.Price ?? 0;

	#endregion

	#region Private methods

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
			_samples.Append(tick.TimestampNanos, new HeatmapPriceLineSample { Price = value });
	}

	#endregion

	#region Private static methods

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.TimestampNanos > 0 && tick.Price > 0 && tick.Volume > 0;

	private static HeatmapIndicatorContext CreateDefaultContext() =>
		new()
		{
			InstrumentId = string.Empty,
			TickSize = 0m,
			LotSize = 1m,
			TimeZone = TimeZoneInfo.Utc,
		};

	private static string ColorToHex(CrossColor color) =>
		$"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

	#endregion

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
}

[HeatmapIndicator("heatmap.value-area", DisplayName = "Value Area")]
public sealed class HeatmapValueAreaIndicator
	: HeatmapIndicator<HeatmapValueAreaSettings>
	, IHeatmapWarmupIndicator
	, IHeatmapTradeTickConsumer
{
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
		var build = HeatmapIndicator.Describe("heatmap.value-area", "Value Area");
		_lines = build.ValueArea("value-area.lines", "Value Area");
		_poc = _lines.Series<HeatmapValueAreaSample>(
			"value-area.poc", HeatmapIndicatorSeriesRole.Poc, HeatmapIndicatorValueKind.Price);
		_high = _lines.Series<HeatmapValueAreaSample>(
			"value-area.high", HeatmapIndicatorSeriesRole.ValueAreaHigh, HeatmapIndicatorValueKind.Price);
		_low = _lines.Series<HeatmapValueAreaSample>(
			"value-area.low", HeatmapIndicatorSeriesRole.ValueAreaLow, HeatmapIndicatorValueKind.Price);
		_descriptor = build.Done();
	}

	#endregion

	#region Readonly initialized fields

	private readonly HeatmapSeriesBuffer<HeatmapValueAreaSample> _samples = new();

	#endregion

	#region Fields

	private HeatmapIndicatorContext _context = CreateDefaultContext();
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

	public override ValueTask ResetAsync(
		HeatmapIndicatorContext context,
		IHeatmapIndicatorRuntime runtime,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		_context = context;
		_current = new ValueAreaAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;
		_samples.Clear();

		return ValueTask.CompletedTask;
	}

	public override ValueTask<HeatmapIndicatorStateSnapshot?> GetSnapshotAsync(
		HeatmapIndicatorSnapshotRequest request,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		var source = _samples.GetSnapshot(request);
		var pocStyle = new HeatmapIndicatorVisualStyle(
			Color: ColorToHex(_settings.PocColor),
			Thickness: _settings.PocThickness);
		var valueAreaStyle = new HeatmapIndicatorVisualStyle(
			Color: ColorToHex(_settings.ValueAreaColor),
			Thickness: _settings.PocThickness);
		var showValueArea = _settings.ShowValueArea;

		var state = _descriptor.NewState()
			.Training(source.IsTraining)
			.Visual(_lines, v =>
			{
				v.Series(_poc, source, sample => sample.Poc, pocStyle);
				if (showValueArea)
				{
					v.Series(_high, source, sample => sample.ValueAreaHigh, valueAreaStyle);
					v.Series(_low, source, sample => sample.ValueAreaLow, valueAreaStyle);
				}
			})
			.Build();

		return new ValueTask<HeatmapIndicatorStateSnapshot?>(state);
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
		IReadOnlyList<HeatmapTradeTick> ticks,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();

		foreach (var tick in ticks)
		{
			if (!IsEligibleTick(tick))
				continue;

			ProcessTickCore(tick);
		}

		return ValueTask.CompletedTask;
	}

	public void ProcessHistoricalTicks(IEnumerable<HeatmapTradeTick> ticks)
	{
		var orderedTicks = ticks
			.Where(IsEligibleTick)
			.OrderBy(tick => tick.TimestampNanos)
			.ToList();

		_current = new ValueAreaAccumulator(HeatmapPeriodKey.Empty);
		_previous = null;
		_samples.Clear();

		foreach (var tick in orderedTicks)
			ProcessTickCore(tick);
	}

	public void ProcessTick(HeatmapTradeTick tick)
	{
		if (!IsEligibleTick(tick))
			return;

		ProcessTickCore(tick);
	}

	public HeatmapValueAreaSample GetValueAtOrBefore(long cutoffTimeNanos) =>
		_samples.GetLatestAtOrBefore(cutoffTimeNanos)?.Value ?? default;

	#endregion

	#region Private methods

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

	#endregion

	#region Private static methods

	private static bool IsEligibleTick(HeatmapTradeTick tick) =>
		tick.TimestampNanos > 0 && tick.Price > 0 && tick.Volume > 0;

	private static HeatmapIndicatorContext CreateDefaultContext() =>
		new()
		{
			InstrumentId = string.Empty,
			TickSize = 0m,
			LotSize = 1m,
			TimeZone = TimeZoneInfo.Utc,
		};

	private static string ColorToHex(CrossColor color) =>
		$"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";

	#endregion

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
