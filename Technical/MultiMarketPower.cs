namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using ATAS.Indicators.Drawing;

using OFT.Attributes;
using OFT.Attributes.Editors;
using OFT.Localization;
using Utils.Common;

[Category(IndicatorCategories.VolumeOrderFlow)]
[DisplayName("CVD pro(multi) / Multi Market Powers")]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.MultiMarketPowerDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602434")]
public class MultiMarketPower : Indicator
{
	#region Nested types
	public enum ViewMode
	{
		Filters,
		SmartMoneySpread
	}

	public enum SessionMode
	{
		DefaultSession,
		CustomSession
	}

	#endregion

	#region Fields

	private readonly ValueDataSeries _filter1Series = new("Filter1Series", "Filter1")
	{
		Color = CrossColor.FromArgb(255, 135, 206, 235),
		IsHidden = true,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

	private readonly ValueDataSeries _filter2Series = new("Filter2Series", "Filter2")
	{
		Color = DefaultColors.Red.Convert(),
		IsHidden = true,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

	private readonly ValueDataSeries _filter3Series = new("Filter3Series", "Filter3")
	{
		Color = DefaultColors.Green.Convert(),
		IsHidden = true,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

	private readonly ValueDataSeries _filter4Series = new("Filter4Series", "Filter4")
	{
		Color = CrossColor.FromArgb(255, 128, 128, 128),
		Width = 2,
		IsHidden = true,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

	private readonly ValueDataSeries _filter5Series = new("Filter5Series", "Filter5")
	{
		Color = CrossColor.FromArgb(255, 205, 92, 92),
		Width = 2,
		IsHidden = true,
		ShowZeroValue = false,
		UseMinimizedModeIfEnabled = true
	};

	private readonly ValueDataSeries _spreadSeries = new("SpreadSeries", "Smart Money Spread")
	{
		VisualType = VisualMode.Histogram,
		Width = 3,
		UseMinimizedModeIfEnabled = true,
		ShowZeroValue = true // important: keep zero-axis visible for histogram interpretation
	};

	private readonly ValueDataSeries _signalSeries = new("SignalSeries", "Signal (SMA)")
	{
		VisualType = VisualMode.Line,
		Width = 2,
		UseMinimizedModeIfEnabled = true
	};

	private bool _pendingRealtimeReplay;
	private bool _bigTradesIsReceived;
	private bool _cumulativeTrades = true;
	private decimal _delta1;
	private decimal _delta2;
	private decimal _delta3;
	private decimal _delta4;
	private decimal _delta5;
	private int _lastBar = -1;
	private decimal _lastDelta1;
	private decimal _lastDelta2;
	private decimal _lastDelta3;
	private decimal _lastDelta4;
	private decimal _lastDelta5;
	private CumulativeTrade _lastTrade;
	private object _locker = new();
	private decimal _maxVolume1 = 5;
	private decimal _maxVolume2 = 10;
	private decimal _maxVolume3 = 20;
	private decimal _maxVolume4 = 40;
	private decimal _maxVolume5;
	private decimal _minVolume1;
	private decimal _minVolume2 = 6;
	private decimal _minVolume3 = 11;
	private decimal _minVolume4 = 21;
	private decimal _minVolume5 = 41;

	private int _requestId;
	private int _sessionBegin;

	private List<MarketDataArg> _ticks = new();
	private List<CumulativeTrade> _trades = new();

	private bool _useFilter1 = true;
	private bool _useFilter2 = true;
	private bool _useFilter3 = true;
	private bool _useFilter4 = true;
	private bool _useFilter5 = true;

	private ViewMode _viewMode = ViewMode.Filters;

	private SessionMode _sessionMode = SessionMode.DefaultSession;
	private TimeSpan _customSessionStart = new(15, 30, 0);
	private int _sessionsBack = 1;
	private int _currentSessionBegin;

	private bool _use4ColorSystem;
	private bool _showSignalLine = true;
	private int _signalPeriod = 14;

	// Simple (zero-line) colors
	private CrossColor _simplePositiveColor = CrossColors.LimeGreen;
	private CrossColor _simpleNegativeColor = CrossColors.IndianRed;

	// 4-color system
	private CrossColor _colorPosSmaUp = CrossColors.LimeGreen;
	private CrossColor _colorPosSmaDown = CrossColors.Gold;
	private CrossColor _colorNegSmaUp = CrossColors.DodgerBlue;
	private CrossColor _colorNegSmaDown = CrossColors.IndianRed;


	#endregion

	#region Properties

	[Display(Name = "View Mode", GroupName = "Visualization", Order = 1010)]
	public ViewMode Mode
	{
		get => _viewMode;
		set
		{
			if (_viewMode == value)
				return;

			_viewMode = value;
			UpdateVisibility();

			if (_viewMode == ViewMode.SmartMoneySpread)
				RecalculateValues();
			else
				RedrawChart();

			// Ensure the chart refreshes without forcing a full recalculation.
			if (CurrentBar > 0)
				RaiseBarValueChanged(CurrentBar - 1);
		}
	}

	[Display(Name = "Session Mode", GroupName = "Session", Order = 1110)]
	public SessionMode SessionPowerMode
	{
		get => _sessionMode;
		set
		{
			if (_sessionMode == value)
				return;

			_sessionMode = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Custom Session Start", GroupName = "Session", Order = 1120)]
	public TimeSpan CustomSessionStart
	{
		get => _customSessionStart;
		set
		{
			if (_customSessionStart == value)
				return;

			_customSessionStart = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Sessions Back", GroupName = "Session", Order = 1130)]
	[Range(1, 30)]
	public int SessionsBack
	{
		get => _sessionsBack;
		set
		{
			if (_sessionsBack == value)
				return;

			_sessionsBack = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Use 4 Color System", Description = "Uses 4 colors based on spread value and SMA direction", GroupName = "Visualization", Order = 1020)]
	public bool Use4ColorSystem
	{
		get => _use4ColorSystem;
		set
		{
			if (_use4ColorSystem == value)
				return;

			_use4ColorSystem = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Show Signal Line", GroupName = "Visualization", Order = 1030)]
	public bool ShowSignalLine
	{
		get => _showSignalLine;
		set
		{
			if (_showSignalLine == value)
				return;

			_showSignalLine = value;
			UpdateVisibility();
			if (CurrentBar > 0)
				RaiseBarValueChanged(CurrentBar - 1);
		}
	}

	[Display(Name = "Signal Period", GroupName = "Visualization", Order = 1040)]
	[Range(2, 500)]
	public int SignalPeriod
	{
		get => _signalPeriod;
		set
		{
			if (_signalPeriod == value)
				return;

			_signalPeriod = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Simple Positive Color", Description = "Color when spread is above zero (simple mode)", GroupName = "Visualization", Order = 1050)]
	public CrossColor SimplePositiveColor
	{
		get => _simplePositiveColor;
		set
		{
			if (_simplePositiveColor == value)
				return;

			_simplePositiveColor = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Simple Negative Color", Description = "Color when spread is above zero (simple mode)", GroupName = "Visualization", Order = 1060)]
	public CrossColor SimpleNegativeColor
	{
		get => _simpleNegativeColor;
		set
		{
			if (_simpleNegativeColor == value)
				return;

			_simpleNegativeColor = value;
			RecalculateValues();
		}
	}

	[Display(Name = "Color Pos Sma Up", GroupName = "Visualization", Order = 1070)]
	public CrossColor ColorPosSmaUp
	{
		get => _colorPosSmaUp;
		set { if (_colorPosSmaUp != value) { _colorPosSmaUp = value; RecalculateValues(); } }
	}

	[Display(Name = "Color Pos Sma Down", GroupName = "Visualization", Order = 1080)]
	public CrossColor ColorPosSmaDown
	{
		get => _colorPosSmaDown;
		set { if (_colorPosSmaDown != value) { _colorPosSmaDown = value; RecalculateValues(); } }
	}

	[Display(Name = "Color Neg Sma Up", GroupName = "Visualization", Order = 1090)]
	public CrossColor ColorNegSmaUp
	{
		get => _colorNegSmaUp;
		set { if (_colorNegSmaUp != value) { _colorNegSmaUp = value; RecalculateValues(); } }
	}

	[Display(Name = "Color Neg Sma Down", GroupName = "Visualization", Order = 1100)]
	public CrossColor ColorNegSmaDown
	{
		get => _colorNegSmaDown;
		set { if (_colorNegSmaDown != value) { _colorNegSmaDown = value; RecalculateValues(); } }
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CumulativeTrades), GroupName = nameof(Strings.Filters), Description = nameof(Strings.CumulativeTradesModeDescription), Order = 90)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	public bool CumulativeTrades
	{
		get => _cumulativeTrades;
		set
		{
			_cumulativeTrades = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = nameof(Strings.Filter1), Description = nameof(Strings.UseFilterDescription), Order = 100)]
	public bool UseFilter1
	{
		get => _useFilter1;
		set
		{
			_useFilter1 = value;
			_filter1Series.VisualType = value ? VisualMode.Line : VisualMode.Hide;
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimumVolume), GroupName = nameof(Strings.Filter1), Description = nameof(Strings.MinVolumeFilterCommonDescription), Order = 130)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MinVolume1
	{
		get => _minVolume1;
		set
		{
			_minVolume1 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MaximumVolume), GroupName = nameof(Strings.Filter1), Description = nameof(Strings.MaxVolumeFilterCommonDescription), Order = 140)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0.0000001, 100000000)]
	public decimal MaxVolume1
	{
		get => _maxVolume1;
		set
		{
			_maxVolume1 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Filter1), Description = nameof(Strings.LineColorDescription), Order = 150)]
	public CrossColor Color1
	{
		get => _filter1Series.Color;
		set => _filter1Series.Color = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineWidth), GroupName = nameof(Strings.Filter1), Description = nameof(Strings.LineWidthDescription), Order = 160)]
    [Range(1, 100)]
    public int LineWidth1
    {
        get => _filter1Series.Width;
        set => _filter1Series.Width = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = nameof(Strings.Filter2), Description = nameof(Strings.UseFilterDescription), Order = 200)]
	public bool UseFilter2
	{
		get => _useFilter2;
		set
		{
			_useFilter2 = value;
			_filter2Series.VisualType = value ? VisualMode.Line : VisualMode.Hide;
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimumVolume), GroupName = nameof(Strings.Filter2), Description = nameof(Strings.MinVolumeFilterCommonDescription), Order = 230)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MinVolume2
	{
		get => _minVolume2;
		set
		{
			_minVolume2 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MaximumVolume), GroupName = nameof(Strings.Filter2), Description = nameof(Strings.MaxVolumeFilterCommonDescription), Order = 240)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MaxVolume2
	{
		get => _maxVolume2;
		set
		{
			_maxVolume2 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Filter2), Description = nameof(Strings.LineColorDescription), Order = 250)]
	public CrossColor Color2
	{
		get => _filter2Series.Color;
		set => _filter2Series.Color = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineWidth), GroupName = nameof(Strings.Filter2), Description = nameof(Strings.LineWidthDescription), Order = 260)]
    [Range(1, 100)]
    public int LineWidth2
    {
        get => _filter2Series.Width;
        set => _filter2Series.Width = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = nameof(Strings.Filter3), Description = nameof(Strings.UseFilterDescription), Order = 300)]
	public bool UseFilter3
	{
		get => _useFilter3;
		set
		{
			_useFilter3 = value;
			_filter3Series.VisualType = value ? VisualMode.Line : VisualMode.Hide;
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimumVolume), GroupName = nameof(Strings.Filter3), Description = nameof(Strings.MinVolumeFilterCommonDescription), Order = 330)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MinVolume3
	{
		get => _minVolume3;
		set
		{
			_minVolume3 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MaximumVolume), GroupName = nameof(Strings.Filter3), Description = nameof(Strings.MaxVolumeFilterCommonDescription), Order = 340)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MaxVolume3
	{
		get => _maxVolume3;
		set
		{
			_maxVolume3 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Filter3), Description = nameof(Strings.LineColorDescription), Order = 350)]
	public CrossColor Color3
	{
		get => _filter3Series.Color;
		set => _filter3Series.Color = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineWidth), GroupName = nameof(Strings.Filter3), Description = nameof(Strings.LineWidthDescription), Order = 360)]
    [Range(1, 100)]
    public int LineWidth3
    {
        get => _filter3Series.Width;
        set => _filter3Series.Width = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = nameof(Strings.Filter4), Description = nameof(Strings.UseFilterDescription), Order = 400)]
	public bool UseFilter4
	{
		get => _useFilter4;
		set
		{
			_useFilter4 = value;
			_filter4Series.VisualType = value ? VisualMode.Line : VisualMode.Hide;
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimumVolume), GroupName = nameof(Strings.Filter4), Description = nameof(Strings.MinVolumeFilterCommonDescription), Order = 430)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MinVolume4
	{
		get => _minVolume4;
		set
		{
			_minVolume4 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MaximumVolume), GroupName = nameof(Strings.Filter4), Description = nameof(Strings.MaxVolumeFilterCommonDescription), Order = 440)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MaxVolume4
	{
		get => _maxVolume4;
		set
		{
			_maxVolume4 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Filter4), Description = nameof(Strings.LineColorDescription), Order = 450)]
	public CrossColor Color4
	{
		get => _filter4Series.Color;
		set => _filter4Series.Color = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineWidth), GroupName = nameof(Strings.Filter4), Description = nameof(Strings.LineWidthDescription), Order = 460)]
    [Range(1, 100)]
    public int LineWidth4
    {
        get => _filter4Series.Width;
        set => _filter4Series.Width = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = nameof(Strings.Filter5), Description = nameof(Strings.UseFilterDescription), Order = 500)]
	public bool UseFilter5
	{
		get => _useFilter5;
		set
		{
			_useFilter5 = value;
			_filter5Series.VisualType = value ? VisualMode.Line : VisualMode.Hide;
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimumVolume), GroupName = nameof(Strings.Filter5), Description = nameof(Strings.MinVolumeFilterCommonDescription), Order = 530)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MinVolume5
	{
		get => _minVolume5;
		set
		{
			_minVolume5 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MaximumVolume), GroupName = nameof(Strings.Filter5), Description = nameof(Strings.MaxVolumeFilterCommonDescription), Order = 540)]
	[PostValueMode(PostValueModes.Delayed, DelayMilliseconds = 500)]
	[Range(0, 100000000)]
	public decimal MaxVolume5
	{
		get => _maxVolume5;
		set
		{
			_maxVolume5 = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Filter5), Description = nameof(Strings.LineColorDescription), Order = 550)]
	public CrossColor Color5
	{
		get => _filter5Series.Color;
		set => _filter5Series.Color = value;
    }

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.LineWidth), GroupName = nameof(Strings.Filter5), Description = nameof(Strings.LineWidthDescription), Order = 560)]
    [Range(1, 100)]
    public int LineWidth5
    {
        get => _filter5Series.Width;
        set => _filter5Series.Width = value;
    }

    #endregion

    #region ctor

    public MultiMarketPower()
		: base(true)
	{
		Panel = IndicatorDataProvider.NewPanel;
		DenyToChangePanel = true;

		DataSeries[0] = _filter1Series;
		DataSeries.Add(_filter2Series);
		DataSeries.Add(_filter3Series);
		DataSeries.Add(_filter4Series);
		DataSeries.Add(_filter5Series);

		DataSeries.Add(_spreadSeries);
		DataSeries.Add(_signalSeries);

		UpdateVisibility();
	}

	#endregion

	#region Protected methods

	protected override void OnCalculate(int bar, decimal value)
	{
		if (!_bigTradesIsReceived || bar != CurrentBar - 1)
			return;

		DataSeries.ForEach(ds =>
		{
			if (ds is ValueDataSeries vds && vds[bar] is 0)
				vds[bar] = vds[bar - 1];
		});
	}
	
	protected override void OnFinishRecalculate()
	{
		_bigTradesIsReceived = false;

        _ticks.Clear();
		_trades.Clear();
		_sessionBegin = FindSessionBeginBar();
		_currentSessionBegin = _sessionBegin;
		_lastBar = CurrentBar - 1;

		var request = new CumulativeTradesRequest(GetCandle(_sessionBegin).Time);
		_requestId = request.RequestId;
		RequestForCumulativeTrades(request);

		UpdateVisibility();
	}
	
	protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
	{
		if (request.RequestId != _requestId)
			return;

		ClearValues();
		var trades = cumulativeTrades.ToList();
		
		CalculateHistory(trades);

		_bigTradesIsReceived = true;
	}

	protected override void OnNewTrade(MarketDataArg trade)
	{
		if (CumulativeTrades || ChartInfo is null)
			return;

		if (!_bigTradesIsReceived)
		{
			_ticks.Add(trade);
			return;
		}

		var newBar = _lastBar < CurrentBar - 1;

		if (newBar)
			_lastBar = CurrentBar - 1;

		CalculateTick(trade);
	}

	protected override void OnCumulativeTrade(CumulativeTrade trade)
	{
		if (!CumulativeTrades)
			return;

		if (!_bigTradesIsReceived)
		{
			_trades.Add(trade);
			return;
		}

		var newBar = _lastBar < CurrentBar - 1;

		if (newBar)
			_lastBar = CurrentBar - 1;

		CalculateTrade(trade, false, newBar);
	}

	protected override void OnUpdateCumulativeTrade(CumulativeTrade trade)
	{
		if (!CumulativeTrades)
			return;

		if (!_bigTradesIsReceived)
		{
			if (_trades.Count != 0)
				_trades[^1] = trade;
			return;
		}

		var newBar = _lastBar < CurrentBar - 1;

		if (newBar)
			_lastBar = CurrentBar - 1;

		CalculateTrade(trade, true, newBar);
	}

	#endregion

	#region Private methods

	private void ClearValues()
	{
		_bigTradesIsReceived = false;
		DataSeries.ForEach(x => x.Clear());
		_delta1 = _delta2 = _delta3 = _delta4 = _delta5 = 0;
	}

	private void CalculateTrade(CumulativeTrade trade, bool isUpdate, bool newBar)
	{
		if (isUpdate && _lastTrade != null)
		{
			if (_lastTrade.IsEqual(trade))
			{
				var prevBarReset = _lastTrade.Time < GetCandle(CurrentBar - 1).Time && newBar;

				var lastVolume = _lastTrade.Volume * (_lastTrade.Direction == TradeDirection.Buy ? 1 : -1);

				if (_lastTrade.Volume >= _minVolume1 && (_lastTrade.Volume <= _maxVolume1 || _maxVolume1 == 0))
				{
					_delta1 -= lastVolume;

					if (prevBarReset)
						_filter1Series[CurrentBar - 2] -= lastVolume;
				}

				if (_lastTrade.Volume >= _minVolume2 && (_lastTrade.Volume <= _maxVolume2 || _maxVolume2 == 0))
				{
					if (prevBarReset)
						_filter2Series[CurrentBar - 2] -= lastVolume;

					_delta2 -= lastVolume;
				}

				if (_lastTrade.Volume >= _minVolume3 && (_lastTrade.Volume <= _maxVolume3 || _maxVolume3 == 0))
				{
					if (prevBarReset)
						_filter3Series[CurrentBar - 2] -= lastVolume;

					_delta3 -= lastVolume;
				}

				if (_lastTrade.Volume >= _minVolume4 && (_lastTrade.Volume <= _maxVolume4 || _maxVolume4 == 0))
				{
					if (prevBarReset)
						_filter4Series[CurrentBar - 2] -= lastVolume;

					_delta4 -= lastVolume;
				}

				if (_lastTrade.Volume >= _minVolume5 && (_lastTrade.Volume <= _maxVolume5 || _maxVolume5 == 0))
				{
					if (prevBarReset)
						_filter5Series[CurrentBar - 2] -= lastVolume;

					_delta5 -= lastVolume;
				}
			}
		}

		var volume = trade.Volume;
		var deltaVolume = volume * (trade.Direction == TradeDirection.Buy ? 1 : -1);

		if (volume >= _minVolume1 && (volume <= _maxVolume1 || _maxVolume1 == 0))
			_delta1 += deltaVolume;

		if (volume >= _minVolume2 && (volume <= _maxVolume2 || _maxVolume2 == 0))
			_delta2 += deltaVolume;

		if (volume >= _minVolume3 && (volume <= _maxVolume3 || _maxVolume3 == 0))
			_delta3 += deltaVolume;

		if (volume >= _minVolume4 && (volume <= _maxVolume4 || _maxVolume4 == 0))
			_delta4 += deltaVolume;

		if (volume >= _minVolume5 && (volume <= _maxVolume5 || _maxVolume5 == 0))
			_delta5 += deltaVolume;

		_filter1Series[CurrentBar - 1] = _delta1;
		_filter2Series[CurrentBar - 1] = _delta2;
		_filter3Series[CurrentBar - 1] = _delta3;
		_filter4Series[CurrentBar - 1] = _delta4;
		_filter5Series[CurrentBar - 1] = _delta5;

		var smartMoney = _delta4 + _delta5;
		var dumbMoney = _delta1 + _delta2;
		var spread = smartMoney - dumbMoney;

		_spreadSeries[CurrentBar - 1] = spread;
		UpdateSpreadVisuals(CurrentBar - 1);

		RaiseBarValueChanged(CurrentBar - 1);
		_lastTrade = trade.MemberwiseClone();
	}

	private void CalculateHistory(List<CumulativeTrade> trades)
	{
		try
		{
			if(trades.Count is 0)
				return;
			
			var searchIdx = 0;

            if (CumulativeTrades)
			{
				trades = trades.OrderBy(t => t.Time).ToList();
                
				for (var i = _sessionBegin; i <= CurrentBar - 1; i++)
					CalculateBarTrades(trades, i, ref searchIdx);

				foreach (var trade in _trades)
					CalculateTrade(trade, false, false);
			}
			else
			{
				var ticks = trades
					.SelectMany(x => x.Ticks)
					.OrderBy(t=>t.Time)
					.ToList();

				for (var i = _sessionBegin; i <= CurrentBar - 1; i++)
					CalculateBarTicks(ticks, i, ref searchIdx);

				foreach (var tick in _ticks)
					CalculateTick(tick);
			}

			RedrawChart();
		}
		catch (NullReferenceException)
		{
			//on reset exception ignored
		}
	}

	private void CalculateBarTicks(List<MarketDataArg> trades, int i, ref int searchIdx)
	{
		if (IsSessionStart(i))
			ResetSessionState(i);

		var candle = GetCandle(i);

		var candleTrades = new List<MarketDataArg>();

		for (var bar = searchIdx; bar < trades.Count; bar++)
		{
			var trade = trades[bar];
			searchIdx = bar;
            
			if (trade.Direction is TradeDirection.Between)
				continue;

			if (trade.Time > candle.LastTime)
				break;

			if (trade.Time < candle.Time)
				continue;

			candleTrades.Add(trade);
		}

        foreach (var tick in candleTrades)
		{
			var deltaVolume = tick.Volume * (tick.Direction is TradeDirection.Buy ? 1 : -1);

			if (IsFiltered(MinVolume1, MaxVolume1, tick.Volume))
				_delta1 += deltaVolume;

			if (IsFiltered(MinVolume2, MaxVolume2, tick.Volume))
				_delta2 += deltaVolume;

			if (IsFiltered(MinVolume3, MaxVolume3, tick.Volume))
				_delta3 += deltaVolume;

			if (IsFiltered(MinVolume4, MaxVolume4, tick.Volume))
				_delta4 += deltaVolume;

			if (IsFiltered(MinVolume5, MaxVolume5, tick.Volume))
				_delta5 += deltaVolume;
		}

		_filter1Series[i] = _delta1;
		_filter2Series[i] = _delta2;
		_filter3Series[i] = _delta3;
		_filter4Series[i] = _delta4;
		_filter5Series[i] = _delta5;

		// Smart Money (4+5) - Dumb Money (1+2)
		var smartMoney = _delta4 + _delta5;
		var dumbMoney = _delta1 + _delta2;
		var spread = smartMoney - dumbMoney;

		_spreadSeries[i] = spread;
		UpdateSpreadVisuals(i);

		RaiseBarValueChanged(i);
	}

	private void CalculateTick(MarketDataArg tick)
	{
		var bar = CurrentBar - 1;

		if (IsSessionStart(bar))
			ResetSessionState(bar);

		var deltaVolume = tick.Volume * (tick.Direction is TradeDirection.Buy ? 1 : -1);

		if (IsFiltered(MinVolume1, MaxVolume1, tick.Volume))
			_delta1 += deltaVolume;

		if (IsFiltered(MinVolume2, MaxVolume2, tick.Volume))
			_delta2 += deltaVolume;

		if (IsFiltered(MinVolume3, MaxVolume3, tick.Volume))
			_delta3 += deltaVolume;

		if (IsFiltered(MinVolume4, MaxVolume4, tick.Volume))
			_delta4 += deltaVolume;

		if (IsFiltered(MinVolume5, MaxVolume5, tick.Volume))
			_delta5 += deltaVolume;

		_filter1Series[^1] = _delta1;
		_filter2Series[^1] = _delta2;
		_filter3Series[^1] = _delta3;
		_filter4Series[^1] = _delta4;
		_filter5Series[^1] = _delta5;

		var smartMoney = _delta4 + _delta5;
		var dumbMoney = _delta1 + _delta2;
		var spread = smartMoney - dumbMoney;

		_spreadSeries[CurrentBar - 1] = spread;
		UpdateSpreadVisuals(CurrentBar - 1);
	}

	private bool IsFiltered(decimal minFilter, decimal maxFilter, decimal volume)
	{
		return volume >= minFilter && (volume <= maxFilter || maxFilter == 0);
	}

	private void CalculateBarTrades(List<CumulativeTrade> trades, int bar, ref int searchIdx, bool realTime = false, bool newBar = false)
	{
		if (IsSessionStart(bar))
			ResetSessionState(bar);

		if (CumulativeTrades && realTime && !newBar)
		{
			_delta1 -= _lastDelta1;
			_delta2 -= _lastDelta2;
			_delta3 -= _lastDelta3;
			_delta4 -= _lastDelta4;
			_delta5 -= _lastDelta5;
		}

		var candle = GetCandle(bar);

		var candleTrades = new List<CumulativeTrade>();

		for (var i = searchIdx; i < trades.Count; i++)
		{
			var trade = trades[i];

			if (trade.Direction is TradeDirection.Between)
				continue;

			if (trade.Time > candle.LastTime)
			{
				searchIdx = i;
				break;
			}

			if (trade.Time < candle.Time)
				continue;

			candleTrades.Add(trade);
		}

        _lastDelta1 = candleTrades
			.Where(x => x.Volume >= _minVolume1 && (x.Volume <= _maxVolume1 || _maxVolume1 == 0))
			.Sum(x => x.Volume * (x.Direction == TradeDirection.Buy ? 1 : -1));

		_delta1 += _lastDelta1;

		_filter1Series[bar] = _delta1;

		_lastDelta2 = candleTrades
			.Where(x => x.Volume >= _minVolume2 && (x.Volume <= _maxVolume2 || _maxVolume2 == 0))
			.Sum(x => x.Volume * (x.Direction == TradeDirection.Buy ? 1 : -1));

		_delta2 += _lastDelta2;

		_filter2Series[bar] = _delta2;

		_lastDelta3 = candleTrades
			.Where(x => x.Volume >= _minVolume3 && (x.Volume <= _maxVolume3 || _maxVolume3 == 0))
			.Sum(x => x.Volume * (x.Direction == TradeDirection.Buy ? 1 : -1));

		_delta3 += _lastDelta3;

		_filter3Series[bar] = _delta3;

		_lastDelta4 = candleTrades
			.Where(x => x.Volume >= _minVolume4 && (x.Volume <= _maxVolume4 || _maxVolume4 == 0))
			.Sum(x => x.Volume * (x.Direction == TradeDirection.Buy ? 1 : -1));

		_delta4 += _lastDelta4;

		_filter4Series[bar] = _delta4;

		_lastDelta5 = candleTrades
			.Where(x => x.Volume >= _minVolume5 && (x.Volume <= _maxVolume5 || _maxVolume5 == 0))
			.Sum(x => x.Volume * (x.Direction == TradeDirection.Buy ? 1 : -1));

		_delta5 += _lastDelta5;

		_filter5Series[bar] = _delta5;

		var smartMoney = _delta4 + _delta5;
		var dumbMoney = _delta1 + _delta2;
		var spread = smartMoney - dumbMoney;

		_spreadSeries[bar] = spread;
		UpdateSpreadVisuals(bar);

		RaiseBarValueChanged(bar);
		_lastBar = bar;
	}

	private void UpdateVisibility()
	{
		var showSpread = _viewMode == ViewMode.SmartMoneySpread;

		_spreadSeries.VisualType = showSpread ? VisualMode.Histogram : VisualMode.Hide;
		_signalSeries.VisualType = (showSpread && _showSignalLine) ? VisualMode.Line : VisualMode.Hide;

		var filterVisual = showSpread ? VisualMode.Hide : VisualMode.Line;

		_filter1Series.VisualType = filterVisual;
		_filter2Series.VisualType = filterVisual;
		_filter3Series.VisualType = filterVisual;
		_filter4Series.VisualType = filterVisual;
		_filter5Series.VisualType = filterVisual;
	}

    private bool IsSessionStart(int bar)
    {
        if (bar <= 0)
            return true;

        if (_sessionMode == SessionMode.DefaultSession)
            return IsNewSession(bar);

        var candle = GetCandle(bar);
        var prev = GetCandle(bar - 1);

        var boundary = _customSessionStart;

        // Interpret boundary in the same time basis as candle.Time.
        var wasBefore = prev.Time.TimeOfDay < boundary;
        var isAfterOrEqual = candle.Time.TimeOfDay >= boundary;

        return wasBefore && isAfterOrEqual;
    }

    private int FindSessionBeginBar()
	{
		var lastBar = Math.Max(0, CurrentBar - 1);
		var begin = lastBar;

		var sessionsToFind = Math.Max(1, _sessionsBack);
		var found = 0;

		for (var i = lastBar; i >= 0; i--)
		{
			if (!IsSessionStart(i))
				continue;

			found++;

			if (found >= sessionsToFind)
			{
				begin = i;
				break;
			}
		}

		return begin;
	}

	private void ResetSessionState(int bar)
	{
		_currentSessionBegin = bar;

		_delta1 = 0;
		_delta2 = 0;
		_delta3 = 0;
		_delta4 = 0;
		_delta5 = 0;

		_filter1Series[bar] = 0;
		_filter2Series[bar] = 0;
		_filter3Series[bar] = 0;
		_filter4Series[bar] = 0;
		_filter5Series[bar] = 0;
		_spreadSeries[bar] = 0;
	}

	private void ReplayBufferedRealtimeAfterHistory()
	{
		// Replay must not mutate buffers while iterating.
		if (!CumulativeTrades && _ticks.Count > 0)
		{
			foreach (var tick in _ticks)
				CalculateTick(tick);

			_ticks.Clear();
		}

		if (CumulativeTrades && _trades.Count > 0)
		{
			foreach (var trade in _trades)
				CalculateTrade(trade, isUpdate: false, newBar: false);

			_trades.Clear();
		}
	}

	private void UpdateSpreadVisuals(int bar)
	{
		// Signal is derived from the spread series, so ensure bar is valid.
		if (bar < 0)
			return;

		// Compute SMA on spread series (simple implementation).
		// Clamp start to _currentSessionBegin so the SMA doesn't bleed across session resets.
		var period = Math.Max(2, _signalPeriod);
		var start = Math.Max(_currentSessionBegin, bar - period + 1);

		decimal sum = 0;
		var count = 0;

		for (var i = start; i <= bar; i++)
		{
			sum += _spreadSeries[i];
			count++;
		}

		var sma = count > 0 ? sum / count : 0m;
		_signalSeries[bar] = sma;

		// Color logic: either simple (sign) or 4-color (sign + SMA slope).
		if (!_use4ColorSystem)
		{
			var simple = _spreadSeries[bar] >= 0 ? _simplePositiveColor : _simpleNegativeColor;
			_spreadSeries.Colors[bar] = simple.Convert();
			return;
		}

		var prevSma = bar > 0 ? _signalSeries[bar - 1] : sma;
		var smaRising = sma >= prevSma;

		CrossColor finalColor;

		if (_spreadSeries[bar] >= 0)
			finalColor = smaRising ? _colorPosSmaUp : _colorPosSmaDown;
		else
			finalColor = smaRising ? _colorNegSmaUp : _colorNegSmaDown;

		_spreadSeries.Colors[bar] = finalColor.Convert();
	}

	#endregion
}