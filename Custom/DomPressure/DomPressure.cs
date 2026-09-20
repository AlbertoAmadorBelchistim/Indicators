namespace ATAS.Indicators.Technical;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;

using ATAS.Indicators.Technical.DomPressureCore;

using OFT.Rendering.Context;
using OFT.Rendering.Tools;

using Utils.Common.Logging;

/// <summary>
/// DOM pressure: the resting volume of the order book (bids minus asks on the levels nearest to
/// the market) drawn behind the bar delta, with a marker where aggression runs against the book
/// (absorption). The market depth has no history: the DOM power only exists for the bars that
/// were open while the chart was running, and it is kept through recalculations of the chart.
/// </summary>
[Category("Order Flow")]
[DisplayName("DOM Pressure")]
[Description("Resting order book volume (bids minus asks) against the bar delta, with absorption markers where aggression runs against the book. The DOM power is realtime only.")]
public class DomPressure : Indicator
{
	#region Fields

	private readonly ValueDataSeries _powerSeries = new("DomPower", "DOM power")
	{
		VisualType = VisualMode.Hide,
		ScaleIt = false,
		ShowZeroValue = false,
		IsHidden = true
	};

	// Book and power store, guarded by _sync: market depth arrives on its own thread.
	private readonly object _sync = new();
	private readonly DepthBook _book = new();
	private readonly PowerStore _store = new();

	private readonly RenderFont _axisFont = new("Arial", 8);
	private readonly RenderPen _barBorderPen = new(Color.Black);

	private int _domDepthLimit = 20;
	private int _absorptionThreshold = 15;
	private PowerSampling _sampling = PowerSampling.BarClose;
	private int _powerWidth = 95;
	private int _deltaWidth = 30;
	private int _powerOpacity = 60;
	private int _deltaOpacity = 255;
	private Color _buyColor = Color.LimeGreen;
	private Color _sellColor = Color.Red;
	private Color _absorptionColor = Color.Yellow;
	private Color _axisColor = Color.Gray;
	private bool _showGuideLines = true;
	private bool _showScaleLabel = true;

	#endregion

	#region Properties

	#region Calculation

	[Display(Name = "DOM levels per side", GroupName = "Calculation", Order = 10,
		Description = "Order book levels added up on each side, nearest to the market first: 20 adds the 20 best bids and the 20 best asks. 0 adds the whole book. Changing it clears the DOM power recorded so far.")]
	[Range(0, 1000)]
	public int DomDepthLimit
	{
		get => _domDepthLimit;
		set
		{
			if (_domDepthLimit == value)
				return;

			_domDepthLimit = value;

			// The recorded power was measured on other levels: it cannot be compared.
			lock (_sync)
				_store.Clear();

			RecalculateValues();
		}
	}

	[Display(Name = "Power sampling", GroupName = "Calculation", Order = 20,
		Description = "The DOM power of a bar: its last value while the bar was open, or the average of every update in the bar.")]
	public PowerSampling Sampling
	{
		get => _sampling;
		set
		{
			_sampling = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Absorption threshold, %", GroupName = "Calculation", Order = 30,
		Description = "Marks absorption when the bar delta runs against the DOM power and is at least this percentage of it.")]
	[Range(1, 1000)]
	public int AbsorptionThreshold
	{
		get => _absorptionThreshold;
		set
		{
			_absorptionThreshold = value;
			RedrawChart();
		}
	}

	#endregion

	#region Visualization

	[Display(Name = "Power width, %", GroupName = "Visualization", Order = 10)]
	[Range(10, 100)]
	public int PowerWidth
	{
		get => _powerWidth;
		set
		{
			_powerWidth = value;
			RedrawChart();
		}
	}

	[Display(Name = "Delta width, %", GroupName = "Visualization", Order = 20)]
	[Range(5, 100)]
	public int DeltaWidth
	{
		get => _deltaWidth;
		set
		{
			_deltaWidth = value;
			RedrawChart();
		}
	}

	[Display(Name = "Power opacity", GroupName = "Visualization", Order = 30)]
	[Range(10, 255)]
	public int PowerOpacity
	{
		get => _powerOpacity;
		set
		{
			_powerOpacity = value;
			RedrawChart();
		}
	}

	[Display(Name = "Delta opacity", GroupName = "Visualization", Order = 40)]
	[Range(10, 255)]
	public int DeltaOpacity
	{
		get => _deltaOpacity;
		set
		{
			_deltaOpacity = value;
			RedrawChart();
		}
	}

	[Display(Name = "Guide lines", GroupName = "Visualization", Order = 50,
		Description = "Dotted lines at 50% and 80% of the DOM power scale, above and below zero.")]
	public bool ShowGuideLines
	{
		get => _showGuideLines;
		set
		{
			_showGuideLines = value;
			RedrawChart();
		}
	}

	[Display(Name = "Scale label", GroupName = "Visualization", Order = 60,
		Description = "The largest DOM power and delta of the visible bars, which fill the panel height.")]
	public bool ShowScaleLabel
	{
		get => _showScaleLabel;
		set
		{
			_showScaleLabel = value;
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

	[Display(Name = "Absorption marker", GroupName = "Colors", Order = 30)]
	public CrossColor AbsorptionColor
	{
		get => _absorptionColor.Convert();
		set
		{
			_absorptionColor = value.Convert();
			RedrawChart();
		}
	}

	[Display(Name = "Axis color", GroupName = "Colors", Order = 40)]
	public CrossColor AxisColor
	{
		get => _axisColor.Convert();
		set
		{
			_axisColor = value.Convert();
			RedrawChart();
		}
	}

	#endregion

	#endregion

	#region ctor

	public DomPressure()
		: base(true)
	{
		Panel = IndicatorDataProvider.NewPanel;
		DenyToChangePanel = true;

		DataSeries[0] = _powerSeries;

		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
	}

	#endregion

	#region Protected methods

	protected override void OnInitialize()
	{
		this.LogInfo($"DomPressure: initialized ({typeof(DomPressure).Assembly.GetName().Version}).");
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		if (bar == 0)
			LoadBook();

		// The power recorded while the chart was running, if the bar was open then.
		var time = GetCandle(bar).Time;
		PowerSample sample;
		bool found;

		lock (_sync)
			found = _store.TryGet(time, out sample);

		_powerSeries[bar] = found ? sample.ValueOf(_sampling) : 0m;
	}

	protected override void MarketDepthChanged(MarketDataArg depth)
	{
		var bar = CurrentBar - 1;

		if (bar < 0)
			return;

		var time = GetCandle(bar).Time;
		PowerSample sample;

		lock (_sync)
		{
			// A book that lost a side (a reconnection, or no snapshot at load) is taken again
			// whole, so the sums do not come from the levels updated since then only.
			if (_book.AskLevels == 0 || _book.BidLevels == 0)
				LoadBookLocked();

			_book.Apply(depth.DataType == MarketDataType.Ask, depth.Price, depth.Volume);

			if (!_book.TryGetSums(_domDepthLimit, out var bids, out var asks))
				return;

			sample = _store.Add(time, bids - asks);
		}

		_powerSeries[bar] = sample.ValueOf(_sampling);
		RedrawChart();
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (ChartInfo == null || Container == null)
			return;

		var region = Container.Region;
		var first = Math.Max(0, FirstVisibleBarNumber);
		var last = Math.Min(LastVisibleBarNumber, CurrentBar - 1);

		if (last < first)
			return;

		var middleY = region.Y + region.Height / 2;

		// Symmetric scales around zero, one for the power and one for the delta, from the
		// visible bars.
		var maxPower = 0m;
		var maxDelta = 0m;

		for (var i = first; i <= last; i++)
		{
			maxPower = Math.Max(maxPower, Math.Abs(_powerSeries[i]));
			maxDelta = Math.Max(maxDelta, Math.Abs(GetCandle(i).Delta));
		}

		var halfHeight = region.Height / 2f * 0.9f;
		var powerScale = maxPower == 0m ? 0f : halfHeight / (float)maxPower;
		var deltaScale = maxDelta == 0m ? 0f : halfHeight / (float)maxDelta;

		context.SetClip(region);

		context.DrawLine(new RenderPen(_axisColor), region.X, middleY, region.Right, middleY);

		if (_showGuideLines)
		{
			var guidePen = new RenderPen(Color.FromArgb(180, _axisColor), 1, System.Drawing.Drawing2D.DashStyle.Dot);

			foreach (var fraction in new[] { 0.5f, 0.8f })
			{
				var offset = (int)(halfHeight * fraction);
				context.DrawLine(guidePen, region.X, middleY - offset, region.Right, middleY - offset);
				context.DrawLine(guidePen, region.X, middleY + offset, region.Right, middleY + offset);
			}
		}

		var barWidth = (int)ChartInfo.PriceChartContainer.BarsWidth;
		var threshold = (decimal)_absorptionThreshold;

		for (var i = first; i <= last; i++)
		{
			var x = ChartInfo.GetXByBar(i);
			var power = _powerSeries[i];
			var delta = GetCandle(i).Delta;

			DrawBar(context, x, barWidth, middleY, power, powerScale, _powerWidth, _powerOpacity, false, out _);
			DrawBar(context, x, barWidth, middleY, delta, deltaScale, _deltaWidth, _deltaOpacity, true, out var deltaTipY);

			if (AbsorptionRule.IsAbsorption(power, delta, threshold))
				DrawAbsorptionMarker(context, x, barWidth, deltaTipY);
		}

		if (_showScaleLabel)
		{
			var text = $"Power max: {FormatK(maxPower)}\nDelta max: {FormatK(maxDelta)}";
			var size = context.MeasureString(text, _axisFont);
			context.DrawString(text, _axisFont, _axisColor, region.Right - size.Width - 5, region.Y + 5);
		}

		context.ResetClip();
	}

	#endregion

	#region Private methods

	private void LoadBook()
	{
		lock (_sync)
			LoadBookLocked();
	}

	// Caller holds _sync.
	private void LoadBookLocked()
	{
		_book.Clear();

		// Null while the settings are loaded, before the indicator is attached to a chart.
		var marketDepth = MarketDepthInfo;

		if (marketDepth is null)
			return;

		foreach (var level in marketDepth.GetMarketDepthSnapshot())
			_book.Apply(level.DataType == MarketDataType.Ask, level.Price, level.Volume);
	}

	private void DrawBar(RenderContext context, int x, int barWidth, int zeroY, decimal value, float scale, int widthPercent, int alpha, bool outlined, out int tipY)
	{
		tipY = zeroY;

		if (value == 0m || scale == 0f)
			return;

		var height = Math.Max(1, (int)(Math.Abs((float)value) * scale));
		var top = value > 0m ? zeroY - height : zeroY;
		tipY = value > 0m ? top : top + height;

		var width = Math.Max(1, barWidth * widthPercent / 100);
		var rect = new Rectangle(x + (barWidth - width) / 2, top, width, height);

		context.FillRectangle(Color.FromArgb(alpha, value > 0m ? _buyColor : _sellColor), rect);

		if (outlined)
			context.DrawRectangle(_barBorderPen, rect);
	}

	private void DrawAbsorptionMarker(RenderContext context, int x, int barWidth, int tipY)
	{
		const int size = 4;
		var centerX = x + barWidth / 2;

		Point[] diamond =
		{
			new(centerX, tipY - size),
			new(centerX + size, tipY),
			new(centerX, tipY + size),
			new(centerX - size, tipY)
		};

		context.FillPolygon(_absorptionColor, diamond);
		context.DrawPolygon(_barBorderPen, diamond);
	}

	private static string FormatK(decimal value)
	{
		return Math.Abs(value) >= 1000m ? $"{value / 1000m:0.#}k" : $"{value:0}";
	}

	#endregion
}
