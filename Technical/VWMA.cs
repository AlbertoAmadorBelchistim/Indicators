namespace ATAS.Indicators.Technical
{
	using System;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	using ATAS.Indicators.Drawing;

	using OFT.Attributes;
	using OFT.Localization;

	[DisplayName("VWMA")]
	[Display(ResourceType = typeof(Strings), Description = nameof(Strings.VWMADescription))]
	public class VWMA : Indicator
	{
		#region Fields

		private int _period = 10;
		private int _lastBar = -1;
		private EMA _ema = new() { SourceDataSeries = new ValueDataSeries("EmaSource") };
		private bool _emaPrimed;
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
			ShowZeroValue = false,
			ScaleIt = false
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

		[Display(ResourceType = typeof(Strings),
			Name = nameof(Strings.Smoothing),
			GroupName = nameof(Strings.Settings),
			Description = nameof(Strings.SmoothDescription),
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
			_emaPrimed = false;
		}

		protected override void OnCalculate(int bar, decimal value)
		{
			var sourceStartBar = Period - 1;
			var renderStartBar = sourceStartBar + (Smooth.Enabled ? Smooth.Value - 1 : 0);

			if (bar is 0)
			{
				_lastBar = -1;
				_emaPrimed = false;
				_volSum = 0;
				_volPriceSum = 0;
				_vwmaSeries.Clear();
				_renderSeries.Clear();
				_renderSeries[bar] = renderStartBar > 0 ? 0 : GetCandle(bar).Close;

				if (renderStartBar > 0)
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

			if (bar < sourceStartBar)
			{
				_renderSeries[bar] = 0;
				_renderSeries.SetPointOfEndLine(bar);
				return;
			}

			var candle = GetCandle(bar);
			var volSum = _volSum + candle.Volume;
			var volPriceSum = _volPriceSum + candle.Volume * candle.Close;

			if (volSum != 0)
				_vwmaSeries[bar] = volPriceSum / volSum;

			var renderValue = Smooth.Enabled
				? CalculateSmoothedValue(bar, sourceStartBar)
				: _vwmaSeries[bar];

			if (bar <= renderStartBar)
			{
				_renderSeries[bar] = bar < renderStartBar ? 0 : renderValue;
				_renderSeries.SetPointOfEndLine(bar);

				if (bar < renderStartBar)
					return;
			}

			_renderSeries[bar] = renderValue;
		}

		private decimal CalculateSmoothedValue(int bar, int sourceStartBar)
		{
			if (!_emaPrimed)
			{
				var seedValue = _vwmaSeries[bar];

				for (var i = 0; i <= sourceStartBar; i++)
					_ema.Calculate(i, seedValue);

				_emaPrimed = true;

				return seedValue;
			}

			return _ema.Calculate(bar, _vwmaSeries[bar]);
		}

		#endregion
	}
}
