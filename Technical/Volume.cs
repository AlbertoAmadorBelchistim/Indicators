namespace ATAS.Indicators.Technical;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System;

using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical.Properties;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;

using Color = System.Drawing.Color;

[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.VolumeIndDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602498")]
public class Volume : Indicator
{
	#region Nested types

	public enum InputType
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Volume))]
		Volume,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Ticks))]
		Ticks,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Ask))]
		Asks,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Bid))]
		Bids
	}

	public enum Location
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Up))]
		Up,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Middle))]
		Middle,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Down))]
		Down
	}

    // ===================== Thresholds (Fixed / Dynamic-Welford) =====================

	public enum ThresholdSource
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdSourceFixed))]
		Fixed = 0,

		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdSourceDynamicWelford))]
		DynamicWelford = 1
	}

	// Session window for dynamic thresholds (time-of-day anchored)
	public enum SessionWindowMode
	{
		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionWindowModeRth))]
		RTH,

		[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionWindowModeFull24h))]
		Full24h
	}

    private struct WelfordAcc
    {
        public int Count;
        public decimal Mean;
        public decimal M2;

        public void Add(decimal x)
        {
            Count++;
            var delta = x - Mean;
            Mean += delta / Count;
            var delta2 = x - Mean;
            M2 += delta * delta2;
        }

        public decimal Std()
        {
            if (Count <= 1) 
                return 0m;

            var variance = M2 / (Count - 1);
            return (decimal)Math.Sqrt((double)variance);
        }

        public void Reset()
        {
            Count = 0;
            Mean = 0m;
            M2 = 0m;
        }
    }


    #endregion

    #region Fields

    private bool _deltaColored;
	private decimal _filter;
	private Color _filterColor = Color.LightBlue;
	private InputType _input = InputType.Volume;
	private int _lastReverseAlert;
	private int _lastVolumeAlert;
	private Color _negColor = Color.Red;
	private Color _neutralColor = Color.Gray;
	private Color _posColor = Color.Green;

    #region Legacy Series

	//For old templates
	private readonly ValueDataSeries _negative = new("NegativeId", "Negative")
    {
	    VisualType = VisualMode.Hide,
		IsHidden = true
    };

    private readonly ValueDataSeries _neutral = new("NeutralId", "Neutral")
    {
	    VisualType = VisualMode.Hide,
		Color = Color.Gray.Convert(),
	    IsHidden = true
    };

    private readonly ValueDataSeries _positive = new("PositiveId", "Positive")
    {
	    VisualType = VisualMode.Hide,
	    Color = Color.Green.Convert(),
        IsHidden = true
    };

	#endregion

    private ValueDataSeries _renderSeries = new("RenderSeries", Strings.Visualization)
    {
	    VisualType = VisualMode.Histogram,
	    ShowZeroValue = false,
	    UseMinimizedModeIfEnabled = true,
	    ResetAlertsOnNewBar = true
    };

    private bool _useFilter;

	protected RenderStringFormat Format = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

	protected Highest HighestVol = new();
	protected ValueDataSeries MaxVolSeries;
	protected Color TextColor = DefaultColors.Blue;

    // ===================== Thresholds (Fixed) =====================

	private readonly ValueDataSeries _thrMajor = new("VolThrMajor", Resources.VolumeThresholdMajor)
	{
		VisualType = VisualMode.Line,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true
	};

	private readonly ValueDataSeries _thrMinor = new("VolThrMinor", Resources.VolumeThresholdMinor)
	{
		VisualType = VisualMode.Line,
		ShowCurrentValue = false,
		UseMinimizedModeIfEnabled = true,
		IgnoredByAlerts = true
	};

    private bool _showThresholdLines;
    private decimal _fixedMinorLevel = 1000m;
    private decimal _fixedMajorLevel = 2000m;

    private ThresholdSource _thresholds = ThresholdSource.Fixed;
    private SessionWindowMode _sessionMode = SessionWindowMode.RTH;

    private int _samplesForMeanStd = 20;
    private decimal _stdMultiplier = 2m;

    private TimeSpan _rthStart = new(9, 30, 0);
    private TimeSpan _rthEnd = new(16, 0, 0);

    private bool _dynReady;
    private decimal _dynMinor;
    private decimal _dynMajor;
    private int _lastBarFed = -1;
    private WelfordAcc _acc;

    #endregion

    #region Properties

    #region Calculation

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Type), GroupName = nameof(Strings.Calculation), Description = nameof(Strings.SourceTypeDescription))]
	public InputType Input
	{
		get => _input;
		set
		{
			_input = value;
			RaisePropertyChanged(nameof(Input));
			RecalculateValues();
		}
	}

    #endregion

    #region Filter

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.UseFilter), GroupName = nameof(Strings.Filter), Description = nameof(Strings.UseFilterDescription))]
    public bool UseFilter
    {
        get => _useFilter;
        set
        {
            _useFilter = value;
            RaisePropertyChanged(nameof(UseFilter));
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Filter), GroupName = nameof(Strings.Filter), Description = nameof(Strings.MinVolumeFilterCommonDescription))]
    public decimal FilterValue
    {
        get => _filter;
        set
        {
            _filter = value;
            RaisePropertyChanged(nameof(FilterValue));
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Filter), Description = nameof(Strings.FilterColorDescription))]
    public CrossColor FilterColor
    {
        get => _filterColor.Convert();
        set
        {
            _filterColor = value.Convert();

            RaisePropertyChanged(nameof(FilterColor));
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.UseAlerts), GroupName = nameof(Strings.Filter), Description = nameof(Strings.UseAlertsDescription))]
    public bool UseVolumeAlerts { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.Filter), Description = nameof(Strings.AlertFileDescription))]
    public string AlertVolumeFile { get; set; } = "alert1";

    #endregion

    #region Thresholds

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowThresholdLines), GroupName = nameof(Resources.ThresholdsGroup), Description = nameof(Resources.ShowThresholdLinesDescription), Order = 500)]
	public bool ShowThresholdLines
	{
		get => _showThresholdLines;
		set
		{
			if (_showThresholdLines == value)
				return;

            _showThresholdLines = value;
            UpdateThresholdSeriesVisibility();
            RedrawChart();
        }
    }

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedMinorLevel), GroupName = nameof(Resources.FixedThresholdGroup), Description = nameof(Resources.FixedMinorLevelDescription),	Order = 520)]
	[Range(0, int.MaxValue)]
	[DisplayFormat(DataFormatString = "F0")]
	public decimal FixedMinorLevel
	{
		get => _fixedMinorLevel;
		set
		{
			if (_fixedMinorLevel == value)
				return;

            _fixedMinorLevel = value;
            RecalculateValues();
            RedrawChart();
        }
    }

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.FixedMajorLevel), GroupName = nameof(Resources.FixedThresholdGroup),	Description = nameof(Resources.FixedMajorLevelDescription),	Order = 530)]
	[Range(0, int.MaxValue)]
	[DisplayFormat(DataFormatString = "F0")]
	public decimal FixedMajorLevel
	{
		get => _fixedMajorLevel;
		set
		{
			if (_fixedMajorLevel == value)
				return;

            _fixedMajorLevel = value;
            RecalculateValues();
            RedrawChart();
        }
    }

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.ThresholdSource), GroupName = nameof(Resources.ThresholdsGroup),	Description = nameof(Resources.ThresholdSourceDescription),	Order = 510)]
	public ThresholdSource Thresholds
	{
		get => _thresholds;
		set
		{
			if (_thresholds == value) 
				return;

			_thresholds = value;
			RecalculateValues();
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.SessionWindowMode), GroupName = nameof(Resources.DynamicThresholdGroup), Description = nameof(Resources.SessionWindowModeDescription), Order = 540)]
	public SessionWindowMode SessionMode
	{
		get => _sessionMode;
		set
		{
			if (_sessionMode == value) 
				return;

			_sessionMode = value;
			RecalculateValues();
			RedrawChart();
		}
	}

	[Display( ResourceType = typeof(Resources),	Name = nameof(Resources.RthStart), GroupName = nameof(Resources.DynamicThresholdGroup),	Description = nameof(Resources.RthStartDescription), Order = 550)]
	public TimeSpan RthStart
	{
		get => _rthStart;
		set
		{
            if (_rthStart == value)
                return;

            _rthStart = value;
			RecalculateValues();
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.RthEnd),	GroupName = nameof(Resources.DynamicThresholdGroup), Description = nameof(Resources.RthEndDescription),	Order = 560)]
	public TimeSpan RthEnd
	{
		get => _rthEnd;
		set
		{
            if (_rthEnd == value)
                return;

            _rthEnd = value;
			RecalculateValues();
			RedrawChart();
		}
	}

	[Display( ResourceType = typeof(Resources),	Name = nameof(Resources.SamplesForMeanStd), GroupName = nameof(Resources.DynamicThresholdGroup), Description = nameof(Resources.SamplesForMeanStdDescription),	Order = 570)]
	[Range(1, 10000)]
	public int SamplesForMeanStd
	{
		get => _samplesForMeanStd;
		set
		{
            var newValue = value < 1 ? 1 : value;

            if (_samplesForMeanStd == newValue)
                return;

            _samplesForMeanStd = newValue;
            RecalculateValues();
			RedrawChart();
		}
	}

	[Display(ResourceType = typeof(Resources), Name = nameof(Resources.StdMultiplier), GroupName = nameof(Resources.DynamicThresholdGroup), Description = nameof(Resources.StdMultiplierDescription),	Order = 580)]
	[Range(typeof(decimal), "0", "10")]
	[DisplayFormat(DataFormatString = "F2")]
	public decimal StdMultiplier
	{
		get => _stdMultiplier;
		set
		{
            var newValue = value < 0 ? 0 : value;

            if (_stdMultiplier == newValue)
                return;

            _stdMultiplier = newValue;
            RecalculateValues();
			RedrawChart();
		}
	}

    #endregion


    #region MaximumVolume

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Show), GroupName = nameof(Strings.MaximumVolume), Description = nameof(Strings.MaximumVolumeDescription))]
    public bool ShowMaxVolume
    {
        get => MaxVolSeries.VisualType is not VisualMode.Hide;
        set => MaxVolSeries.VisualType = value ? VisualMode.Line : VisualMode.Hide;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Period), GroupName = nameof(Strings.MaximumVolume), Description = nameof(Strings.MaximumVolumePeriodDescription))]
    [Range(1, 100000)]
    public int HiVolPeriod
    {
        get => HighestVol.Period;
        set => HighestVol.Period = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.MaximumVolume), Description = nameof(Strings.ColorDescription))]
    public CrossColor LineColor
    {
        get => MaxVolSeries.Color;
        set => MaxVolSeries.Color = value;
    }

    #endregion

    #region Volume label

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Show), GroupName = nameof(Strings.VolumeLabel), Description = nameof(Strings.VolumeLabelDescription))]
    public bool ShowVolume { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.VolumeLabel), Description = nameof(Strings.LabelTextColorDescription))]
    public CrossColor FontColor
    {
        get => TextColor.Convert();
        set => TextColor = value.Convert();
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Location), GroupName = nameof(Strings.VolumeLabel), Description = nameof(Strings.LabelLocationDescription))]
    public Location VolLocation { get; set; } = Location.Middle;

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Font), GroupName = nameof(Strings.VolumeLabel), Description = nameof(Strings.FontSettingDescription))]
    public FontSetting Font { get; set; } = new("Arial", 10);

    #endregion

    #region Divergence alert

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = nameof(Strings.ReverseAlert), Description = nameof(Strings.ReverseAlertDescription))]
    public bool UseReverseAlerts { get; set; }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.ReverseAlert), Description = nameof(Strings.AlertFileDescription))]
    public string AlertReverseFile { get; set; } = "alert1";

    #endregion

    #region Drawing

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.DeltaColored), GroupName = nameof(Strings.Drawing), Description = nameof(Strings.DeltaColoredDescription))]
	public bool DeltaColored
	{
		get => _deltaColored;
		set
		{
			_deltaColored = value;
			RaisePropertyChanged(nameof(DeltaColored));
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Positive), GroupName = nameof(Strings.Drawing), Description = nameof(Strings.PositiveValueColorDescription))]
	public CrossColor PosColor
	{
		get => _posColor.Convert();
		set
		{
			_posColor = value.Convert();
			_positive.Color = value;
			RaisePropertyChanged(nameof(PosColor));
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Negative), GroupName = nameof(Strings.Drawing), Description = nameof(Strings.NegativeValueColorDescription))]
	public CrossColor NegColor
	{
		get => _negColor.Convert();
		set
		{
			_negColor = value.Convert();
			_negative.Color = value;
			RaisePropertyChanged(nameof(NegColor));
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Neutral), GroupName = nameof(Strings.Drawing), Description = nameof(Strings.NeutralColorDescription))]
	public CrossColor NeutralColor
	{
		get => _neutralColor.Convert();
		set
		{
			_neutralColor = value.Convert();
			_neutral.Color = value;
			RaisePropertyChanged(nameof(NeutralColor));
			RecalculateValues();
		}
	}

	#endregion

	#endregion

    #region ctor

    public Volume()
		: base(true)
	{
		EnableCustomDrawing = true;
		SubscribeToDrawingEvents(DrawingLayouts.Final);

		Panel = IndicatorDataProvider.NewPanel;

		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

		MaxVolSeries = (ValueDataSeries)HighestVol.DataSeries[0];
		MaxVolSeries.IsHidden = true;
		MaxVolSeries.VisualType = VisualMode.Hide;
		MaxVolSeries.UseMinimizedModeIfEnabled = true;
		MaxVolSeries.IgnoredByAlerts = true;
		DataSeries[0] = _renderSeries;
		DataSeries.Add(MaxVolSeries);
		DataSeries[1].IgnoredByAlerts = true;

		//Legacy templates
		DataSeries.Add(_positive);
		DataSeries.Add(_negative);
		DataSeries.Add(_neutral);
		_positive.PropertyChanged += PositiveChanged;
		_negative.PropertyChanged += NegativeChanged;
		_neutral.PropertyChanged += NeutralChanged;

        // Threshold series (Volume panel)
        DataSeries.Add(_thrMinor);
        DataSeries.Add(_thrMajor);

        UpdateThresholdSeriesVisibility();
        SetupThresholdLineStyle();
    }

    #endregion

    protected override void OnApplyDefaultColors()
    {
	    if (ChartInfo != null)
	    {
		    PosColor = ChartInfo.ColorsStore.UpCandleColor.Convert();
		    NegColor = ChartInfo.ColorsStore.DownCandleColor.Convert();
		    NeutralColor = ChartInfo.ColorsStore.DojiBarPen.Color.Convert();
	    }
    }

    #region Public methods

    public override string ToString()
	{
		return "Volume";
	}

    #endregion

    #region Protected methods

    protected override void OnInitialize()
    {
		_positive.VisualType = VisualMode.Hide;
		_negative.VisualType = VisualMode.Hide;
		_neutral.VisualType = VisualMode.Hide;
    }

    protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (!ShowVolume || ChartInfo.ChartVisualMode != ChartVisualModes.Clusters || Panel == IndicatorDataProvider.CandlesPanel)
			return;

		var minWidth = GetMinWidth(context, FirstVisibleBarNumber, LastVisibleBarNumber);
		var barWidth = ChartInfo.GetXByBar(1) - ChartInfo.GetXByBar(0);

		if (minWidth > barWidth)
			return;

		var strHeight = context.MeasureString("0", Font.RenderObject).Height;

		var y = VolLocation switch
		{
			Location.Up => Container.Region.Y,
			Location.Down => Container.Region.Bottom - strHeight,
			_ => Container.Region.Y + (Container.Region.Bottom - Container.Region.Y) / 2
		};

		for (var i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
		{
			var value = _renderSeries[i];
			var renderText = ChartInfo.TryGetMinimizedVolumeString(value);

			var strRect = new Rectangle(ChartInfo.GetXByBar(i),
				y,
				barWidth,
				strHeight);
			context.DrawString(renderText, Font.RenderObject, TextColor, strRect, Format);
		}
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		var candle = GetCandle(bar);

        var val = GetInputValue(candle);
        _renderSeries[bar] = val;

        if (bar == 0)
        {
            ResetDynamicState();
        }
        else if (Thresholds == ThresholdSource.DynamicWelford && IsSessionStart(bar))
        {
            ResetDynamicState();
            CutThresholdsAt(bar - 1);
        }

		if (bar == CurrentBar - 1)
		{
			if (UseVolumeAlerts && _lastVolumeAlert != bar && val >= _filter && _filter != 0)
			{
				AddAlert(AlertVolumeFile, $"{Resources.Candle} {GetInputLabel()}: {val}");
				_lastVolumeAlert = bar;
			}

            if (UseReverseAlerts && _lastReverseAlert != bar)
            {
                if ((candle.Delta < 0 && candle.Close > candle.Open) || (candle.Delta > 0 && candle.Close < candle.Open))
                {
					AddAlert(AlertReverseFile, $"{Resources.Candle} {GetInputLabel()}: {val} {Resources.ReverseAlert}");
                    _lastReverseAlert = bar;
                }
            }
        }

        var inside = InSession(bar);
        _dynReady = _acc.Count >= _samplesForMeanStd;
        _dynMinor = 0m;
        _dynMajor = 0m;

        if (ShowThresholdLines)
        {
            if (Thresholds == ThresholdSource.Fixed)
            {
                _thrMinor[bar] = _fixedMinorLevel;
                _thrMajor[bar] = _fixedMajorLevel;
            }
            else if (Thresholds == ThresholdSource.DynamicWelford && inside && _dynReady)
            {
                var mean = _acc.Mean;
                var std = _acc.Std();

                _dynMinor = mean;
                _dynMajor = mean + _stdMultiplier * std;

                _thrMinor[bar] = _dynMinor;
                _thrMajor[bar] = _dynMajor;
            }
            else
            {
                CutThresholdsAt(bar - 1);
            }
        }
        else
        {
            CutThresholdsAt(bar - 1);
        }

        var barToFeed = bar - 1;

        if (Thresholds == ThresholdSource.DynamicWelford && barToFeed >= 0 && _lastBarFed != barToFeed)
        {
            var prevCandle = GetCandle(barToFeed);

            if (InSession(barToFeed))
            {
                var prevVal = GetInputValue(prevCandle);

                if (prevVal >= 0)
                    _acc.Add(prevVal);
            }

            _lastBarFed = barToFeed;
        }

        HighestVol.Calculate(bar, val);

		if (_useFilter && val > _filter)
		{
			_renderSeries.Colors[bar] = _filterColor;
			return;
		}

		if (_deltaColored)
		{
			if (candle.Delta > 0)
				_renderSeries.Colors[bar] = _posColor;
			else if (candle.Delta < 0)
				_renderSeries.Colors[bar] = _negColor;
			else
				_renderSeries.Colors[bar] = _neutralColor;
		}
		else
		{
			if (candle.Close > candle.Open)
				_renderSeries.Colors[bar] = _posColor;
			else if (candle.Close < candle.Open)
				_renderSeries.Colors[bar] = _negColor;
			else
				_renderSeries.Colors[bar] = _neutralColor;
		}
	}

    #endregion

    #region Private methods

	private int GetMinWidth(RenderContext context, int startBar, int endBar)
	{
		var maxLength = 0;

		for (var i = startBar; i <= endBar; i++)
		{
			var value = _renderSeries[i];
			var length = $"{value:0.#####}".Length;

			if (length > maxLength)
				maxLength = length;
		}

		var sampleStr = "";

		for (var i = 0; i < maxLength; i++)
			sampleStr += '0';

		return context.MeasureString(sampleStr, Font.RenderObject).Width;
	}

	private void NeutralChanged(object sender, PropertyChangedEventArgs e)
	{
		_neutralColor = _neutral.Color.Convert();
	}

	private void NegativeChanged(object sender, PropertyChangedEventArgs e)
	{
		_negColor = _negative.Color.Convert();
	}

	private void PositiveChanged(object sender, PropertyChangedEventArgs e)
	{
		_posColor = _positive.Color.Convert();
	}

    private void UpdateThresholdSeriesVisibility()
    {
        var vis = _showThresholdLines ? VisualMode.Line : VisualMode.Hide;
        _thrMajor.VisualType = vis;
        _thrMinor.VisualType = vis;
    }

    private void CutThresholdsAt(int bar)
    {
        var b = Math.Max(0, bar);
        _thrMajor.SetPointOfEndLine(b);
        _thrMinor.SetPointOfEndLine(b);
    }

    private void SetupThresholdLineStyle()
    {
        // Major: solid
        _thrMajor.Color = CrossColor.FromArgb(255, 169, 169, 169);
        _thrMajor.Width = 1;
        _thrMajor.LineDashStyle = LineDashStyle.Solid;

        // Minor: dotted
        _thrMinor.Color = CrossColor.FromArgb(255, 105, 105, 105);
        _thrMinor.Width = 1;
        _thrMinor.LineDashStyle = LineDashStyle.Dot;
    }

    private void ResetDynamicState()
    {
        _acc.Reset();
        _dynReady = false;
        _dynMinor = 0m;
        _dynMajor = 0m;
        _lastBarFed = -1;
    }

    // Returns true if bar timestamp (adjusted to instrument TZ) is inside the session window
    private bool InSession(int bar)
    {
        if (_sessionMode == SessionWindowMode.Full24h)
            return true;

        var time = GetCandle(bar).Time.TimeOfDay;

        if (_rthStart <= _rthEnd)
            return time >= _rthStart && time < _rthEnd;

        return time >= _rthStart || time < _rthEnd;
    }

    // Detect session start to reset Welford (daily, time-of-day anchored)
    private bool IsSessionStart(int bar)
    {
        if (bar <= 0)
            return false;

        if (_sessionMode == SessionWindowMode.Full24h)
            return IsNewSession(bar);

        return !InSession(bar - 1) && InSession(bar);
    }

    private decimal GetInputValue(IndicatorCandle candle)
    {
        return Input switch
        {
            InputType.Ticks => candle.Ticks,
            InputType.Asks => candle.Ask,
            InputType.Bids => candle.Bid,
            _ => candle.Volume
        };
    }

    private string GetInputLabel()
    {
        return Input switch
        {
            InputType.Ticks => Strings.Ticks,
            InputType.Asks => Strings.Ask,
            InputType.Bids => Strings.Bid,
            _ => Strings.Volume
        };
    }

    #endregion
}