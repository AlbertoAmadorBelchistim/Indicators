namespace ATAS.Indicators.Technical;

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using ATAS.Indicators.Drawing;

using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering.Settings;

using Color = System.Drawing.Color;

[DisplayName("Woodies CCI")]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.WoodiesCCIDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602565")]
public class WoodiesCCI : Indicator
{
	#region Fields

  	private int _trendCciPeriod = 14;
 	private int _entryCciPeriod = 6;

	private readonly ValueDataSeries _cciSeries = new("CciSeries", "CCI")
	{
		VisualType = VisualMode.Histogram, 
		ShowCurrentValue = false, 
		Width = 2
	};
	
	private readonly ValueDataSeries _entryCci = new("Entry CCI");

	private readonly ValueDataSeries _lsmaSeries = new("LsmaSeries", "LSMA")
	{
		VisualType = VisualMode.Block, 
		ShowCurrentValue = false, 
		ScaleIt = false, 
		Width = 2, 
		IgnoredByAlerts = true,
		ShowTooltip = false
	};
	
	private readonly ValueDataSeries _trendCci = new("Trend CCI");

	private LineSeries _line100 = new("Line100", "100")
	{
		Color = Color.Gray.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		Value = 100,
		Width = 1,
		IsHidden = true,
		DescriptionKey = nameof(Strings.OverboughtLimitDescription)
	};

	private LineSeries _line200 = new("Line200", "200")
	{
		Color = Color.Gray.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		Value = 200,
		Width = 1,
		IsHidden = true,
        DescriptionKey = nameof(Strings.OverboughtLimitDescription)
    	};

	private LineSeries _line300 = new("Line300", "300")
	{
		Color = Color.Gray.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		Value = 300,
		Width = 1,
		IsHidden = true,
		UseScale = true,
        DescriptionKey = nameof(Strings.OverboughtLimitDescription)
    	};

	private LineSeries _lineM100 = new("LineM100", "-100")
	{
		Color = Color.Gray.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		Value = -100,
		Width = 1,
		IsHidden = true,
        DescriptionKey = nameof(Strings.OversoldLimitDescription)
    	};

	private LineSeries _lineM200 = new("LineM200", "-200")
	{
		Color = Color.Gray.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		Value = -200,
		Width = 1,
		IsHidden = true,
        DescriptionKey = nameof(Strings.OversoldLimitDescription)
    	};

	private LineSeries _lineM300 = new("LineM300", "-300")
	{
		Color = Color.Gray.Convert(),
		LineDashStyle = LineDashStyle.Dash,
		Value = -300,
		Width = 1,
		UseScale = true,
		IsHidden = true,
        DescriptionKey = nameof(Strings.OversoldLimitDescription)
    	};

	private bool _drawLines = true;
	private int _lsmaPeriod = 25;
	private int _trendPeriod = 5;

	private int _trendUp, _trendDown;
	private Color _trendUpColor = DefaultColors.Blue;
	private Color _trendDownColor = DefaultColors.Maroon;
	private	Color _noTrendColor = DefaultColors.Gray;
	private Color _timeBarColor = DefaultColors.Yellow;
	private Color _positiveLsmaColor = DefaultColors.Green;
	private Color _negativeLsmaColor = DefaultColors.Red;

	#endregion

	#region Properties

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = "LsmaPeriod", GroupName = nameof(Strings.Settings), Description = nameof(Strings.SMAPeriodDescription))]
	[Range(1, 10000)]
	public int LSMAPeriod
	{
		get => _lsmaPeriod;
		set
		{
			_lsmaPeriod = value;
			RecalculateValues();
		}
	}

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = "TrendPeriod", GroupName = nameof(Strings.Settings), Description = nameof(Strings.PeriodDescription))]
	[Range(1, 10000)]
	public int TrendPeriod
	{
		get => _trendPeriod;
		set
		{
			_trendPeriod = value;
			RecalculateValues();
		}
	}

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = "TrendCciPeriod", GroupName = nameof(Strings.Settings), Description = nameof(Strings.PeriodDescription))]
	[Range(1, 10000)]
	public int TrendCCIPeriod
	{
		get => _trendCciPeriod;
		set
		{
			_trendCciPeriod = value;
			RecalculateValues();
		}
	}

	[Parameter]
	[Display(ResourceType = typeof(Strings), Name = "EntryCciPeriod", GroupName = nameof(Strings.Settings), Description = nameof(Strings.PeriodDescription))]
	[Range(1, 10000)]
	public int EntryCCIPeriod
	{
		get => _entryCciPeriod;
		set
		{
			_entryCciPeriod = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Show), GroupName = nameof(Strings.Line), Description = nameof(Strings.DrawLinesDescription))]
	public bool DrawLines
	{
		get => _drawLines;
		set
		{
			_drawLines = value;

			if (value)
			{
				if (LineSeries.Any())
					return;

				LineSeries.Add(_line100);
				LineSeries.Add(_line200);
				LineSeries.Add(_line300);
				LineSeries.Add(_lineM100);
				LineSeries.Add(_lineM200);
				LineSeries.Add(_lineM300);
			}
			else
			{
				LineSeries.Clear();
			}

			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.p300), GroupName = nameof(Strings.Line), Description = nameof(Strings.OverboughtLimitDescription))]
	public LineSeries Line300
	{
		get => _line300;
		set => _line300 = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.p200), GroupName = nameof(Strings.Line), Description = nameof(Strings.OverboughtLimitDescription))]
	public LineSeries Line200
	{
		get => _line200;
		set => _line200 = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.p100), GroupName = nameof(Strings.Line), Description = nameof(Strings.OverboughtLimitDescription))]
	public LineSeries Line100
	{
		get => _line100;
		set => _line100 = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.m100), GroupName = nameof(Strings.Line), Description = nameof(Strings.OversoldLimitDescription))]
	public LineSeries LineM100
	{
		get => _lineM100;
		set => _lineM100 = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.m200), GroupName = nameof(Strings.Line), Description = nameof(Strings.OversoldLimitDescription))]
	public LineSeries LineM200
	{
		get => _lineM200;
		set => _lineM200 = value;
	}
	
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.m300), GroupName = nameof(Strings.Line), Description = nameof(Strings.OversoldLimitDescription))]
	public LineSeries LineM300
	{
		get => _lineM300;
		set => _lineM300 = value;
	}

    [Display(ResourceType = typeof(Strings), Name = "CciTrendUp", GroupName = nameof(Strings.Colors), Description = nameof(Strings.BullishColorDescription))]
    public Color TrendUpColor
    {
        get => _trendUpColor;
        set
        {
            _trendUpColor = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = "CciTrendDown", GroupName = nameof(Strings.Colors), Description = nameof(Strings.BearishColorDescription))]
    public Color TrendDownColor
    {
        get => _trendDownColor;
        set
        {
            _trendDownColor = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = "NoTrend", GroupName = nameof(Strings.Colors), Description = nameof(Strings.NeutralColorDescription))]
    public Color NoTrendColor
    {
        get => _noTrendColor;
        set
        {
            _noTrendColor = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = "TimeBar", GroupName = nameof(Strings.Colors), Description = nameof(Strings.NewTrendColorDescription))]
    public Color TimeBarColor
    {
        get => _timeBarColor;
        set
        {
            _timeBarColor = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = "NegativeLsma", GroupName = nameof(Strings.Colors), Description = nameof(Strings.NegativeValueColorDescription))]
    public Color NegativeLsmaColor
    {
        get => _negativeLsmaColor;
        set
        {
            _negativeLsmaColor = value;
            RecalculateValues();
        }
    }

    [Display(ResourceType = typeof(Strings), Name = "PositiveLsma", GroupName = nameof(Strings.Colors), Description = nameof(Strings.PositiveValueColorDescription))]
    public Color PositiveLsmaColor
    {
        get => _positiveLsmaColor;
        set
        {
            _positiveLsmaColor = value;
            RecalculateValues();
        }
    }

    #endregion

    #region ctor

    public WoodiesCCI() : base(true)
	{
        Panel = IndicatorDataProvider.NewPanel;
        DenyToChangePanel = true;

        	var zeroLineDataSeries = (ValueDataSeries)DataSeries[0];
		zeroLineDataSeries.ShowCurrentValue = false;
		zeroLineDataSeries.Name = "Zero Line";
		zeroLineDataSeries.Color = Color.Gray.Convert();
		zeroLineDataSeries.VisualType = VisualMode.Hide;
		zeroLineDataSeries.IgnoredByAlerts = true;
		zeroLineDataSeries.DescriptionKey = Strings.ZeroLineDescription;


        DataSeries.Add(_cciSeries);
		DataSeries.Add(_lsmaSeries);

        LineSeries.Add(_line100);
		LineSeries.Add(_line200);
		LineSeries.Add(_line300);
		LineSeries.Add(_lineM100);
		LineSeries.Add(_lineM200);
		LineSeries.Add(_lineM300);

	DataSeries.Add(_trendUpSeries);
	DataSeries.Add(_trendDownSeries);

	DataSeries.Add(_trendCci);
	DataSeries.Add(_entryCci);

	_trendCci.Color = DefaultColors.Purple.Convert();
	_trendCci.Id = "TrendCciDataSeries";
	_trendCci.Name = "Trend CCI";
	_trendCci.Width = 2;
	_trendCci.IgnoredByAlerts = true;

	_entryCci.Id = "EntryCciDataSeries";
	_entryCci.Name = "Entry CCI";
	_entryCci.IgnoredByAlerts = true;
	_entryCci.Color = DefaultColors.Orange.Convert();

	_trendUpSeries.Color = DefaultColors.Blue.Convert();
	_trendDownSeries.Color = DefaultColors.Red.Convert();
	}

	#endregion

	#region Protected methods

 	private readonly ValueDataSeries _trendUpSeries = new("TrendUpSeries");
	private readonly ValueDataSeries _trendDownSeries = new("TrendDownSeries");
	
	protected override void OnCalculate(int bar, decimal value)
	{
		if (bar == 0)
    			return;

		// Ensure there are enough bars to calculate both Trend and Entry CCI
		if (bar < Math.Max(TrendCCIPeriod, EntryCCIPeriod))
    			return;

		// Ensure there are enough bars to calculate LSMA
		if (bar < LSMAPeriod + 2)
    			return;
		// Calculate Trend and Entry CCI values manually
		var trendCci = CalculateCCI(bar, TrendCCIPeriod);
		var entryCci = CalculateCCI(bar, EntryCCIPeriod);

		// Store them in their respective series for visualization
		_trendCci[bar] = trendCci;
		_entryCci[bar] = entryCci;

  		// Use Trend CCI value as the main CCI series to color the histogram
		_cciSeries[bar] = _trendCci[bar];

		// Get previous bar's trend CCI value to detect trend changes
		var prevTrendCci = CalculateCCI(bar - 1, TrendCCIPeriod);

		// Retrieve previous trend counters
		var up = (int)_trendUpSeries[bar - 1];
		var down = (int)_trendDownSeries[bar - 1];

		// Detect and color upward trend based on trend CCI behavior
		if (trendCci > 0)
		{
    			if (prevTrendCci < 0)
        			up = 0;

    			up++;
    			down = 0;

    			if (up < TrendPeriod)
        			_cciSeries.Colors[bar] = _noTrendColor;
    			else if (up == TrendPeriod)
        			_cciSeries.Colors[bar] = _timeBarColor;
    			else
        			_cciSeries.Colors[bar] = _trendUpColor;
		}
  		// Detect and color downward trend
		else if (trendCci < 0)
		{
    			if (prevTrendCci > 0)
        			down = 0;

    			down++;
    			up = 0;

    			if (down < TrendPeriod)
        			_cciSeries.Colors[bar] = _noTrendColor;
    			else if (down == TrendPeriod)
        			_cciSeries.Colors[bar] = _timeBarColor;
    			else
        			_cciSeries.Colors[bar] = _trendDownColor;
		}
		
  		// Save trend counters to display them or use later
		_trendUpSeries[bar] = up;
		_trendDownSeries[bar] = down;

		// === LSMA calculation ===
		// Calculate weighted sum of prices
		decimal summ = 0;

		var lengthvar = (decimal)((LSMAPeriod + 1) / 3.0);

		for (var i = LSMAPeriod; i >= 1; i--)
			summ += (i - lengthvar) * GetCandle(bar - LSMAPeriod + i).Close;

		var wt = summ * 6 / (LSMAPeriod * (LSMAPeriod + 1));

  		// Store LSMA value (wt)
		_lsmaSeries[bar] = 0.00001m;

		// Color LSMA based on whether it's above or below price
		_lsmaSeries.Colors[bar] = wt > GetCandle(bar).Close
					? _negativeLsmaColor
					: _positiveLsmaColor;
	}

 	// --- Manual CCI calculation to fully support historical bars ---
    	private decimal CalculateCCI(int bar, int period)
    	{
        	if (bar < period)
            		return 0;

        	decimal typicalPriceSum = 0;
        	for (int i = bar - period + 1; i <= bar; i++)
        	{
            		var candle = GetCandle(i);
            		typicalPriceSum += (candle.High + candle.Low + candle.Close) / 3m;
        	}

        	decimal typicalPriceAvg = typicalPriceSum / period;

        	decimal meanDeviation = 0;
        	for (int i = bar - period + 1; i <= bar; i++)
        	{
            		var candle = GetCandle(i);
            		var tp = (candle.High + candle.Low + candle.Close) / 3m;
            		meanDeviation += Math.Abs(tp - typicalPriceAvg);
        	}

        	meanDeviation /= period;

        	var currentTP = (GetCandle(bar).High + GetCandle(bar).Low + GetCandle(bar).Close) / 3m;

        	if (meanDeviation == 0)
            		return 0;

        	return (currentTP - typicalPriceAvg) / (0.015m * meanDeviation);
    	}
	
	#endregion
 }
