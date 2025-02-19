namespace ATAS.Indicators.Technical
{
	using System;
	using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using ATAS.Indicators.Drawing;

    using OFT.Attributes;
    using OFT.Localization;

    [DisplayName("Standard Deviation Bands")]
    [Display(ResourceType = typeof(Strings), Description = nameof(Strings.StdDevBandsDescription))]
    [HelpLink("https://help.atas.net/en/support/solutions/articles/72000602614")]
	public class StdDevBands : Indicator
	{
        #region Fields

        private readonly Highest _highest = new() { Period = 10 };
        private readonly Lowest _lowest = new() { Period = 10 };
        private readonly SMA _smaHigh = new() { Period = 10 };
        private readonly SMA _smaLow = new() { Period = 10 };
        private readonly StdDev _stdHigh = new() { Period = 10 };
        private readonly StdDev _stdLow = new() { Period = 10 };

		private readonly ValueDataSeries _smaBotSeries = new("SmaBotSeries", Strings.SMA1)
		{
            DescriptionKey = nameof(Strings.SmaSetingsDescription),
        };

		private readonly ValueDataSeries _smaTopSeries = new("SmaTopSeries", Strings.SMA2)
		{
			DescriptionKey = nameof(Strings.SmaSetingsDescription),
		};

        private readonly ValueDataSeries _botSeries = new("BotSeries", Strings.BottomBand)
		{
			Color = System.Drawing.Color.DodgerBlue.Convert(),
			IgnoredByAlerts = true,
            DescriptionKey = nameof(Strings.BottomBandDscription),
        };
		
        private readonly ValueDataSeries _topSeries = new("TopSeries", Strings.TopBand)
        {
			Color = System.Drawing.Color.DodgerBlue.Convert(),
			IgnoredByAlerts = true,
            DescriptionKey = nameof(Strings.TopBandDscription),
        };

		private int _width = 2;
		private int _lastAlertTop;
		private bool _onLineTop;
		private int _lastAlertUpperSma;
		private bool _onLineUpperSma;
		private int _lastAlertLowerSma;
		private bool _onLineLowerSma;
		private int _lastAlertBot;
		private bool _onLineBot;

		#endregion

        #region Properties

        [Parameter]
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Period), GroupName = nameof(Strings.Settings), Description = nameof(Strings.StdPeriodDescription), Order = 100)]
		[Range(1, 10000)]
        public int Period
		{
			get => _stdHigh.Period;
			set
			{
				_stdHigh.Period = _stdLow.Period = _highest.Period = _lowest.Period =
					_smaHigh.Period = _smaLow.Period = value;

				RecalculateValues();
			}
		}

        [Parameter]
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.BBandsWidth), GroupName = nameof(Strings.Settings), Description = nameof(Strings.SMAPeriodDescription), Order = 110)]
		[Range(1, 1000)]
        public int SmaPeriod
		{
			get => _width;
			set
			{
				_width = value;
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

        #region UpperSmaAlert

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.UseAlerts),
            GroupName = nameof(Strings.UpperSma),
            Description = nameof(Strings.UseAlertDescription),
            Order = 200)]
        public bool UseAlertsUpperSma { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.RepeatAlert),
            GroupName = nameof(Strings.UpperSma),
            Description = nameof(Strings.RepeatAlertDescription),
            Order = 210)]
        public bool RepeatAlertUpperSma { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.ApproximationFilter),
            GroupName = nameof(Strings.UpperSma),
            Description = nameof(Strings.ApproximationFilterDescription),
            Order = 220)]
        [Range(0, 100000)]
        public int AlertSensitivityUpperSma { get; set; } = 1;

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.AlertFile),
            GroupName = nameof(Strings.UpperSma),
            Description = nameof(Strings.AlertFileDescription),
            Order = 230)]
        public string AlertFileUpperSma { get; set; } = "alert1";

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.FontColor),
            GroupName = nameof(Strings.UpperSma),
            Description = nameof(Strings.AlertTextColorDescription),
            Order = 240)]
        public CrossColor FontColorUpperSma { get; set; } = DefaultColors.White.Convert();

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.BackGround),
            GroupName = nameof(Strings.UpperSma),
            Description = nameof(Strings.AlertFillColorDescription),
            Order = 250)]
        public CrossColor BackgroundColorUpperSma { get; set; } = DefaultColors.Gray.Convert();

        #endregion

        #region LowerSmaAlert

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.UseAlerts),
            GroupName = nameof(Strings.LowerSma),
            Description = nameof(Strings.UseAlertDescription),
            Order = 200)]
        public bool UseAlertsLowerSma { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.RepeatAlert),
            GroupName = nameof(Strings.LowerSma),
            Description = nameof(Strings.RepeatAlertDescription),
            Order = 210)]
        public bool RepeatAlertLowerSma { get; set; }

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.ApproximationFilter),
            GroupName = nameof(Strings.LowerSma),
            Description = nameof(Strings.ApproximationFilterDescription),
            Order = 220)]
        [Range(0, 100000)]
        public int AlertSensitivityLowerSma { get; set; } = 1;

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.AlertFile),
            GroupName = nameof(Strings.LowerSma),
            Description = nameof(Strings.AlertFileDescription),
            Order = 230)]
        public string AlertFileLowerSma { get; set; } = "alert1";

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.FontColor),
            GroupName = nameof(Strings.LowerSma),
            Description = nameof(Strings.AlertTextColorDescription),
            Order = 240)]
        public CrossColor FontColorLowerSma { get; set; } = DefaultColors.White.Convert();

        [Display(ResourceType = typeof(Strings),
            Name = nameof(Strings.BackGround),
            GroupName = nameof(Strings.LowerSma),
            Description = nameof(Strings.AlertFillColorDescription),
            Order = 250)]
        public CrossColor BackgroundColorLowerSma { get; set; } = DefaultColors.Gray.Convert();

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

        public StdDevBands()
		{
			DataSeries[0] = _topSeries;
			DataSeries.Add(_botSeries);
			DataSeries.Add(_smaTopSeries);
			DataSeries.Add(_smaBotSeries);
		}

		#endregion

		#region Protected methods

		protected override void OnCalculate(int bar, decimal value)
		{
			var high = _highest.Calculate(bar, value);
			var low = _lowest.Calculate(bar, value);

			_topSeries[bar] = _smaHigh.Calculate(bar, high) + _width * _stdHigh.Calculate(bar, high);
			_botSeries[bar] = _smaLow.Calculate(bar, low) - _width * _stdLow.Calculate(bar, low);
			_smaTopSeries[bar] = _smaHigh[bar];
			_smaBotSeries[bar] = _smaLow[bar];

			CheckAlerts(bar);
		}

		private void CheckAlerts(int bar)
		{
			if (bar != CurrentBar - 1)
				return;

			if (UseAlertsTop && (RepeatAlertTop || _lastAlertTop != bar && !RepeatAlertTop))
			{
				var close = GetCandle(bar).Close;
				var onLine = Math.Abs(_topSeries[bar] - close) / InstrumentInfo.TickSize <= AlertSensitivityTop;

				if (onLine && !_onLineTop)
				{
					AddAlert(AlertFileTop, InstrumentInfo.Instrument, "StdDev Bands top approximation alert", BackgroundColorTop, FontColorTop);
					_lastAlertTop = bar;
				}

				_onLineTop = onLine;
			}

			if (UseAlertsUpperSma && (RepeatAlertUpperSma || _lastAlertUpperSma != bar && !RepeatAlertUpperSma))
			{
				var close = GetCandle(bar).Close;
				var onLine = Math.Abs(_smaTopSeries[bar] - close) / InstrumentInfo.TickSize <= AlertSensitivityUpperSma;

				if (onLine && !_onLineUpperSma)
				{
					AddAlert(AlertFileUpperSma, InstrumentInfo.Instrument, "StdDev Bands Upper SMA approximation alert", BackgroundColorUpperSma,
						FontColorUpperSma);
					_lastAlertUpperSma = bar;
				}

				_onLineUpperSma = onLine;
			}

			if (UseAlertsLowerSma && (RepeatAlertLowerSma || _lastAlertLowerSma != bar && !RepeatAlertLowerSma))
			{
				var close = GetCandle(bar).Close;
				var onLine = Math.Abs(_smaBotSeries[bar] - close) / InstrumentInfo.TickSize <= AlertSensitivityLowerSma;

				if (onLine && !_onLineLowerSma)
				{
					AddAlert(AlertFileLowerSma, InstrumentInfo.Instrument, "StdDev Bands Lower SMA approximation alert", BackgroundColorLowerSma,
						FontColorLowerSma);
					_lastAlertLowerSma = bar;
				}

				_onLineLowerSma = onLine;
			}

			if (UseAlertsBot && (RepeatAlertBot || _lastAlertBot != bar && !RepeatAlertBot))
			{
				var close = GetCandle(bar).Close;
				var onLine = Math.Abs(_botSeries[bar] - close) / InstrumentInfo.TickSize <= AlertSensitivityBot;

				if (onLine && !_onLineBot)
				{
					AddAlert(AlertFileTop, InstrumentInfo.Instrument, "StdDev Bands bottom approximation alert", BackgroundColorBot, FontColorBot);
					_lastAlertBot = bar;
				}

				_onLineBot = onLine;
			}
		}

		#endregion
	}
}