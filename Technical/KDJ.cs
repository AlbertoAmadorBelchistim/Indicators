namespace ATAS.Indicators.Technical
{
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;

	using ATAS.Indicators.Drawing;

	using OFT.Attributes;
    using OFT.Localization;

    [DisplayName("KDJ")]
    [Display(ResourceType = typeof(Strings), Description = nameof(Strings.KDJDescription))]
    [HelpLink("https://help.atas.net/support/solutions/articles/72000602287")]
	public class KDJ : Indicator
	{
		#region Fields

		private readonly KdSlow _kdSlow = new()
		{
			SlowPeriodD = 10,
			PeriodK = 10,
			PeriodD = 10
		};

		private readonly ValueDataSeries _dSeries = new("DSeries", Strings.SMA)
		{
			Color = DefaultColors.Green.Convert(),
			IgnoredByAlerts = true,
			DescriptionKey = nameof(Strings.SmaSetingsDescription)
		};

		private readonly ValueDataSeries _kSeries = new("KSeries", Strings.Line)
		{
			Color = DefaultColors.Red.Convert(),
			DescriptionKey = nameof(Strings.BaseLineSettingsDescription)
		};

        private readonly ValueDataSeries _renderSeries = new("RenderSeries", Strings.Visualization) { Color = DefaultColors.Blue.Convert() };

        #endregion

        #region Properties

        [Parameter]
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.PeriodK), GroupName = nameof(Strings.ShortPeriod), Description = nameof(Strings.ShortPeriodKDescription), Order = 100)]
		[Range(1, 10000)]
		public int PeriodK
		{
			get => _kdSlow.PeriodK;
			set
			{
				_kdSlow.PeriodK = value;
				RecalculateValues();
			}
		}

        [Parameter]
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.PeriodD), GroupName = nameof(Strings.ShortPeriod), Description = nameof(Strings.ShortPeriodDDescription), Order = 110)]
		[Range(1, 10000)]
        public int PeriodD
		{
			get => _kdSlow.PeriodD;
			set
			{
				_kdSlow.PeriodD = value;
				RecalculateValues();
			}
		}

        [Parameter]
        [Display(ResourceType = typeof(Strings), Name = nameof(Strings.PeriodD), GroupName = nameof(Strings.LongPeriod), Description = nameof(Strings.LongPeriodDDescription), Order = 120)]
		[Range(1, 10000)]
        public int SlowPeriodD
		{
			get => _kdSlow.SlowPeriodD;
			set
			{
				_kdSlow.SlowPeriodD = value;
				RecalculateValues();
			}
		}

		#endregion

		#region ctor

		public KDJ()
			: base(true)
		{
			Panel = IndicatorDataProvider.NewPanel;
			
			Add(_kdSlow);
			DataSeries[0] = _renderSeries;

			DataSeries.Add(_kSeries);
			DataSeries.Add(_dSeries);
		}

		#endregion

		#region Protected methods

        protected override void OnCalculate(int bar, decimal value)
        {
	        if (bar < CurrentBar - 1)
		        return;

	        this[bar] = CalcKdj(bar);
	        _kSeries[bar] = ((ValueDataSeries)_kdSlow.DataSeries[0])[bar];
	        _dSeries[bar] = ((ValueDataSeries)_kdSlow.DataSeries[1])[bar];
        }

        protected override void OnFinishRecalculate()
        {
	        for (var bar = 0; bar < CurrentBar; bar++)
	        {
		        this[bar] = CalcKdj(bar);
		        _kSeries[bar] = ((ValueDataSeries)_kdSlow.DataSeries[0])[bar];
		        _dSeries[bar] = ((ValueDataSeries)_kdSlow.DataSeries[1])[bar];
	        }
        }

        #endregion

		private decimal CalcKdj(int bar)
		{
			return 3 * ((ValueDataSeries)_kdSlow.DataSeries[0])[bar] -
				2 * ((ValueDataSeries)_kdSlow.DataSeries[1])[bar];
        }
	}
}