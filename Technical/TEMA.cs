namespace ATAS.Indicators.Technical
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	using OFT.Attributes;
    using OFT.Localization;

	[DisplayName("Triple Exponential Moving Average")]
	[Display(ResourceType = typeof(Strings), Description = nameof(Strings.TEMADescription))]
	[HelpLink("https://help.atas.net/en/support/solutions/articles/72000602492")]
	public class TEMA : Indicator
	{
		#region Fields

		private readonly EMA _emaFirst = new() { Period = 10 };
		private readonly EMA _emaSecond = new() { Period = 10 };
		private readonly EMA _emaThird = new() { Period = 10 };
		private int _lastAlert;
		private bool _onLine;

		#endregion

		#region Properties

		[Parameter]
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Period), GroupName = nameof(Strings.Settings),
			Description = nameof(Strings.PeriodDescription), Order = 100)]
		[Range(1, 10000)]
		public int Period
		{
			get => _emaFirst.Period;
			set
			{
				_emaFirst.Period = _emaSecond.Period = _emaThird.Period = value;
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
			_emaFirst.Calculate(bar, value);
			_emaSecond.Calculate(bar, _emaFirst[bar]);
			_emaThird.Calculate(bar, _emaSecond[bar]);
			this[bar] = 3 * _emaFirst[bar] - 3 * _emaSecond[bar] + _emaThird[bar];

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