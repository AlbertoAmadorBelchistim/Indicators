namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

using ATAS.Indicators.Technical.BigTrades;

using OFT.Rendering.Context;
using OFT.Rendering.Tools;

using Utils.Common.Logging;

/// <summary>
/// Bubbles on the trades that are large for the current market: a trade is marked when its
/// volume is above a percentile of the volumes of the last trades, so the threshold follows the
/// market instead of being a fixed size. History and realtime go through the same engine.
/// </summary>
[Category("Order Flow")]
[DisplayName("Adaptive Big Trades")]
[Description("Marks the trades that are large relative to the recent trades: above a volume percentile of the last N trades, so no fixed size threshold is needed.")]
public class AdaptiveBigTrades : Indicator
{
	#region Nested types

	public enum DirectionFilter
	{
		[Display(Name = "Buys and sells")]
		Both,

		[Display(Name = "Buys only")]
		OnlyBuys,

		[Display(Name = "Sells only")]
		OnlySells
	}

	public enum SizeMode
	{
		[Display(Name = "Fixed")]
		Fixed,

		[Display(Name = "Scaled by volume")]
		Scaled
	}

	private readonly record struct PendingTrade(TradeKey Key, decimal Volume);

	#endregion

	#region Fields

	// Engine, request state and the realtime buffer, guarded by _sync.
	private readonly object _sync = new();
	private BigTradeEngine _engine;
	private bool _historyLoaded;
	private int _requestId;
	private readonly List<PendingTrade> _pending = new();

	private readonly RenderFont _textFont = new("Arial", 9);
	private RenderFont _font = new("Arial", 10);

	private int _sessionsToLoad = 2;
	private int _windowTrades = 5000;
	private decimal _percentile = 80m;
	private int _minTrades = 300;
	private DirectionFilter _direction = DirectionFilter.Both;
	private int _minPriceDistanceTicks;
	private SizeMode _sizeMode = SizeMode.Scaled;
	private int _objectSize = 12;
	private int _maxRadius = 40;
	private bool _showText;
	private int _opacity = 150;
	private int _outlineWidth = 1;
	private bool _showThreshold = true;
	private Color _buyColor = Color.FromArgb(50, 205, 50);
	private Color _sellColor = Color.FromArgb(220, 20, 60);

	#endregion

	#region Properties

	#region Data

	[Display(Name = "Sessions to load", GroupName = "Data", Order = 10,
		Description = "Sessions of trades loaded for the history, counting the current one. 0 loads the whole chart.")]
	[Range(0, 100)]
	public int SessionsToLoad
	{
		get => _sessionsToLoad;
		set
		{
			_sessionsToLoad = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Calculation

	[Display(Name = "Window, trades", GroupName = "Calculation", Order = 10,
		Description = "The threshold is taken from the volumes of this many last trades.")]
	[Range(50, 1000000)]
	public int WindowTrades
	{
		get => _windowTrades;
		set
		{
			_windowTrades = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Percentile, %", GroupName = "Calculation", Order = 20,
		Description = "A trade is big when its volume is above this percentile of the window: 80 marks roughly the largest 20% of the trades, 99 the largest 1%.")]
	[Range(1, 99.99)]
	public decimal Percentile
	{
		get => _percentile;
		set
		{
			_percentile = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Minimum trades", GroupName = "Calculation", Order = 30,
		Description = "No trade is marked until the window has this many trades, so the threshold is stable.")]
	[Range(1, 100000)]
	public int MinTrades
	{
		get => _minTrades;
		set
		{
			_minTrades = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Filters

	[Display(Name = "Direction", GroupName = "Filters", Order = 10)]
	public DirectionFilter Direction
	{
		get => _direction;
		set
		{
			_direction = value;
			RedrawChart();
		}
	}

	[Display(Name = "Minimum distance, ticks", GroupName = "Filters", Order = 20,
		Description = "In a bar, a bubble closer than this to a larger one is not drawn. 0 draws every bubble.")]
	[Range(0, 100)]
	public int MinPriceDistanceTicks
	{
		get => _minPriceDistanceTicks;
		set
		{
			_minPriceDistanceTicks = value;
			RedrawChart();
		}
	}

	#endregion

	#region Visualization

	[Display(Name = "Size mode", GroupName = "Visualization", Order = 10,
		Description = "Fixed size, or scaled by the volume against the threshold (up to 3 times the size).")]
	public SizeMode BubbleSizeMode
	{
		get => _sizeMode;
		set
		{
			_sizeMode = value;
			RedrawChart();
		}
	}

	[Display(Name = "Size", GroupName = "Visualization", Order = 20,
		Description = "Radius of the bubble in pixels, or of a bubble at the threshold when scaled.")]
	[Range(1, 100)]
	public int ObjectSize
	{
		get => _objectSize;
		set
		{
			_objectSize = value;
			RedrawChart();
		}
	}

	[Display(Name = "Maximum radius", GroupName = "Visualization", Order = 30)]
	[Range(3, 200)]
	public int MaxRadius
	{
		get => _maxRadius;
		set
		{
			_maxRadius = value;
			RedrawChart();
		}
	}

	[Display(Name = "Show volume", GroupName = "Visualization", Order = 40)]
	public bool ShowText
	{
		get => _showText;
		set
		{
			_showText = value;
			RedrawChart();
		}
	}

	[Display(Name = "Font size", GroupName = "Visualization", Order = 50)]
	[Range(6, 24)]
	public int FontSize
	{
		get => (int)_font.Size;
		set
		{
			_font = new RenderFont("Arial", value);
			RedrawChart();
		}
	}

	[Display(Name = "Opacity", GroupName = "Visualization", Order = 60)]
	[Range(10, 255)]
	public int Opacity
	{
		get => _opacity;
		set
		{
			_opacity = value;
			RedrawChart();
		}
	}

	[Display(Name = "Outline width", GroupName = "Visualization", Order = 70)]
	[Range(0, 5)]
	public int OutlineWidth
	{
		get => _outlineWidth;
		set
		{
			_outlineWidth = value;
			RedrawChart();
		}
	}

	[Display(Name = "Show threshold", GroupName = "Visualization", Order = 80,
		Description = "The current threshold in the top left corner of the chart.")]
	public bool ShowThreshold
	{
		get => _showThreshold;
		set
		{
			_showThreshold = value;
			RedrawChart();
		}
	}

	#endregion

	#region Colors

	[Display(Name = "Buy color", GroupName = "Colors", Order = 10)]
	public CrossColor BuyColor
	{
		get => _buyColor.Convert();
		set
		{
			_buyColor = value.Convert();
			RedrawChart();
		}
	}

	[Display(Name = "Sell color", GroupName = "Colors", Order = 20)]
	public CrossColor SellColor
	{
		get => _sellColor.Convert();
		set
		{
			_sellColor = value.Convert();
			RedrawChart();
		}
	}

	#endregion

	#endregion

	#region ctor

	public AdaptiveBigTrades()
		: base(true)
	{
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		DrawAbovePrice = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
	}

	#endregion

	#region Protected methods

	protected override void OnInitialize()
	{
		this.LogInfo($"AdaptiveBigTrades: initialized ({typeof(AdaptiveBigTrades).Assembly.GetName().Version}).");
	}

	protected override void OnCalculate(int bar, decimal value)
	{
	}

	// A recalculation starts over: realtime trades wait in a buffer until the history is in.
	protected override void OnRecalculate()
	{
		lock (_sync)
		{
			_engine = new BigTradeEngine(_windowTrades, _percentile, _minTrades);
			_historyLoaded = false;
			_requestId = 0;
			_pending.Clear();
		}
	}

	protected override void OnFinishRecalculate()
	{
		if (CurrentBar < 1)
		{
			lock (_sync)
				_historyLoaded = true;

			return;
		}

		var first = FirstBarToLoad();
		var request = new CumulativeTradesRequest(GetCandle(first).Time, GetCandle(CurrentBar - 1).LastTime, 0, 0);

		lock (_sync)
			_requestId = request.RequestId;

		this.LogInfo($"AdaptiveBigTrades: history request from bar {first} of {CurrentBar}.");
		RequestForCumulativeTrades(request);
	}

	protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
	{
		lock (_sync)
		{
			if (request.RequestId != _requestId || _engine == null)
				return;
		}

		var trades = (cumulativeTrades ?? Enumerable.Empty<CumulativeTrade>())
			.Where(t => t.Direction is TradeDirection.Buy or TradeDirection.Sell)
			.OrderBy(t => t.Time)
			.ToList();

		lock (_sync)
		{
			foreach (var trade in trades)
				_engine.Add(KeyOf(trade), trade.Volume);

			// The trades that arrived meanwhile; the ones the history already has are updates.
			foreach (var pending in _pending)
				_engine.Add(pending.Key, pending.Volume);

			_pending.Clear();
			_historyLoaded = true;

			this.LogInfo($"AdaptiveBigTrades: history loaded, {trades.Count} trades, {_engine.Bubbles.Count} big, threshold {_engine.CurrentThreshold?.ToString("0.##") ?? "-"}.");
		}

		RedrawChart();
	}

	protected override void OnCumulativeTrade(CumulativeTrade trade)
	{
		OnRealtimeTrade(trade);
	}

	protected override void OnUpdateCumulativeTrade(CumulativeTrade trade)
	{
		OnRealtimeTrade(trade);
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (ChartInfo == null || InstrumentInfo == null || CurrentBar < 1)
			return;

		var firstBar = Math.Max(0, FirstVisibleBarNumber);
		var lastBar = Math.Min(LastVisibleBarNumber, CurrentBar - 1);

		if (lastBar < firstBar)
			return;

		var from = GetCandle(firstBar).Time;
		var visible = new List<Bubble>();
		decimal? threshold;

		lock (_sync)
		{
			if (_engine == null)
				return;

			threshold = _engine.CurrentThreshold;
			var bubbles = _engine.Bubbles;

			for (var i = _engine.FirstIndexAtOrAfter(from); i < bubbles.Count; i++)
			{
				var bubble = bubbles[i];

				if (PassesDirection(bubble.Side))
					visible.Add(new Bubble { Key = bubble.Key, Volume = bubble.Volume, Threshold = bubble.Threshold });
			}
		}

		// Larger bubbles first, so the distance filter keeps them and smaller ones draw on top.
		visible.Sort((a, b) => b.Volume.CompareTo(a.Volume));

		var drawnByBar = new Dictionary<int, List<decimal>>();
		var minDistance = _minPriceDistanceTicks * InstrumentInfo.TickSize;
		var outline = _outlineWidth > 0 ? new RenderPen(Color.Black, _outlineWidth) : null;
		var mouse = MouseLocationInfo.LastPosition;
		Bubble hovered = null;

		foreach (var bubble in visible)
		{
			var bar = BarOfTime(bubble.Time);

			if (bar < firstBar || bar > lastBar)
				continue;

			if (_minPriceDistanceTicks > 0)
			{
				if (!drawnByBar.TryGetValue(bar, out var prices))
					drawnByBar[bar] = prices = new List<decimal>();

				if (prices.Any(p => Math.Abs(p - bubble.Price) < minDistance))
					continue;

				prices.Add(bubble.Price);
			}

			var x = ChartInfo.GetXByBar(bar, false);
			var y = ChartInfo.GetYByPrice(bubble.Price, false);
			var radius = RadiusOf(bubble);
			var rect = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
			var color = bubble.Side > 0 ? _buyColor : _sellColor;

			context.FillEllipse(Color.FromArgb(_opacity, color), rect);

			if (outline != null)
				context.DrawEllipse(outline, rect);

			if (_showText)
			{
				var text = $"{bubble.Volume:0.##}";
				var size = context.MeasureString(text, _font);
				context.DrawString(text, _font, Color.White, x - size.Width / 2, y - size.Height / 2);
			}

			if (rect.Contains(mouse))
				hovered = bubble;
		}

		if (_showThreshold)
		{
			var text = threshold.HasValue
				? $"Big trade > {threshold.Value:0.##} ({_percentile:0.##}th percentile of the last {_windowTrades} trades)"
				: $"Big trades: waiting for {_minTrades} trades";

			context.DrawString(text, _textFont, ChartInfo.ColorsStore.AxisTextColor, Container.Region.X + 5, Container.Region.Y + 5);
		}

		if (hovered != null)
			DrawTooltip(context, hovered, mouse);
	}

	#endregion

	#region Private methods

	private void OnRealtimeTrade(CumulativeTrade trade)
	{
		if (trade.Direction is not (TradeDirection.Buy or TradeDirection.Sell))
			return;

		var key = KeyOf(trade);

		lock (_sync)
		{
			if (_engine == null)
				return;

			if (!_historyLoaded)
			{
				_pending.Add(new PendingTrade(key, trade.Volume));
				return;
			}

			_engine.Add(key, trade.Volume);
		}

		RedrawChart();
	}

	private static TradeKey KeyOf(CumulativeTrade trade)
	{
		return new TradeKey(trade.Time, trade.FirstPrice, trade.Direction == TradeDirection.Buy ? 1 : -1);
	}

	private int FirstBarToLoad()
	{
		if (_sessionsToLoad <= 0)
			return 0;

		var sessions = 0;

		for (var bar = CurrentBar - 1; bar > 0; bar--)
		{
			if (IsNewSession(bar) && ++sessions == _sessionsToLoad)
				return bar;
		}

		return 0;
	}

	// Last bar starting at or before the time (bar times never decrease).
	private int BarOfTime(DateTime time)
	{
		int lo = 0, hi = CurrentBar - 1, result = -1;

		while (lo <= hi)
		{
			var mid = lo + (hi - lo) / 2;

			if (GetCandle(mid).Time <= time)
			{
				result = mid;
				lo = mid + 1;
			}
			else
				hi = mid - 1;
		}

		return result;
	}

	private bool PassesDirection(int side)
	{
		return _direction switch
		{
			DirectionFilter.OnlyBuys => side > 0,
			DirectionFilter.OnlySells => side < 0,
			_ => true
		};
	}

	private int RadiusOf(Bubble bubble)
	{
		if (_sizeMode == SizeMode.Fixed || bubble.Threshold <= 0m)
			return Math.Min(_objectSize, _maxRadius);

		var factor = Math.Min(3.0, (double)(bubble.Volume / bubble.Threshold));
		return Math.Max(3, Math.Min((int)Math.Round(_objectSize * factor), _maxRadius));
	}

	private void DrawTooltip(RenderContext context, Bubble bubble, Point mouse)
	{
		var text = $"{(bubble.Side > 0 ? "Buy" : "Sell")} {bubble.Volume:0.##} @ {ChartInfo.GetPriceString(bubble.Price)}\n" +
			$"Threshold {bubble.Threshold:0.##}, {bubble.Time.AddHours(InstrumentInfo.TimeZone):HH:mm:ss.fff}";

		var size = context.MeasureString(text, _textFont);
		var rect = new Rectangle(mouse.X + 12, mouse.Y, size.Width + 10, size.Height + 6);

		context.FillRectangle(Color.FromArgb(200, 0, 0, 0), rect);
		context.DrawString(text, _textFont, Color.White, rect.X + 5, rect.Y + 3);
	}

	#endregion
}
