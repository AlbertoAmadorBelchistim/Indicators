namespace ATAS.Indicators.Technical
{
	using System;
	using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using ATAS.Indicators.Drawing;

    using OFT.Attributes;
    using OFT.Localization;

    [DisplayName("Keltner Channel")]
    [Display(ResourceType = typeof(Strings), Description = nameof(Strings.KeltnerChannelDescription))]
    [HelpLink("https://help.atas.net/en/support/solutions/articles/72000602574")]
	public class KeltnerChannel : Indicator
	{
		#region Fields

		private readonly ATR _atr = new() { Period = 34 };
		private readonly RangeDataSeries _keltner = new("Keltner", "BackGround")
		{
			DrawAbovePrice = false ,
            DescriptionKey = nameof(Strings.RangeAreaDescription)
        };

		private readonly SMA _sma = new() { Period = 34 };

        private int _days = 20;
        private decimal _koef = 4;
		private int _targetBar;
		private int _lastAlertTop;
		private bool _onLineTop;
		private int _lastAlertMid;
		private bool _onLineMid;
		private bool _onLineBot;
		private int _lastAlertBot;

		#endregion

		#region Properties

		[Parameter]
		[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Calculation), Name = nameof(Strings.DaysLookBack), Order = int.MaxValue, Description = nameof(Strings.DaysLookBackDescription))]
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

		[Parameter]
		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.Period),
			GroupName = nameof(Strings.Calculation),
            Description = nameof(Strings.SMAPeriodDescription),
            Order = 20)]
		[Range(1, 10000)]
        public int Period
		{
			get => _sma.Period;
			set
			{
				_sma.Period = _atr.Period = value;
				RecalculateValues();
			}
		}

		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.OffsetMultiplier),
			GroupName = nameof(Strings.Calculation),
            Description = nameof(Strings.ATRMultiplierDescription),
            Order = 20)]
		[Parameter]
		[Range(0.00000001, 10000000)]
        public decimal Koef
		{
			get => _koef;
			set
			{
				_koef = value;
				RecalculateValues();
			}
		}

        #region TopAlert
        
        [Display(ResourceType = typeof(Strings),
	        Name = nameof(Strings.UseAlerts),
	        GroupName = nameof(Strings.TopBand),
	        Description = nameof(Strings.UseAlertDescription),
	        Order = 100)]
        public bool UseAlertsTop { get; set; }

        [Display(ResourceType = typeof(Strings),
	        Name = nameof(Strings.RepeatAlert),
	        GroupName = nameof(Strings.TopBand),
	        Description = nameof(Strings.RepeatAlertDescription),
	        Order = 110)]
        public bool RepeatAlertTop { get; set; }

        [Display(ResourceType = typeof(Strings),
	        Name = nameof(Strings.ApproximationFilter),
	        GroupName = nameof(Strings.TopBand),
	        Description = nameof(Strings.ApproximationFilterDescription),
	        Order = 120)]
        [Range(0, 100000)]
        public int AlertSensitivityTop { get; set; } = 1;

        [Display(ResourceType = typeof(Strings),
	        Name = nameof(Strings.AlertFile),
	        GroupName = nameof(Strings.TopBand),
	        Description = nameof(Strings.AlertFileDescription),
	        Order = 130)]
        public string AlertFileTop { get; set; } = "alert1";

        [Display(ResourceType = typeof(Strings),
	        Name = nameof(Strings.FontColor),
	        GroupName = nameof(Strings.TopBand),
	        Description = nameof(Strings.AlertTextColorDescription),
	        Order = 140)]
        public CrossColor FontColorTop { get; set; } = DefaultColors.White.Convert();

        [Display(ResourceType = typeof(Strings),
	        Name = nameof(Strings.BackGround),
	        GroupName = nameof(Strings.TopBand),
	        Description = nameof(Strings.AlertFillColorDescription),
	        Order = 150)]
        public CrossColor BackgroundColorTop { get; set; } = DefaultColors.Gray.Convert();

		#endregion

		#region MidAlert

		[Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.UseAlerts),
            GroupName = nameof(Strings.MiddleBand),
            Description = nameof(Strings.UseAlertDescription),
            Order = 200)]
        public bool UseAlertsMid { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.RepeatAlert),
            GroupName = nameof(Strings.MiddleBand),
            Description = nameof(Strings.RepeatAlertDescription),
            Order = 210)]
        public bool RepeatAlertMid { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.ApproximationFilter),
            GroupName = nameof(Strings.MiddleBand),
            Description = nameof(Strings.ApproximationFilterDescription),
            Order = 220)]
        [Range(0, 100000)]
        public int AlertSensitivityMid { get; set; } = 1;

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.AlertFile),
            GroupName = nameof(Strings.MiddleBand),
            Description = nameof(Strings.AlertFileDescription),
            Order = 230)]
        public string AlertFileMid { get; set; } = "alert1";

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.FontColor),
            GroupName = nameof(Strings.MiddleBand),
            Description = nameof(Strings.AlertTextColorDescription),
            Order = 240)]
        public CrossColor FontColorMid { get; set; } = DefaultColors.White.Convert();

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.BackGround),
            GroupName = nameof(Strings.MiddleBand),
            Description = nameof(Strings.AlertFillColorDescription),
            Order = 250)]
        public CrossColor BackgroundColorMid { get; set; } = DefaultColors.Gray.Convert();

        #endregion

        #region BotAlert

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.UseAlerts),
            GroupName = nameof(Strings.BottomBand),
            Description = nameof(Strings.UseAlertDescription),
            Order = 300)]
        public bool UseAlertsBot { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.RepeatAlert),
            GroupName = nameof(Strings.BottomBand),
            Description = nameof(Strings.RepeatAlertDescription),
            Order = 310)]
        public bool RepeatAlertBot { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.ApproximationFilter),
            GroupName = nameof(Strings.BottomBand),
            Description = nameof(Strings.ApproximationFilterDescription),
            Order = 320)]
        [Range(0, 100000)]
        public int AlertSensitivityBot { get; set; } = 1;

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.AlertFile),
            GroupName = nameof(Strings.BottomBand),
            Description = nameof(Strings.AlertFileDescription),
            Order = 330)]
        public string AlertFileBot { get; set; } = "alert1";

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.FontColor),
            GroupName = nameof(Strings.BottomBand),
            Description = nameof(Strings.AlertTextColorDescription),
            Order = 340)]
        public CrossColor FontColorBot { get; set; } = DefaultColors.White.Convert();

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.BackGround),
            GroupName = nameof(Strings.BottomBand),
            Description = nameof(Strings.AlertFillColorDescription),
            Order = 350)]
        public CrossColor BackgroundColorBot { get; set; } = DefaultColors.Gray.Convert();
		
        #endregion

		#endregion

		#region ctor

		public KeltnerChannel()
			: base(true)
		{
			DenyToChangePanel = true;

			DataSeries.Add(new ValueDataSeries("UpperId", "Upper")
			{
				VisualType = VisualMode.Line,
				DescriptionKey = nameof(Strings.TopBandDscription)
			});

			DataSeries.Add(new ValueDataSeries("LowerId", "Lower")
			{
				VisualType = VisualMode.Line,
                DescriptionKey = nameof(Strings.BottomBandDscription)
            });

			DataSeries.Add(_keltner);
			Add(_atr);
		}

		#endregion

		#region Protected methods

		protected override void OnCalculate(int bar, decimal value)
		{
			if (bar == 0)
			{
				DataSeries.ForEach(x => x.Clear());
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

					if (_targetBar > 0)
					{
						((ValueDataSeries)DataSeries[0]).SetPointOfEndLine(_targetBar - 1);
						((ValueDataSeries)DataSeries[1]).SetPointOfEndLine(_targetBar - 1);
						((ValueDataSeries)DataSeries[2]).SetPointOfEndLine(_targetBar - 1);
					}
				}
			}

			var currentCandle = GetCandle(bar);
			var ema = _sma.Calculate(bar, currentCandle.Close);

			if (bar < _targetBar)
				return;

			var atr = _atr[bar] * Koef;
			var upAtr = ema + atr;
			var downAtr = ema - atr;

            this[bar] = ema;
			DataSeries[1][bar] = upAtr;
			DataSeries[2][bar] = downAtr;
			_keltner[bar].Upper = upAtr;
			_keltner[bar].Lower = downAtr;

			if (bar != CurrentBar - 1)
				return;

			if (UseAlertsTop && (RepeatAlertTop || _lastAlertTop != bar && !RepeatAlertTop))
			{
				var close = GetCandle(bar).Close;
				var onLine = Math.Abs(upAtr - close) / InstrumentInfo.TickSize <= AlertSensitivityTop;

				if (onLine && !_onLineTop)
				{
					AddAlert(AlertFileTop, InstrumentInfo.Instrument, "Keltner top approximation alert", BackgroundColorTop, FontColorTop);
					_lastAlertTop = bar;
				}

				_onLineTop = onLine;
			}

			if (UseAlertsMid && (RepeatAlertMid || _lastAlertMid != bar && !RepeatAlertMid))
			{
				var close = GetCandle(bar).Close;
				var onLine = Math.Abs(this[bar] - close) / InstrumentInfo.TickSize <= AlertSensitivityMid;

				if (onLine && !_onLineMid)
				{
					AddAlert(AlertFileMid, InstrumentInfo.Instrument, "Keltner middle approximation alert", BackgroundColorMid, FontColorMid);
					_lastAlertMid = bar;
				}

				_onLineMid = onLine;
			}

			if (UseAlertsBot && (RepeatAlertBot || _lastAlertBot != bar && !RepeatAlertBot))
			{
				var close = GetCandle(bar).Close;
				var onLine = Math.Abs(downAtr - close) / InstrumentInfo.TickSize <= AlertSensitivityBot;

				if (onLine && !_onLineBot)
				{
					AddAlert(AlertFileTop, InstrumentInfo.Instrument, "Keltner bottom approximation alert", BackgroundColorBot, FontColorBot);
					_lastAlertBot = bar;
				}

				_onLineBot = onLine;
			}
		}

		#endregion
	}
}