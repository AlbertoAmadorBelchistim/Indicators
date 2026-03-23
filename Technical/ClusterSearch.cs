namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using OFT.Attributes;
using OFT.Localization;

using Utils.Common;
using Utils.Common.Collections;
using Utils.Common.Collections.Synchronized;

using static DynamicLevels;

[Category(IndicatorCategories.VolumeOrderFlow)]
[DisplayName("Cluster Search")]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.ClusterSearchDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602240")]
public partial class ClusterSearch : Indicator
{
	#region Fields

	private readonly PriceSelectionDataSeries _renderDataSeries = new("RenderDataSeries", "Price");
	private bool _autoFilter;
	private decimal _autoFilterValue;

	private HashSet<decimal> _alertPrices = [];

	private decimal _lastHigh;
	private decimal _lastLow;

	private int _barsRange = 1;
	private CandleDirection _candleDirection = CandleDirection.Any;
	private CrossColor _clusterPriceColor;
	private CrossColor _clusterTransColor;
	private int _days = 20;
	private decimal _deltaFilter;
	private decimal _deltaImbalance;
	private bool _fixedSizes;
	private bool _isFinishRecalculate;
	private bool _lastBarFormationValid;
	private int _lastBar = -1;

	private SyncList<PriceSelectionValue> _lastSeriesBar = [];
	private decimal _maxAverageTrade;
	private Filter _maxFilter = new() { Enabled = true, Value = 99999 };
	private decimal _maxPercent;
	private int _maxSize = 50;
	private decimal _minAverageTrade;
	private Filter _minFilter = new() { Enabled = true, Value = 1000 };
	private decimal _minFilterValue;
	private decimal _minPercent;
	private int _minSize = 5;
	private bool _onlyOneSelectionPerBar;
	private Filter _pipsFromHigh = new() { Value = 100000000 };
	private Filter _pipsFromLow = new() { Value = 100000000 };
	private decimal _pocPrice;
	private decimal _pocVolume;
	private PriceLocation _priceLocation = PriceLocation.Any;
	private int _priceRange = 1;
	private bool _showPriceSelection = true;
	private int _size = 10;
	private int _targetBar;
	private TimeSpan _timeFrom = TimeSpan.Zero;
	private TimeSpan _timeTo = TimeSpan.Zero;
	private CalcMode _type = CalcMode.Volume;
	private bool _usePrevClose;
	private bool _useTimeFilter;
	private int _visualObjectsTransparency;
	private ObjectType _visualType = ObjectType.Rectangle;

	#endregion

	#region ctor

	public ClusterSearch()
		: base(true)
	{
		VisualObjectsTransparency = 70;
		PriceSelectionColor = ClusterColor = CrossColor.FromArgb(100, 255, 0, 255);

		VisualType = ObjectType.Rectangle;

		DenyToChangePanel = true;
		_renderDataSeries.IsHidden = true;
		DataSeries[0] = _renderDataSeries;
	}

	#endregion

	#region Protected methods

	protected override void OnInitialize()
	{
		_maxFilter.PropertyChanged += Filter_PropertyChanged;
		_minFilter.PropertyChanged += Filter_PropertyChanged;
		PipsFromHigh.PropertyChanged += Filter_PropertyChanged;
		PipsFromLow.PropertyChanged += Filter_PropertyChanged;

		MinCandleHeight.PropertyChanged += Filter_PropertyChanged;
		MaxCandleHeight.PropertyChanged += Filter_PropertyChanged;
		MinCandleBodyHeight.PropertyChanged += Filter_PropertyChanged;
		MaxCandleBodyHeight.PropertyChanged += Filter_PropertyChanged;
	}

	protected override void OnNewTrades(IEnumerable<MarketDataArg> trades)
	{
		if (!_isFinishRecalculate || UsePrevClose)
			return;

		var bar = _lastBar;

		if (bar < 0)
			return;

		var candle = GetCandle(bar);

		var isValid = CheckBarFormation(candle);

		if (candle.High != _lastHigh || candle.Low != _lastLow || isValid != _lastBarFormationValid)
		{
			_lastHigh = candle.High;
			_lastLow = candle.Low;
			CalculateBarFull(bar);
			return;
		}

		if (!isValid)
			return;

		if (CalcType is CalcMode.MaxVolume)
		{
			CalculateBarFull(bar);
			return;
		}

		var endPrice = Math.Max(candle.Low, candle.High - (PriceRange - 1) * InstrumentInfo.TickSize);
		var totalVolume = GetTotalVolume(bar);
		var ranges = GetPriceRanges(bar, endPrice);

		_renderDataSeries[bar] = _lastSeriesBar;

		foreach (var trade in trades)
		{
			var startPrice = PriceRange > 1
				? trade.Price - (PriceRange - 1) * InstrumentInfo.TickSize
				: trade.Price;

			for (var price = Math.Max(candle.Low, startPrice); price <= Math.Min(endPrice, trade.Price); price += InstrumentInfo.TickSize)
			{
				var inRange = false;

				foreach (var range in ranges)
				{
					if (price >= range.From && price <= range.To)
					{
						inRange = true;
						break;
					}
				}

				if (!inRange)
				{
					RemoveOldSelection(bar, price);
					continue;
				}

				var info = GetMergedClusterInfo(bar, price);

				if (CheckClusterFilters(info, totalVolume))
					PlaceToDataSeries(bar, info);
				else
					RemoveOldSelection(bar, price);
			}
		}
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		if (bar is 0 && UsePrevClose)
			return;

		if (UsePrevClose)
			bar--;

		if (bar < _targetBar)
			return;

		var isNewBar = _lastBar != bar;

		if (isNewBar)
		{
			if (_lastBar >= 0 && _isFinishRecalculate)
			{
				CalculateBarFull(_lastBar);
				SaveBarResults(_lastBar);
			}
			else if (_lastBar >= 0)
				SaveBarResults(_lastBar);

			_lastBar = bar;
			_alertPrices.Clear();
		}

		if (isNewBar || !_isFinishRecalculate)
		{
			CalculateBarFull(bar);

			var candle = GetCandle(bar);
			_lastHigh = candle.High;
			_lastLow = candle.Low;
		}
	}

	protected override void OnRecalculate()
	{
		if (InstrumentInfo is null)
			return;

		_lastBar = -1;
		_isFinishRecalculate = false;

		_autoFilterValue = 0;
		_targetBar = 0;

		if (AutoFilter)
		{
			_minFilter.PropertyChanged -= Filter_PropertyChanged;
			MinimumFilter.Value = 0;
			_minFilter.PropertyChanged += Filter_PropertyChanged;
		}

		if (Days is 0)
			return;

		var days = 0;

		for (var i = CurrentBar - 1; i >= 0; i--)
		{
			_targetBar = i;

			if (!IsNewSession(i))
				continue;

			days++;

			if (days == Days)
				break;
		}

		_lastSeriesBar.Clear();
		_renderDataSeries.Clear();

		_minFilterValue = MinimalFilter();
	}

	protected override void OnFinishRecalculate()
	{
		try
		{
			if (!AutoFilter)
				return;

			var valuesList = new List<PriceSelectionValue>();

			for (var i = 0; i < _renderDataSeries.Count; i++)
			{
				if (_renderDataSeries[i].Count is 0)
					continue;

				valuesList.AddRange(_renderDataSeries[i]);
			}

			if (valuesList.Count is 0)
				return;

			valuesList = valuesList.OrderByDescending(x => (decimal)x.Context).ToList();

			_autoFilterValue = valuesList.Count <= 10
				? (decimal)valuesList.Last().Context
				: (decimal)valuesList.Skip(10).First().Context;

			_minFilter.PropertyChanged -= Filter_PropertyChanged;
			MinimumFilter.Value = _autoFilterValue;
			_minFilter.PropertyChanged += Filter_PropertyChanged;
			_minFilterValue = MinimalFilter();

			for (var i = 0; i < _renderDataSeries.Count; i++)
			{
				if (_renderDataSeries[i].Count is 0)
					continue;

				_renderDataSeries[i].RemoveAll(x => (decimal)x.Context < _autoFilterValue);

				_renderDataSeries[i].ForEach(l =>
				{
					var clusterSize = FixedSizes ? _size : (int)((decimal)l.Context * _size / _minFilterValue);

					if (!FixedSizes)
					{
						clusterSize = Math.Min(clusterSize, MaxSize);
						clusterSize = Math.Max(clusterSize, MinSize);
					}

					l.Size = clusterSize;
				});
			}
		}
		finally
		{
			OnChangeProperty(nameof(MinimumFilter));

			_isFinishRecalculate = true;
		}
	}

	#endregion

	#region Private methods

	private void SaveBarResults(int bar)
	{
		if (CheckBarFormation(GetCandle(bar)))
		{
			var copy = new SyncList<PriceSelectionValue>();

			foreach (var p in _lastSeriesBar)
			{
				copy.Add(new PriceSelectionValue(p.MinimumPrice)
				{
					MaximumPrice = p.MaximumPrice,
					Size = p.Size,
					Tooltip = p.Tooltip,
					SelectionSide = p.SelectionSide,
					VisualObject = p.VisualObject,
					PriceSelectionColor = p.PriceSelectionColor,
					ObjectColor = p.ObjectColor,
					ObjectsTransparency = p.ObjectsTransparency,
					Context = p.Context,
					DrawValue = p.DrawValue,
					RenderValue = p.RenderValue
				});
			}

			_renderDataSeries[bar] = copy;
		}
		else
			_renderDataSeries[bar] = [];
	}

	private void CalculateBarFull(int bar)
	{
		var candle = GetCandle(bar);

		_lastSeriesBar.Clear();
		_pocPrice = 0;
		_pocVolume = 0;

		_lastBarFormationValid = CheckBarFormation(candle);

		if (!_lastBarFormationValid)
		{
			_renderDataSeries[bar] = new SyncList<PriceSelectionValue>();
			return;
		}

		_renderDataSeries[bar] = _lastSeriesBar;

		var endPrice = Math.Max(candle.Low, candle.High - (PriceRange - 1) * InstrumentInfo.TickSize);
		var totalVolume = GetTotalVolume(bar);
		var ranges = GetPriceRanges(bar, endPrice);

		if (CalcType is CalcMode.MaxVolume)
		{
			CustomVolumeInfo pocInfo = null;

			for (var price = candle.Low; price <= endPrice; price += InstrumentInfo.TickSize)
			{
				var info = GetMergedClusterInfo(bar, price);

				if (pocInfo is null || info.Volume > pocInfo.Volume)
					pocInfo = info;
			}

			if (pocInfo is null || pocInfo.Volume is 0)
				return;

			_pocPrice = pocInfo.Price;
			_pocVolume = pocInfo.Volume;

			var inRange = false;

			foreach (var range in ranges)
			{
				if (pocInfo.Price >= range.From && pocInfo.Price <= range.To)
				{
					inRange = true;
					break;
				}
			}

			if (inRange && CheckClusterFilters(pocInfo, totalVolume))
				PlaceToDataSeries(bar, pocInfo);

			return;
		}

		foreach (var range in ranges)
		{
			for (var price = range.From; price <= range.To; price += InstrumentInfo.TickSize)
			{
				var info = GetMergedClusterInfo(bar, price);

				if (CheckClusterFilters(info, totalVolume))
					PlaceToDataSeries(bar, info);
			}
		}
	}

	private CustomVolumeInfo GetMergedClusterInfo(int bar, decimal price)
	{
		var info = new CustomVolumeInfo(price);
		var endPrice = price + (PriceRange - 1) * InstrumentInfo.TickSize;
		var endBar = Math.Max(0, bar - (BarsRange - 1));

		for (var i = bar; i >= endBar; i--)
		{
			var candle = GetCandle(i);

			for (var p = price; p <= endPrice; p += InstrumentInfo.TickSize)
			{
				var level = candle.GetPriceVolumeInfo(p);

				if (level is null)
					continue;

				info.Ask += level.Ask;
				info.Bid += level.Bid;
				info.Between += level.Between;
				info.Volume += level.Volume;
				info.Ticks += level.Ticks;
			}
		}

		return info;
	}

	private bool CheckClusterFilters(CustomVolumeInfo info, decimal totalVolume)
	{
		var value = GetCalcValue(info);

		if (AutoFilter)
		{
			if (_autoFilterValue is not 0 && value < _autoFilterValue)
				return false;
		}

		if (MinimumFilter.Enabled && value < MinimumFilter.Value)
			return false;

		if (MaximumFilter.Enabled && value > MaximumFilter.Value)
			return false;

		var avgTrade = info.AvgTrade;

		if (MinAverageTrade != 0 && avgTrade < MinAverageTrade)
			return false;

		if (MaxAverageTrade != 0 && avgTrade > MaxAverageTrade)
			return false;

		if (MinPercent != 0 || MaxPercent != 0)
		{
			var curPerc = totalVolume is not 0 ? 100 * info.Volume / totalVolume : 0;

			if (curPerc < MinPercent || MaxPercent is not 0 && curPerc > MaxPercent)
				return false;
		}

		if (DeltaImbalance != 0)
		{
			var vol = info.Volume;
			var askImbalance = vol is not 0 ? info.Ask * 100.0m / vol : 0;
			var bidImbalance = vol is not 0 ? info.Bid * 100.0m / vol : 0;

			switch (DeltaImbalance)
			{
				case > 0 when askImbalance < DeltaImbalance:
				case < 0 when bidImbalance < Math.Abs(DeltaImbalance):
					return false;
			}
		}

		if (DeltaFilter != 0)
		{
			switch (DeltaFilter)
			{
				case > 0 when info.Delta < DeltaFilter:
				case < 0 when info.Delta > DeltaFilter:
					return false;
			}
		}

		return true;
	}

	private decimal GetCalcValue(CustomVolumeInfo info)
	{
		return CalcType switch
		{
			CalcMode.Bid => info.Bid,
			CalcMode.Ask => info.Ask,
			CalcMode.Delta => info.Delta,
			CalcMode.Volume or CalcMode.MaxVolume => info.Volume,
			CalcMode.Tick => info.Ticks,
			_ => 0
		};
	}

	private decimal GetTotalVolume(int bar)
	{
		var total = 0m;
		var endBar = Math.Max(0, bar - (BarsRange - 1));

		for (var i = bar; i >= endBar; i--)
			total += GetCandle(i).Volume;

		return total;
	}

	private List<(decimal From, decimal To)> GetPriceRanges(int bar, decimal endPrice)
	{
		var ranges = new List<(decimal From, decimal To)>();
		var candle = GetCandle(bar);

		var maxPrice = PipsFromLow.Enabled
			? candle.Low + PipsFromLow.Value * InstrumentInfo.TickSize
			: candle.High;

		var minPrice = PipsFromHigh.Enabled
			? candle.High - PipsFromHigh.Value * InstrumentInfo.TickSize
			: candle.Low;

		if (minPrice > maxPrice)
			return ranges;

		maxPrice = Math.Min(candle.High, maxPrice);
		minPrice = Math.Max(candle.Low, minPrice);

		switch (PriceLoc)
		{
			case PriceLocation.AtHigh when maxPrice != candle.High:
			case PriceLocation.AtLow when minPrice != candle.Low:
			case PriceLocation.AtHighOrLow when maxPrice != candle.High && minPrice != candle.Low:
				return ranges;

			case PriceLocation.Any:
				return [(minPrice, maxPrice)];

			case PriceLocation.AtHighOrLow:
			case PriceLocation.AtHigh:
			case PriceLocation.AtLow:
			{
				if (PriceLoc is PriceLocation.AtHighOrLow or PriceLocation.AtHigh)
				{
					if (maxPrice >= endPrice)
						ranges.Add((endPrice, endPrice));
				}

				if (PriceLoc is PriceLocation.AtHighOrLow or PriceLocation.AtLow)
				{
					if (minPrice <= candle.Low)
						ranges.Add((candle.Low, candle.Low));
				}

				return ranges;
			}
			case PriceLocation.AtUpperLowerWick or PriceLocation.UpperWick or PriceLocation.LowerWick or PriceLocation.Body:
			{
				var maxBody = Math.Max(candle.Close, candle.Open);
				var minBody = Math.Min(candle.Close, candle.Open);

				if (PriceLoc is PriceLocation.Body)
				{
					maxBody = Math.Min(maxBody, maxPrice);
					minBody = Math.Max(minBody, minPrice);
					return [(minBody, maxBody)];
				}

				if (PriceLoc is PriceLocation.UpperWick or PriceLocation.AtUpperLowerWick)
				{
					var upperWickFrom = maxBody + InstrumentInfo.TickSize;
					var upperWickTo = maxPrice - (PriceRange - 1) * InstrumentInfo.TickSize;

					if (upperWickTo >= upperWickFrom)
						ranges.Add((upperWickFrom, upperWickTo));
				}

				if (PriceLoc is PriceLocation.LowerWick or PriceLocation.AtUpperLowerWick)
				{
					var lowerWickFrom = minPrice;
					var lowerWickTo = minBody - PriceRange * InstrumentInfo.TickSize;

					if (lowerWickTo >= lowerWickFrom)
						ranges.Add((lowerWickFrom, lowerWickTo));
				}

				return ranges;
			}
		}

		return ranges;
	}

	private bool CheckBarFormation(IndicatorCandle candle)
	{
		if ((CandleDir is CandleDirection.Bearish && candle.Close >= candle.Open)
		    ||
		    (CandleDir is CandleDirection.Bullish && candle.Close <= candle.Open)
		    ||
		    (CandleDir is CandleDirection.Neutral && candle.Close != candle.Open))
			return false;

		if (UseTimeFilter)
		{
			var time = candle.Time.AddHours(InstrumentInfo.TimeZone);

			if (TimeFrom < TimeTo)
			{
				if (time < time.Date + TimeFrom)
					return false;

				if (time > time.Date + TimeTo)
					return false;
			}
			else
			{
				if (time < time.Date + TimeFrom && time > time.Date + TimeTo)
					return false;
			}
		}

		if (MinCandleHeight.Enabled || MaxCandleHeight.Enabled)
		{
			var height = (candle.High - candle.Low) / InstrumentInfo.TickSize + 1;

			if (MinCandleHeight.Enabled && height < MinCandleHeight.Value)
				return false;

			if (MaxCandleHeight.Enabled && height > MaxCandleHeight.Value)
				return false;
		}

		if (MinCandleBodyHeight.Enabled || MaxCandleBodyHeight.Enabled)
		{
			var bHeight = Math.Abs(candle.Close - candle.Open) / InstrumentInfo.TickSize + 1;

			if (MinCandleBodyHeight.Enabled && bHeight < MinCandleBodyHeight.Value)
				return false;

			if (MaxCandleBodyHeight.Enabled && bHeight > MaxCandleBodyHeight.Value)
				return false;
		}

		return true;
	}

	private void SetSize()
	{
		if (_fixedSizes)
		{
			for (var i = 0; i < _renderDataSeries.Count; i++)
				_renderDataSeries[i].ForEach(x => x.Size = _size);
		}
		else
		{
			var filterValue = MinimalFilter();

			for (var i = 0; i < _renderDataSeries.Count; i++)
			{
				_renderDataSeries[i].ForEach(x =>
				{
					x.Size = (int)((decimal)x.Context * _size / Math.Max(filterValue, 1));

					if (x.Size > MaxSize)
						x.Size = MaxSize;

					if (x.Size < MinSize)
						x.Size = MinSize;
				});
			}
		}
	}

	private void Filter_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		RecalculateValues();
		RedrawChart();
	}

	private void AddClusterAlert(string msg)
	{
		if (!UseAlerts)
			return;

		AddAlert(AlertFile, InstrumentInfo.Instrument, msg, AlertColor, ClusterColor);
	}

	private decimal MinimalFilter()
	{
		if (AutoFilter)
			return Math.Max(_autoFilterValue, 1);

		var minFilter = MinimumFilter.Enabled ? MinimumFilter.Value : 0;
		var maxFilter = MaximumFilter.Enabled ? MaximumFilter.Value : 0;

		if (MinimumFilter.Value >= 0 && MaximumFilter.Value >= 0)
			return minFilter;

		if (MinimumFilter.Value < 0 && MaximumFilter.Value >= 0)
			return Math.Min(Math.Abs(minFilter), maxFilter);

		return Math.Abs(maxFilter);
	}

	#endregion

	#region Filters

	[Browsable(false)]
	[Obsolete]
	public MiddleClusterType Type
	{
		get => CalcType switch
		{
			CalcMode.Bid => MiddleClusterType.Bid,
			CalcMode.Ask => MiddleClusterType.Ask,
			CalcMode.Delta => MiddleClusterType.Delta,
			CalcMode.Volume => MiddleClusterType.Volume,
			CalcMode.Tick => MiddleClusterType.Tick,
			_ => MiddleClusterType.Volume
		};
		set => CalcType = value switch
		{
			MiddleClusterType.Bid => CalcMode.Bid,
			MiddleClusterType.Ask => CalcMode.Ask,
			MiddleClusterType.Delta => CalcMode.Delta,
			MiddleClusterType.Volume or MiddleClusterType.Time => CalcMode.Volume,
			MiddleClusterType.Tick => CalcMode.Tick,
			_ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
		};
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Name = nameof(Strings.CalculationMode),
		Description = nameof(Strings.CalculationModeDescription), Order = 200)]
	public CalcMode CalcType
	{
		get => _type;
		set
		{
			_type = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Name = nameof(Strings.AutoFilter),
		Description = nameof(Strings.ClusterSearchAutofilterDescription), Order = 215)]
	public bool AutoFilter
	{
		get => _autoFilter;
		set
		{
			_autoFilter = value;

			if (value)
			{
				MinimumFilter.Enabled = true;
				MaximumFilter.Enabled = false;
			}

			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Description = nameof(Strings.MinimumFilterDescription),
		Name = nameof(Strings.MinValue), Order = 220)]
	public Filter MinimumFilter
	{
		get => _minFilter;
		set => SetTrackedProperty(ref _minFilter, value, _ => RecalculateValues());
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Description = nameof(Strings.MaximumFilterDescription),
		Name = nameof(Strings.MaxValue), Order = 230)]
	public Filter MaximumFilter
	{
		get => _maxFilter;
		set => SetTrackedProperty(ref _maxFilter, value, _ => RecalculateValues());
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Name = nameof(Strings.MinimumAverageTrade), Order = 470,
		Description = nameof(Strings.MinAvgTradeDescription))]
	[Range(0, 10000000)]
	public decimal MinAverageTrade
	{
		get => _minAverageTrade;
		set
		{
			_minAverageTrade = value;
			OnChangeProperty();

			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Name = nameof(Strings.MaximumAverageTrade), Order = 480,
		Description = nameof(Strings.MaxAvgTradeDescription))]
	[Range(0, 10000000)]
	public decimal MaxAverageTrade
	{
		get => _maxAverageTrade;
		set
		{
			_maxAverageTrade = value;
			OnChangeProperty();

			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Name = nameof(Strings.MinVolumePercent), Order = 490,
		Description = nameof(Strings.MinPercentDescription))]
	[Range(0, 100)]
	public decimal MinPercent
	{
		get => _minPercent;
		set
		{
			_minPercent = value;
			OnChangeProperty();

			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Filters), Name = nameof(Strings.MaxVolumePercent), Order = 492,
		Description = nameof(Strings.MaxPercentDescription))]
	[Range(0, 100)]
	public decimal MaxPercent
	{
		get => _maxPercent;
		set
		{
			_maxPercent = value;
			OnChangeProperty();

			RecalculateValues();
		}
	}

	#endregion

	#region DeltaFilters

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.DeltaFilters), Name = nameof(Strings.DeltaImbalance), Order = 300,
		Description = nameof(Strings.DeltaImbalanceDescription))]
	[Range(-100, 100)]
	public decimal DeltaImbalance
	{
		get => _deltaImbalance;
		set
		{
			_deltaImbalance = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.DeltaFilters), Name = nameof(Strings.DeltaFilter), Order = 310,
		Description = nameof(Strings.DeltaFilterDescription))]
	public decimal DeltaFilter
	{
		get => _deltaFilter;
		set
		{
			_deltaFilter = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Location filters

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.LocationFilters), Name = nameof(Strings.CandleDirection),
		Description = nameof(Strings.CandleDirectionDescription), Order = 400)]
	public CandleDirection CandleDir
	{
		get => _candleDirection;
		set
		{
			_candleDirection = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.LocationFilters), Name = nameof(Strings.BarsRange), Order = 410,
		Description = nameof(Strings.BarsRangeDescription))]
	[Range(1, 10000)]
	public int BarsRange
	{
		get => _barsRange;
		set
		{
			_barsRange = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.LocationFilters), Name = nameof(Strings.PriceRange), Order = 420,
		Description = nameof(Strings.PriceRangeDescription))]
	[Range(1, 100000)]
	public int PriceRange
	{
		get => _priceRange;
		set
		{
			_priceRange = value;
			RecalculateValues();
		}
	}

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.LocationFilters), Name = nameof(Strings.PipsFromHigh), Order = 430,
		Description = nameof(Strings.PipsFromHighDescription))]
	public Filter PipsFromHigh
	{
		get => _pipsFromHigh;
		set
		{
			_pipsFromHigh = value;
			RecalculateValues();
		}
	}

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.LocationFilters), Name = nameof(Strings.PipsFromLow), Order = 440,
		Description = nameof(Strings.PipsFromLowDescription))]
	public Filter PipsFromLow
	{
		get => _pipsFromLow;
		set
		{
			_pipsFromLow = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.LocationFilters), Name = nameof(Strings.PriceLocation), Order = 450,
		Description = nameof(Strings.PriceLocationDescription))]
	public PriceLocation PriceLoc
	{
		get => _priceLocation;
		set
		{
			_priceLocation = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Candle size filters

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimumCandleHeight), GroupName = nameof(Strings.CandleHeight), Order = 460,
		Description = nameof(Strings.MinCandleHeightDescription))]
	public FilterInt MinCandleHeight { get; set; } = new()
		{ Value = 1, Enabled = false };

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MaximumCandleHeight), GroupName = nameof(Strings.CandleHeight), Order = 461,
		Description = nameof(Strings.MaxCandleHeightDescription))]
	public FilterInt MaxCandleHeight { get; set; } = new()
		{ Value = 1, Enabled = false };

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MinimumCandleBodyHeight), GroupName = nameof(Strings.CandleHeight), Order = 470,
		Description = nameof(Strings.MinCandleBodyHeightDescription))]
	public FilterInt MinCandleBodyHeight { get; set; } = new()
		{ Value = 1, Enabled = false };

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.MaximumCandleBodyHeight), GroupName = nameof(Strings.CandleHeight), Order = 471,
		Description = nameof(Strings.MaxCandleBodyHeightDescription))]
	public FilterInt MaxCandleBodyHeight { get; set; } = new()
		{ Value = 1, Enabled = false };

	#endregion

	#region Time filtration

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.TimeFiltration), Name = nameof(Strings.UseTimeFilter), Order = 500,
		Description = nameof(Strings.UseTimeFilterDescription))]
	public bool UseTimeFilter
	{
		get => _useTimeFilter;
		set
		{
			_useTimeFilter = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.TimeFiltration), Name = nameof(Strings.TimeFrom), Order = 510,
		Description = nameof(Strings.TimeFromDescription))]
	public TimeSpan TimeFrom
	{
		get => _timeFrom;
		set
		{
			_timeFrom = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.TimeFiltration), Name = nameof(Strings.TimeTo), Order = 520,
		Description = nameof(Strings.TimeToDescription))]
	public TimeSpan TimeTo
	{
		get => _timeTo;
		set
		{
			_timeTo = value;
			RecalculateValues();
		}
	}

	#endregion

	#region Visualization

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.OnlyOneSelectionPerBar), Order = 590,
		Description = nameof(Strings.OneSelectionPerBarDescription))]
	public bool OnlyOneSelectionPerBar
	{
		get => _onlyOneSelectionPerBar;
		set
		{
			_onlyOneSelectionPerBar = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.VisualMode), Order = 600,
		Description = nameof(Strings.VisualModeDescription))]
	public ObjectType VisualType
	{
		get => _visualType;
		set
		{
			_visualType = value;

			for (var i = 0; i < _renderDataSeries.Count; i++)
				_renderDataSeries[i].ForEach(x => { x.VisualObject = value; });
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.ObjectsColor), Order = 605,
		Description = nameof(Strings.VisualObjectsDescription))]
	public CrossColor ClusterColor
	{
		get => _clusterTransColor;
		set
		{
			_clusterTransColor = value;

			for (var i = 0; i < _renderDataSeries.Count; i++)
				_renderDataSeries[i].ForEach(x => { x.ObjectColor = _clusterTransColor; });
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.VisualObjectsTransparency), Order = 610,
		Description = nameof(Strings.VisualObjectsTransparencyDescription))]
	[Range(0, 100)]
	public int VisualObjectsTransparency
	{
		get => _visualObjectsTransparency;
		set
		{
			_visualObjectsTransparency = value;

			for (var i = 0; i < _renderDataSeries.Count; i++)
			{
				_renderDataSeries[i].ForEach(x =>
				{
					x.ObjectColor = _clusterTransColor;
					x.ObjectsTransparency = _visualObjectsTransparency;
				});
			}
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.ShowPriceSelection), Order = 615,
		Description = nameof(Strings.ShowPriceSelectionDescription))]
	public bool ShowPriceSelection
	{
		get => _showPriceSelection;
		set
		{
			_showPriceSelection = value;

			for (var i = 0; i < _renderDataSeries.Count; i++)
				_renderDataSeries[i].ForEach(x => { x.PriceSelectionColor = value ? _clusterPriceColor : CrossColors.Transparent; });
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.PriceSelectionColor), Order = 620,
		Description = nameof(Strings.PriceSelectionColorDescription))]
	public CrossColor PriceSelectionColor
	{
		get => _clusterPriceColor;
		set
		{
			_clusterPriceColor = value;

			for (var i = 0; i < _renderDataSeries.Count; i++)
				_renderDataSeries[i].ForEach(x => x.PriceSelectionColor = ShowPriceSelection ? _clusterPriceColor : CrossColors.Transparent);
		}
	}

	[Browsable(false)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.ClusterSelectionTransparency), Order = 625,
		Description = nameof(Strings.PriceSelectionTransparencyDescription))]
	[Range(0, 100)]
	public int Transparency { get; set; }

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.FixedSizes), Order = 640,
		Description = nameof(Strings.FixedSizesDescription))]
	public bool FixedSizes
	{
		get => _fixedSizes;
		set
		{
			_fixedSizes = value;
			SetSize();
		}
	}

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.Size), Order = 650,
		Description = nameof(Strings.SizeDescription))]
	public int Size
	{
		get => _size;
		set
		{
			_size = value;
			SetSize();
		}
	}

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.MinimumSize), Order = 660,
		Description = nameof(Strings.MinimumSizeDescription))]
	public int MinSize
	{
		get => _minSize;
		set
		{
			_minSize = value;

			if (!_fixedSizes)
			{
				var filterValue = MinimalFilter();

				for (var i = 0; i < _renderDataSeries.Count; i++)
				{
					_renderDataSeries[i].ForEach(x =>
					{
						x.Size = (int)((decimal)x.Context * _size / Math.Max(filterValue, 1));

						if (x.Size > MaxSize)
							x.Size = MaxSize;

						if (x.Size < value)
							x.Size = value;
					});
				}
			}
		}
	}

	[Range(1, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Visualization), Name = nameof(Strings.MaximumSize), Order = 670,
		Description = nameof(Strings.MaximumSizeDescription))]
	public int MaxSize
	{
		get => _maxSize;
		set
		{
			_maxSize = value;

			if (!_fixedSizes)
			{
				var filterValue = MinimalFilter();

				for (var i = 0; i < _renderDataSeries.Count; i++)
				{
					_renderDataSeries[i].ForEach(x =>
					{
						x.Size = (int)((decimal)x.Context * _size / Math.Max(filterValue, 1));

						if (x.Size > value)
							x.Size = value;

						if (x.Size < MinSize)
							x.Size = MinSize;
					});
				}
			}
		}
	}

	#endregion

	#region Alerts

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Alerts), Name = nameof(Strings.UseAlerts), Order = 700,
		Description = nameof(Strings.UseAlertDescription))]
	public bool UseAlerts { get; set; }

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Alerts), Name = nameof(Strings.AlertFile), Order = 720,
		Description = nameof(Strings.AlertFileDescription))]
	public string AlertFile { get; set; } = "alert2";

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Alerts), Name = nameof(Strings.BackGround), Order = 740,
		Description = nameof(Strings.AlertBackgroundDescription))]
	public CrossColor AlertColor { get; set; } = CrossColors.Black;

	#endregion

	#region Calculation

	[Range(0, int.MaxValue)]
	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Calculation), Name = nameof(Strings.DaysLookBack), Order = int.MaxValue,
		Description = nameof(Strings.DaysLookBackDescription))]
	public int Days
	{
		get => _days;
		set
		{
			_days = value;
			RecalculateValues();
		}
	}

	[Display(ResourceType = typeof(Strings), GroupName = nameof(Strings.Calculation), Name = nameof(Strings.UsePreviousClose), Order = 800,
		Description = nameof(Strings.CalculateOnBarCloseDescription))]
	public bool UsePrevClose
	{
		get => _usePrevClose;
		set
		{
			_usePrevClose = value;
			RecalculateValues();
		}
	}

	#endregion
}
