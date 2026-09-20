namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Text;

using OFT.Attributes.Editors;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;

using Utils.Common.Logging;

[DisplayName("Diagonal Imbalance")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Description("Detects diagonal bid/ask imbalances in the footprint and stacked imbalance zones.")]
public class DiagonalImbalance : Indicator
{
	#region Nested Types: Engine

	// One diagonal imbalance found in a bar, on a row of RowTicks price levels.
	// Buy:  Ask of the row against Bid of the row below.
	// Sell: Bid of the row against Ask of the row above.
	// Price is the lowest price of the row and TopPrice the highest (equal when RowTicks is 1).
	internal readonly struct ImbalanceLevel
	{
		public ImbalanceLevel(decimal price, decimal topPrice, bool isBuy, decimal dominant, decimal passive)
		{
			Price = price;
			TopPrice = topPrice;
			IsBuy = isBuy;
			Dominant = dominant;
			Passive = passive;
		}

		public decimal Price { get; }

		public decimal TopPrice { get; }

		public bool IsBuy { get; }

		public decimal Dominant { get; }

		public decimal Passive { get; }
	}

	// Run of at least MinStackedLevels consecutive price levels of a bar with an imbalance
	// on the same side. Low and High are the prices of the lowest and highest level.
	internal readonly struct StackedBlock
	{
		public StackedBlock(decimal low, decimal high, bool isBuy, int levels)
		{
			Low = low;
			High = high;
			IsBuy = isBuy;
			Levels = levels;
		}

		public decimal Low { get; }

		public decimal High { get; }

		public bool IsBuy { get; }

		public int Levels { get; }
	}

	// Zone created from a stacked block when its bar closes. It extends to the right until
	// price trades through it: below Low for a buy zone, above High for a sell zone.
	internal sealed class ImbalanceZone
	{
		public ImbalanceZone(int startBar, StackedBlock block)
		{
			StartBar = startBar;
			Low = block.Low;
			High = block.High;
			IsBuy = block.IsBuy;
			Levels = block.Levels;
		}

		public int StartBar { get; }

		public decimal Low { get; }

		public decimal High { get; }

		public bool IsBuy { get; }

		public int Levels { get; }

		// Last bar covered by the zone; -1 while the zone is active.
		public int EndBar { get; private set; } = -1;

		public ZoneEnd EndReason { get; private set; } = ZoneEnd.Active;

		public bool IsActive => EndBar < 0;

		// Retest tracking for alerts: whether the last observed price was inside the zone
		// (null until first observed) and when the last retest alert fired.
		public bool? PriceInside { get; set; }

		public DateTime LastRetestAlertUtc { get; set; } = DateTime.MinValue;

		public bool Contains(decimal price)
		{
			return price >= Low && price <= High;
		}

		public void End(int bar, ZoneEnd reason)
		{
			EndBar = bar;
			EndReason = reason;
		}

		// A buy zone (stacked Ask imbalances) acts as support and a sell zone (stacked Bid
		// imbalances) as resistance; the far edge is Low for a buy zone and High for a sell zone.
		public bool IsBrokenBy(ZoneBreakMode mode, decimal barLow, decimal barHigh, decimal barClose)
		{
			return mode switch
			{
				ZoneBreakMode.Touch => IsBuy ? barLow <= High : barHigh >= Low,
				ZoneBreakMode.CloseBeyond => IsBuy ? barClose < Low : barClose > High,
				_ => IsBuy ? barLow < Low : barHigh > High
			};
		}
	}

	#endregion

	#region Nested Types: Visuals

	public enum MarkLayout
	{
		// Footprint halves when the bar is at least MinSplitBarWidth wide, full bar otherwise.
		[Display(Name = "Auto")]
		Auto,

		// Sell marks on the left (Bid) half, buy marks on the right (Ask) half.
		[Display(Name = "Footprint halves")]
		FootprintHalves,

		// Marks cover the whole bar width.
		[Display(Name = "Full bar")]
		FullBar
	}

	#endregion

	#region Nested Types: Zones

	public enum ZoneBreakMode
	{
		// Price trades into the zone.
		[Display(Name = "Touch")]
		Touch,

		// Price trades beyond the far edge of the zone (below a buy zone, above a sell zone).
		[Display(Name = "Trade through")]
		TradeThrough,

		// A bar closes beyond the far edge of the zone. Only closed bars can break it.
		[Display(Name = "Close beyond")]
		CloseBeyond
	}

	internal enum ZoneEnd
	{
		Active,
		Broken,
		Expired,
		Replaced
	}

	#endregion

	#region Fields

	// Number of most recent closed bars whose imbalances are written to the log after a recalculation.
	private const int LoggedHistoryBars = 20;

	// Maximum number of levels listed per logged bar.
	private const int LoggedLevelsPerBar = 12;

	// Minimum bar width in pixels to draw buy and sell marks on separate halves of the bar.
	private const int MinSplitBarWidth = 6;

	private static readonly ImbalanceLevel[] NoImbalances = Array.Empty<ImbalanceLevel>();
	private static readonly StackedBlock[] NoStacks = Array.Empty<StackedBlock>();

	// Imbalances per bar index; null until the bar has been calculated as a closed bar.
	private readonly List<ImbalanceLevel[]> _barImbalances = new();

	// Stacked blocks per closed bar, same indexing as _barImbalances.
	private readonly List<StackedBlock[]> _barStacks = new();

	// Guards _barImbalances and the forming bar: written by the calculation thread,
	// read by the render thread.
	private readonly object _barsLock = new();

	// Latest evaluation of the forming bar; replaced on every update and discarded
	// once that bar closes and is evaluated as a closed bar.
	private int _formingBar = -1;
	private ImbalanceLevel[] _formingLevels = NoImbalances;

	// Zones in creation order (StartBar ascending), and the subset still active. Guarded by _barsLock.
	private readonly List<ImbalanceZone> _zones = new();
	private readonly List<ImbalanceZone> _activeZones = new();

	// Visible zones copied under the lock by OnRender, reused between frames: (start, end, zone).
	private readonly List<(int StartBar, int EndBar, ImbalanceZone Zone)> _renderZones = new();

	// Visible bars copied under the lock by OnRender, reused between frames.
	private readonly List<(int Bar, ImbalanceLevel[] Levels)> _renderBars = new();

	private readonly PriceVolumeInfo _levelCache = new();

	private decimal _imbalanceRatio = 3m;
	private decimal _minDominantVolume = 20m;
	private bool _ignoreZeroLevels;
	private decimal _minVolumeDifference;
	private int _minStackedLevels = 3;
	private int _rowTicks = 1;

	// Per-row Ask and Bid sums of the bar being evaluated; reused between bars (calculation thread only).
	private decimal[] _rowAsk = new decimal[64];
	private decimal[] _rowBid = new decimal[64];

	private ZoneBreakMode _zoneBreakMode = ZoneBreakMode.TradeThrough;
	private int _maxZoneAgeBars;
	private int _maxActiveZones = 50;

	private bool _showMarks = true;
	private Color _buyColor = Color.FromArgb(140, 0, 200, 83);
	private Color _sellColor = Color.FromArgb(140, 229, 57, 53);

	private bool _showZones = true;
	private Color _buyZoneColor = Color.FromArgb(50, 0, 200, 83);
	private Color _sellZoneColor = Color.FromArgb(50, 229, 57, 53);

	private MarkLayout _markLayout = MarkLayout.Auto;
	private int _zoneBorderWidth = 1;
	private bool _showZoneLabels = true;
	private int _labelFontSize = 8;

	// Rendering resources, rebuilt when the related properties change.
	private RenderPen _buyZonePen;
	private RenderPen _sellZonePen;
	private RenderFont _labelFont = new("Arial", 8);

	private int _days = 5;
	private bool _useSessionFilter;
	private TimeSpan _sessionStart = new(9, 30, 0);
	private TimeSpan _sessionEnd = new(16, 0, 0);

	// First bar evaluated, derived from Days on every recalculation.
	private int _firstCalculatedBar;

	private bool _alertOnNewZone;
	private bool _alertOnRetest;
	private string _alertFile = "alert2";
	private int _alertCooldownSeconds = 60;

	// Alerts produced under _barsLock and raised after it is released.
	private readonly List<(string Message, bool IsBuy)> _pendingAlerts = new();

	private bool _historyLoaded;
	private int _historyBuyCount;
	private int _historySellCount;
	private int _historyBuyStacks;
	private int _historySellStacks;

	#endregion

	#region Properties

	[Display(Name = "Imbalance ratio", GroupName = "Calculation", Order = 100,
		Description = "Minimum ratio between the dominant side and the diagonal passive side, e.g. 3 = 300%.")]
	[Range(1, 100)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal ImbalanceRatio
	{
		get => _imbalanceRatio;
		set
		{
			if (_imbalanceRatio == value)
				return;

			_imbalanceRatio = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Minimum dominant volume", GroupName = "Calculation", Order = 110,
		Description = "Minimum volume on the dominant side for a level to count as an imbalance.")]
	[Range(0, 1000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinDominantVolume
	{
		get => _minDominantVolume;
		set
		{
			if (_minDominantVolume == value)
				return;

			_minDominantVolume = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Minimum volume difference", GroupName = "Calculation", Order = 115,
		Description = "Minimum difference between the dominant side and the diagonal passive side. 0 disables the filter.")]
	[Range(0, 1000000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public decimal MinVolumeDifference
	{
		get => _minVolumeDifference;
		set
		{
			if (_minVolumeDifference == value)
				return;

			_minVolumeDifference = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Ignore zero levels", GroupName = "Calculation", Order = 120,
		Description = "When enabled, a level whose diagonal passive side has no volume is never an imbalance. " +
			"When disabled, it counts as an infinite ratio and only the minimum dominant volume applies.")]
	public bool IgnoreZeroLevels
	{
		get => _ignoreZeroLevels;
		set
		{
			if (_ignoreZeroLevels == value)
				return;

			_ignoreZeroLevels = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Ticks per row", GroupName = "Calculation", Order = 125,
		Description = "Number of price levels merged into one row before comparing, like a footprint with grouped rows. " +
			"Rows are aligned to multiples of this size.")]
	[Range(1, 50)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int RowTicks
	{
		get => _rowTicks;
		set
		{
			if (_rowTicks == value)
				return;

			_rowTicks = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Stacked imbalance levels", GroupName = "Calculation", Order = 130,
		Description = "Minimum number of consecutive price levels with an imbalance on the same side " +
			"to form a stacked imbalance.")]
	[Range(2, 20)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int MinStackedLevels
	{
		get => _minStackedLevels;
		set
		{
			if (_minStackedLevels == value)
				return;

			_minStackedLevels = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Properties: Scope

	[Display(Name = "Days to calculate", GroupName = "Scope", Order = 50,
		Description = "Number of sessions, counting the current one, in which imbalances are evaluated. 0 = the whole chart.")]
	[Range(0, 1000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int Days
	{
		get => _days;
		set
		{
			if (_days == value)
				return;

			_days = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Use session filter", GroupName = "Scope", Order = 60,
		Description = "Only evaluate bars that open inside the session time range (chart time zone). " +
			"Zones created inside the session keep extending and can break outside it.")]
	public bool UseSessionFilter
	{
		get => _useSessionFilter;
		set
		{
			if (_useSessionFilter == value)
				return;

			_useSessionFilter = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Session start", GroupName = "Scope", Order = 70,
		Description = "Start of the session time range (inclusive). A start later than the end spans midnight.")]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public TimeSpan SessionStart
	{
		get => _sessionStart;
		set
		{
			if (_sessionStart == value)
				return;

			_sessionStart = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Session end", GroupName = "Scope", Order = 80,
		Description = "End of the session time range (exclusive).")]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public TimeSpan SessionEnd
	{
		get => _sessionEnd;
		set
		{
			if (_sessionEnd == value)
				return;

			_sessionEnd = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Properties: Zones

	[Display(Name = "Zone break rule", GroupName = "Zones", Order = 300,
		Description = "Touch: price trades into the zone. Trade through: price trades beyond its far edge " +
			"(below a buy zone, above a sell zone). Close beyond: a bar closes beyond its far edge.")]
	public ZoneBreakMode BreakMode
	{
		get => _zoneBreakMode;
		set
		{
			if (_zoneBreakMode == value)
				return;

			_zoneBreakMode = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Maximum zone age (bars)", GroupName = "Zones", Order = 310,
		Description = "A zone that has not been broken after this many bars stops extending. 0 = no limit.")]
	[Range(0, 100000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int MaxZoneAgeBars
	{
		get => _maxZoneAgeBars;
		set
		{
			if (_maxZoneAgeBars == value)
				return;

			_maxZoneAgeBars = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Maximum active zones", GroupName = "Zones", Order = 320,
		Description = "When a new zone would exceed this number of active zones, the oldest active zone stops extending. 0 = no limit.")]
	[Range(0, 1000)]
	[PostValueMode(PostValueModes.OnLostFocus)]
	public int MaxActiveZones
	{
		get => _maxActiveZones;
		set
		{
			if (_maxActiveZones == value)
				return;

			_maxActiveZones = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Properties: Visuals

	[Display(Name = "Show imbalance marks", GroupName = "Visuals", Order = 200,
		Description = "Highlights every level with a diagonal imbalance.")]
	public bool ShowMarks
	{
		get => _showMarks;
		set
		{
			_showMarks = value;
			RedrawChart();
		}
	}

	[Display(Name = "Mark layout", GroupName = "Visuals", Order = 205,
		Description = "Footprint halves: sell marks on the left (Bid) half and buy marks on the right (Ask) half. " +
			"Full bar: marks use the whole bar width. Auto: halves when the bar is wide enough, full bar otherwise.")]
	public MarkLayout MarksLayout
	{
		get => _markLayout;
		set
		{
			_markLayout = value;
			RedrawChart();
		}
	}

	[Display(Name = "Buy imbalance color", GroupName = "Visuals", Order = 210,
		Description = "Color of buy imbalances (Ask against the Bid one tick below).")]
	public CrossColor BuyColor
	{
		get => _buyColor.Convert();
		set
		{
			_buyColor = value.Convert();
			RedrawChart();
		}
	}

	[Display(Name = "Sell imbalance color", GroupName = "Visuals", Order = 220,
		Description = "Color of sell imbalances (Bid against the Ask one tick above).")]
	public CrossColor SellColor
	{
		get => _sellColor.Convert();
		set
		{
			_sellColor = value.Convert();
			RedrawChart();
		}
	}

	[Display(Name = "Show stacked zones", GroupName = "Visuals", Order = 230,
		Description = "Draws each stacked imbalance as a zone that extends to the right until price trades through it.")]
	public bool ShowZones
	{
		get => _showZones;
		set
		{
			_showZones = value;
			RedrawChart();
		}
	}

	[Display(Name = "Buy zone color", GroupName = "Visuals", Order = 240,
		Description = "Fill color of zones from stacked buy imbalances.")]
	public CrossColor BuyZoneColor
	{
		get => _buyZoneColor.Convert();
		set
		{
			_buyZoneColor = value.Convert();
			UpdateZonePens();
			RedrawChart();
		}
	}

	[Display(Name = "Sell zone color", GroupName = "Visuals", Order = 250,
		Description = "Fill color of zones from stacked sell imbalances.")]
	public CrossColor SellZoneColor
	{
		get => _sellZoneColor.Convert();
		set
		{
			_sellZoneColor = value.Convert();
			UpdateZonePens();
			RedrawChart();
		}
	}

	[Display(Name = "Zone border width", GroupName = "Visuals", Order = 260,
		Description = "Width of the zone outline, drawn in the zone color without transparency. 0 = no outline.")]
	[Range(0, 5)]
	public int ZoneBorderWidth
	{
		get => _zoneBorderWidth;
		set
		{
			_zoneBorderWidth = value;
			UpdateZonePens();
			RedrawChart();
		}
	}

	[Display(Name = "Show zone labels", GroupName = "Visuals", Order = 270,
		Description = "Writes the number of stacked levels (for example x4) at the start of each zone.")]
	public bool ShowZoneLabels
	{
		get => _showZoneLabels;
		set
		{
			_showZoneLabels = value;
			RedrawChart();
		}
	}

	[Display(Name = "Label font size", GroupName = "Visuals", Order = 280,
		Description = "Font size of the zone labels.")]
	[Range(6, 24)]
	public int LabelFontSize
	{
		get => _labelFontSize;
		set
		{
			_labelFontSize = value;
			_labelFont = new RenderFont("Arial", value);
			RedrawChart();
		}
	}

	#endregion

	#region Properties: Alerts

	[Display(Name = "Alert on new zone", GroupName = "Alerts", Order = 400,
		Description = "Alert when a bar closes with a stacked imbalance and a new zone is created.")]
	public bool AlertOnNewZone
	{
		get => _alertOnNewZone;
		set => _alertOnNewZone = value;
	}

	[Display(Name = "Alert on zone retest", GroupName = "Alerts", Order = 410,
		Description = "Alert when price moves from outside an active zone into it.")]
	public bool AlertOnRetest
	{
		get => _alertOnRetest;
		set => _alertOnRetest = value;
	}

	[Display(Name = "Alert sound", GroupName = "Alerts", Order = 420,
		Description = "Sound file used by the alerts.")]
	public string AlertFile
	{
		get => _alertFile;
		set => _alertFile = value;
	}

	[Display(Name = "Retest cooldown (seconds)", GroupName = "Alerts", Order = 430,
		Description = "Minimum time between two retest alerts of the same zone.")]
	[Range(0, 86400)]
	public int AlertCooldownSeconds
	{
		get => _alertCooldownSeconds;
		set => _alertCooldownSeconds = value;
	}

	#endregion

	#region Ctor

	public DiagonalImbalance()
		: base(true)
	{
		DenyToChangePanel = true;

		// The indicator draws on the price panel; the default series stays hidden
		// so it does not show up as an empty line in the chart or the Drawing panel.
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);

		UpdateZonePens();
	}

	#endregion

	#region Protected Methods

	protected override void OnInitialize()
	{
		this.LogInfo($"DiagonalImbalance: initialized ({typeof(DiagonalImbalance).Assembly.GetName().Version}).");
	}

	protected override void OnRecalculate()
	{
		lock (_barsLock)
		{
			_barImbalances.Clear();
			_barStacks.Clear();
			_zones.Clear();
			_activeZones.Clear();
			_pendingAlerts.Clear();
			_formingBar = -1;
			_formingLevels = NoImbalances;
		}

		_historyLoaded = false;
		_historyBuyCount = 0;
		_historySellCount = 0;
		_historyBuyStacks = 0;
		_historySellStacks = 0;

		_firstCalculatedBar = FindFirstCalculatedBar();
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		var lastBar = CurrentBar - 1;

		if (bar < lastBar)
		{
			// Closed bar (history).
			CalculateClosedBar(bar);
			return;
		}

		// A new forming bar means the previous one just closed: evaluate it once as a closed bar.
		if (bar > 0 && !IsCalculated(bar - 1))
			CalculateClosedBar(bar - 1);

		// Forming bar: re-evaluated in full on every update, because each level is compared
		// with its neighbour and a trade on one level can change the result of the next one.
		// A bar holds a few dozen levels, so the full pass is cheap enough to run per update.
		CalculateFormingBar(bar);
	}

	protected override void OnFinishRecalculate()
	{
		_historyLoaded = true;

		var lastClosed = CurrentBar - 2;

		this.LogInfo($"DiagonalImbalance: history calculated, {lastClosed + 1} closed bars, " +
			$"{_historyBuyCount} buy / {_historySellCount} sell imbalances, " +
			$"{_historyBuyStacks} buy / {_historySellStacks} sell stacks " +
			$"(ratio {_imbalanceRatio}, min dominant volume {_minDominantVolume}, " +
			$"min volume difference {_minVolumeDifference}, ignore zero levels {_ignoreZeroLevels}, " +
			$"ticks per row {_rowTicks}, stacked levels {_minStackedLevels}), {CountActiveZones()} active zones " +
			$"(break rule {_zoneBreakMode}, max age {_maxZoneAgeBars}, max active {_maxActiveZones}); " +
			$"scope from bar {_firstCalculatedBar} (days {_days}), session filter " +
			$"{(_useSessionFilter ? $"{_sessionStart:hh\\:mm}-{_sessionEnd:hh\\:mm}" : "off")}.");

		for (var bar = Math.Max(0, lastClosed - LoggedHistoryBars + 1); bar <= lastClosed; bar++)
			LogBar(bar);
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if ((!_showMarks && !_showZones) || ChartInfo is null || InstrumentInfo is null)
			return;

		var tickSize = InstrumentInfo.TickSize;

		if (tickSize <= 0)
			return;

		var container = ChartInfo.PriceChartContainer;
		var barWidth = Math.Max(1, (int)container.BarsWidth);

		// Footprint halves: sell imbalances (Bid) on the left half, buy imbalances (Ask) on the
		// right half, like a Bid x Ask footprint. Auto uses halves only on wide enough bars.
		var split = _markLayout switch
		{
			MarkLayout.FootprintHalves => barWidth >= 2,
			MarkLayout.FullBar => false,
			_ => barWidth >= MinSplitBarWidth
		};
		var leftWidth = split ? barWidth / 2 : barWidth;
		var rightWidth = split ? barWidth - leftWidth : barWidth;

		_renderBars.Clear();
		_renderZones.Clear();

		lock (_barsLock)
		{
			var firstBar = Math.Max(0, FirstVisibleBarNumber);

			if (_showZones)
			{
				foreach (var zone in _zones)
				{
					if (zone.StartBar <= LastVisibleBarNumber && (zone.IsActive || zone.EndBar >= firstBar))
						_renderZones.Add((zone.StartBar, zone.EndBar, zone));
				}
			}

			var lastBar = Math.Min(LastVisibleBarNumber, _barImbalances.Count - 1);

			for (var bar = firstBar; bar <= lastBar; bar++)
			{
				var barLevels = _barImbalances[bar];

				if (barLevels is { Length: > 0 })
					_renderBars.Add((bar, barLevels));
			}

			if (_formingBar >= firstBar && _formingBar <= LastVisibleBarNumber && _formingLevels.Length > 0)
				_renderBars.Add((_formingBar, _formingLevels));
		}

		// Zones first, so the imbalance marks stay on top of them.
		var chartRight = Container.Region.Right;

		foreach (var (startBar, endBar, zone) in _renderZones)
		{
			// From the bar with the stack to the end of the breaking bar, or to the right edge
			// of the chart while the zone is active.
			var left = ChartInfo.GetXByBar(startBar);
			var right = endBar < 0 ? chartRight : ChartInfo.GetXByBar(endBar) + barWidth;

			if (right <= left)
				continue;

			var top = ChartInfo.GetYByPrice(zone.High, true);
			var bottom = ChartInfo.GetYByPrice(zone.Low - tickSize, true);

			var rect = new Rectangle(left, top, right - left, Math.Max(1, bottom - top));
			context.FillRectangle(zone.IsBuy ? _buyZoneColor : _sellZoneColor, rect);

			if (_zoneBorderWidth > 0)
				context.DrawRectangle(zone.IsBuy ? _buyZonePen : _sellZonePen, rect);

			if (_showZoneLabels)
			{
				var label = $"x{zone.Levels}";
				var size = context.MeasureString(label, _labelFont);

				// Only when the label fits inside the zone.
				if (size.Height <= rect.Height && size.Width + 4 <= rect.Width)
				{
					context.DrawString(label, _labelFont, Opaque(zone.IsBuy ? _buyZoneColor : _sellZoneColor),
						rect.X + 2, rect.Y + (rect.Height - size.Height) / 2);
				}
			}
		}

		if (!_showMarks)
			return;

		foreach (var (bar, levels) in _renderBars)
		{
			var x = ChartInfo.GetXByBar(bar);

			foreach (var level in levels)
			{
				// GetYByPrice(price, true) is the top edge of the price row.
				var top = ChartInfo.GetYByPrice(level.TopPrice, true);
				var bottom = ChartInfo.GetYByPrice(level.Price - tickSize, true);
				var height = Math.Max(1, bottom - top);

				var rect = level.IsBuy
					? new Rectangle(split ? x + leftWidth : x, top, rightWidth, height)
					: new Rectangle(x, top, leftWidth, height);

				context.FillRectangle(level.IsBuy ? _buyColor : _sellColor, rect);
			}
		}
	}

	#endregion

	#region Private Methods: Drawing

	private void UpdateZonePens()
	{
		var width = Math.Max(1, _zoneBorderWidth);
		_buyZonePen = new RenderPen(Opaque(_buyZoneColor), width);
		_sellZonePen = new RenderPen(Opaque(_sellZoneColor), width);
	}

	private static Color Opaque(Color color)
	{
		return Color.FromArgb(255, color.R, color.G, color.B);
	}

	#endregion

	#region Private Methods: Engine

	private bool IsCalculated(int bar)
	{
		return bar < _barImbalances.Count && _barImbalances[bar] != null;
	}

	private void CalculateFormingBar(int bar)
	{
		var levels = IsInScope(bar) ? FindImbalances(bar) : NoImbalances;
		var candle = GetCandle(bar);

		lock (_barsLock)
		{
			_formingBar = bar;
			_formingLevels = levels;

			// Before any zone can end on this update, so a retest that also breaks the zone
			// (Touch rule) is still reported.
			if (_historyLoaded)
				CheckRetests(bar, candle.Close);

			// The High and Low of a forming bar only extend, so a touch or a trade through seen now
			// is final. Its Close is not, so CloseBeyond waits for the bar to close.
			UpdateZones(bar, candle.Low, candle.High, candle.Close, false);
		}

		RaisePendingAlerts();
	}

	private void CalculateClosedBar(int bar)
	{
		var inScope = IsInScope(bar);
		var levels = inScope ? FindImbalances(bar) : NoImbalances;
		var stacks = inScope ? FindStacks(levels) : NoStacks;

		lock (_barsLock)
		{
			while (_barImbalances.Count <= bar)
			{
				_barImbalances.Add(null);
				_barStacks.Add(NoStacks);
			}

			_barImbalances[bar] = levels;
			_barStacks[bar] = stacks;

			// Existing zones are tested against this bar first; the bar's own stacks become
			// zones afterwards, so a zone is only ever broken by a later bar.
			var candle = GetCandle(bar);
			UpdateZones(bar, candle.Low, candle.High, candle.Close, true);

			foreach (var stack in stacks)
			{
				var zone = new ImbalanceZone(bar, stack);
				AddZone(zone);

				if (_historyLoaded && _alertOnNewZone)
				{
					_pendingAlerts.Add(($"Diagonal Imbalance: new {(zone.IsBuy ? "buy" : "sell")} zone " +
						$"{zone.Low}-{zone.High} (x{zone.Levels})", zone.IsBuy));
				}
			}

			if (_formingBar == bar)
			{
				_formingBar = -1;
				_formingLevels = NoImbalances;
			}
		}

		foreach (var level in levels)
		{
			if (level.IsBuy)
				_historyBuyCount++;
			else
				_historySellCount++;
		}

		foreach (var stack in stacks)
		{
			if (stack.IsBuy)
				_historyBuyStacks++;
			else
				_historySellStacks++;
		}

		// After the history, a bar only reaches this point when it closes (in realtime or replay,
		// possibly several at once). Bars without imbalances are not logged.
		if (_historyLoaded && levels.Length > 0)
			LogBar(bar);

		RaisePendingAlerts();
	}

	// Called under _barsLock. Event-based: a retest is the move of the last price from outside
	// a zone into it, not the price being inside, so a price that stays in the zone alerts once.
	private void CheckRetests(int bar, decimal price)
	{
		var now = DateTime.UtcNow;

		foreach (var zone in _activeZones)
		{
			if (zone.StartBar >= bar)
				continue;

			var inside = zone.Contains(price);
			var entered = inside && zone.PriceInside == false;
			zone.PriceInside = inside;

			if (!entered || !_alertOnRetest)
				continue;

			if ((now - zone.LastRetestAlertUtc).TotalSeconds < _alertCooldownSeconds)
				continue;

			zone.LastRetestAlertUtc = now;
			_pendingAlerts.Add(($"Diagonal Imbalance: price {price} retests {(zone.IsBuy ? "buy" : "sell")} zone " +
				$"{zone.Low}-{zone.High} from bar {zone.StartBar}", zone.IsBuy));
		}
	}

	private void RaisePendingAlerts()
	{
		List<(string Message, bool IsBuy)> alerts;

		lock (_barsLock)
		{
			if (_pendingAlerts.Count == 0)
				return;

			alerts = new List<(string Message, bool IsBuy)>(_pendingAlerts);
			_pendingAlerts.Clear();
		}

		foreach (var (message, isBuy) in alerts)
		{
			this.LogInfo($"DiagonalImbalance: alert: {message}");
			AddAlert(_alertFile, InstrumentInfo?.Instrument ?? string.Empty, message,
				Opaque(isBuy ? _buyZoneColor : _sellZoneColor).Convert(), Color.White.Convert());
		}
	}

	// Called under _barsLock. Ends the active zones older than the bar that the bar breaks
	// (according to BreakMode) or that reach MaxZoneAgeBars at this bar.
	private void UpdateZones(int bar, decimal barLow, decimal barHigh, decimal barClose, bool barClosed)
	{
		for (var i = _activeZones.Count - 1; i >= 0; i--)
		{
			var zone = _activeZones[i];

			if (zone.StartBar >= bar)
				continue;

			ZoneEnd reason;

			if ((barClosed || _zoneBreakMode != ZoneBreakMode.CloseBeyond)
				&& zone.IsBrokenBy(_zoneBreakMode, barLow, barHigh, barClose))
				reason = ZoneEnd.Broken;
			else if (_maxZoneAgeBars > 0 && bar - zone.StartBar >= _maxZoneAgeBars)
				reason = ZoneEnd.Expired;
			else
				continue;

			EndZone(i, bar, reason);
		}
	}

	// Called under _barsLock. Adds a zone; beyond MaxActiveZones the oldest active zone is retired.
	private void AddZone(ImbalanceZone zone)
	{
		_zones.Add(zone);
		_activeZones.Add(zone);

		// _activeZones keeps creation order, so index 0 is the oldest active zone.
		while (_maxActiveZones > 0 && _activeZones.Count > _maxActiveZones)
			EndZone(0, zone.StartBar, ZoneEnd.Replaced);
	}

	private void EndZone(int activeIndex, int bar, ZoneEnd reason)
	{
		var zone = _activeZones[activeIndex];
		_activeZones.RemoveAt(activeIndex);
		zone.End(bar, reason);

		if (_historyLoaded)
		{
			this.LogInfo($"DiagonalImbalance: zone {(zone.IsBuy ? "B" : "S")} {zone.Low}-{zone.High} " +
				$"from bar {zone.StartBar} {reason.ToString().ToLowerInvariant()} at bar {bar}.");
		}
	}

	private int CountActiveZones()
	{
		lock (_barsLock)
			return _activeZones.Count;
	}

	// Bars before the first calculated bar or, with the session filter, opening outside the
	// session produce no imbalances and no zones. Zone updates still run on every bar.
	private bool IsInScope(int bar)
	{
		if (bar < _firstCalculatedBar)
			return false;

		if (!_useSessionFilter || _sessionStart == _sessionEnd)
			return true;

		var time = ChartTime(GetCandle(bar).Time).TimeOfDay;

		return _sessionStart < _sessionEnd
			? time >= _sessionStart && time < _sessionEnd
			: time >= _sessionStart || time < _sessionEnd;
	}

	private DateTime ChartTime(DateTime utc)
	{
		if (InstrumentInfo is null)
			return utc;

#if ATAS_STABLE || ATAS_LATEST
		return utc.AddHours(InstrumentInfo.TimeZone);
#else
		return utc.Add(InstrumentInfo.TimeZoneOffset);
#endif
	}

	// Walks back from the last bar counting session starts until Days sessions are covered.
	private int FindFirstCalculatedBar()
	{
		if (_days <= 0)
			return 0;

		var sessions = 0;

		for (var bar = CurrentBar - 1; bar > 0; bar--)
		{
			if (!IsNewSession(bar))
				continue;

			sessions++;

			if (sessions == _days)
				return bar;
		}

		return 0;
	}

	// Merges the bar into rows of RowTicks price levels, aligned to multiples of the row size
	// (with one tick per row each price is its own row), then compares each row with the row
	// below: Ask of row R against Bid of row R-1 is a buy imbalance at R, and Bid of R-1 against
	// Ask of R is a sell imbalance at R-1. Only rows inside the bar are compared, so the lowest
	// row has no row below and the highest none above.
	private ImbalanceLevel[] FindImbalances(int bar)
	{
		var tickSize = InstrumentInfo?.TickSize ?? 0m;

		if (tickSize <= 0)
			return NoImbalances;

		var candle = GetCandle(bar);
		var rowSize = tickSize * _rowTicks;
		var firstRow = RowStart(candle.Low, rowSize);
		var rows = (int)((RowStart(candle.High, rowSize) - firstRow) / rowSize) + 1;

		if (rows < 2)
			return NoImbalances;

		if (_rowAsk.Length < rows)
		{
			_rowAsk = new decimal[rows * 2];
			_rowBid = new decimal[rows * 2];
		}

		Array.Clear(_rowAsk, 0, rows);
		Array.Clear(_rowBid, 0, rows);

		for (var price = candle.Low; price <= candle.High; price += tickSize)
		{
			var info = candle.GetPriceVolumeInfo(price, _levelCache);

			if (info is null)
				continue;

			var row = (int)((RowStart(price, rowSize) - firstRow) / rowSize);
			_rowAsk[row] += info.Ask;
			_rowBid[row] += info.Bid;
		}

		List<ImbalanceLevel> found = null;
		var rowTop = rowSize - tickSize;

		for (var row = 1; row < rows; row++)
		{
			var price = firstRow + row * rowSize;
			var belowPrice = price - rowSize;
			var ask = _rowAsk[row];
			var belowBid = _rowBid[row - 1];

			// Buy imbalance at row R: Ask(R) against Bid(R - 1).
			if (IsImbalance(ask, belowBid))
				(found ??= new()).Add(new ImbalanceLevel(price, price + rowTop, true, ask, belowBid));

			// Sell imbalance at row R - 1: Bid(R - 1) against Ask(R).
			if (IsImbalance(belowBid, ask))
				(found ??= new()).Add(new ImbalanceLevel(belowPrice, belowPrice + rowTop, false, belowBid, ask));
		}

		return found?.ToArray() ?? NoImbalances;
	}

	private static decimal RowStart(decimal price, decimal rowSize)
	{
		return Math.Floor(price / rowSize) * rowSize;
	}

	// Both levels of a comparison are always inside the bar (Low..High): the Low has no
	// level below and the High none above, so the bar edges are never compared against
	// prices that did not trade. Inside the bar, a price without trades reads as zero.
	private bool IsImbalance(decimal dominant, decimal passive)
	{
		if (dominant <= 0 || dominant < _minDominantVolume)
			return false;

		// Absolute filter: a high ratio between two small numbers (e.g. 9 vs 2) is not significant.
		if (dominant - passive < _minVolumeDifference)
			return false;

		// Zero passive side: infinite ratio, unless zero levels are ignored.
		if (passive == 0)
			return !_ignoreZeroLevels;

		return dominant >= passive * _imbalanceRatio;
	}

	// Groups the imbalances of one bar into runs of consecutive rows on the same side.
	// FindImbalances returns each side in ascending price order, so a single pass per side
	// is enough: a row continues the run when it starts exactly one row above the previous one.
	// The block spans from the lowest price of its first row to the highest price of its last row.
	private StackedBlock[] FindStacks(ImbalanceLevel[] levels)
	{
		var tickSize = InstrumentInfo?.TickSize ?? 0m;

		if (tickSize <= 0 || levels.Length < _minStackedLevels)
			return NoStacks;

		var rowSize = tickSize * _rowTicks;
		List<StackedBlock> found = null;

		for (var side = 0; side < 2; side++)
		{
			var isBuy = side == 0;
			var runLow = 0m;
			var runHigh = 0m;
			var lastStart = 0m;
			var runLevels = 0;

			foreach (var level in levels)
			{
				if (level.IsBuy != isBuy)
					continue;

				if (runLevels > 0 && level.Price - lastStart == rowSize)
				{
					lastStart = level.Price;
					runHigh = level.TopPrice;
					runLevels++;
					continue;
				}

				if (runLevels >= _minStackedLevels)
					(found ??= new()).Add(new StackedBlock(runLow, runHigh, isBuy, runLevels));

				runLow = lastStart = level.Price;
				runHigh = level.TopPrice;
				runLevels = 1;
			}

			if (runLevels >= _minStackedLevels)
				(found ??= new()).Add(new StackedBlock(runLow, runHigh, isBuy, runLevels));
		}

		if (found is null)
			return NoStacks;

		found.Sort((a, b) => a.Low.CompareTo(b.Low));
		return found.ToArray();
	}

	#endregion

	#region Private Methods: Diagnostics

	private static string FormatRow(ImbalanceLevel level)
	{
		return level.TopPrice == level.Price ? $"{level.Price}" : $"{level.Price}-{level.TopPrice}";
	}

	private void LogBar(int bar)
	{
		if (!IsCalculated(bar))
			return;

		var levels = _barImbalances[bar];
		var candle = GetCandle(bar);

		var buy = 0;
		var sell = 0;

		foreach (var level in levels)
		{
			if (level.IsBuy)
				buy++;
			else
				sell++;
		}

		var sb = new StringBuilder();
		sb.Append($"DiagonalImbalance: bar {bar} {candle.Time:yyyy-MM-dd HH:mm:ss} " +
			$"[{candle.Low}-{candle.High}]: {buy} buy / {sell} sell");

		for (var i = 0; i < levels.Length && i < LoggedLevelsPerBar; i++)
		{
			var level = levels[i];
			sb.Append(level.IsBuy
				? $"; B@{FormatRow(level)} ask {level.Dominant} vs bid {level.Passive}"
				: $"; S@{FormatRow(level)} bid {level.Dominant} vs ask {level.Passive}");
		}

		if (levels.Length > LoggedLevelsPerBar)
			sb.Append($"; +{levels.Length - LoggedLevelsPerBar} more");

		foreach (var stack in _barStacks[bar])
			sb.Append($"; STACK {(stack.IsBuy ? "B" : "S")} {stack.Low}-{stack.High} x{stack.Levels}");

		this.LogInfo(sb.ToString());
	}

	#endregion
}
