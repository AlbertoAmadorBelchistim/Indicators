namespace ATAS.Indicators.Technical
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	using OFT.Attributes;
    using OFT.Localization;

	[DisplayName("WMA")]
	[Display(ResourceType = typeof(Strings), Description = nameof(Strings.WMADescription))]
	[HelpLink("https://help.atas.net/en/support/solutions/articles/72000602622")]
	public class WMA : Indicator
	{
		#region Fields

		private int _lastBar = -1;
		private int _myPeriod;
		private int _period = 10;
		private decimal _priorSum;
		private decimal _priorWsum;
		private decimal _sum;
		private decimal _wsum;
		private int _lastAlert;
		private bool _onLine;

		#endregion

		#region Properties

		[Parameter]
		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.Period),
			GroupName = nameof(Strings.Common),
			Description = nameof(Strings.PeriodDescription),
			Order = 20)]
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

		#region Protected methods

		protected override void OnCalculate(int bar, decimal value)
		{
			if (bar < _lastBar)
			{
				_wsum = 0;
				_sum = 0;
			}

			if (bar != _lastBar)
			{
				_lastBar = bar;
				_priorWsum = _wsum;
				_priorSum = _sum;
				_myPeriod = Math.Min(bar + 1, Period);
			}

			_wsum = _priorWsum - (bar >= Period ? _priorSum : 0) + _myPeriod * value;
			_sum = _priorSum + value - (bar >= Period ? (decimal)SourceDataSeries[bar - Period] : 0);
			this[bar] = _wsum / (0.5m * _myPeriod * (_myPeriod + 1));


			if (bar != CurrentBar - 1 || !UseAlerts)
				return;

			if (_lastAlert == bar && !RepeatAlert)
				return;

			var close = GetCandle(bar).Close;
			var onLine = Math.Abs(this[bar] - close) / InstrumentInfo.TickSize <= AlertSensitivity;

			if (onLine && !_onLine)
			{
				AddAlert(AlertFile, InstrumentInfo.Instrument, $"Triple EMA approximation alert: {this[bar]:0.#####}", BackgroundColor, FontColor);
				_lastAlert = bar;
			}

			_onLine = onLine;
		}

		#endregion
	}
}