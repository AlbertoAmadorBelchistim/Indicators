namespace ATAS.Indicators.Technical;

using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical.Properties;

using DevExpress.Utils.Serializing;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using Pen = CrossPen;

[DisplayName("Initial Balance")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.InitialBalanceIndDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602294")]
public class InitialBalance : Indicator
{
	#region Nested types

	public enum PeriodType
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Minutes))]
		Minutes,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bars))]
		Bars
	}

	public enum LabelPositionType
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.None))]
		None,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.TillBar))]
		TillBar,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Left))]
		Left,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Right))]
		Right
	}

	public enum OverlayLineType
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.None))]
		None,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.TillBar))]
		TillBar,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.FullWidth))]
		FullWidth
	}

	#endregion

	#region Fields

	private readonly ValueDataSeries _ibh = new("Ibh", "IBH")
	{
		Color = DefaultColors.Blue.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1,
		DescriptionKey = nameof(Strings.TopBandDscription)
	};

	private readonly ValueDataSeries _ibhx1 = new("Ibhx1", "IBHX1")
	{
		Color = DefaultColors.Fuchsia.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1
	};

	private readonly ValueDataSeries _ibhx2 = new("Ibhx2", "IBHX2")
	{
		Color = DefaultColors.Fuchsia.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1
	};

	private readonly ValueDataSeries _ibhx3 = new("Ibhx3", "IBHX3")
	{
		Color = DefaultColors.Fuchsia.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1
	};

	private readonly ValueDataSeries _ibl = new("Ibl", "IBL")
	{
		Color = DefaultColors.Red.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1,
		DescriptionKey = nameof(Strings.BottomBandDscription)
	};

	private readonly ValueDataSeries _iblx1 = new("Iblx1", "IBLX1")
	{
		Color = DefaultColors.Purple.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1
	};

	private readonly ValueDataSeries _iblx2 = new("Iblx2", "IBLX2")
	{
		Color = DefaultColors.Purple.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1
	};

	private readonly ValueDataSeries _iblx3 = new("Iblx3", "IBLX3")
	{
		Color = DefaultColors.Purple.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1
	};

	private readonly ValueDataSeries _ibm = new("Ibm", "IBM")
	{
		Color = DefaultColors.Green.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		VisualType = VisualMode.Square,
		Width = 1,
		DescriptionKey = nameof(Strings.MidBandDescription)
	};

	private readonly ValueDataSeries _mid = new("MidId", "Mid")
	{
		Color = CrossColor.FromArgb(0, 0, 255, 0),
		LineDashStyle = LineDashStyle.Solid,
		VisualType = VisualMode.Square,
		Width = 1,
		DescriptionKey = nameof(Strings.SessionAveragePriceDescription)
	};

	private RangeDataSeries _ibhx32 = new("Ibhx32", "ibhx32")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};
	private RangeDataSeries _ibhx21 = new("Ibhx21", "ibhx21")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};
	private RangeDataSeries _ibhx1h = new("Ibhx1h", "ibhx1h")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};
	private RangeDataSeries _ibHm = new("IbHm", "ibHm")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};
	private RangeDataSeries _ibMl = new("IbM1", "ibM1")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};
	private RangeDataSeries _ibl1 = new("Ibl1", "ibl1")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};
	private RangeDataSeries _iblx12 = new("Ibl12", "ibl12")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};
	private RangeDataSeries _iblx23 = new("Ibl23", "ibl23")
	{
		RangeColor = System.Drawing.Color.Transparent.Convert(),
		DrawAbovePrice = false,
		IsHidden = true
	};

	private CrossColor _borderColor = DefaultColors.Red.Convert();
	private int _borderWidth = 1;
	private bool _calculate;
	private bool _customSessionStart;
	private int _days = 20;
	private bool _drawText = true;
	private TimeSpan _endDate;
	private DateTime _endTime = DateTime.MaxValue;
	private CrossColor _fillColor = DefaultColors.Yellow.Convert();
	private bool _highLowIsSet;
	private decimal _ibMax = decimal.MinValue;
	private decimal _ibMin = decimal.MaxValue;
	private decimal _ibmValue = decimal.Zero;

	private bool _initialized;
	private int _lastStartBar = -1;
	private decimal _maxValue = decimal.MinValue;
	private decimal _minValue = decimal.MaxValue;
	private int _period = 60;
	private PeriodType _periodMode = PeriodType.Minutes;
	private DrawingRectangle _rectangle = new(0, 0, 0, 0, CrossPens.Gray, new CrossSolidBrush(DefaultColors.Yellow));
	private bool _showOpenRange = true;
	private TimeSpan _startDate = new(9, 0, 0);
	private int _targetBar;
	private decimal _x1 = 1m;
	private decimal _x2 = 2m;
	private decimal _x3 = 3m;
	private decimal ibhx1 = decimal.Zero;
	private decimal ibhx2 = decimal.Zero;
	private decimal ibhx3 = decimal.Zero;
	private decimal iblx1 = decimal.Zero;
	private decimal iblx2 = decimal.Zero;
	private decimal iblx3 = decimal.Zero;
	private decimal mid = decimal.Zero;

	private bool _isStarted;

	private float _fontSize = 12.0f;
	private RenderFont _font;

	private readonly RenderStringFormat _stringLeftFormat = new()
	{
		Alignment = StringAlignment.Near,
		LineAlignment = StringAlignment.Center,
		Trimming = StringTrimming.EllipsisCharacter,
		FormatFlags = StringFormatFlags.NoWrap
	};

	private LabelPositionType _labelPosition = LabelPositionType.TillBar;
	private OverlayLineType _overlayLineType = OverlayLineType.None;

	private int _lastIbEndBar = -1;
	private int _lastCustomSessionBar = -1;

	// Snapshot values captured at IB completion (exact bar)
	private decimal _sIbh;
	private decimal _sIbm;
	private decimal _sIbl;
	private decimal _sIbhx1;
	private decimal _sIbhx2;
	private decimal _sIbhx3;
	private decimal _sIblx1;
	private decimal _sIblx2;
	private decimal _sIblx3;
	private decimal _sMid;

    private bool _showDuringFormation = true;
    private bool _wasInCustomSession;


    #endregion

    #region Properties

    [Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Calculation), 
		Name = nameof(Strings.DaysLookBack), Order = int.MaxValue, Description = nameof(Strings.DaysLookBackDescription))]
	[Range(0, 1000)]
	public int Days
	{
		get => _days;
		set
		{
			_days = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Show),
		GroupName = nameof(Strings.OpenRange), Description = nameof(Strings.ShowOpenRangeDescription), Order = 10)]
	public bool ShowOpenRange
	{
		get => _showOpenRange;
		set
		{
			_showOpenRange = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BorderWidth),
		GroupName = nameof(Strings.OpenRange), Description = nameof(Strings.BorderWidthPixelDescription), Order = 20)]
	[Range(1, 100)]
	public int BorderWidth
	{
		get => _borderWidth;
		set
		{
			_borderWidth = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BorderColor),
		GroupName = nameof(Strings.OpenRange), Description = nameof(Strings.BorderColorDescription),Order = 30)]
	public CrossColor BorderColor
	{
		get => _borderColor;
		set
		{
			_borderColor = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.FillColor),
		GroupName = nameof(Strings.OpenRange), Description = nameof(Strings.FillColorDescription),Order = 40)]
	public CrossColor FillColor
	{
		get => _fillColor;
		set
		{
			_fillColor = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CustomSession),
		GroupName = nameof(Strings.SessionTime), Description = nameof(Strings.IsCustomSessionDescription),Order = 10)]
	public bool CustomSessionStart
	{
		get => _customSessionStart;
		set
		{
			_customSessionStart = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.StartTime),
		GroupName = nameof(Strings.SessionTime), Description = nameof(Strings.StartTimeDescription), Order = 20)]
	public TimeSpan StartDate
	{
		get => _startDate;
		set
		{
			_startDate = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.EndTime),
		GroupName = nameof(Strings.SessionTime), Description = nameof(Strings.EndTimeDescription), Order = 20)]
	public TimeSpan EndDate
	{
		get => _endDate;
		set
		{
			_endDate = value;
			RecalculateValues();
		}
	}

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Period),
		GroupName = nameof(Strings.SessionTime), Description = nameof(Strings.PeriodDescription), Order = 30)]
	[Range(1, 10000)]
	public int Period
	{
		get => _period;
		set
		{
			_period = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.PeriodType),
		GroupName = nameof(Strings.SessionTime), Description = nameof(Strings.PeriodTypeDescription), Order = 40)]
	public PeriodType PeriodMode
	{
		get => _periodMode;
		set
		{
			_periodMode = value;
			RecalculateValues();
		}
	}

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Multiplier1),
		GroupName = nameof(Strings.Multiplier), Description = nameof(Strings.MultiplierDescription), Order = 100)]
	public decimal X1
	{
		get => _x1;
		set
		{
			_x1 = value;
			RecalculateValues();
		}
	}

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Multiplier2),
		GroupName = nameof(Strings.Multiplier), Description = nameof(Strings.MultiplierDescription),Order = 110)]
	public decimal X2
	{
		get => _x2;
		set
		{
			_x2 = value;
			RecalculateValues();
		}
	}

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Multiplier3),
		GroupName = nameof(Strings.Multiplier), Description = nameof(Strings.MultiplierDescription), Order = 120)]
	public decimal X3
	{
		get => _x3;
		set
		{
			_x3 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.Text),
		GroupName = nameof(Resources.Show), Description = nameof(Resources.IsNeedShowLabelDescription), Order = 130)]
	public bool DrawText
	{
		get => _drawText;
		set
		{
			_drawText = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.FontSize),
		GroupName = nameof(Resources.Show), Description = nameof(Resources.FontSizeDescription), Order = 135)]
	[Range(6, 48)]
	public float FontSize
	{
		get => _fontSize;
		set
		{
			// Prevent noise redraws from the property grid.
			if (Math.Abs(_fontSize - value) < 0.01f)
				return;

			_fontSize = value;
			_font = new RenderFont("Arial", _fontSize);

			// Purely visual change: do not recalculate series.
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.LabelPosition),
		GroupName = nameof(Resources.Show), Description = nameof(Resources.LabelPositionDescription), Order = 136)]
	public LabelPositionType LabelPosition
	{
		get => _labelPosition;
		set
		{
			if (_labelPosition == value)
				return;

			_labelPosition = value;
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.OverlayLineType),
	GroupName = nameof(Resources.Show), Description = nameof(Resources.OverlayLineTypeDescription), Order = 137)]
	public OverlayLineType OverlayLines
	{
		get => _overlayLineType;
		set
		{
			if (_overlayLineType == value)
				return;

			_overlayLineType = value;
			RedrawChart();
		}
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowDuringFormation),
        GroupName = nameof(Resources.Show), Description = nameof(Resources.ShowDuringFormationDescription), Order = 139)]
    public bool ShowDuringFormation
    {
        get => _showDuringFormation;
        set
        {
            if (_showDuringFormation == value)
                return;

            _showDuringFormation = value;

            // Visual-only change, but we must update series clipping + redraw.
            RecalculateValues();
            RedrawChart();
        }
    }


    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBHX32), 
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription), Order = 200)]
	public CrossColor Ibhx32
	{
		get=>_ibhx32.RangeColor; 
		set=>_ibhx32.RangeColor = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBHX21),
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription),Order = 210)]
	public CrossColor Ibhx21 
	{
		get => _ibhx21.RangeColor;
		set => _ibhx21.RangeColor = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBHX1H),
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription), Order = 220)]
	public CrossColor Ibhx1h 
	{
		get => _ibhx1h.RangeColor;
		set => _ibhx1h.RangeColor = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBHM), 
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription), Order = 230)]
	public CrossColor IbHm
	{
		get => _ibHm.RangeColor;
		set => _ibHm.RangeColor = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBML), 
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription), Order = 240)]
	public CrossColor IbMl
	{
		get => _ibMl.RangeColor;
		set => _ibMl.RangeColor = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBL1), 
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription), Order = 250)]
	public CrossColor Ibl1
	{
		get => _ibl1.RangeColor;
		set => _ibl1.RangeColor = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBLX12),
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription), Order = 260)]
	public CrossColor Iblx12
	{
		get => _iblx12.RangeColor;
		set => _iblx12.RangeColor = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.IBLX23),
		GroupName = nameof(Strings.BackGround), Description = nameof(Strings.AreaColorDescription), Order = 270)]
	public CrossColor Iblx23
	{
		get => _iblx23.RangeColor;
		set => _iblx23.RangeColor = value;
	}

	#endregion
	
	#region ctor

	public InitialBalance()
		: base(true)
	{
		DenyToChangePanel = true;
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);
		_font = new RenderFont("Arial", _fontSize);

		DataSeries[0] = _mid;
		DataSeries.Add(_ibh);
		DataSeries.Add(_ibl);
		DataSeries.Add(_ibm);
		DataSeries.Add(_ibhx1);
		DataSeries.Add(_ibhx2);
		DataSeries.Add(_ibhx3);
		DataSeries.Add(_iblx1);
		DataSeries.Add(_iblx2);
		DataSeries.Add(_iblx3);

		DataSeries.Add(_ibhx32);
		DataSeries.Add(_ibhx21);
		DataSeries.Add(_ibhx1h);
		DataSeries.Add(_ibHm);
		DataSeries.Add(_ibMl);
		DataSeries.Add(_ibl1);
		DataSeries.Add(_iblx12);
		DataSeries.Add(_iblx23);

		_ibh.PropertyChanged += DataSeriesPropertyChanged;
		_ibl.PropertyChanged += DataSeriesPropertyChanged;
		_ibm.PropertyChanged += DataSeriesPropertyChanged;
		_ibhx1.PropertyChanged += DataSeriesPropertyChanged;
		_ibhx2.PropertyChanged += DataSeriesPropertyChanged;
		_ibhx3.PropertyChanged += DataSeriesPropertyChanged;
		_iblx1.PropertyChanged += DataSeriesPropertyChanged;
		_iblx2.PropertyChanged += DataSeriesPropertyChanged;
		_iblx3.PropertyChanged += DataSeriesPropertyChanged;
	}

	#endregion

	#region Protected methods

	protected override void OnInitialize()
	{
		DataSeries.ForEach(ds =>
		{
			if (ds is RangeDataSeries rds)
				rds.ScaleIt = false;
		});
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		if (bar == 0)
		{
			DataSeries.ForEach(x => x.Clear());
			ibhx1 = decimal.Zero;
			ibhx2 = decimal.Zero;
			ibhx3 = decimal.Zero;
			iblx1 = decimal.Zero;
			iblx2 = decimal.Zero;
			iblx3 = decimal.Zero;
			mid = decimal.Zero;
			_maxValue = decimal.MinValue;
			_minValue = decimal.MaxValue;
			_ibMax = decimal.MinValue;
			_ibMin = decimal.MaxValue;
			_ibmValue = decimal.Zero;
			_highLowIsSet = false;
			_lastStartBar = -1;
			_endTime = DateTime.MaxValue;
			_calculate = false;
			_initialized = false;
			_targetBar = 0;
			_isStarted = false;
            _wasInCustomSession = false;

            if (_days <= 0)
				return;

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

		if (bar < _targetBar)
			return;

		_initialized = true;
		var candle = GetCandle(bar);

		var time = candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
		var lastTime = candle.LastTime.AddHours(InstrumentInfo.TimeZone).TimeOfDay;
		
		if (CustomSessionStart)
		{
			bool inSession;

			if (StartDate < EndDate)
				inSession = (time >= StartDate || lastTime >= StartDate) && time < EndDate;
			else if (StartDate > EndDate)
			{
				inSession = ((time >= StartDate || lastTime >= StartDate) && time > EndDate)
						 || ((time <= EndDate || lastTime <= EndDate) && time < EndDate);
			}
			else
				inSession = true;

            if (inSession)
            {
                _wasInCustomSession = true;
            }
            else
            {
                // Transition: first bar outside after being inside
                if (_wasInCustomSession)
                {
                    _lastCustomSessionBar = Math.Max(-1, bar - 1);

                    foreach (var dataSeries in DataSeries)
                        if (dataSeries is ValueDataSeries series)
                            series.SetPointOfEndLine(_lastCustomSessionBar);

                    _wasInCustomSession = false;
                }

                return;
            }
		}

		var candleFullDateTime = candle.Time.AddHours(InstrumentInfo.TimeZone);
		var isStart = false;
		var isEnd = false;

		if (!_isStarted)
		{
			isStart = _customSessionStart
				   ? bar != 0 && (time >= StartDate || lastTime >= StartDate) && (GetPrevDateTime(bar).TimeOfDay < StartDate || GetPrevDateTime(bar).Date < candleFullDateTime.Date)
				   : IsNewSession(bar);
		}

		if (_isStarted)
		{
			isEnd = (PeriodMode is PeriodType.Minutes && candleFullDateTime >= _endTime && GetPrevDateTime(bar) < _endTime)
				 || (PeriodMode is PeriodType.Bars && bar - _lastStartBar >= Period);
		}

        var endThisBar = isEnd;

        if (isStart)
		{
			//Clear all values
			_maxValue = decimal.MinValue;
			_minValue = decimal.MaxValue;
			_ibMax = decimal.MinValue;
			_ibMin = decimal.MaxValue;
			_ibmValue = decimal.Zero;
			ibhx1 = decimal.Zero;
			ibhx2 = decimal.Zero;
			ibhx3 = decimal.Zero;
			iblx1 = decimal.Zero;
			iblx2 = decimal.Zero;
			iblx3 = decimal.Zero;
			_calculate = true;
			_highLowIsSet = false;
			_lastStartBar = bar;
			_endTime = candleFullDateTime.AddMinutes(_period);
			_isStarted = true;
			_lastIbEndBar = -1;
			_lastCustomSessionBar = -1;
			_sIbh = _sIbm = _sIbl = 0m;
			_sIbhx1 = _sIbhx2 = _sIbhx3 = 0m;
			_sIblx1 = _sIblx2 = _sIblx3 = 0m;
			_sMid = 0m;
            _wasInCustomSession = true;

            foreach (var dataSeries in DataSeries)
				if (dataSeries is ValueDataSeries series)
					series.SetPointOfEndLine(bar - 1);

			if (ShowOpenRange)
			{
				var pen = new CrossPen(ConvertColor(_borderColor))
				{
					Width = _borderWidth
				};
				var brush = new CrossSolidBrush(ConvertColor(_fillColor));

				_rectangle = new DrawingRectangle(bar, decimal.Zero, bar, decimal.Zero, pen, brush);

				if (Rectangles.LastOrDefault()?.FirstBar == bar)
					Rectangles.RemoveAt(Rectangles.Count - 1);

				Rectangles.Add(_rectangle);
			}
		}

        else if (endThisBar)
        {
            // Mark completion bar, but DO NOT snapshot yet and DO NOT stop calculation yet.
            _lastIbEndBar = bar;
        }

        else if (isEnd)
		{
			// Snapshot values at the exact IB completion bar (before we stop calculation)
			_lastIbEndBar = bar;

			_sIbh = _ibh[bar];
			_sIbm = _ibm[bar];
			_sIbl = _ibl[bar];

			_sIbhx1 = _ibhx1[bar];
			_sIbhx2 = _ibhx2[bar];
			_sIbhx3 = _ibhx3[bar];

			_sIblx1 = _iblx1[bar];
			_sIblx2 = _iblx2[bar];
			_sIblx3 = _iblx3[bar];

			_sMid = _mid[bar];

			_calculate = _isStarted = false;
		}

		if (_calculate)
		{
            if (!ShowDuringFormation)
            {
                // Keep lines hidden during formation: clip end line to previous bar.
                foreach (var dataSeries in DataSeries)
                    if (dataSeries is ValueDataSeries series)
                        series.SetPointOfEndLine(bar - 1);
            }

            if (candle.High > _maxValue)
			{
				_highLowIsSet = true;
				_ibMax = _maxValue = candle.High;
			}

			if (candle.Low < _minValue)
			{
				_highLowIsSet = true;
				_ibMin = _minValue = candle.Low;
			}

			if (ShowOpenRange)
			{
				_rectangle.SecondBar = bar;
				_rectangle.FirstPrice = _ibMax;
				_rectangle.SecondPrice = _ibMin;
			}
		}

		if (candle.High > _maxValue)
			_maxValue = candle.High;

		if (candle.Low < _minValue)
			_minValue = candle.Low;

		if (!_highLowIsSet)
			return;

		_mid[bar] = mid = (_minValue + _maxValue) / 2m;
		_ibh[bar] = _ibMax;
		_ibl[bar] = _ibMin;
		_ibmValue = _ibm[bar] = (_ibMin + _ibMax) / 2m;
		var diff = _ibMax - _ibMin;

		ibhx1 = _ibhx1[bar] = _ibMax + diff * _x1;
		ibhx2 = _ibhx2[bar] = _ibMax + diff * _x2;
		ibhx3 = _ibhx3[bar] = _ibMax + diff * _x3;
		iblx1 = _iblx1[bar] = _ibMin - diff * _x1;
		iblx2 = _iblx2[bar] = _ibMin - diff * _x2;
		iblx3 = _iblx3[bar] = _ibMin - diff * _x3;

		_ibhx32[bar].Upper = ibhx3;
		_ibhx32[bar].Lower = _ibhx21[bar].Upper = ibhx2;
		_ibhx21[bar].Lower = _ibhx1h[bar].Upper = ibhx1;
		_ibhx1h[bar].Lower = _ibHm[bar].Upper = _ibh[bar];
		_ibHm[bar].Lower = _ibMl[bar].Upper = _ibm[bar];
		_ibMl[bar].Lower = _ibl1[bar].Upper = _ibl[bar];
		_ibl1[bar].Lower = _iblx12[bar].Upper = iblx1;
		_iblx12[bar].Lower = _iblx23[bar].Upper = iblx2;
		_iblx23[bar].Lower = iblx3;

        if (endThisBar)
        {
            // Snapshot values after the exact bar has been computed.
            _sMid = mid;

            _sIbh = _ibMax;
            _sIbm = _ibmValue;
            _sIbl = _ibMin;

            _sIbhx1 = ibhx1;
            _sIbhx2 = ibhx2;
            _sIbhx3 = ibhx3;

            _sIblx1 = iblx1;
            _sIblx2 = iblx2;
            _sIblx3 = iblx3;

            _calculate = false;
            _isStarted = false;
        }

    }

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (layout != DrawingLayouts.Final)
			return;

		if (!_drawText || ChartInfo is null || CurrentBar <= 0)
			return;

		if (_labelPosition == LabelPositionType.None)
			return;

        if (!ShowDuringFormation && _lastIbEndBar < 0)
            return;

        var barIndex = Math.Max(0, CurrentBar - 1);

		// Anchor bar for X positioning
		var anchorBarIndex = barIndex;

		if (CustomSessionStart && _lastCustomSessionBar >= 0)
			anchorBarIndex = _lastCustomSessionBar;

		// Price values (snapshot after IB completion)
		var midPrice = _lastIbEndBar >= 0 ? _sMid : _mid[barIndex];

		var ibh = _lastIbEndBar >= 0 ? _sIbh : _ibh[barIndex];
		var ibm = _lastIbEndBar >= 0 ? _sIbm : _ibm[barIndex];
		var ibl = _lastIbEndBar >= 0 ? _sIbl : _ibl[barIndex];

		var ibhx1 = _lastIbEndBar >= 0 ? _sIbhx1 : _ibhx1[barIndex];
		var ibhx2 = _lastIbEndBar >= 0 ? _sIbhx2 : _ibhx2[barIndex];
		var ibhx3 = _lastIbEndBar >= 0 ? _sIbhx3 : _ibhx3[barIndex];

		var iblx1 = _lastIbEndBar >= 0 ? _sIblx1 : _iblx1[barIndex];
		var iblx2 = _lastIbEndBar >= 0 ? _sIblx2 : _iblx2[barIndex];
		var iblx3 = _lastIbEndBar >= 0 ? _sIblx3 : _iblx3[barIndex];

		var region = ChartInfo.PriceChartContainer.Region;

		var paddingRight = 5;

		// Measure widest label text for right anchoring
		int maxLabelWidth = 0;
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelMid, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBH, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBM, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBL, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBHX1, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBHX2, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBHX3, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBLX1, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBLX2, _font).Width);
		maxLabelWidth = Math.Max(maxLabelWidth, context.MeasureString(Resources.InitialBalance_LabelIBLX3, _font).Width);

		// Box adds +4 width in DrawLabel; keep consistent
		var rightX = region.Right - paddingRight - (maxLabelWidth + 4);

		var xBar = ChartInfo.GetXByBar(anchorBarIndex);
		var barWidth = (int)ChartInfo.PriceChartContainer.BarsWidth;
		var xBarEnd = (int)xBar + barWidth;

		int x;

		switch (_labelPosition)
		{
			case LabelPositionType.Left:
				x = region.X + 5;
				break;

			case LabelPositionType.Right:
				x = rightX;
				break;

			default: // TillBar
				x = xBarEnd + 5;
				break;
		}

		var rMid = GetLabelRect(context, Resources.InitialBalance_LabelMid, midPrice, x);

		var rIbh = GetLabelRect(context, Resources.InitialBalance_LabelIBH, ibh, x);
		var rIbm = GetLabelRect(context, Resources.InitialBalance_LabelIBM, ibm, x);
		var rIbl = GetLabelRect(context, Resources.InitialBalance_LabelIBL, ibl, x);

		var rHx1 = GetLabelRect(context, Resources.InitialBalance_LabelIBHX1, ibhx1, x);
		var rHx2 = GetLabelRect(context, Resources.InitialBalance_LabelIBHX2, ibhx2, x);
		var rHx3 = GetLabelRect(context, Resources.InitialBalance_LabelIBHX3, ibhx3, x);

		var rLx1 = GetLabelRect(context, Resources.InitialBalance_LabelIBLX1, iblx1, x);
		var rLx2 = GetLabelRect(context, Resources.InitialBalance_LabelIBLX2, iblx2, x);
		var rLx3 = GetLabelRect(context, Resources.InitialBalance_LabelIBLX3, iblx3, x);

		// 1) Draw overlay connectors first (under labels)
		if (_overlayLineType != OverlayLineType.None)
		{
			DrawOverlay(context, _mid, midPrice, region, xBarEnd, (rMid?.Left ?? x));

			DrawOverlay(context, _ibh, ibh, region, xBarEnd, (rIbh?.Left ?? x));
			DrawOverlay(context, _ibm, ibm, region, xBarEnd, (rIbm?.Left ?? x));
			DrawOverlay(context, _ibl, ibl, region, xBarEnd, (rIbl?.Left ?? x));

			DrawOverlay(context, _ibhx1, ibhx1, region, xBarEnd, (rHx1?.Left ?? x));
			DrawOverlay(context, _ibhx2, ibhx2, region, xBarEnd, (rHx2?.Left ?? x));
			DrawOverlay(context, _ibhx3, ibhx3, region, xBarEnd, (rHx3?.Left ?? x));

			DrawOverlay(context, _iblx1, iblx1, region, xBarEnd, (rLx1?.Left ?? x));
			DrawOverlay(context, _iblx2, iblx2, region, xBarEnd, (rLx2?.Left ?? x));
			DrawOverlay(context, _iblx3, iblx3, region, xBarEnd, (rLx3?.Left ?? x));
		}

		DrawLabel(context, Resources.InitialBalance_LabelMid, _mid, midPrice, x);

		DrawLabel(context, Resources.InitialBalance_LabelIBH, _ibh, ibh, x);
		DrawLabel(context, Resources.InitialBalance_LabelIBM, _ibm, ibm, x);
		DrawLabel(context, Resources.InitialBalance_LabelIBL, _ibl, ibl, x);

		DrawLabel(context, Resources.InitialBalance_LabelIBHX1, _ibhx1, ibhx1, x);
		DrawLabel(context, Resources.InitialBalance_LabelIBHX2, _ibhx2, ibhx2, x);
		DrawLabel(context, Resources.InitialBalance_LabelIBHX3, _ibhx3, ibhx3, x);

		DrawLabel(context, Resources.InitialBalance_LabelIBLX1, _iblx1, iblx1, x);
		DrawLabel(context, Resources.InitialBalance_LabelIBLX2, _iblx2, iblx2, x);
		DrawLabel(context, Resources.InitialBalance_LabelIBLX3, _iblx3, iblx3, x);
	}

	private Rectangle? DrawLabel(RenderContext context, string text, ValueDataSeries series, decimal price, int x)
	{
		if (price == 0m || price == decimal.MinValue || price == decimal.MaxValue)
			return null;

		var region = ChartInfo.PriceChartContainer.Region;
		var y = ChartInfo.GetYByPrice(price, false);

		if (y < region.Y || y > region.Bottom)
			return null;

		var size = context.MeasureString(text, _font);

		var textColor = ChartInfo.ColorsStore.MouseTextColor;
		var backgroundColor = ChartInfo.ColorsStore.BaseBackgroundColor;

		var pen = new PenSettings
		{
			Color = series.Color,
			Width = series.Width,
			LineDashStyle = series.LineDashStyle
		}.RenderObject;

		// Box rect (with padding)
		var rect = new Rectangle(x - 2, y - size.Height / 2 - 1, size.Width + 4, size.Height + 2);

		context.FillRectangle(backgroundColor, rect);
		context.DrawRectangle(pen, rect);

		var textRect = new Rectangle(x, y - size.Height / 2, size.Width, size.Height);
		context.DrawString(text, _font, textColor, textRect, _stringLeftFormat);

		return rect;
	}

	private DateTime GetPrevDateTime(int bar)
	{
		return GetCandle(bar - 1).Time.AddHours(InstrumentInfo.TimeZone);
	}

	#endregion

	#region Private methods

	private void DataSeriesPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!_initialized)
			return;

		RecalculateValues();
	}

	private System.Drawing.Color ConvertColor(CrossColor color)
	{
		return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	private void DrawOverlay(RenderContext context, ValueDataSeries series, decimal price, Rectangle region, int xBarEnd, int xTarget)
{
	if (_overlayLineType == OverlayLineType.None)
		return;

	if (price == 0m || price == decimal.MinValue || price == decimal.MaxValue)
		return;

	var y = ChartInfo.GetYByPrice(price, false);

	if (y < region.Y || y > region.Bottom)
		return;

	var pen = new PenSettings
	{
		Color = series.Color,
		Width = series.Width,
		LineDashStyle = series.LineDashStyle
	}.RenderObject;

	int x1;
	int x2;

	switch (_overlayLineType)
	{
		case OverlayLineType.FullWidth:
			x1 = region.X;
			x2 = region.Right;
			break;

		default: // TillBar
			// From end of last bar to (just before) label box
			x1 = xBarEnd;
			x2 = Math.Max(x1, xTarget - 3);
			break;
	}

	// Draw under labels
	context.DrawLine(pen, x1, y, x2, y);
}

	private Rectangle? GetLabelRect(RenderContext context, string text, decimal price, int x)
	{
		if (price == 0m || price == decimal.MinValue || price == decimal.MaxValue)
			return null;

		var region = ChartInfo.PriceChartContainer.Region;
		var y = ChartInfo.GetYByPrice(price, false);

		if (y < region.Y || y > region.Bottom)
			return null;

		var size = context.MeasureString(text, _font);
		return new Rectangle(x - 2, y - size.Height / 2 - 1, size.Width + 4, size.Height + 2);
	}

	#endregion
}