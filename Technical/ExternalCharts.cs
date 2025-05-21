namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

using ATAS.Indicators.Drawing;

using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;

using Color = System.Drawing.Color;

[DisplayName("External Chart")]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.ExternalChartsDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602383")]
public class ExternalCharts : Indicator
{
	#region Nested types

	public class RectangleInfo
	{
		#region Properties

		public decimal ClosePrice { get; set; }

		public int FirstPos { get; set; }

		public decimal Low { get; set; }

		public decimal OpenPrice { get; set; }

		public int SecondPos { get; set; }

		public decimal High { get; set; }

		#endregion
	}

	public enum TimeFrameScale
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.M1))]
		M1 = 1,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.M5))]
		M5 = 5,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.M10))]
		M10 = 10,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.M15))]
		M15 = 15,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.M30))]
		M30 = 30,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Hourly))]
		Hourly = 60,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.H2))]
		H2 = 120,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.H4))]
		H4 = 240,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.H6))]
		H6 = 360,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Daily))]
		Daily = 1440,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Weekly))]
		Weekly = 10080,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Monthly))]
		Monthly = 0
	}

	#endregion

	#region Static and constants

	private static readonly Dictionary<TimeFrameScale, TimeSpan> _periodTimeSpans = new()
	{
		{ TimeFrameScale.M1, TimeSpan.FromMinutes(1) },
		{ TimeFrameScale.M5, TimeSpan.FromMinutes(5) },
		{ TimeFrameScale.M10, TimeSpan.FromMinutes(10) },
		{ TimeFrameScale.M15, TimeSpan.FromMinutes(15) },
		{ TimeFrameScale.M30, TimeSpan.FromMinutes(30) },
		{ TimeFrameScale.Hourly, TimeSpan.FromHours(1) },
		{ TimeFrameScale.H2, TimeSpan.FromHours(2) },
		{ TimeFrameScale.H4, TimeSpan.FromHours(4) },
		{ TimeFrameScale.H6, TimeSpan.FromHours(6) }
	};

	#endregion

	#region Fields

	private readonly object _locker = new();
	private readonly List<RectangleInfo> _rectangles = new();
	private int _avgWidth;

	private DateTime _barStartTime;
	private int _days = 20;
	private CrossColor _downColor = DefaultColors.Red.Convert();
	private RenderPen _downPen = new(DefaultColors.Red, 1);
	private CrossColor _gridColor = CrossColor.FromArgb(50, 128, 128, 128);
	private RenderPen _gridPen = new(Color.FromArgb(50, 128, 128, 128), 1);
	private int _lastBar = -1;
	private DashStyle _style = DashStyle.Solid;
	private int _targetBar;
	private TimeFrameScale _tFrame = TimeFrameScale.Hourly;

	private CrossColor _upColor = Color.RoyalBlue.Convert();
	private RenderPen _upPen = new(Color.RoyalBlue, 1);
	private int _width = 1;

	#endregion

	#region Properties

	private bool IsSessionTframe => TFrame is TimeFrameScale.Daily or TimeFrameScale.Weekly or TimeFrameScale.Monthly;

	//Old property
	[Browsable(false)]
	public CrossColor AreaColor { get; set; }

	[Range(0, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Calculation), Name = nameof(Strings.DaysLookBack), Order = int.MaxValue,
		Description = nameof(Strings.DaysLookBackDescription))]
	public int Days
	{
		get => _days;
		set
		{
			_days = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowGrid), GroupName = nameof(Strings.Grid),
		Description = nameof(Strings.DisplayClusterBorderDescription), Order = 7)]
	public bool ShowGrid { get; set; }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Grid),
		Description = nameof(Strings.GridColorDescription), Order = 8)]
	public CrossColor GridColor
	{
		get => _gridColor;
		set
		{
			_gridColor = value;
			_gridPen = new RenderPen(value.Convert(), 1);
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowAsCandle), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.ShowAsCandleDescription), Order = 9)]
	public bool ExtCandleMode { get; set; }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BullishColor), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.BullishColorDescription), Order = 30)]
	public CrossColor UpCandleColor
	{
		get => _upColor;
		set
		{
			_upColor = value;
			_upPen = new RenderPen(value.Convert(), Width, Style.To());
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BearlishColor), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.BearishColorDescription), Order = 40)]
	public CrossColor DownCandleColor
	{
		get => _downColor;
		set
		{
			_downColor = value;
			_downPen = new RenderPen(value.Convert(), Width, Style.To());
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BackgroundBullish), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.BullishFillColorDescription), Order = 45)]
	public Color UpBackground { get; set; } = Color.FromArgb(100, Color.LightSkyBlue);

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BackgroundBearlish), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.BearishFillColorDescription), Order = 47)]
	public Color DownBackground { get; set; } = Color.FromArgb(100, Color.DarkRed);

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Width), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.BorderWidthPixelDescription), Order = 50)]
	[Range(1, 100)]
	public int Width
	{
		get => _width;
		set
		{
			_width = value;
			_upPen = new RenderPen(UpCandleColor.Convert(), value, Style.To());
			_downPen = new RenderPen(DownCandleColor.Convert(), value, Style.To());
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.DashStyle), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.BorderStyleDescription), Order = 60)]
	public LineDashStyle Style
	{
		get => _style.To();
		set
		{
			_style = value.To();

			_upPen = new RenderPen(UpCandleColor.Convert(), Width, value.To());
			_downPen = new RenderPen(DownCandleColor.Convert(), Width, value.To());
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.FillCandles), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.IsNeedFillDescription), Order = 65)]
	public bool FillCandles { get; set; }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ShowAboveChart), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.DrawAbovePriceDescription), Order = 70)]
	public bool Above
	{
		get => DrawAbovePrice;
		set
		{
			DrawAbovePrice = value;
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.ExternalPeriod), GroupName = nameof(Strings.TimeFrame),
		Description = nameof(Strings.SelectTimeframeDescription), Order = 5)]
	public TimeFrameScale TFrame
	{
		get => _tFrame;
		set
		{
			_tFrame = value;
			RecalculateValues();
		}
	}

	#endregion

	#region ctor

	public ExternalCharts()
		: base(true)
	{
		DrawAbovePrice = true;
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.LatestBar | DrawingLayouts.Historical);

		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
	}

	#endregion

	#region Protected methods

	protected override void OnCalculate(int bar, decimal value)
	{
		if (DataProvider is null)
			return;

		lock (_locker)
		{
			var candle = GetCandle(bar);

			if (bar == 0)
			{
				_rectangles.Clear();
				_targetBar = 0;

				if (_days > 0)
				{
					var days = 0;

					for (var i = CurrentBar - 1; i >= 0; i--)
					{
						_targetBar = i;

						if (!IsNewSession(i))
							continue;

						days++;

						if (days == _days)
							break;
					}
				}
			}

			if (bar < _targetBar)
				return;

			if (bar == _targetBar)
			{
				if (!IsSessionTframe)
					_barStartTime = DataProvider.GetCustomStartTime(candle.Time, _periodTimeSpans[TFrame]);

				AddRect(bar);
				return;
			}

			if (IsSessionTframe)
			{
				if ((TFrame is TimeFrameScale.Daily && IsNewSession(bar))
				    ||
				    (TFrame is TimeFrameScale.Weekly && IsNewWeek(bar))
				    ||
				    (TFrame is TimeFrameScale.Monthly && IsNewMonth(bar))
				    ||
				    bar == _targetBar)
					AddRect(bar);
				else
					UpdateLastRect(bar);
			}
			else
			{
				if (bar == _lastBar)
				{
					UpdateLastRect(bar);
					return;
				}

				if (_barStartTime != DataProvider.GetCustomStartTime(candle.Time, _periodTimeSpans[TFrame]))
				{
					_barStartTime = DataProvider.GetCustomStartTime(candle.Time, _periodTimeSpans[TFrame]);
					AddRect(bar);
				}
				else
					UpdateLastRect(bar);

				_lastBar = bar;
			}
		}
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (ChartInfo is null || InstrumentInfo is null)
			return;

		if (ChartInfo.PriceChartContainer.TotalBars == ChartInfo.PriceChartContainer.LastVisibleBarNumber)
		{
			if (layout != DrawingLayouts.LatestBar)
				return;
		}
		else
		{
			if (layout != DrawingLayouts.Historical)
				return;
		}

		lock (_locker)
		{
			var chartType = ChartInfo.ChartVisualMode;
			var useShift = chartType is ChartVisualModes.Clusters or ChartVisualModes.Line;

			for (var r = _rectangles.Count - 1; r >= 0; r--)
			{
				var rect = _rectangles[r];

				if (rect.FirstPos > ChartInfo.PriceChartContainer.LastVisibleBarNumber)
					continue;

				if (rect.SecondPos < ChartInfo.PriceChartContainer.FirstVisibleBarNumber)
					break;

				var x1 = ChartInfo.GetXByBar(rect.FirstPos);
				var x2 = ChartInfo.GetXByBar(rect.SecondPos + 1);

				if (r == _rectangles.Count - 1)
				{
					var rightX = x1 + (int)(_avgWidth * (ChartInfo.PriceChartContainer.BarSpacing + ChartInfo.PriceChartContainer.BarsWidth));
					x2 = Math.Max(rightX, x2);
				}

				var yBot = ChartInfo.GetYByPrice(rect.Low - (useShift ? InstrumentInfo.TickSize : 0), useShift);
				var yTop = ChartInfo.GetYByPrice(rect.High, useShift);

				if (ShowGrid && chartType == ChartVisualModes.Clusters)
				{
					for (var i = rect.Low - InstrumentInfo.TickSize; i <= rect.High; i += InstrumentInfo.TickSize)
					{
						var y = ChartInfo.GetYByPrice(i);
						context.DrawLine(_gridPen, x1, y, x2, y);
					}

					for (var i = rect.FirstPos; i <= rect.SecondPos + 1; i++)
					{
						var x = ChartInfo.GetXByBar(i);
						context.DrawLine(_gridPen, x, yBot, x, yTop);
					}
				}

				var bearish = rect.OpenPrice > rect.ClosePrice;

				var renderPen = bearish
					? _downPen
					: _upPen;

				var renderRectangle = new Rectangle(x1, yTop, x2 - x1, yBot - yTop);

				if (ExtCandleMode)
				{
					var y1 = ChartInfo.GetYByPrice(Math.Min(rect.OpenPrice, rect.ClosePrice), false);
					var y2 = ChartInfo.GetYByPrice(Math.Max(rect.OpenPrice, rect.ClosePrice), false);
					renderRectangle = new Rectangle(x1, y2, x2 - x1, y1 - y2);

					var midX = (x2 + x1) / 2;
					context.DrawLine(renderPen, midX, y2, midX, yTop);
					context.DrawLine(renderPen, midX, y1, midX, yBot);
				}

				if (FillCandles)
					context.FillRectangle(bearish ? DownBackground : UpBackground, renderRectangle);

				context.DrawRectangle(renderPen, renderRectangle);
			}
		}
	}

	#endregion

	#region Private methods

	private void UpdateLastRect(int bar)
	{
		var candle = GetCandle(bar);

		_rectangles[^1].ClosePrice = candle.Close;
		_rectangles[^1].SecondPos = bar;

		if (_rectangles[^1].High < candle.High)
			_rectangles[^1].High = candle.High;

		if (_rectangles[^1].Low > candle.Low)
			_rectangles[^1].Low = candle.Low;
	}

	private void AddRect(int bar)
	{
		var candle = GetCandle(bar);

		if (_rectangles.Count is not 0)
		{
			_avgWidth = 1 + (int)_rectangles
				.TakeLast(20)
				.Average(r => r.SecondPos - r.FirstPos);
		}

		_rectangles.Add(new RectangleInfo
		{
			FirstPos = bar,
			SecondPos = bar,
			Low = candle.Low,
			High = candle.High,
			OpenPrice = candle.Open,
			ClosePrice = candle.Close
		});
	}

	#endregion
}