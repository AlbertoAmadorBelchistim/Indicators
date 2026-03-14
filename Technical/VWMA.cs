namespace ATAS.Indicators.Technical
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	using ATAS.Indicators.Drawing;
    using ATAS.Indicators.Technical.Properties;

    using OFT.Attributes;
	using OFT.Localization;

	[DisplayName("VWMA")]
	[Display(ResourceType = typeof(Resources), Description = nameof(Resources.VWMADescription))]
	public class VWMA : Indicator
	{
		#region Fields

		private int _period = 10;
		private int _lastBar = -1;
		private EMA _ema = new() { SourceDataSeries = new ValueDataSeries("EmaSource") };
		private FilterInt _smooth;

		private decimal _volSum;
		private decimal _volPriceSum;
		private ValueDataSeries _vwmaSeries = new("VwmaSeries")
		{
			IsHidden = true
		};

		private ValueDataSeries _renderSeries = new("RenderSeries", "VWMA")
		{
			Color = DefaultColors.Red.Convert(),
			ShowZeroValue = false
		};

		#endregion

		#region Properties

		[Parameter]
		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.Period),
			GroupName = nameof(Strings.Settings),
			Description = nameof(Strings.PeriodDescription),
			Order = 20)]
		[Range(1, 10000)]
		public int Period
		{
			get => _period;
			set
			{
				_period = Math.Max(1, value);

				RecalculateValues();
			}
		}

		[Display(ResourceType = typeof(Resources),
			Name = nameof(Resources.Smoothing),
			GroupName = nameof(Resources.Settings),
			Description = nameof(Resources.SmoothDescription),
			Order = 30)]
		[Range(1, 10000)]
		public FilterInt Smooth
		{
			get => _smooth;
			set => SetTrackedProperty(ref _smooth, value, _ =>
			{
				_ema.Period = _smooth.Value;
				RecalculateValues();
				RedrawChart();
			});
		}

		#endregion

		#region ctor

		public VWMA() : base(true)
		{
			DenyToChangePanel = true;
			Smooth = new(true) { Value = 10 };
			DataSeries[0] = _renderSeries;
		}

		#endregion

		#region Protected methods

		protected override void OnRecalculate()
		{
			_ema = new() { SourceDataSeries = new ValueDataSeries("EmaSource") };
			_ema.Period = _smooth.Value;
		}

		protected override void OnCalculate(int bar, decimal value)
		{
			if (bar is 0)
			{
				_lastBar = -1;
				_volSum = 0;
				_volPriceSum = 0;
				_renderSeries.Clear();
				_renderSeries[bar] = GetCandle(bar).Close;

				if (Period > 1)
					_renderSeries.SetPointOfEndLine(bar);

				return;
			}

			if (_lastBar != bar)
			{
				_lastBar = bar;

				var prevCandle = GetCandle(bar - 1);
				_volSum += prevCandle.Volume;
				_volPriceSum += prevCandle.Volume * prevCandle.Close;

				if (bar >= Period)
				{
					var oldCandle = GetCandle(bar - Period);
					_volSum -= oldCandle.Volume;
					_volPriceSum -= oldCandle.Volume * oldCandle.Close;
				}
			}

			if (bar < Period)
			{
				_renderSeries.SetPointOfEndLine(bar);
				return;
			}

			var candle = GetCandle(bar);
			var volSum = _volSum + candle.Volume;
			var volPriceSum = _volPriceSum + candle.Volume * candle.Close;

			if (volSum != 0)
				_vwmaSeries[bar] = volPriceSum / volSum;

			_renderSeries[bar] = Smooth.Enabled
				? _ema.Calculate(bar, _vwmaSeries[bar])
				: _vwmaSeries[bar];
		}

		#endregion
	}
}
