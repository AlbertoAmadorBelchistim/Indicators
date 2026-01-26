namespace ATAS.Indicators.Technical;

using ATAS.DataFeedsCore;
using ATAS.DataFeedsCore.Statistics;
using ATAS.Indicators.Technical.Properties;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Text;
using static ATAS.Indicators.Technical.TradesOnChart;
using Color = System.Drawing.Color;
using DashStyle = System.Drawing.Drawing2D.DashStyle;
using Pen = OFT.Rendering.Tools.RenderPen;

[HelpLink("https://help.atas.net/support/solutions/articles/72000633119")]
[Category(IndicatorCategories.Trading)]
[DisplayName("Trades On Chart")]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.TradesOnChartDescription))]
public class TradesOnChart : Indicator
{
	#region Nested Types

	internal class TradeObj
	{
		internal int OpenBar { get; set; }
		internal decimal OpenPrice { get; set; }
		internal int CloseBar { get; set; }
		internal decimal ClosePrice { get; set; }
		internal OrderDirections Direction { get; set; }
		internal decimal PnL { get; set; }
		internal decimal PnLTicks { get; set; }
		internal DateTime OpenTime { get; set; }
		internal DateTime CloseTime { get; set; }
		internal decimal Volume { get; set; }
		internal string Security { get; set; }


		public TradeObj(HistoryMyTrade trade)
		{
			OpenPrice = trade.OpenPrice;
			ClosePrice = trade.ClosePrice;
			Direction = trade.OpenVolume > 0 ? OrderDirections.Buy : OrderDirections.Sell;
			PnL = trade.PnL;
			PnLTicks = trade.TicksPnL;
			OpenTime = trade.OpenTime;
			CloseTime = trade.CloseTime;
			Volume = Math.Abs(trade.OpenVolume);
			Security = trade.Security.Code;
		}
	}

    public enum LabelDisplayMode
    {
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Hide))]
        Hide = 0,
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Short))]
        Short = 1,
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Full))]
        Full = 2,
        [Display(Name = "Card")]
        Card = 3
    }

    public enum LabelHorizontalAnchor
    {
        [Display(Name = "Close bar")]
        CloseBar = 0,

        [Display(Name = "Trade midpoint")]
        Midpoint = 1
    }

	private readonly struct TradeKey : IEquatable<TradeKey>
	{
		public readonly DateTime OpenTime;
		public readonly DateTime CloseTime;
		public readonly decimal OpenPrice;
		public readonly decimal ClosePrice;
		public readonly decimal Volume;
		public readonly OrderDirections Direction;
		public readonly string Security;

		public TradeKey(DateTime openTime, DateTime closeTime, decimal openPrice, decimal closePrice, decimal volume, OrderDirections direction, string security)
		{
			OpenTime = openTime;
			CloseTime = closeTime;
			OpenPrice = openPrice;
			ClosePrice = closePrice;
			Volume = volume;
			Direction = direction;
			Security = security ?? string.Empty;
		}

		public bool Equals(TradeKey other)
		{
			return OpenTime == other.OpenTime
				&& CloseTime == other.CloseTime
				&& OpenPrice == other.OpenPrice
				&& ClosePrice == other.ClosePrice
				&& Volume == other.Volume
				&& Direction == other.Direction
				&& string.Equals(Security, other.Security, StringComparison.InvariantCultureIgnoreCase);
		}

		public override bool Equals(object obj) => obj is TradeKey other && Equals(other);

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = OpenTime.GetHashCode();
				hash = (hash * 397) ^ CloseTime.GetHashCode();
				hash = (hash * 397) ^ OpenPrice.GetHashCode();
				hash = (hash * 397) ^ ClosePrice.GetHashCode();
				hash = (hash * 397) ^ Volume.GetHashCode();
				hash = (hash * 397) ^ (int)Direction;
				hash = (hash * 397) ^ StringComparer.InvariantCultureIgnoreCase.GetHashCode(Security ?? string.Empty);
				return hash;
			}
		}
	}

	#endregion

	#region Fields

	private RenderFont _font = new RenderFont("Arial", 10F, FontStyle.Regular, GraphicsUnit.Point, 204);
	private RenderFont _labelFont = new RenderFont("Arial", 8F, FontStyle.Regular, GraphicsUnit.Point, 204);
	private RenderStringFormat _stringFormat = new RenderStringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
	private readonly List<TradeObj> _trades = new();
	private readonly object _tradesSync = new();
	private readonly HashSet<TradeKey> _tradeKeys = new();
	private Pen _buyPen;
	private Pen _sellPen;
	private readonly Pen _cardBorderPen = new Pen(Color.FromArgb(200, 255, 255, 255), 1.5f);
	private Color _buyColor;
	private Color _sellColor;
	private Color _profitColor;
	private Color _lossColor;
	private float _lineWidth = 2f;
	private DashStyle _lineStyle = DashStyle.Dash;
	private readonly List<Rectangle> _labelsAbove = new();
	private readonly List<Rectangle> _labelsBelow = new();
	private readonly List<Rectangle> _labelCollisionRects = new();
	private const int _maxLabelCollisionIterations = 25;
	private const int _labelCollisionTailScan = 64;
	private readonly List<TradeObj> _tooltipTrades = new();
	private readonly List<(TradeObj Trade, bool MouseOverMarker1, bool MouseOverMarker2)> _tradeInfoBuffer = new();
	private readonly RenderStringFormat _tooltipTextFormat = new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near };
	private readonly RenderStringFormat _labelLeftFormat = new() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center };
	private readonly RenderStringFormat _labelRightFormat = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
	private readonly StringBuilder _tooltipSb = new(256);
	private readonly StringBuilder _labelSb = new(128);
	private bool _subscriptionsAttached;
	private int _labelDistance = 10;

    private ITradingStatistics? _statistics;

    #endregion

	#region Properties

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowLines), GroupName = "Lines and markers", Order = 60)]
    private bool _showLine = true;

    public bool ShowLine
    {
        get => _showLine;
        set
        {
            if (_showLine == value)
                return;
            _showLine = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowDescription), GroupName = "Details", Order = 120)]
    private bool _showTooltip = true;

    public bool ShowTooltip
    {
        get => _showTooltip;
        set
        {
            if (_showTooltip == value)
                return;
            _showTooltip = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LabelDisplay), GroupName = "Labels", Order = 10)]
    private LabelDisplayMode _labelDisplay = LabelDisplayMode.Hide;

    public LabelDisplayMode LabelDisplay
    {
        get => _labelDisplay;
        set
        {
            if (_labelDisplay == value)
                return;
            _labelDisplay = value;
            RedrawChart();
        }
    }

    [Range(0, 200)]
    [Display(Name = "Label distance", Description = "Vertical spacing between trade markers and labels (px).", GroupName = "Labels", Order = 30)]
    public int LabelDistance
    {
        get => _labelDistance;
        set
        {
            var v = Math.Max(0, value);
            if (_labelDistance == v)
                return;
            _labelDistance = v;
            RedrawChart();
        }
    }

    [Display(Name = "Label centering", Description = "Defines the horizontal reference used to position trade labels.", GroupName = "Labels", Order = 20)]
    private LabelHorizontalAnchor _labelXAnchor = LabelHorizontalAnchor.CloseBar;

    public LabelHorizontalAnchor LabelXAnchor
    {
        get => _labelXAnchor;
        set
        {
            if (_labelXAnchor == value)
                return;
            _labelXAnchor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.BuyColor), GroupName = "Lines and markers", Order = 100)]
    public Color BuyColor
    {
        get => _buyColor;
        set
        {
            _buyColor = value;
            _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle);
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.SellColor), GroupName = "Lines and markers", Order = 110)]
    public Color SellColor
    {
        get => _sellColor;
        set
        {
            _sellColor = value;
            _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle);
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.ProfitColor), GroupName = "Labels", Order = 40)]
    public Color ProfitColor
    {
        get => _profitColor;
        set
        {
            _profitColor = value;
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LossColor), GroupName = "Labels", Order = 50)]
    public Color LossColor
    {
        get => _lossColor;
        set
        {
            _lossColor = value;
            RedrawChart();
        }
    }

    [Range(1, 20)]
    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineWidth), GroupName = "Lines and markers", Order = 70)]
    public float LineWidth
    {
        get => _lineWidth;
        set
        {
            _lineWidth = value;
            _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle);
            _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle);
            RedrawChart();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.DashStyle), GroupName = "Lines and markers", Order = 80)]
    public DashStyle LineStyle
    {
        get => _lineStyle;
        set
        {
            _lineStyle = value;
            _buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle);
            _sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle);
            RedrawChart();
        }
    }

    [Range(1, 10)]
    [Display(Name = "Marker size", Description = "Size of entry/exit markers.", GroupName = "Lines and markers", Order = 90)]
    private int _markerSize = 2;

	public int MarkerSize
	{
		get => _markerSize;
		set
		{
			var v = Math.Max(1, Math.Min(10, value));

			if (_markerSize == v)
				return;

			_markerSize = v;
			RedrawChart();
		}
	}

	#endregion

	#region ctor

	public TradesOnChart() : base(true)
	{
		DenyToChangePanel = true;
		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

		SubscribeToDrawingEvents(DrawingLayouts.Final);
		EnableCustomDrawing = true;
	}

	#endregion

	#region Protected Methods

	private void AttachSubscriptions()
	{
		if (_subscriptionsAttached)
			return;

        if (TradingManager != null)
            TradingManager.PortfolioSelected += TradingManager_PortfolioSelected;

        if (TradingStatisticsProvider != null)
        {
            TradingStatisticsProvider.StatisticsReloaded += OnRecalculate;
            TradingStatisticsProvider.SourceChanged += OnTradingStatisticsProviderSourceChanged;

            if (TradingStatisticsProvider.Statistics is { } stat)
                OnTradingStatisticsProviderSourceChanged(stat);
        }

        _subscriptionsAttached = true;
    }

	private void DetachSubscriptions()
	{
		if (!_subscriptionsAttached)
			return;

        if (TradingStatisticsProvider != null)
        {
            TradingStatisticsProvider.StatisticsReloaded -= OnRecalculate;
            TradingStatisticsProvider.SourceChanged -= OnTradingStatisticsProviderSourceChanged;
        }

        if (_statistics != null)
            _statistics.HistoryMyTrades.Added -= OnTradeAdded;

        _statistics = null;

		if (TradingManager != null)
			TradingManager.PortfolioSelected -= TradingManager_PortfolioSelected;

		_subscriptionsAttached = false;
	}

    private void OnTradingStatisticsProviderSourceChanged(ITradingStatistics stat)
    {
        if (_statistics != null)
            _statistics.HistoryMyTrades.Added -= OnTradeAdded;

        _statistics = stat;
        _statistics.HistoryMyTrades.Added += OnTradeAdded;

        OnRecalculate();
    }

    protected override void OnInitialize()
    {
        AttachSubscriptions();
        OnRecalculate();
    }

	protected override void OnDispose()
	{
		DetachSubscriptions();
		base.OnDispose();
	}

	private void TradingManager_PortfolioSelected(Portfolio obj)
	{
		OnRecalculate();
	}

	protected override void OnApplyDefaultColors()
	{
		if (ChartInfo is null) return;

		BuyColor = Color.FromArgb(0xFF, 0x2C, 0x4F, 0x3A);
		SellColor = Color.FromArgb(0xFF, 0x64, 0x27, 0x33);
		ProfitColor = Color.FromArgb(0xFF, 0x16, 0x7A, 0x3B);
		LossColor = Color.FromArgb(0xFF, 0xB0, 0x49, 0x4F);
	}

	protected override void OnRecalculate()
	{
		_buyPen = GetNewPen(_buyColor, _lineWidth, _lineStyle);
		_sellPen = GetNewPen(_sellColor, _lineWidth, _lineStyle);

		lock (_tradesSync)
		{
			_trades.Clear();
			_tradeKeys.Clear();
		}

		AddHistoryMyTrade();
	}

	protected override void OnCalculate(int bar, decimal value)
	{
	   
	}

	#region Rendering

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (ChartInfo is null) return;

		DrawTrades(context);
	}

	private void DrawTrades(RenderContext context)
	{
		TradeObj[] tradesSnapshot;

		lock (_tradesSync)
			tradesSnapshot = _trades.Count == 0
				? Array.Empty<TradeObj>()
				: _trades.ToArray();

		if (tradesSnapshot.Length == 0)
			return;

		_tooltipTrades.Clear();
		_tradeInfoBuffer.Clear();
		_labelsAbove.Clear();
		_labelsBelow.Clear();
		_labelCollisionRects.Clear();

		foreach (var trade in tradesSnapshot)
		{
			if (trade.OpenBar > LastVisibleBarNumber || trade.CloseBar < FirstVisibleBarNumber)
				continue;

			var x1 = ChartInfo.GetXByBar(trade.OpenBar, false);
			var y1 = ChartInfo.GetYByPrice(trade.OpenPrice, false);
			var x2 = ChartInfo.GetXByBar(trade.CloseBar, false);
			var y2 = ChartInfo.GetYByPrice(trade.ClosePrice, false);
			var pen = GetPenByDirection(trade.Direction);

			if(ShowLine)
				context.DrawLine(pen, x1, y1, x2, y2);

			var mouseOver = DrawMarker(context, new Point(x1, y1), trade.Direction, true);
			var mouseOver2 = DrawMarker(context, new Point(x2, y2), trade.Direction, false);

			_tradeInfoBuffer.Add((trade, mouseOver, mouseOver2));
		}

		foreach (var (trade, mouseOver, mouseOver2) in _tradeInfoBuffer)
		{
			var mouseOverLabel = false;

			if (LabelDisplay != LabelDisplayMode.Hide)
			{
				var candle = GetCandle(trade.CloseBar);

				if (candle is not null)
				{
					var isAbove = trade.Direction == OrderDirections.Buy;

					// X anchor is always resolved in pixels (CloseBar or visual midpoint)
					var anchorX = GetLabelAnchorX(trade);

					var (labelRect, labelHover) =
						DrawTradeLabel(context, trade, anchorX, candle, isAbove);

					mouseOverLabel = labelHover;

					_labelCollisionRects.Add(labelRect);

					if (isAbove)
						_labelsAbove.Add(labelRect);
					else
						_labelsBelow.Add(labelRect);
				}
			}

			if (ShowTooltip && (mouseOver || mouseOver2 || mouseOverLabel))
				_tooltipTrades.Add(trade);
		}

		if (_tooltipTrades.Count > 0)
		{
			var y = MouseLocationInfo.LastPosition.Y;

			foreach (var trade in _tooltipTrades)
			{
				DrawTooltip(context, trade, MouseLocationInfo.LastPosition.X, ref y);
				y += 5;
			}
		}
	}

	private void DrawTooltip(RenderContext context, TradeObj trade, int x, ref int y)
	{
		var directionColor = trade.Direction == OrderDirections.Buy ? _buyColor : _sellColor;
		var resultColor = trade.PnL > 0 ? _profitColor : _lossColor;
		var cornerRadius = 3;

        var direction = trade.Direction == OrderDirections.Buy ? "Long" : "Short";
        var openTime = trade.OpenTime.AddHours(InstrumentInfo.TimeZone);
		var closeTime = trade.CloseTime.AddHours(InstrumentInfo.TimeZone);

		_tooltipSb.Clear();
		_tooltipSb.Append(direction);
		_tooltipSb.Append(' ');
		_tooltipSb.Append(trade.Volume);
		_tooltipSb.Append(' ');
		_tooltipSb.Append(trade.Security);
		_tooltipSb.Append(Environment.NewLine);
		_tooltipSb.Append(Environment.NewLine);

        _tooltipSb.Append("Entry\t:  ");
        _tooltipSb.Append(ChartInfo.GetPriceString(trade.OpenPrice));
        _tooltipSb.Append("  ");
        _tooltipSb.Append(openTime.ToString("dd MMM HH:mm:ss"));
        _tooltipSb.Append(Environment.NewLine);

        _tooltipSb.Append("Exit\t:  ");
        _tooltipSb.Append(ChartInfo.GetPriceString(trade.ClosePrice));
        _tooltipSb.Append("  ");
        _tooltipSb.Append(closeTime.ToString("dd MMM HH:mm:ss"));

		var topText = _tooltipSb.ToString();

        _tooltipSb.Clear();
        _tooltipSb.Append("Result:  ");
        if (trade.PnL > 0)
            _tooltipSb.Append('+');
        _tooltipSb.Append(trade.PnL);
        _tooltipSb.Append("  (");
        _tooltipSb.Append(trade.PnLTicks);
        _tooltipSb.Append(" ticks)");

		var bottomText = _tooltipSb.ToString();


		var topSize = context.MeasureString(topText, _font);
		var bottomSize = context.MeasureString(bottomText, _font);

		var padding = 10;
		var width = (int)Math.Max(topSize.Width, bottomSize.Width) + padding * 2;
		var topHeight = (int)topSize.Height + padding * 2;
		var bottomHeight = (int)bottomSize.Height + padding * 2;

		var topRect = new Rectangle(x, y, width, topHeight + cornerRadius * 2);
		var bottomRect = new Rectangle(x, y + topHeight, width, bottomHeight);

		context.FillRectangle(directionColor, topRect, cornerRadius);
		context.FillRectangle(resultColor, bottomRect, cornerRadius);

		var overlapCover = new Rectangle(x, y + topHeight, width, cornerRadius * 2);
		context.FillRectangle(resultColor, overlapCover);

		var topTextRect = new Rectangle(x + padding, y + padding, width - padding * 2, topHeight - padding * 2);
		var bottomTextRect = new Rectangle(x + padding, y + topHeight + padding, width - padding * 2, bottomHeight - padding * 2);

		context.DrawString(topText, _font, Color.White, topTextRect, _tooltipTextFormat);
		context.DrawString(bottomText, _font, Color.White, bottomTextRect, _tooltipTextFormat);

		y += topHeight + bottomHeight;
	}

	private bool DrawMarker(RenderContext context, Point point, OrderDirections direction, bool isOpen)
	{
		var shift = MarkerSize * 4;
		var dir = direction == OrderDirections.Buy ? 1 : -1;
		var y2 = isOpen ? (point.Y + shift * dir) : (point.Y + shift * (-dir));
		var point2 = new Point(point.X - shift, y2);
		var point3 = new Point(point2.X + shift * 2, point2.Y);
		var color = GetMarkerColor(direction, isOpen);

		var points = new Point[] { point, point2, point3 };

		context.FillPolygon(color, points);

		context.DrawPolygon(ChartInfo.ColorsStore.Grid, points);

		if (IsPointInTriangle(MouseLocationInfo.LastPosition, point, point2, point3))
		{
			return true;
		}

		return false;
	}

	private void BuildLabelTexts(TradeObj trade, string direction, string pnlSign, out string leftText, out string rightText)
	{
		if (LabelDisplay == LabelDisplayMode.Full)
		{
			var entryPrice = ChartInfo.GetPriceString(trade.OpenPrice);
			var exitPrice = ChartInfo.GetPriceString(trade.ClosePrice);

			_labelSb.Clear();
			_labelSb.Append(direction);
			_labelSb.Append(' ');
			_labelSb.Append(trade.Volume);
			_labelSb.Append(" | ");
			_labelSb.Append(entryPrice);
			_labelSb.Append('→');
			_labelSb.Append(exitPrice);
			leftText = _labelSb.ToString();

			_labelSb.Clear();
			_labelSb.Append(' ');
			_labelSb.Append(pnlSign);
			_labelSb.Append(trade.PnL);
			_labelSb.Append(" (");
			_labelSb.Append(trade.PnLTicks);
			_labelSb.Append("t)");
			rightText = _labelSb.ToString();
		}
		else
		{
			_labelSb.Clear();
			_labelSb.Append(direction);
			_labelSb.Append(' ');
			_labelSb.Append(trade.Volume);
			leftText = _labelSb.ToString();

			_labelSb.Clear();
			_labelSb.Append(' ');
			_labelSb.Append(pnlSign);
			_labelSb.Append(trade.PnL);
			_labelSb.Append(" (");
			_labelSb.Append(trade.PnLTicks);
			_labelSb.Append("t)");
			rightText = _labelSb.ToString();
		}
	}

	private void BuildCardLabelLines(TradeObj trade, out string header, out string body, out string footer, out bool isProfit)
	{
		// Direction text (prefer explicit words for fast reading)
		var directionText = trade.Direction == OrderDirections.Buy
			? Resources.TradeDirectionLong
			: Resources.TradeDirectionShort;

		// Header: "LONG 0.001 BTCUSDT"
		_labelSb.Clear();
		_labelSb.Append(directionText);
		_labelSb.Append(' ');
		_labelSb.Append(trade.Volume);

		if (!string.IsNullOrWhiteSpace(trade.Security))
		{
			_labelSb.Append(" | ");
			_labelSb.Append(trade.Security);
		}

		header = _labelSb.ToString();

		// Body: Entry/Exit + times (same as tooltip, but compact)
		var openTime = trade.OpenTime.AddHours(InstrumentInfo.TimeZone);
		var closeTime = trade.CloseTime.AddHours(InstrumentInfo.TimeZone);

		_labelSb.Clear();
		_labelSb.Append(Resources.TradeEntry);
		_labelSb.Append(": ");
		_labelSb.Append(ChartInfo.GetPriceString(trade.OpenPrice));
		_labelSb.Append(" @ ");
		_labelSb.Append(openTime.ToString("HH:mm:ss"));

		_labelSb.Append(Environment.NewLine);

		_labelSb.Append(Resources.TradeExit);
		_labelSb.Append(": ");
		_labelSb.Append(ChartInfo.GetPriceString(trade.ClosePrice));
		_labelSb.Append(" @ ");
		_labelSb.Append(closeTime.ToString("HH:mm:ss"));

		body = _labelSb.ToString();

		// Footer: "Result: -0.0001 (-1 ticks)" (no background color; text color indicates sign)
		isProfit = trade.PnL >= 0;

		_labelSb.Clear();
		_labelSb.Append(Resources.TradeResult);
		_labelSb.Append(": ");

		if (trade.PnL > 0)
			_labelSb.Append('+');

		_labelSb.Append(trade.PnL);

		_labelSb.Append("  (");

		if (trade.PnLTicks > 0)
			_labelSb.Append('+');

		_labelSb.Append(trade.PnLTicks);
		_labelSb.Append(' ');
		_labelSb.Append(Resources.TradeTicks);
		_labelSb.Append(')');

		footer = _labelSb.ToString();
	}

	private (Rectangle Rect, bool MouseOver) DrawTradeLabel(RenderContext context, TradeObj trade, int anchorX, IndicatorCandle candle, bool isAbove)
	{

		if (candle is null)
			return (Rectangle.Empty, false);

		var direction = trade.Direction == OrderDirections.Buy
			? Resources.TradeDirectionLong
			: Resources.TradeDirectionShort;

		var pnlSign = trade.PnL > 0 ? "+" : "";

		if (LabelDisplay == LabelDisplayMode.Card)
			return DrawTradeCardLabel(context, trade, anchorX, candle, isAbove);

		BuildLabelTexts(trade, direction, pnlSign, out var leftText, out var rightText);

		var leftSize = context.MeasureString(leftText, _labelFont);
		var rightSize = context.MeasureString(rightText, _labelFont);

		var padding = 3;
		var leftWidth = leftSize.Width + padding;
		var rightWidth = rightSize.Width + padding;
		var rectWidth = leftWidth + rightWidth;
		var rectHeight = Math.Max(leftSize.Height, rightSize.Height) + padding * 2;

		var labelX = anchorX - (int)(rectWidth / 2f);

		var markerOffset = (MarkerSize * 4) + LabelDistance;
		var baseY = isAbove
			? ChartInfo.GetYByPrice(candle.High, false) - markerOffset - rectHeight
			: ChartInfo.GetYByPrice(candle.Low, false) + markerOffset;

		var spacing = 3;
		var stepSize = rectHeight + spacing;
		var yPosition = baseY;

		var testRect = new Rectangle(labelX, yPosition, rectWidth, rectHeight);

		// Collision resolution (hot-path): avoid LINQ and bound the work.
		var iter = 0;

		while (iter++ < _maxLabelCollisionIterations)
		{
			var hasCollision = false;

			// Scan only the tail of recently placed rectangles (most likely collisions).
			var start = Math.Max(0, _labelCollisionRects.Count - _labelCollisionTailScan);

			for (var i = start; i < _labelCollisionRects.Count; i++)
			{
				if (_labelCollisionRects[i].IntersectsWith(testRect))
				{
					hasCollision = true;
					break;
				}
			}

			if (!hasCollision)
				break;

			// Move vertically away from the band depending on placement.
			if (isAbove)
			{
				testRect = new Rectangle(testRect.X, testRect.Y - stepSize, testRect.Width, testRect.Height);
			}
			else
			{
				testRect = new Rectangle(testRect.X, testRect.Y + stepSize, testRect.Width, testRect.Height);
			}
		}


		var directionColor = trade.Direction == OrderDirections.Buy ? _buyColor : _sellColor;
		var resultColor = trade.PnL > 0 ? _profitColor : _lossColor;
		var cornerRadius = 3;

		var leftSectionRect = new Rectangle(testRect.X, testRect.Y, leftWidth + cornerRadius * 2, testRect.Height);
		context.FillRectangle(directionColor, leftSectionRect, cornerRadius);

		var rightSectionRect = new Rectangle(testRect.X + leftWidth, testRect.Y, rightWidth, testRect.Height);
		context.FillRectangle(resultColor, rightSectionRect, cornerRadius);

		var overlapCover = new Rectangle(testRect.X + leftWidth, testRect.Y, cornerRadius * 2, testRect.Height);
		context.FillRectangle(resultColor, overlapCover);

		var leftTextRect = new Rectangle(testRect.X + padding, testRect.Y + padding, leftWidth - padding, testRect.Height - padding * 2);
		var rightTextRect = new Rectangle(testRect.X + leftWidth, testRect.Y + padding, rightWidth - padding, testRect.Height - padding * 2);

		context.DrawString(leftText, _labelFont, Color.White, leftTextRect, _labelLeftFormat);
		context.DrawString(rightText, _labelFont, Color.White, rightTextRect, _labelRightFormat);

		var mouseOver = testRect.Contains(MouseLocationInfo.LastPosition);

		return (testRect, mouseOver);
	}

	private (Rectangle Rect, bool MouseOver) DrawTradeCardLabel(
	RenderContext context,
	TradeObj trade,
	int anchorX,
	IndicatorCandle candle,
	bool isAbove)
	{
		BuildCardLabelLines(trade, out var headerText, out var bodyText, out var footerText, out var isProfit);

		// Measure blocks
		var headerSize = context.MeasureString(headerText, _labelFont);
		var bodySize = context.MeasureString(bodyText, _labelFont);
		var footerSize = context.MeasureString(footerText, _labelFont);

		var paddingX = 7;
		var paddingY = 6;
		var blockGap = 4;

		var maxTextWidth = Math.Max(headerSize.Width, Math.Max(bodySize.Width, footerSize.Width));
		var width = (int)maxTextWidth + (paddingX * 2);

		var headerHeight = (int)headerSize.Height + (paddingY * 2);
		var bodyHeight = (int)bodySize.Height + (paddingY * 2);
		var footerHeight = (int)footerSize.Height + (paddingY * 2);

		var height = headerHeight + blockGap + bodyHeight + blockGap + footerHeight;

		// Position
		var x = anchorX - (width / 2);
		var markerOffset = (MarkerSize * 4) + LabelDistance;

		var baseY = isAbove
			? ChartInfo.GetYByPrice(candle.High, false) - markerOffset - height
			: ChartInfo.GetYByPrice(candle.Low, false) + markerOffset;

		var testRect = new Rectangle(x, baseY, width, height);

		// Anchor point on price (marker position)
		var anchorY = isAbove
			? ChartInfo.GetYByPrice(candle.High, false)
			: ChartInfo.GetYByPrice(candle.Low, false);

		// Collision resolution
		var stepSize = height + 2;
		var iter = 0;

		while (iter++ < _maxLabelCollisionIterations)
		{
			var hasCollision = false;
			var start = Math.Max(0, _labelCollisionRects.Count - _labelCollisionTailScan);

			for (var i = start; i < _labelCollisionRects.Count; i++)
			{
				if (_labelCollisionRects[i].IntersectsWith(testRect))
				{
					hasCollision = true;
					break;
				}
			}

			if (!hasCollision)
				break;

			testRect = isAbove
				? new Rectangle(testRect.X, testRect.Y - stepSize, testRect.Width, testRect.Height)
				: new Rectangle(testRect.X, testRect.Y + stepSize, testRect.Width, testRect.Height);
		}

		// Connector
		context.DrawLine(_cardBorderPen, anchorX, anchorY, anchorX, isAbove ? testRect.Bottom : testRect.Top);

		// Colors
		var directionColor = trade.Direction == OrderDirections.Buy ? _buyColor : _sellColor;
		var pnlTextColor = isProfit ? _profitColor : _lossColor;

		// Split rects
		var headerRect = new Rectangle(testRect.X, testRect.Y, testRect.Width, headerHeight);
		var bodyRect = new Rectangle(testRect.X, headerRect.Bottom + blockGap, testRect.Width, bodyHeight);
		var footerRect = new Rectangle(testRect.X, bodyRect.Bottom + blockGap, testRect.Width, footerHeight);

		// Backgrounds:
		// - Header: direction color (strong cue)
		// - Body: neutral dark
		// - Footer: neutral dark (keep structure stable); PnL shown by text color
		context.FillRectangle(directionColor, headerRect);
		context.FillRectangle(Color.FromArgb(60, 0, 0, 0), bodyRect);
		context.FillRectangle(Color.FromArgb(60, 0, 0, 0), footerRect);

		// Border
		context.DrawRectangle(_cardBorderPen, testRect);

		// Separators
		context.DrawLine(_cardBorderPen, testRect.Left, headerRect.Bottom + (blockGap / 2), testRect.Right, headerRect.Bottom + (blockGap / 2));
		context.DrawLine(_cardBorderPen, testRect.Left, bodyRect.Bottom + (blockGap / 2), testRect.Right, bodyRect.Bottom + (blockGap / 2));

		// Text rects
		var headerTextRect = new Rectangle(headerRect.X + paddingX, headerRect.Y + paddingY, headerRect.Width - paddingX * 2, headerRect.Height - paddingY * 2);
		var bodyTextRect = new Rectangle(bodyRect.X + paddingX, bodyRect.Y + paddingY, bodyRect.Width - paddingX * 2, bodyRect.Height - paddingY * 2);
		var footerTextRect = new Rectangle(footerRect.X + paddingX, footerRect.Y + paddingY, footerRect.Width - paddingX * 2, footerRect.Height - paddingY * 2);

		// Draw text
		context.DrawString(headerText, _labelFont, Color.White, headerTextRect, _labelRightFormat);     // centered
		context.DrawString(bodyText, _labelFont, Color.White, bodyTextRect, _labelLeftFormat);         // left
		// Subtle outline for contrast on busy charts
		var shadowRect = new Rectangle(footerTextRect.X + 1, footerTextRect.Y + 1, footerTextRect.Width, footerTextRect.Height);
		context.DrawString(footerText, _labelFont, Color.FromArgb(180, 0, 0, 0), shadowRect, _labelRightFormat);

		context.DrawString(footerText, _labelFont, pnlTextColor, footerTextRect, _labelRightFormat);

		var mouseOver = testRect.Contains(MouseLocationInfo.LastPosition);
		return (testRect, mouseOver);
	}

	#endregion

	#endregion

	#region Private Methods

	private void AddHistoryMyTrade()
	{
		if (TradingManager?.Portfolio == null|| TradingManager?.Security == null)
			return;

		// Limit history processing to the currently visible chart range.
		// This reduces CPU usage and visual clutter on large accounts / long sessions.
		DateTime fromTime;
		DateTime toTime;

		try
		{
			var firstBar = Math.Max(0, FirstVisibleBarNumber);
			var lastBar = Math.Max(firstBar, LastVisibleBarNumber);

			fromTime = GetCandle(firstBar).Time;
			toTime = GetCandle(lastBar).Time;
		}
		catch
		{
			// Fallback: if candles are not available (rare during initialization), do not filter by time.
			fromTime = DateTime.MinValue;
			toTime = DateTime.MaxValue;
		}

        var allTrades = _statistics?
            .HistoryMyTrades
            .Where(t =>
                t.AccountID == TradingManager.Portfolio.AccountID &&
                t.Security.SecurityId.Equals(TradingManager.Security.SecurityId, StringComparison.InvariantCultureIgnoreCase) &&
                t.CloseTime >= fromTime &&
                t.OpenTime <= toTime
                ) ?? [];

        var newTrades = new List<TradeObj>();

        foreach (var trade in allTrades)
        {
            var tradeObj = CreateTradePair(trade);
            if (tradeObj != null)
                newTrades.Add(tradeObj);
        }

        if (newTrades.Count == 0)
            return;

        lock (_tradesSync)
        {
            _trades.AddRange(newTrades);
        }
    }

	private static bool IsTradeClosed(HistoryMyTrade trade)
	{
		// Best-effort closure check.
		// We avoid showing partially populated trades that may arrive via realtime events.
		if (trade is null)
			return false;

		if (trade.CloseTime == default)
			return false;

		// Some feeds may temporarily duplicate OpenTime/CloseTime during transitions.
		if (trade.CloseTime <= trade.OpenTime)
			return false;

		// ClosePrice usually becomes valid only when the trade is finalized.
		if (trade.ClosePrice == 0)
			return false;

		return true;
	}

	private void OnTradeAdded(HistoryMyTrade trade)
	{
		if (TradingManager?.Portfolio == null || TradingManager?.Security == null)
			return;

		if (trade.AccountID != TradingManager.Portfolio.AccountID)
			return;

		if (trade.Security.Instrument != TradingManager.Security.Instrument)
			return;

		// Ensure we only draw finalized trades.
		if (!IsTradeClosed(trade))
			return;

		var tradeObj = CreateTradePair(trade);
		if (tradeObj == null)
			return;

		lock (_tradesSync)
		{
			_trades.Add(tradeObj);
		}

		// Draw ASAP (do not wait for a full recalculation cycle).
		RedrawChart();
	}

	private TradeObj CreateTradePair(HistoryMyTrade trade)
	{
		// Map open/close time to bars (best-effort).
		var enterBar = GetBarByTime(trade.OpenTime);
		if (enterBar < 0)
			return null;

		var exitBar = GetBarByTime(trade.CloseTime);

		// If we can't map the close time (e.g., chart not loaded that far),
		// keep the trade stable by snapping to the entry bar.
		if (exitBar < 0)
			exitBar = enterBar;

		var temp = new TradeObj(trade)
		{
			OpenBar = enterBar,
			CloseBar = exitBar,
		};

		// Deduplicate across history reloads and realtime events.
		// We key by semantic trade identity, not by bar mapping.
		var key = new TradeKey(
			temp.OpenTime,
			temp.CloseTime,
			temp.OpenPrice,
			temp.ClosePrice,
			temp.Volume,
			temp.Direction,
			temp.Security
		);

		lock (_tradesSync)
		{
			if (!_tradeKeys.Add(key))
				return null;
		}

		return temp;
	}

	private int GetBarByTime(DateTime time)
	{
		// Binary search for the last bar whose candle time is <= requested time.
		// This is hot-path for history rebuild and realtime updates.
		var lo = 0;
		var hi = CurrentBar - 1;

		if (hi < 0)
			return -1;

		// Quick rejects / fast paths.
		if (GetCandle(lo).Time > time)
			return -1;

		if (GetCandle(hi).Time <= time)
			return hi;

		var result = -1;

		while (lo <= hi)
		{
			var mid = lo + ((hi - lo) / 2);
			var midTime = GetCandle(mid).Time;

			if (midTime <= time)
			{
				result = mid;
				lo = mid + 1;
			}
			else
			{
				hi = mid - 1;
			}
		}

		return result;
	}

	private bool IsPointInTriangle(Point p, Point p0, Point p1, Point p2)
	{
		double area = TriangleArea(p0, p1, p2);
		double area1 = TriangleArea(p, p0, p1);
		double area2 = TriangleArea(p, p1, p2);
		double area3 = TriangleArea(p, p2, p0);

		return Math.Abs(area - (area1 + area2 + area3)) < 0.001;
	}

	private double TriangleArea(Point p0, Point p1, Point p2)
	{
		return Math.Abs((p0.X * (p1.Y - p2.Y) + p1.X * (p2.Y - p0.Y) + p2.X * (p0.Y - p1.Y)) / 2.0);
	}

	private Color GetMarkerColor(OrderDirections direction, bool isOpen)
	{
		return direction switch
		{
			OrderDirections.Buy => isOpen ? _buyColor : _sellColor,
			OrderDirections.Sell => isOpen ? _sellColor : _buyColor,
			_ => Color.Transparent
		};
	}

	private Pen GetPenByDirection(OrderDirections directions)
	{
		return directions switch
		{
			OrderDirections.Buy => _buyPen,
			_ => _sellPen,
		};
	}

	private Pen GetNewPen(Color color, float lineWidth, DashStyle lineStyle)
	{
		return new Pen(color, lineWidth) { DashStyle = lineStyle };
	}

	private int GetLabelAnchorX(TradeObj trade)
	{
		if (ChartInfo is null)
			return 0;

		if (LabelXAnchor == LabelHorizontalAnchor.Midpoint)
		{
			var x1 = ChartInfo.GetXByBar(trade.OpenBar, false);
			var x2 = ChartInfo.GetXByBar(trade.CloseBar, false);

			return (int)(((long)x1 + (long)x2) / 2L);
		}

		// Default: CloseBar
		return ChartInfo.GetXByBar(trade.CloseBar, false);
	}

	#endregion
}
