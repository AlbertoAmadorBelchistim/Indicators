namespace ATAS.Indicators.Technical
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	using OFT.Attributes;
    using OFT.Localization;

    [DisplayName("T3")]
    [Display(ResourceType = typeof(Strings), Description = nameof(Strings.T3IndDescription))]
    [HelpLink("https://help.atas.net/en/support/solutions/articles/72000606641")]
	public class T3 : Indicator
	{
		#region Fields

		private readonly List<EMA> _emaSix = new();
		private readonly ValueDataSeries _renderSeries = new("RenderSeries", Strings.Visualization)
		{
			UseMinimizedModeIfEnabled = true
		};
		private decimal _multiplier = 1;
        private int _period = 10;
        private int _lastAlert;
        private bool _onLine;

        #endregion

        #region Properties

        [Parameter]
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Period), GroupName = nameof(Strings.Settings), Description = nameof(Strings.PeriodDescription), Order = 100)]
		[Range(1, 10000)]
		public int Period
		{
			get => _period;
			set
			{
				_period = value;
				_emaSix.ForEach(x => x.Period = value);
				RecalculateValues();
			}
		}

        [Parameter]
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Multiplier), GroupName = nameof(Strings.Settings), Description = nameof(Strings.MultiplierDescription), Order = 110)]
        [Range(1,1000000)]
		public decimal Multiplier
		{
			get => _multiplier;
			set
			{
				_multiplier = value;
				RecalculateValues();
			}
		}

		#region Alert

		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.UseAlerts),
			GroupName = nameof(Strings.ApproximationAlert),
			Description = nameof(Strings.UseAlertsDescription),
			Order = 300)]
		public bool UseAlerts { get; set; }


		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.RepeatAlert),
			GroupName = nameof(Strings.ApproximationAlert),
			Description = nameof(Strings.RepeatAlertDescription),
			Order = 310)]
		[Range(0, 100000)]
		public bool RepeatAlert { get; set; }

		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.ApproximationFilter),
			GroupName = nameof(Strings.ApproximationAlert),
			Description = nameof(Strings.ApproximationFilterDescription),
			Order = 320)]
		[Range(0, 100000)]
		public int AlertSensitivity { get; set; } = 1;

		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.AlertFile),
			GroupName = nameof(Strings.ApproximationAlert),
			Description = nameof(Strings.AlertFileDescription),
			Order = 330)]
		public string AlertFile { get; set; } = "alert1";

		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.FontColor),
			GroupName = nameof(Strings.ApproximationAlert),
			Description = nameof(Strings.AlertTextColorDescription),
			Order = 340)]
		public CrossColor FontColor { get; set; } = System.Drawing.Color.White.Convert();

		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.BackGround),
			GroupName = nameof(Strings.ApproximationAlert),
			Description = nameof(Strings.AlertFillColorDescription),
			Order = 350)]
		public CrossColor BackgroundColor { get; set; } = System.Drawing.Color.DimGray.Convert();

		#endregion

		#endregion

        #region ctor

        public T3()
		{
			for (var i = 0; i < 6; i++)
			{
				_emaSix.Add(new EMA
					{ Period = _period });
			}

			DataSeries[0] = _renderSeries;
		}

		#endregion

		#region Protected methods

		protected override void OnCalculate(int bar, decimal value)
		{
			_emaSix[0].Calculate(bar, value);

			for (var i = 1; i < _emaSix.Count; i++)
				_emaSix[i].Calculate(bar, _emaSix[i - 1][bar]);

			_renderSeries[bar] = -(decimal)Math.Pow((double)_multiplier, 3) * _emaSix[5][bar] +
				3 * _multiplier * _multiplier * (1 + _multiplier) * _emaSix[4][bar] -
				3 * _multiplier * (1 + _multiplier) * (1 + _multiplier) * _emaSix[3][bar] +
				(decimal)Math.Pow(1 + (double)_multiplier, 3) * _emaSix[2][bar];

			if (bar != CurrentBar - 1 || !UseAlerts)
				return;

			if (_lastAlert == bar && !RepeatAlert)
				return;

			var close = GetCandle(bar).Close;
			var onLine = Math.Abs(this[bar] - close) / InstrumentInfo.TickSize <= AlertSensitivity;

			if (onLine && !_onLine)
			{
				AddAlert(AlertFile, InstrumentInfo.Instrument, $"T3 approximation alert: {this[bar]:0.#####}", BackgroundColor, FontColor);
				_lastAlert = bar;
			}

			_onLine = onLine;
		}

		#endregion
	}
}