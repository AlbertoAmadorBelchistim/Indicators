namespace ATAS.Indicators.Technical;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;

using ATAS.Indicators.Drawing;

using OFT.Attributes;
using OFT.Rendering;
using OFT.Rendering.Context;
using OFT.Rendering.Control;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;

using ATAS.Indicators.Technical.ClusterStatsCore;

using Utils.Common.Logging;

using Color = CrossColor;
using Res = ATAS.Indicators.Technical.ClusterStatisticProResources;

[DisplayName("Cluster Statistic Pro")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(Res), Description = nameof(Res.ClusterStatisticDescription))]
public class ClusterStatisticPro : Indicator
{
	#region Nested types

	public class SortedRows : SortedList<int, DataType>
	{
		#region Properties

		public int SkipIdx { get; set; } = -1;

		#endregion
	}

	public class RenderOrder : Dictionary<DataType, RenderInfo>
	{
		#region Fields

		public readonly SortedRows AvailableStrings = new();
		public Action OnChanged;

		#endregion

		#region ctor

		public RenderOrder()
		{
			Add(DataType.Ask, new RenderInfo(0));
			Add(DataType.Bid, new RenderInfo(1));
			Add(DataType.Delta, new RenderInfo(2));
			Add(DataType.DeltaVolume, new RenderInfo(3));
			Add(DataType.SessionDelta, new RenderInfo(4));
			Add(DataType.SessionDeltaVolume, new RenderInfo(5));
			Add(DataType.MaxDelta, new RenderInfo(6));
			Add(DataType.MinDelta, new RenderInfo(7));
			Add(DataType.DeltaChange, new RenderInfo(8));
			Add(DataType.Volume, new RenderInfo(9));
			Add(DataType.VolumeSecond, new RenderInfo(10));
			Add(DataType.SessionVolume, new RenderInfo(11));
			Add(DataType.Trades, new RenderInfo(12));
			Add(DataType.Height, new RenderInfo(13));
			Add(DataType.Time, new RenderInfo(14));
			Add(DataType.Duration, new RenderInfo(15));
			Add(DataType.DeltaSecond, new RenderInfo(16));
			Add(DataType.BuyImbalance, new RenderInfo(17));
			Add(DataType.SellImbalance, new RenderInfo(18));
			Add(DataType.NetImbalance, new RenderInfo(19));
			Add(DataType.StackedBuyImbalance, new RenderInfo(20));
			Add(DataType.StackedSellImbalance, new RenderInfo(21));
			Add(DataType.StackedNetImbalance, new RenderInfo(22));
			Add(DataType.PeakVolPerSec, new RenderInfo(23));
			Add(DataType.PeakDeltaPerSec, new RenderInfo(24));
			Add(DataType.PeakDeltaPerVol, new RenderInfo(25));
		}

		#endregion

		#region Public methods

		public void SetEnabled(DataType type, bool enabled)
		{
			this[type].Enabled = enabled;
			RebuildCache();
		}

		public void UpdateOrder(DataType from, DataType to)
		{
			var fromOrder = this[from].Order;
			var toOrder = this[to].Order;

			if (fromOrder > toOrder)
			{
				foreach (var row in this.Where(row => row.Value.Order < fromOrder && row.Value.Order >= toOrder))
					row.Value.Order++;
			}
			else
			{
				foreach (var row in this.Where(row => row.Value.Order > fromOrder && row.Value.Order <= toOrder))
					row.Value.Order--;
			}

			this[from].Order = toOrder;
			RebuildCache();
		}

		#endregion

		#region Private methods

		private void RebuildCache()
		{
			AvailableStrings.Clear();

			foreach (var (type, info) in this)
			{
				if (!info.Enabled)
					continue;

				AvailableStrings.Add(info.Order, type);
			}

			OnChanged?.Invoke();
		}

		#endregion
	}

	/// <summary>
	/// Largest absolute value of a series over the closed bars, kept up to date from OnCalculate so
	/// rendering does not scan the history. The bar in progress is added when read: its value is
	/// not final, so it must not stay in the maximum.
	/// </summary>
	private sealed class ClosedBarsMax(ValueDataSeries series)
	{
		private decimal _max;
		private int _closedBars;

		public void Update(int bar)
		{
			if (bar == 0)
			{
				_max = 0;
				_closedBars = 0;
			}

			for (; _closedBars < bar; _closedBars++)
				_max = Math.Max(Math.Abs(series[_closedBars]), _max);
		}

		public decimal Value(int currentBar)
		{
			return currentBar > 0 ? Math.Max(_max, Math.Abs(series[currentBar - 1])) : 0;
		}

		// For values written after OnCalculate went past them (the tape history arrives later).
		public void Recompute(int currentBar)
		{
			_max = 0;
			_closedBars = 0;
			Update(Math.Max(0, currentBar - 1));
		}

		public decimal Visible(int firstBar, int lastBar)
		{
			var max = 0m;

			for (var i = Math.Max(0, firstBar); i <= lastBar; i++)
				max = Math.Max(Math.Abs(series[i]), max);

			return max;
		}
	}

	public class RenderInfo(int order, bool enabled = false)
	{
		#region Properties

		public int Order { get; set; } = order;

		public bool Enabled { get; set; } = enabled;

		#endregion
	}

	private struct MaxValues
	{
		public decimal MaxAsk { get; set; }

		public decimal MaxBid { get; set; }

		public decimal MaxSessionDelta { get; set; }

		public decimal MaxDeltaPerVolume { get; set; }

		public decimal MaxSessionDeltaPerVolume { get; set; }

		public decimal MaxDelta { get; set; }

		public decimal MinDelta { get; set; }

		public decimal MaxMaxDelta { get; set; }

		public decimal MaxMinDelta { get; set; }

		public decimal MaxVolume { get; set; }

		public decimal MaxTicks { get; set; }

		public decimal MaxDuration { get; set; }

		public decimal CumVolume { get; set; }

		public decimal MaxDeltaChange { get; set; }

		public decimal MaxHeight { get; set; }

		public decimal MaxVolumeSec { get; set; }
	}

	public enum DataType
	{
		Ask,
		Bid,
		Delta,
		DeltaVolume,
		SessionDelta,
		SessionDeltaVolume,
		MaxDelta,
		MinDelta,
		DeltaChange,
		Volume,
		VolumeSecond,
		SessionVolume,
		Trades,
		Height,
		Time,
		Duration,
		DeltaSecond,
		BuyImbalance,
		SellImbalance,
		NetImbalance,
		StackedBuyImbalance,
		StackedSellImbalance,
		StackedNetImbalance,
		PeakVolPerSec,
		PeakDeltaPerSec,
		PeakDeltaPerVol,
		None
	}

	public enum SessionMode
	{
		[Display(ResourceType = typeof(Res), Name = nameof(Res.None))]
		None,

		[Display(ResourceType = typeof(Res), Name = nameof(Res.Default))]
		DefaultSession,

		[Display(ResourceType = typeof(Res), Name = nameof(Res.CustomSession))]
		CustomSession
	}

	#endregion

	#region Static and constants

	private const int _headerOffset = 3;

	private static readonly RenderStringFormat _tipFormat = new()
	{
		Alignment = StringAlignment.Center,
		LineAlignment = StringAlignment.Center
	};

	#endregion

	#region Fields

	private readonly ValueDataSeries _candleDurations = new("durations");
	private readonly ValueDataSeries _candleHeights = new("heights");
	private readonly ValueDataSeries _cDelta = new("cDelta");
	private readonly ValueDataSeries _cDeltaPerVol = new("DeltaPerVol");
	private readonly ValueDataSeries _cVolume = new("cVolume");
	private readonly ValueDataSeries _deltaChange = new("deltaChange");
	private readonly ValueDataSeries _deltaPerVol = new("BarDeltaPerVol");

	private readonly RenderStringFormat _stringLeftFormat = new()
	{
		Alignment = StringAlignment.Near,
		LineAlignment = StringAlignment.Center,
		Trimming = StringTrimming.EllipsisCharacter,
		FormatFlags = StringFormatFlags.NoWrap
	};

	private readonly ValueDataSeries _volPerSecond = new("VolPerSecond");
	private readonly ValueDataSeries _deltaPerSecond = new("DeltaPerSecond");
	private readonly ValueDataSeries _buyImbalance = new("BuyImbalance");
	private readonly ValueDataSeries _sellImbalance = new("SellImbalance");
	private readonly ValueDataSeries _netImbalance = new("NetImbalance");
	private readonly ValueDataSeries _stackedBuyImbalance = new("StackedBuyImbalance");
	private readonly ValueDataSeries _stackedSellImbalance = new("StackedSellImbalance");
	private readonly ValueDataSeries _stackedNetImbalance = new("StackedNetImbalance");
	private readonly ImbalanceSettings _imbalance = new();

	// Speed of the tape (peak rows): the engine, the history request and the realtime prints that
	// arrive while the history is loading, guarded by _tapeSync. _tape is null while no peak row
	// is shown.
	private readonly ValueDataSeries _peakVolPerSec = new("PeakVolPerSec");
	private readonly ValueDataSeries _peakDeltaPerSec = new("PeakDeltaPerSec");
	private readonly ValueDataSeries _peakDeltaPerVol = new("PeakDeltaPerVol");
	private readonly object _tapeSync = new();
	private readonly List<Tick> _pendingTicks = new();
	private PeakRateEngine _tape;
	private bool _tapeHistoryLoaded;
	private int _tapeRequestId;
	private DateTime _lastTickTime;
	private int _lastTickCount;
	private DateTime _bufferStartTime;
	private int _bufferStartCount;
	private int _tapeWindowSeconds = 5;
	private int _tapeMinVolume = 150;
	private int _tapeSessionsToLoad = 2;
	private int _firstTapeBar = -1;
	private PeakMean _meanVolPerSec;
	private PeakMean _meanDeltaPerSec;
	private bool _peakAutoFilter = true;
	private int _peakAutoFilterPeriod = 3;
	private bool _peakAutoFilterEma = true;
	private readonly List<PriceLevel> _levels = new();

	// Maxima of the rows added by this indicator, by row.
	private readonly Dictionary<DataType, ClosedBarsMax> _rowMaxima = new();
	private readonly Dictionary<DataType, decimal> _rowScale = new();
	private bool _atHeader;

	private bool _atPanel;

	private byte _bgAlpha = 255;
	private int _bgTransparency = 10;
	private bool _centerAlign;
	private decimal _cumVolume;
	private FontSetting _font;
	private bool _layoutChanged = true;
	private System.Drawing.Color _headerBackground = System.Drawing.Color.FromArgb(0xFF, 84, 84, 84);

	private int _headerWidth = 130;

	private int _height = 15;

	private int _lastBar = -1;
	private bool _alertsArmed;
	private int _lastAskAlert;
	private decimal _lastAskValue;
	private int _lastBidAlert;
	private decimal _lastBidValue;
	private int _lastDeltaAlert;
	private decimal _lastDeltaValue;
	private int _lastDeltaPerVolumeAlert;
	private decimal _lastDeltaPerVolumeValue;
	private int _lastSessionDeltaAlert;
	private decimal _lastSessionDeltaValue;
	private int _lastSessionDeltaPerVolumeAlert;
	private decimal _lastSessionDeltaPerVolumeValue;
	private int _lastMaxDeltaAlert;
	private decimal _lastMaxDeltaValue;
	private int _lastMinDeltaAlert;
	private decimal _lastMinDeltaValue;
	private int _lastDeltaChangeAlert;
	private decimal _lastDeltaChangeValue;
	private int _lastVolumeAlert;
	private decimal _lastVolumeValue;
	private int _lastVolumePerSecondAlert;
	private decimal _lastVolumePerSecondValue;
	private int _lastSessionVolumeAlert;
	private decimal _lastSessionVolumeValue;
	private int _lastTradesAlert;
	private decimal _lastTradesValue;
	private int _lastHeightAlert;
	private bool _useNetImbalanceAlert;
	private bool _netImbalanceAlertOnClose;
	private readonly NetImbalanceAlert _netImbalanceAlert = new();
	private decimal _lastHeightValue;

	private RenderPen _linePen = new(System.Drawing.Color.Transparent);
	private decimal _maxAsk;
	private decimal _maxBid;
	private decimal _maxDelta;
	private decimal _maxDeltaChange;
	private decimal _maxDeltaPerVolume;
	private decimal _maxDuration;
	private decimal _maxClosedVolumeSec;
	private int _volumeSecClosedBars;
	private decimal _maxHeight;
	private decimal _maxMaxDelta;
	private decimal _maxMinDelta;
	private decimal _maxSessionDelta;
	private decimal _maxSessionDeltaPerVolume;
	private decimal _maxTicks;
	private decimal _maxVolume;
	private decimal _minDelta;

	private DataType _pressedString = DataType.None;
	
	private int _selectionOffset;
	private RenderPen _selectionPen = new(System.Drawing.Color.Transparent, 3);
	private int _selectionY;
	private bool _showAsk;
	private bool _showBid;
	private bool _showDelta;
	private bool _showDeltaChange;
	private bool _showDeltaPerVolume;
	private bool _showDuration;
	private bool _showHighLow;
	private bool _showMaximumDelta;
	private bool _showMinimumDelta;
	private bool _showSessionDelta;
	private bool _showSessionDeltaPerVolume;
	private bool _showSessionVolume;
	private bool _showTicks;
	private bool _showTime;
	private bool _showVolume;
	private bool _showVolumePerSecond;
	private bool _showDeltaPerSecond;
	private System.Drawing.Color _textColor;
	private int _fontHeight;
	private SessionMode _sessionMode = SessionMode.DefaultSession;

	[Browsable(false)]
	public RenderOrder RowsOrder = new();
    private FilterTimeSpan _customSessionStart;

    #endregion

    #region Properties

    private int StrCount => RowsOrder.AvailableStrings.Count;
	
    #region Rows

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowAsk), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowAsksDescription), Order = 110, ResourceType = typeof(Res))]
    public bool ShowAsk
    {
        get => _showAsk;
        set
        {
            _showAsk = value;
            RowsOrder.SetEnabled(DataType.Ask, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowBid), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowBidsDescription), Order = 110, ResourceType = typeof(Res))]
    public bool ShowBid
    {
        get => _showBid;
        set
        {
            _showBid = value;
            RowsOrder.SetEnabled(DataType.Bid, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowDelta), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowDeltaDescription), Order = 120, ResourceType = typeof(Res))]
    public bool ShowDelta
    {
        get => _showDelta;
        set
        {
            _showDelta = value;
            RowsOrder.SetEnabled(DataType.Delta, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowDeltaPerVolume), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowDeltaPerVolumeDescription), Order = 130, ResourceType = typeof(Res))]
    public bool ShowDeltaPerVolume
    {
        get => _showDeltaPerVolume;
        set
        {
            _showDeltaPerVolume = value;
            RowsOrder.SetEnabled(DataType.DeltaVolume, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowSessionDelta), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowSessionDeltaDescription), Order = 140, ResourceType = typeof(Res))]
    public bool ShowSessionDelta
    {
        get => _showSessionDelta;
        set
        {
            _showSessionDelta = value;
            RowsOrder.SetEnabled(DataType.SessionDelta, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowSessionDeltaPerVolume), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowSessionDeltaPerVolumeDescription), Order = 150, ResourceType = typeof(Res))]
    public bool ShowSessionDeltaPerVolume
    {
        get => _showSessionDeltaPerVolume;
        set
        {
            _showSessionDeltaPerVolume = value;
            RowsOrder.SetEnabled(DataType.SessionDeltaVolume, value);

            if (value)
                _headerWidth = 180;
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowMaximumDelta), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowMaximumDeltaDescription), Order = 160, ResourceType = typeof(Res))]
    public bool ShowMaximumDelta
    {
        get => _showMaximumDelta;
        set
        {
            _showMaximumDelta = value;
            RowsOrder.SetEnabled(DataType.MaxDelta, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowMinimumDelta), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowMinimumDeltaDescription), Order = 170, ResourceType = typeof(Res))]
    public bool ShowMinimumDelta
    {
        get => _showMinimumDelta;
        set
        {
            _showMinimumDelta = value;
            RowsOrder.SetEnabled(DataType.MinDelta, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowDeltaChange), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowDeltaChangeDescription), Order = 175, ResourceType = typeof(Res))]
    public bool ShowDeltaChange
    {
        get => _showDeltaChange;
        set
        {
            _showDeltaChange = value;
            RowsOrder.SetEnabled(DataType.DeltaChange, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowVolume), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowVolumesDescription), Order = 180, ResourceType = typeof(Res))]
    public bool ShowVolume
    {
        get => _showVolume;
        set
        {
            _showVolume = value;
            RowsOrder.SetEnabled(DataType.Volume, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowVolumePerSecond), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowVolumePerSecondDescription), Order = 190, ResourceType = typeof(Res))]
    public bool ShowVolumePerSecond
    {
        get => _showVolumePerSecond;
        set
        {
            _showVolumePerSecond = value;
            RowsOrder.SetEnabled(DataType.VolumeSecond, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowDeltaPerSecond), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowDeltaPerSecondDescription), Order = 190, ResourceType = typeof(Res))]
    public bool ShowDeltaPerSecond
    {
        get => _showDeltaPerSecond;
        set
        {
            _showDeltaPerSecond = value;
            RowsOrder.SetEnabled(DataType.DeltaSecond, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowBuyImbalances), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowBuyImbalancesDescription), Order = 197, ResourceType = typeof(Res))]
    public bool ShowBuyImbalance
    {
        get => RowsOrder[DataType.BuyImbalance].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.BuyImbalance, value);
            OnImbalanceUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowSellImbalances), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowSellImbalancesDescription), Order = 198, ResourceType = typeof(Res))]
    public bool ShowSellImbalance
    {
        get => RowsOrder[DataType.SellImbalance].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.SellImbalance, value);
            OnImbalanceUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowNetImbalances), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowNetImbalancesDescription), Order = 199, ResourceType = typeof(Res))]
    public bool ShowNetImbalance
    {
        get => RowsOrder[DataType.NetImbalance].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.NetImbalance, value);
            OnImbalanceUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowStackedBuyImbalances), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowStackedBuyImbalancesDescription), Order = 199, ResourceType = typeof(Res))]
    public bool ShowStackedBuyImbalance
    {
        get => RowsOrder[DataType.StackedBuyImbalance].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.StackedBuyImbalance, value);
            OnImbalanceUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowStackedSellImbalances), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowStackedSellImbalancesDescription), Order = 199, ResourceType = typeof(Res))]
    public bool ShowStackedSellImbalance
    {
        get => RowsOrder[DataType.StackedSellImbalance].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.StackedSellImbalance, value);
            OnImbalanceUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowStackedNetImbalances), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowStackedNetImbalancesDescription), Order = 199, ResourceType = typeof(Res))]
    public bool ShowStackedNetImbalance
    {
        get => RowsOrder[DataType.StackedNetImbalance].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.StackedNetImbalance, value);
            OnImbalanceUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowPeakVolPerSec), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowPeakVolPerSecDescription), Order = 190, ResourceType = typeof(Res))]
    public bool ShowPeakVolPerSec
    {
        get => RowsOrder[DataType.PeakVolPerSec].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.PeakVolPerSec, value);
            OnTapeUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowPeakDeltaPerSec), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowPeakDeltaPerSecDescription), Order = 190, ResourceType = typeof(Res))]
    public bool ShowPeakDeltaPerSec
    {
        get => RowsOrder[DataType.PeakDeltaPerSec].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.PeakDeltaPerSec, value);
            OnTapeUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowPeakDeltaPerVol), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowPeakDeltaPerVolDescription), Order = 190, ResourceType = typeof(Res))]
    public bool ShowPeakDeltaPerVol
    {
        get => RowsOrder[DataType.PeakDeltaPerVol].Enabled;
        set
        {
            RowsOrder.SetEnabled(DataType.PeakDeltaPerVol, value);
            OnTapeUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowSessionVolume), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowSessionVolumeDescription), Order = 191, ResourceType = typeof(Res))]
    public bool ShowSessionVolume
    {
        get => _showSessionVolume;
        set
        {
            _showSessionVolume = value;
            RowsOrder.SetEnabled(DataType.SessionVolume, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowTradesCount), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowTradesCountDescription), Order = 192, ResourceType = typeof(Res))]
    public bool ShowTicks
    {
        get => _showTicks;
        set
        {
            _showTicks = value;
            RowsOrder.SetEnabled(DataType.Trades, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowHeight), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowCandleHeightDescription), Order = 193, ResourceType = typeof(Res))]
    public bool ShowHighLow
    {
        get => _showHighLow;
        set
        {
            _showHighLow = value;
            RowsOrder.SetEnabled(DataType.Height, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowTime), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowCandleTimeDescription), Order = 194, ResourceType = typeof(Res))]
    public bool ShowTime
    {
        get => _showTime;
        set
        {
            _showTime = value;
            RowsOrder.SetEnabled(DataType.Time, value);
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ShowDuration), GroupName = nameof(Res.Rows), Description = nameof(Res.ShowCandleDurationDescription), Order = 196, ResourceType = typeof(Res))]
    public bool ShowDuration
    {
        get => _showDuration;
        set
        {
            _showDuration = value;
            RowsOrder.SetEnabled(DataType.Duration, value);
        }
    }

    #endregion

    #region Session

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.SessionDeltaMode), GroupName = nameof(Res.Session), Description = nameof(Res.SessionModeDescription), Order = 100, ResourceType = typeof(Res))]
    public SessionMode SessionCumMode
    {
        get => _sessionMode;
        set
        {
            _sessionMode = value;
            CustomSessionStart.Enabled = value == SessionMode.CustomSession;
            RecalculateValues();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.CustomSessionStart), GroupName = nameof(Res.Session), Description = nameof(Res.CustomSessionStartDescription), Order = 110, ResourceType = typeof(Res))]
    public FilterTimeSpan CustomSessionStart
	{
		get => _customSessionStart;
		set => SetTrackedProperty(ref _customSessionStart, value, propName =>
		{
			if (propName == nameof(FilterTimeSpan.Value) && _sessionMode == SessionMode.CustomSession)
				RecalculateValues();
		});
    }

    #endregion

    #region Speed of tape

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.SotTimeWindowSecName), GroupName = nameof(Res.MaxVolPerSecGroup), Description = nameof(Res.SotTimeWindowSecDescription), Order = 140, ResourceType = typeof(Res))]
    [Range(1, 600)]
    public int SotTimeWindowSec
    {
        get => _tapeWindowSeconds;
        set
        {
            _tapeWindowSeconds = Math.Max(1, value);
            OnTapeSettingsChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.SotMinVolumeName), GroupName = nameof(Res.MaxVolPerSecGroup), Description = nameof(Res.SotMinVolumeDescription), Order = 141, ResourceType = typeof(Res))]
    [Range(0, 1000000)]
    public int SotMinVolume
    {
        get => _tapeMinVolume;
        set
        {
            _tapeMinVolume = Math.Max(0, value);
            OnTapeSettingsChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.TapeSessionsToLoad), GroupName = nameof(Res.MaxVolPerSecGroup), Description = nameof(Res.TapeSessionsToLoadDescription), Order = 142, ResourceType = typeof(Res))]
    [Range(0, 100)]
    public int TapeSessionsToLoad
    {
        get => _tapeSessionsToLoad;
        set
        {
            _tapeSessionsToLoad = Math.Max(0, value);
            OnTapeSettingsChanged();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.SotUseAutoFilterName), GroupName = nameof(Res.MaxVolPerSecGroup), Description = nameof(Res.SotUseAutoFilterDescription), Order = 143, ResourceType = typeof(Res))]
    public bool SotUseAutoFilter
    {
        get => _peakAutoFilter;
        set
        {
            _peakAutoFilter = value;
            RebuildPeakMeans();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.SotAutoFilterPeriodName), GroupName = nameof(Res.MaxVolPerSecGroup), Description = nameof(Res.SotAutoFilterPeriodDescription), Order = 144, ResourceType = typeof(Res))]
    [Range(1, 200)]
    public int SotAutoFilterPeriod
    {
        get => _peakAutoFilterPeriod;
        set
        {
            _peakAutoFilterPeriod = Math.Max(1, value);
            RebuildPeakMeans();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.SotAutoFilterUseEmaName), GroupName = nameof(Res.MaxVolPerSecGroup), Description = nameof(Res.SotAutoFilterUseEmaDescription), Order = 145, ResourceType = typeof(Res))]
    public bool SotAutoFilterUseEma
    {
        get => _peakAutoFilterEma;
        set
        {
            _peakAutoFilterEma = value;
            RebuildPeakMeans();
        }
    }

    #endregion

    #region Imbalances

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ImbalanceThresholdPercentName), GroupName = nameof(Res.ImbalanceGroup), Description = nameof(Res.ImbalanceThresholdPercentDescription), Order = 150, ResourceType = typeof(Res))]
    [Range(101, 10000)]
    public int ImbalanceThreshold
    {
        get => (int)_imbalance.RatioPercent;
        set
        {
            _imbalance.RatioPercent = value;
            RebuildImbalances();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ImbalanceMinDominantVolumeName), GroupName = nameof(Res.ImbalanceGroup), Description = nameof(Res.ImbalanceMinDominantVolumeDescription), Order = 151, ResourceType = typeof(Res))]
    [Range(0, 1000000)]
    public int ImbalanceMinDominantVolume
    {
        get => (int)_imbalance.MinDominantVolume;
        set
        {
            _imbalance.MinDominantVolume = value;
            RebuildImbalances();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.ImbalanceMinDifferenceName), GroupName = nameof(Res.ImbalanceGroup), Description = nameof(Res.ImbalanceMinDifferenceDescription), Order = 152, ResourceType = typeof(Res))]
    [Range(0, 1000000)]
    public int ImbalanceMinDifference
    {
        get => (int)_imbalance.MinDifference;
        set
        {
            _imbalance.MinDifference = value;
            RebuildImbalances();
        }
    }

    [Tab(TabName = nameof(Res.Data), TabOrder = 0, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.StackedImbalanceMinLevelsName), GroupName = nameof(Res.ImbalanceGroup), Description = nameof(Res.StackedImbalanceMinLevelsDescription), Order = 153, ResourceType = typeof(Res))]
    [Range(2, 20)]
    public int StackedImbalanceMinLevels
    {
        get => _imbalance.StackedMinLevels;
        set
        {
            _imbalance.StackedMinLevels = value;
            RebuildImbalances();
        }
    }

    #endregion

    #region Colors

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.BackGround), GroupName = nameof(Res.Visualization), Description = nameof(Res.LabelFillColorDescription), Order = 200, ResourceType = typeof(Res))]
    public Color BackGroundColor { get; set; } = Color.FromArgb(120, 0, 0, 0);

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Transparency), GroupName = nameof(Res.Visualization), Order = 205, ResourceType = typeof(Res))]
    [Range(1, 10)]
    public int BgTransparency
    {
        get => _bgTransparency;
        set
        {
            _bgTransparency = value;
            _bgAlpha = (byte)(255 * value / 10);
        }
    }

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Grid), GroupName = nameof(Res.Visualization), Description = nameof(Res.GridColorDescription), Order = 210, ResourceType = typeof(Res))]
    public Color GridColor
    {
        get => _linePen.Color.Convert();
        set
        {
            _linePen = new RenderPen(value.Convert());
            _selectionPen = new RenderPen(value.Convert(), 3);
        }
    }

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.VisibleProportion), GroupName = nameof(Res.Visualization), Description = nameof(Res.VisibleProportionDescription), Order = 220, ResourceType = typeof(Res))]
    public bool VisibleProportion { get; set; }

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.RatiosAsPercent), GroupName = nameof(Res.Visualization), Description = nameof(Res.RatiosAsPercentDescription), Order = 225, ResourceType = typeof(Res))]
    public bool RatiosAsPercent { get; set; } = true;

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Volume), GroupName = nameof(Res.Visualization), Description = nameof(Res.VolumeColorDescription), Order = 230, ResourceType = typeof(Res))]
    public Color VolumeColor { get; set; } = System.Drawing.Color.DarkGray.Convert();

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AskColor), GroupName = nameof(Res.Visualization), Description = nameof(Res.AskColorDescription), Order = 240, ResourceType = typeof(Res))]
    public Color AskColor { get; set; } = System.Drawing.Color.Green.Convert();

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.BidColor), GroupName = nameof(Res.Visualization), Description = nameof(Res.BidColorDescription), Order = 250, ResourceType = typeof(Res))]
    public Color BidColor { get; set; } = System.Drawing.Color.Red.Convert();

    #endregion

    #region Text

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Color), GroupName = nameof(Res.Text), Description = nameof(Res.LabelTextColorDescription), Order = 300, ResourceType = typeof(Res))]
    public Color TextColor
    {
        get => _textColor.Convert();
        set => _textColor = value.Convert();
    }

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Font), GroupName = nameof(Res.Text), Description = nameof(Res.FontSettingDescription), Order = 310, ResourceType = typeof(Res))]
    public FontSetting Font
    {
        get => _font;
        set => SetTrackedProperty(ref _font, value, OnFontPropertyChanged);
    }

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.CenterAlign), GroupName = nameof(Res.Text), Description = nameof(Res.CenterAlignDescription), Order = 320, ResourceType = typeof(Res))]
    public bool CenterAlign
    {
        get => _centerAlign;
        set
        {
            _centerAlign = value;
            _stringLeftFormat.Alignment = value ? StringAlignment.Center : StringAlignment.Near;
        }
    }

    #endregion

    #region Headers

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Color), GroupName = nameof(Res.Headers), Description = nameof(Res.HeaderBackgroundDescription), Order = 330, ResourceType = typeof(Res))]
    public Color HeaderBackground
    {
        get => _headerBackground.Convert();
        set => _headerBackground = value.Convert();
    }

    [Tab(TabName = nameof(Res.Visualization), TabOrder = 1, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.HideRowsDescription), GroupName = nameof(Res.Headers), Description = nameof(Res.HideHeadersDescription), Order = 340, ResourceType = typeof(Res))]
    public bool HideRowsDescription { get; set; }

    #endregion

    #region Volume Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.VolumeAlert), Description = nameof(Res.UseAlertDescription), Order = 400, ResourceType = typeof(Res))]
    public bool UseVolumeAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.VolumeAlert), Description = nameof(Res.AlertFilterDescription), Order = 410, ResourceType = typeof(Res))]
    [Range(0, int.MaxValue)]
    public decimal VolumeAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.VolumeAlert), Description = nameof(Res.AlertFileDescription), Order = 420, ResourceType = typeof(Res))]
    public string VolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Delta alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.DeltaAlert), Description = nameof(Res.UseAlertDescription), Order = 500, ResourceType = typeof(Res))]
    public bool UseDeltaAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.DeltaAlert), Description = nameof(Res.AlertFilterDescription), Order = 510, ResourceType = typeof(Res))]
    public decimal DeltaAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.DeltaAlert), Description = nameof(Res.AlertFileDescription), Order = 520, ResourceType = typeof(Res))]
    public string DeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Ask Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.AskAlert), Description = nameof(Res.UseAlertDescription), Order = 600, ResourceType = typeof(Res))]
    public bool UseAskAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.AskAlert), Description = nameof(Res.AlertFilterDescription), Order = 610, ResourceType = typeof(Res))]
    [Range(0, int.MaxValue)]
    public decimal AskAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.AskAlert), Description = nameof(Res.AlertFileDescription), Order = 620, ResourceType = typeof(Res))]
    public string AskAlertFile { get; set; } = "alert1";

    #endregion

    #region Bid Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.BidAlert), Description = nameof(Res.UseAlertDescription), Order = 700, ResourceType = typeof(Res))]
    public bool UseBidAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.BidAlert), Description = nameof(Res.AlertFilterDescription), Order = 710, ResourceType = typeof(Res))]
    [Range(0, int.MaxValue)]
    public decimal BidAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.BidAlert), Description = nameof(Res.AlertFileDescription), Order = 720, ResourceType = typeof(Res))]
    public string BidAlertFile { get; set; } = "alert1";

    #endregion

    #region Delta Per Volume Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.DeltaPerVolumeAlert), Description = nameof(Res.UseAlertDescription), Order = 800, ResourceType = typeof(Res))]
    public bool UseDeltaPerVolumeAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.DeltaPerVolumeAlert), Description = nameof(Res.AlertFilterDescription), Order = 810, ResourceType = typeof(Res))]
    [Range(0, 100)]
    public decimal DeltaPerVolumeAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.DeltaPerVolumeAlert), Description = nameof(Res.AlertFileDescription), Order = 820, ResourceType = typeof(Res))]
    public string DeltaPerVolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Session Delta Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.SessionDeltaAlert), Description = nameof(Res.UseAlertDescription), Order = 900, ResourceType = typeof(Res))]
    public bool UseSessionDeltaAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.SessionDeltaAlert), Description = nameof(Res.AlertFilterDescription), Order = 910, ResourceType = typeof(Res))]
    public decimal SessionDeltaAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.SessionDeltaAlert), Description = nameof(Res.AlertFileDescription), Order = 920, ResourceType = typeof(Res))]
    public string SessionDeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Session Delta Per Volume Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.SessionDeltaPerVolumeAlert), Description = nameof(Res.UseAlertDescription), Order = 1000, ResourceType = typeof(Res))]
    public bool UseSessionDeltaPerVolumeAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.SessionDeltaPerVolumeAlert), Description = nameof(Res.AlertFilterDescription), Order = 1010, ResourceType = typeof(Res))]
    public decimal SessionDeltaPerVolumeAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.SessionDeltaPerVolumeAlert), Description = nameof(Res.AlertFileDescription), Order = 1020, ResourceType = typeof(Res))]
    public string SessionDeltaPerVolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Max Delta Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.MaxDeltaAlert), Description = nameof(Res.UseAlertDescription), Order = 1100, ResourceType = typeof(Res))]
    public bool UseMaxDeltaAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.MaxDeltaAlert), Description = nameof(Res.AlertFilterDescription), Order = 1110, ResourceType = typeof(Res))]
    public decimal MaxDeltaAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.MaxDeltaAlert), Description = nameof(Res.AlertFileDescription), Order = 1120, ResourceType = typeof(Res))]
    public string MaxDeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Min Delta Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.MinDeltaAlert), Description = nameof(Res.UseAlertDescription), Order = 1200, ResourceType = typeof(Res))]
    public bool UseMinDeltaAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.MinDeltaAlert), Description = nameof(Res.AlertFilterDescription), Order = 1210, ResourceType = typeof(Res))]
    public decimal MinDeltaAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.MinDeltaAlert), Description = nameof(Res.AlertFileDescription), Order = 1220, ResourceType = typeof(Res))]
    public string MinDeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Delta Change Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.DeltaChangeAlert), Description = nameof(Res.UseAlertDescription), Order = 1300, ResourceType = typeof(Res))]
    public bool UseDeltaChangeAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.DeltaChangeAlert), Description = nameof(Res.AlertFilterDescription), Order = 1310, ResourceType = typeof(Res))]
    public decimal DeltaChangeAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.DeltaChangeAlert), Description = nameof(Res.AlertFileDescription), Order = 1320, ResourceType = typeof(Res))]
    public string DeltaChangeAlertFile { get; set; } = "alert1";

    #endregion

    #region Volume Per Second Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.VolumePerSecondAlert), Description = nameof(Res.UseAlertDescription), Order = 1400, ResourceType = typeof(Res))]
    public bool UseVolumePerSecondAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.VolumePerSecondAlert), Description = nameof(Res.AlertFilterDescription), Order = 1410, ResourceType = typeof(Res))]
    [Range(0, int.MaxValue)]
    public decimal VolumePerSecondAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.VolumePerSecondAlert), Description = nameof(Res.AlertFileDescription), Order = 1420, ResourceType = typeof(Res))]
    public string VolumePerSecondAlertFile { get; set; } = "alert1";

    #endregion

    #region Session Volume Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.SessionVolumeAlert), Description = nameof(Res.UseAlertDescription), Order = 1500, ResourceType = typeof(Res))]
    public bool UseSessionVolumeAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.SessionVolumeAlert), Description = nameof(Res.AlertFilterDescription), Order = 1510, ResourceType = typeof(Res))]
    [Range(0, int.MaxValue)]
    public decimal SessionVolumeAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.SessionVolumeAlert), Description = nameof(Res.AlertFileDescription), Order = 1520, ResourceType = typeof(Res))]
    public string SessionVolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Trades Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.TradesAlert), Description = nameof(Res.UseAlertDescription), Order = 1600, ResourceType = typeof(Res))]
    public bool UseTradesAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.TradesAlert), Description = nameof(Res.AlertFilterDescription), Order = 1610, ResourceType = typeof(Res))]
    [Range(0, int.MaxValue)]
    public decimal TradesAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.TradesAlert), Description = nameof(Res.AlertFileDescription), Order = 1620, ResourceType = typeof(Res))]
    public string TradesAlertFile { get; set; } = "alert1";

    #endregion

    #region Height Alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.HeightAlert), Description = nameof(Res.UseAlertDescription), Order = 1700, ResourceType = typeof(Res))]
    public bool UseHeightAlert { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Filter), GroupName = nameof(Res.HeightAlert), Description = nameof(Res.AlertFilterDescription), Order = 1710, ResourceType = typeof(Res))]
    [Range(0, int.MaxValue)]
    public decimal HeightAlertValue { get; set; }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.HeightAlert), Description = nameof(Res.AlertFileDescription), Order = 1720, ResourceType = typeof(Res))]
    public string HeightAlertFile { get; set; } = "alert1";

    #endregion

    #region Net imbalance alert

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.Enabled), GroupName = nameof(Res.NetImbalanceAlertGroup), Description = nameof(Res.NetImbalanceAlertDescription), Order = 1800, ResourceType = typeof(Res))]
    public bool UseNetImbalanceAlert
    {
        get => _useNetImbalanceAlert;
        set
        {
            _useNetImbalanceAlert = value;
            _netImbalanceAlert.Reset();
            OnImbalanceUseChanged();
        }
    }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.NetImbalanceAlertThresholdAbs), GroupName = nameof(Res.NetImbalanceAlertGroup), Description = nameof(Res.NetImbalanceAlertThresholdDescription), Order = 1810, ResourceType = typeof(Res))]
    [Range(1, 100000)]
    public int NetImbalanceAlertValue { get; set; } = 6;

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.NetImbalanceAlertUseClosedCandle), GroupName = nameof(Res.NetImbalanceAlertGroup), Description = nameof(Res.NetImbalanceAlertUseClosedCandleDescription), Order = 1820, ResourceType = typeof(Res))]
    public bool UseClosedCandleForNetImbalanceAlert
    {
        get => _netImbalanceAlertOnClose;
        set
        {
            _netImbalanceAlertOnClose = value;
            _netImbalanceAlert.Reset();
        }
    }

    [Tab(TabName = nameof(Res.Alerts), TabOrder = 2, ResourceType = typeof(Res))]
    [Display(Name = nameof(Res.AlertFile), GroupName = nameof(Res.NetImbalanceAlertGroup), Description = nameof(Res.AlertFileDescription), Order = 1830, ResourceType = typeof(Res))]
    public string NetImbalanceAlertFile { get; set; } = "alert1";

    #endregion

    #endregion

    #region ctor

    public ClusterStatisticPro()
		: base(true)
	{
		DenyToChangePanel = true;
		Panel = IndicatorDataProvider.NewPanel;
		EnableCustomDrawing = true;
		RowsOrder.OnChanged = () => _layoutChanged = true;
		ShowDelta = ShowSessionDelta = ShowVolume = true;
		SubscribeToDrawingEvents(DrawingLayouts.LatestBar | DrawingLayouts.Historical | DrawingLayouts.Final);

		DataSeries[0].IsHidden = true;
		((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
		ShowDescription = false;

		_rowMaxima[DataType.DeltaSecond] = new ClosedBarsMax(_deltaPerSecond);
		_rowMaxima[DataType.BuyImbalance] = new ClosedBarsMax(_buyImbalance);
		_rowMaxima[DataType.SellImbalance] = new ClosedBarsMax(_sellImbalance);
		_rowMaxima[DataType.NetImbalance] = new ClosedBarsMax(_netImbalance);
		_rowMaxima[DataType.StackedBuyImbalance] = new ClosedBarsMax(_stackedBuyImbalance);
		_rowMaxima[DataType.StackedSellImbalance] = new ClosedBarsMax(_stackedSellImbalance);
		_rowMaxima[DataType.StackedNetImbalance] = new ClosedBarsMax(_stackedNetImbalance);
		_rowMaxima[DataType.PeakVolPerSec] = new ClosedBarsMax(_peakVolPerSec);
		_rowMaxima[DataType.PeakDeltaPerSec] = new ClosedBarsMax(_peakDeltaPerSec);
		_rowMaxima[DataType.PeakDeltaPerVol] = new ClosedBarsMax(_peakDeltaPerVol);

		Font = new FontSetting("Arial", 9);
		CustomSessionStart = new(false);
    }

	#endregion

	#region Public methods

	public override bool ProcessMouseDown(RenderControlMouseEventArgs e)
	{
		var cursor = e.Location;

		if (!Container.Region.Contains(cursor) || e.X > _headerWidth)
			return base.ProcessMouseDown(e);

		if (StrCount <= 1)
			return base.ProcessMouseDown(e);

		var height = Container.Region.Height / StrCount;

		var rowNum = Math.Max((e.Y - Container.Region.Top) / height, 0);
		rowNum = Math.Min(rowNum, StrCount - 1);

		_selectionOffset = 0;
		_selectionY = e.Y;
		_pressedString = RowsOrder.AvailableStrings.GetValueAtIndex(rowNum);
		CacheChanged();

		return true;
	}

	public override bool ProcessMouseMove(RenderControlMouseEventArgs e)
	{
		_atPanel = Container.Region.Contains(e.Location);
		_atHeader = e.X <= _headerWidth && _atPanel;

		if (_pressedString is DataType.None)
			return base.ProcessMouseMove(e);

		if (StrCount <= 1)
			return base.ProcessMouseMove(e);

		var height = Container.Region.Height / StrCount;

		var rowNum = Math.Max((e.Y - Container.Region.Top) / height, 0);
		rowNum = Math.Min(rowNum, StrCount - 1);

		var currentString = RowsOrder.AvailableStrings.GetValueAtIndex(rowNum);

		if (_pressedString != currentString)
		{
			RowsOrder.UpdateOrder(_pressedString, currentString);
			CacheChanged();

			_selectionY += (e.Y > _selectionY ? 1 : -1) * height;
		}

		_selectionOffset = _selectionY - e.Y;

		return true;
	}

	public override StdCursor GetCursor(RenderControlMouseEventArgs e)
	{
		if ((!Container.Region.Contains(e.Location) || e.X > _headerWidth) && _pressedString is DataType.None)
			return base.GetCursor(e);

		return StdCursor.Hand;
	}

	public override bool ProcessMouseUp(RenderControlMouseEventArgs e)
	{
		_pressedString = DataType.None;
		CacheChanged();
		return base.ProcessMouseUp(e);
	}

	#endregion

	#region Protected methods

	protected override void OnRecalculate()
	{
		// The last bar is calculated with the history too: its values would "cross" the alert
		// levels from zero and alert on every chart load or settings change.
		_alertsArmed = false;

		// The tape starts over: realtime prints wait in a buffer until the history is in.
		lock (_tapeSync)
		{
			_tape = PeakRowsEnabled() ? new PeakRateEngine(_tapeWindowSeconds, _tapeMinVolume) : null;
			_tapeHistoryLoaded = false;
			_tapeRequestId = 0;
			_pendingTicks.Clear();
			_bufferStartTime = _lastTickTime;
			_bufferStartCount = _lastTickCount;
		}
	}

	protected override void OnFinishRecalculate()
	{
		_alertsArmed = true;
		RequestTapeHistory();
	}

	protected override void OnNewTrade(MarketDataArg trade)
	{
		var tick = new Tick(trade.Time, trade.Volume, SideOf(trade.Direction));

		lock (_tapeSync)
		{
			// Prints of the same time, counted to skip the ones the history already has.
			if (tick.Time == _lastTickTime)
				_lastTickCount++;
			else
			{
				_lastTickTime = tick.Time;
				_lastTickCount = 1;
			}

			if (_tape == null)
				return;

			if (!_tapeHistoryLoaded)
			{
				_pendingTicks.Add(tick);
				return;
			}

			ProcessTick(BarOfTime(tick.Time), tick);
		}
	}

	protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
	{
		lock (_tapeSync)
		{
			if (_tape == null || request.RequestId != _tapeRequestId)
				return;
		}

		// The prints of the cumulative trades, the same data the realtime path receives.
		var ticks = (cumulativeTrades ?? Enumerable.Empty<CumulativeTrade>())
			.Where(t => t.Ticks != null)
			.SelectMany(t => t.Ticks)
			.Select(t => new Tick(t.Time, t.Volume, SideOf(t.Direction)))
			.OrderBy(t => t.Time)
			.ToList();

		int replayed = 0, skipped = 0;

		lock (_tapeSync)
		{
			if (_tape == null || request.RequestId != _tapeRequestId)
				return;

			var bar = 0;

			foreach (var tick in ticks)
			{
				while (bar + 1 < CurrentBar && GetCandle(bar + 1).Time <= tick.Time)
					bar++;

				ProcessTick(bar, tick);
			}

			// Buffered realtime prints: the ones older than the end of the history are in it,
			// and so are as many of the ones at its last time as it has, minus those that had
			// already arrived when the buffer started.
			var historyEnd = ticks.Count > 0 ? ticks[^1].Time : DateTime.MinValue;
			var atEnd = 0;

			for (var i = ticks.Count - 1; i >= 0 && ticks[i].Time == historyEnd; i--)
				atEnd++;

			var toSkip = atEnd - (_bufferStartTime == historyEnd ? _bufferStartCount : 0);

			foreach (var tick in _pendingTicks)
			{
				if (tick.Time < historyEnd || (tick.Time == historyEnd && toSkip-- > 0))
				{
					skipped++;
					continue;
				}

				ProcessTick(BarOfTime(tick.Time), tick);
				replayed++;
			}

			_pendingTicks.Clear();
			_tapeHistoryLoaded = true;

			foreach (var type in new[] { DataType.PeakVolPerSec, DataType.PeakDeltaPerSec, DataType.PeakDeltaPerVol })
				_rowMaxima[type].Recompute(CurrentBar);

			RebuildPeakMeansLocked();
		}

		this.LogInfo($"ClusterStatisticPro: tape history {ticks.Count} prints; realtime buffer {replayed} replayed, {skipped} already in the history.");
		RedrawChart();
	}

	protected override void OnApplyDefaultColors()
	{
		HeaderBackground = DefaultColors.Gray.Convert();
		TextColor = System.Drawing.Color.White.Convert();

		if (ChartInfo is null)
			return;

		static Color WithoutAlpha(Color c) => System.Drawing.Color.FromArgb(c.R, c.G, c.B).Convert();

		BidColor = WithoutAlpha(ChartInfo.ColorsStore.FootprintBidColor.Convert());
		AskColor = WithoutAlpha(ChartInfo.ColorsStore.FootprintAskColor.Convert());
		VolumeColor = WithoutAlpha(ChartInfo.ColorsStore.PaneSeparators.Color.Convert());
		GridColor = WithoutAlpha(ChartInfo.ColorsStore.Grid.Color.Convert());

		var bg = ChartInfo.ColorsStore.BaseBackgroundColor;
		BackGroundColor = Color.FromArgb(128, bg.R, bg.G, bg.B);
	}

	protected override void OnCalculate(int bar, decimal value)
	{
		var candle = GetCandle(bar);

		var candleSeconds = Convert.ToDecimal((candle.LastTime - candle.Time).TotalSeconds);

		if (candleSeconds is 0)
			candleSeconds = 1;

		_volPerSecond[bar] = candle.Volume / candleSeconds;
		_deltaPerSecond[bar] = candle.Delta / candleSeconds;

		if (ShouldComputeImbalances())
			CalculateImbalances(bar, candle);

		if (_useNetImbalanceAlert && _alertsArmed && bar == CurrentBar - 1)
			CheckNetImbalanceAlert(bar);

		if (bar == CurrentBar - 1 && bar > 0)
			AddClosedBarToPeakMeans(bar - 1);

		foreach (var maximum in _rowMaxima.Values)
			maximum.Update(bar);

		// Highest Volume/sec of the closed bars, kept here so rendering does not scan the history.
		// The bar in progress is added when rendering: its rate is not final (one trade in its
		// first second can be a very high rate), so it must not stay in the maximum.
		if (bar == 0)
		{
			_maxClosedVolumeSec = 0;
			_volumeSecClosedBars = 0;
		}

		for (; _volumeSecClosedBars < bar; _volumeSecClosedBars++)
			_maxClosedVolumeSec = Math.Max(_volPerSecond[_volumeSecClosedBars], _maxClosedVolumeSec);

		if (bar == 0)
		{
			_cVolume[bar] = _cumVolume = candle.Volume;
			_cDelta[bar] = candle.Delta;
			_deltaPerVol[bar] = candle.Volume is 0
				? 0
				: Math.Abs(candle.Delta * 100m / candle.Volume);
			_cDeltaPerVol[bar] = candle.Volume is 0
				? 0
				: candle.Delta * 100m / candle.Volume;
			_deltaChange[bar] = 0; // No previous candle
			_candleHeights[bar] = candle.High - candle.Low;
			_candleDurations[bar] = (int)(candle.LastTime - candle.Time).TotalSeconds;

			_maxVolume = candle.Volume;
			_maxDelta = Math.Abs(candle.Delta);
			_maxMaxDelta = Math.Abs(candle.MaxDelta);
			_maxMinDelta = Math.Abs(candle.MinDelta);
			_maxDeltaChange = 0;
			_minDelta = candle.MinDelta;
			_maxHeight = _candleHeights[bar];
			_maxTicks = candle.Ticks;
			_maxDuration = _candleDurations[bar];
			_maxSessionDelta = Math.Abs(candle.Delta);
			_maxDeltaPerVolume = _deltaPerVol[bar];
			_maxSessionDeltaPerVolume = Math.Abs(_cDeltaPerVol[bar]);
			_maxAsk = candle.Ask;
			_maxBid = candle.Bid;
			return;
		}

		_deltaPerVol[bar] = candle.Volume is 0 
			? 0
			: Math.Abs(candle.Delta * 100m / candle.Volume);

		var prevCandle = GetCandle(bar - 1);

		if (CheckStartBar(bar))
		{
			_cVolume[bar] = _cumVolume = candle.Volume;
			_cDelta[bar] = candle.Delta;
			_deltaChange[bar] = 0; // No previous candle in this session
			_maxSessionDelta = 0;
			_maxSessionDeltaPerVolume = 0;

			// Reset last session values to avoid false alerts when new session starts
			_lastSessionDeltaValue = 0m;
			_lastSessionDeltaPerVolumeValue = 0m;
			_lastSessionVolumeValue = 0m;
			_lastDeltaChangeValue = 0m;
		}
		else
		{
			_cumVolume = _cVolume[bar] = _cVolume[bar - 1] + candle.Volume;
			_cDelta[bar] = _cDelta[bar - 1] + candle.Delta;
			_deltaChange[bar] = candle.Delta - prevCandle.Delta;
		}

		_maxSessionDelta = Math.Max(Math.Abs(_cDelta[bar]), _maxSessionDelta);

		_maxAsk = Math.Max(candle.Ask, _maxAsk);
		_maxBid = Math.Max(candle.Bid, _maxBid);

		_maxDeltaChange = Math.Max(Math.Abs(_deltaChange[bar]), _maxDeltaChange);

		_maxDelta = Math.Max(Math.Abs(candle.Delta), _maxDelta);

		_maxMaxDelta = Math.Max(Math.Abs(candle.MaxDelta), _maxMaxDelta);
		_maxMinDelta = Math.Max(Math.Abs(candle.MinDelta), _maxMinDelta);

		_maxVolume = Math.Max(candle.Volume, _maxVolume);

		_minDelta = Math.Min(candle.MinDelta, _minDelta);

		if (candle.Volume is not 0)
			_maxDeltaPerVolume = Math.Max(Math.Abs(100 * candle.Delta / candle.Volume), _maxDeltaPerVolume);

		var candleHeight = candle.High - candle.Low;
		_maxHeight = Math.Max(candleHeight, _maxHeight);
		_candleHeights[bar] = candleHeight;

		_maxTicks = Math.Max(candle.Ticks, _maxTicks);

		_candleDurations[bar] = (int)(candle.LastTime - candle.Time).TotalSeconds;
		_maxDuration = Math.Max(_candleDurations[bar], _maxDuration);

		if (_cVolume[bar] is not 0)
			_cDeltaPerVol[bar] = _cDelta[bar] * 100.0m / _cVolume[bar];

		_maxSessionDeltaPerVolume = Math.Max(Math.Abs(_cDeltaPerVol[bar]), _maxSessionDeltaPerVolume);

		if (_lastBar != bar)
		{
			_lastAskValue = 0m;
			_lastBidValue = 0m;
			_lastDeltaValue = 0m;
			_lastDeltaPerVolumeValue = 0m;
			_lastMaxDeltaValue = 0m;
			_lastMinDeltaValue = 0m;
			_lastDeltaChangeValue = 0m;
			_lastVolumeValue = 0m;
			_lastVolumePerSecondValue = 0m;
			_lastTradesValue = 0m;
			_lastHeightValue = 0m;

			// Session values are cumulative and should not be reset to 0 on new bar,
			// otherwise alerts would falsely trigger due to "crossing" from 0
		}

		if (bar == CurrentBar - 1 && _alertsArmed)
		{
			// Ask Alert (exceeding)
			if (UseAskAlert && _lastAskAlert != bar)
			{
				if (_lastAskValue < AskAlertValue && candle.Ask >= AskAlertValue)
				{
					AddAlert(AskAlertFile, $"Cluster statistic ask alert: {candle.Ask}");
					_lastAskAlert = bar;
				}
			}

			// Bid Alert (exceeding)
			if (UseBidAlert && _lastBidAlert != bar)
			{
				if (_lastBidValue < BidAlertValue && candle.Bid >= BidAlertValue)
				{
					AddAlert(BidAlertFile, $"Cluster statistic bid alert: {candle.Bid}");
					_lastBidAlert = bar;
				}
			}

			// Delta Alert (crossing)
			if (UseDeltaAlert && _lastDeltaAlert != bar)
			{
				if ((_lastDeltaValue < DeltaAlertValue && candle.Delta >= DeltaAlertValue)
				    || (_lastDeltaValue > DeltaAlertValue && candle.Delta <= DeltaAlertValue))
				{
					AddAlert(DeltaAlertFile, $"Cluster statistic delta alert: {candle.Delta}");
					_lastDeltaAlert = bar;
				}
			}

			// Delta Per Volume Alert (crossing)
			if (UseDeltaPerVolumeAlert && _lastDeltaPerVolumeAlert != bar)
			{
				var deltaPerVol = _deltaPerVol[bar];
				if ((_lastDeltaPerVolumeValue < DeltaPerVolumeAlertValue && deltaPerVol >= DeltaPerVolumeAlertValue)
				    || (_lastDeltaPerVolumeValue > DeltaPerVolumeAlertValue && deltaPerVol <= DeltaPerVolumeAlertValue))
				{
					AddAlert(DeltaPerVolumeAlertFile, $"Cluster statistic delta/volume alert: {deltaPerVol:F2}%");
					_lastDeltaPerVolumeAlert = bar;
				}
			}

			// Session Delta Alert (crossing)
			if (UseSessionDeltaAlert && _lastSessionDeltaAlert != bar)
			{
				var sessionDelta = _cDelta[bar];
				if ((_lastSessionDeltaValue < SessionDeltaAlertValue && sessionDelta >= SessionDeltaAlertValue)
				    || (_lastSessionDeltaValue > SessionDeltaAlertValue && sessionDelta <= SessionDeltaAlertValue))
				{
					AddAlert(SessionDeltaAlertFile, $"Cluster statistic session delta alert: {sessionDelta}");
					_lastSessionDeltaAlert = bar;
				}
			}

			// Session Delta Per Volume Alert (crossing)
			if (UseSessionDeltaPerVolumeAlert && _lastSessionDeltaPerVolumeAlert != bar)
			{
				var sessionDeltaPerVol = _cDeltaPerVol[bar];
				if ((_lastSessionDeltaPerVolumeValue < SessionDeltaPerVolumeAlertValue && sessionDeltaPerVol >= SessionDeltaPerVolumeAlertValue)
				    || (_lastSessionDeltaPerVolumeValue > SessionDeltaPerVolumeAlertValue && sessionDeltaPerVol <= SessionDeltaPerVolumeAlertValue))
				{
					AddAlert(SessionDeltaPerVolumeAlertFile, $"Cluster statistic session delta/volume alert: {sessionDeltaPerVol:F2}%");
					_lastSessionDeltaPerVolumeAlert = bar;
				}
			}

			// Max Delta Alert (crossing)
			if (UseMaxDeltaAlert && _lastMaxDeltaAlert != bar)
			{
				if ((_lastMaxDeltaValue < MaxDeltaAlertValue && candle.MaxDelta >= MaxDeltaAlertValue)
				    || (_lastMaxDeltaValue > MaxDeltaAlertValue && candle.MaxDelta <= MaxDeltaAlertValue))
				{
					AddAlert(MaxDeltaAlertFile, $"Cluster statistic max delta alert: {candle.MaxDelta}");
					_lastMaxDeltaAlert = bar;
				}
			}

			// Min Delta Alert (crossing)
			if (UseMinDeltaAlert && _lastMinDeltaAlert != bar)
			{
				if ((_lastMinDeltaValue < MinDeltaAlertValue && candle.MinDelta >= MinDeltaAlertValue)
				    || (_lastMinDeltaValue > MinDeltaAlertValue && candle.MinDelta <= MinDeltaAlertValue))
				{
					AddAlert(MinDeltaAlertFile, $"Cluster statistic min delta alert: {candle.MinDelta}");
					_lastMinDeltaAlert = bar;
				}
			}

			// Delta Change Alert (crossing)
			if (UseDeltaChangeAlert && _lastDeltaChangeAlert != bar)
			{

                var deltaChange = _deltaChange[bar];

                if ((_lastDeltaChangeValue < DeltaChangeAlertValue && deltaChange >= DeltaChangeAlertValue)
				    || (_lastDeltaChangeValue > DeltaChangeAlertValue && deltaChange <= DeltaChangeAlertValue))
				{
					AddAlert(DeltaChangeAlertFile, $"Cluster statistic delta change alert: {deltaChange}");
					_lastDeltaChangeAlert = bar;
				}
			}

			// Volume Alert (exceeding)
			if (UseVolumeAlert && _lastVolumeAlert != bar)
			{
				if (_lastVolumeValue < VolumeAlertValue && candle.Volume >= VolumeAlertValue)
				{
					AddAlert(VolumeAlertFile, $"Cluster statistic volume alert: {candle.Volume}");
					_lastVolumeAlert = bar;
				}
			}

			// Volume Per Second Alert (exceeding)
			if (UseVolumePerSecondAlert && _lastVolumePerSecondAlert != bar)
			{
				var volPerSec = _volPerSecond[bar];
				if (_lastVolumePerSecondValue < VolumePerSecondAlertValue && volPerSec >= VolumePerSecondAlertValue)
				{
					AddAlert(VolumePerSecondAlertFile, $"Cluster statistic volume/sec alert: {volPerSec:F2}");
					_lastVolumePerSecondAlert = bar;
				}
			}

			// Session Volume Alert (exceeding)
			if (UseSessionVolumeAlert && _lastSessionVolumeAlert != bar)
			{
				var sessionVol = _cVolume[bar];
				if (_lastSessionVolumeValue < SessionVolumeAlertValue && sessionVol >= SessionVolumeAlertValue)
				{
					AddAlert(SessionVolumeAlertFile, $"Cluster statistic session volume alert: {sessionVol}");
					_lastSessionVolumeAlert = bar;
				}
			}

			// Trades Alert (exceeding)
			if (UseTradesAlert && _lastTradesAlert != bar)
			{
				if (_lastTradesValue < TradesAlertValue && candle.Ticks >= TradesAlertValue)
				{
					AddAlert(TradesAlertFile, $"Cluster statistic trades alert: {candle.Ticks}");
					_lastTradesAlert = bar;
				}
			}

			// Height Alert (exceeding)
			if (UseHeightAlert && _lastHeightAlert != bar)
			{
				var height = _candleHeights[bar];
				if (_lastHeightValue < HeightAlertValue && height >= HeightAlertValue)
				{
					AddAlert(HeightAlertFile, $"Cluster statistic height alert: {height}");
					_lastHeightAlert = bar;
				}
			}
		}

		// Update last values for next comparison
		_lastAskValue = candle.Ask;
		_lastBidValue = candle.Bid;
		_lastDeltaValue = candle.Delta;
		_lastDeltaPerVolumeValue = _deltaPerVol[bar];
		_lastSessionDeltaValue = _cDelta[bar];
		_lastSessionDeltaPerVolumeValue = _cDeltaPerVol[bar];
		_lastMaxDeltaValue = candle.MaxDelta;
		_lastMinDeltaValue = candle.MinDelta;
		_lastDeltaChangeValue = _deltaChange[bar];
        _lastVolumeValue = candle.Volume;
		_lastVolumePerSecondValue = _volPerSecond[bar];
		_lastSessionVolumeValue = _cVolume[bar];
		_lastTradesValue = candle.Ticks;
		_lastHeightValue = _candleHeights[bar];
		_lastBar = bar;
	}

	protected override void OnRender(RenderContext context, DrawingLayouts layout)
	{
		if (ChartInfo is not { PriceChartContainer.BarsWidth: > 2 })
			return;

		if (LastVisibleBarNumber > CurrentBar - 1)
			return;

		if (StrCount is 0)
			return;

		var bounds = context.ClipBounds;

		_height = Container.Region.Height / StrCount;

		if (_layoutChanged)
		{
			var maxWidth = 0;

			foreach (var type in RowsOrder.AvailableStrings.Values)
			{
				var size = context.MeasureString(GetHeader(type), Font.RenderObject);

				if (size.Width > maxWidth)
				{
					maxWidth = size.Width;
					_fontHeight = size.Height;
				}
			}

			_headerWidth = maxWidth + 10;
			_layoutChanged = false;
		}

		var fullBarsWidth = (int)(ChartInfo.PriceChartContainer.BarsWidth + ChartInfo.PriceChartContainer.BarSpacing);
		var showHeadersText = _fontHeight * 0.9 <= _height;
		var showValues = fullBarsWidth >= 30 && showHeadersText;

		try
		{
			context.SetClip(Container.Region);

			context.SetTextRenderingHint(RenderTextRenderingHint.Aliased);

			var overPixels = Container.Region.Height % StrCount;

			var y = Container.Region.Y;

			var maxX = ChartInfo.GetXByBar(LastVisibleBarNumber) + fullBarsWidth;

			var maxValues = CreateMaxValues();

			var drawHeaders = !HideRowsDescription
				|| Container.Region.Contains(MouseLocationInfo.LastPosition)
				|| _pressedString is not DataType.None;

			var selectionY = 0;

			if ((layout is DrawingLayouts.LatestBar or DrawingLayouts.Historical && _pressedString is DataType.None)
			    ||
			    (_pressedString is not DataType.None && layout is DrawingLayouts.Final))
			{
				var startBar = LastVisibleBarNumber;

				if (layout is DrawingLayouts.Historical)
					startBar = Math.Min(startBar, CurrentBar - 2);

				for (var bar = startBar; bar >= FirstVisibleBarNumber; bar--)
				{
					if (layout is DrawingLayouts.LatestBar)
					{
						if (bar < CurrentBar - 1)
							break;
					}

					var x = ChartInfo.GetXByBar(bar);

					var y1 = y;
					var candle = GetCandle(bar);

					DrawBarValues(context, maxValues, candle, x, ref y1, ref selectionY, fullBarsWidth, showValues, overPixels, bar);
				}
			}

			if (layout is DrawingLayouts.Historical || _pressedString is not DataType.None)
				DrawValuesTable(context, fullBarsWidth, maxX);

			if ((drawHeaders && layout is DrawingLayouts.Final && (HideRowsDescription || _pressedString is not DataType.None))
			    ||
			    (layout is DrawingLayouts.Historical && !HideRowsDescription && _pressedString is DataType.None))
			{
				for (var i = 0; i < RowsOrder.AvailableStrings.Count; i++)
				{
					var type = RowsOrder.AvailableStrings.GetValueAtIndex(i);
					var rectHeight = _height + (overPixels > 0 ? 1 : 0);

					if (i == RowsOrder.AvailableStrings.SkipIdx && i != RowsOrder.AvailableStrings.Count - 1)
					{
						y += rectHeight;
						overPixels--;
						continue;
					}

					DrawHeader(type);

					if (_pressedString is not DataType.None && i == RowsOrder.AvailableStrings.Count - 1 && i != RowsOrder.AvailableStrings.SkipIdx)
						DrawHeader(_pressedString);

					y += rectHeight;
					overPixels--;

					void DrawHeader(DataType type)
					{
						var isSelected = type == _pressedString;
						var rectY = type == _pressedString ? selectionY - _selectionOffset : y;

						if (isSelected)
							rectY = Math.Max(Container.Region.Y, Math.Min(Container.Region.Bottom - rectHeight, rectY));

						var descRect = new Rectangle(0, rectY, _headerWidth, rectHeight);
						context.FillRectangle(_headerBackground, descRect);

						if (showHeadersText)
						{
							var text = GetHeader(type);

							var textRect = descRect with
							{
								X = descRect.X + _headerOffset
							};

							context.DrawString(text, Font.RenderObject, _textColor, textRect, _stringLeftFormat);
						}

						if (type == _pressedString)
						{
							var selectionRect = descRect with
							{
								X = Container.Region.X,
								Width = maxX - Container.Region.X
							};

							switch (_selectionOffset)
							{
								case < 0:
									context.FillRectangle(_headerBackground,
										new Rectangle(Container.Region.X, selectionY, selectionRect.Width, rectY - selectionY));
									context.DrawLine(_linePen, Container.Region.X, selectionY, maxX, selectionY);
									break;
								case > 0:
									context.FillRectangle(_headerBackground,
										new Rectangle(Container.Region.X, rectY + rectHeight, selectionRect.Width, selectionY - rectY));
									context.DrawLine(_linePen, Container.Region.X, selectionY + rectHeight, maxX, selectionY + rectHeight);
									break;
							}

							context.DrawRectangle(_selectionPen, selectionRect);
						}
						else if (i is not 0 && i - 1 != RowsOrder.AvailableStrings.SkipIdx)
							context.DrawLine(_linePen, Container.Region.X, rectY, maxX, rectY);
					}
				}

				var tableRect = new Rectangle(Container.Region.X, Container.Region.Y, maxX - Container.Region.X, Container.Region.Height - 1);
				context.DrawLine(_linePen, _headerWidth, Container.Region.Y, _headerWidth, Container.Region.Bottom);
				context.DrawRectangle(_linePen, tableRect);
			}

			if (_pressedString is not DataType.None)
				return;

			if (!_atPanel)
				return;

			if (layout is DrawingLayouts.Final)
			{
				if (!Container.Region.Contains(MouseLocationInfo.LastPosition))
					return;

				if ((_atHeader && showHeadersText) || (!_atHeader && showValues))
					return;

				var bar = MouseLocationInfo.BarBelowMouse;
				var rowNum = Math.Max((MouseLocationInfo.LastPosition.Y - Container.Region.Top) / _height, 0);
				rowNum = Math.Min(rowNum, StrCount - 1);

				var type = RowsOrder.AvailableStrings.GetValueAtIndex(rowNum);

				var tipColor = System.Drawing.Color.Transparent;
				var tipText = "";

				if (_atHeader)
				{
					tipText = GetHeader(type);
					tipColor = _headerBackground;
				}
				else
				{
					var candle = GetCandle(bar);
					var rate = GetRate(maxValues, type, candle, bar);

					tipColor = GetBrush(type, candle, bar, rate);
					tipText = GetValueText(type, candle, bar);
				}

				DrawToolTip(context, MouseLocationInfo.LastPosition, tipText, tipColor);
			}
		}
		catch (ArgumentOutOfRangeException)
		{
			//Chart cleared
		}
		catch (Exception e)
		{
			this.LogError("Cluster statistic rendering error ", e);
			throw;
		}
		finally
		{
			context.SetTextRenderingHint(RenderTextRenderingHint.AntiAlias);
			context.SetClip(bounds);
		}
	}

	#endregion

	#region Private methods

	// Offset of the chart time zone. Latest and Stable only have the whole-hour TimeZone.
#if ATAS_LATEST || ATAS_STABLE
	private TimeSpan TimeOffset => TimeSpan.FromHours(InstrumentInfo.TimeZone);
#else
	private TimeSpan TimeOffset => InstrumentInfo.TimeZoneOffset;
#endif

	private void DrawValuesTable(RenderContext context, int barWidth, int maxX)
	{
		var x = 0;

		for (var bar = FirstVisibleBarNumber; bar <= LastVisibleBarNumber; bar++)
		{
			x = ChartInfo.GetXByBar(bar);
			context.DrawLine(_linePen, x, Container.Region.Y, x, Container.Region.Bottom);
		}

		x += barWidth;
		context.DrawLine(_linePen, x, Container.Region.Y, x, Container.Region.Bottom);

		var overPixels = Container.Region.Height % StrCount;

		var y = Container.Region.Y;

		var skipIdx = RowsOrder.AvailableStrings.SkipIdx;

		for (var i = 0; i < RowsOrder.AvailableStrings.Count; i++)
		{
			if (_pressedString is not DataType.None)
			{
				if ((_selectionOffset < 0 && i == skipIdx + 1) || (_selectionOffset > 0 && i == skipIdx))
				{
					y += _height + (overPixels > 0 ? 1 : 0);
					overPixels--;
					continue;
				}
			}

			context.DrawLine(_linePen, Container.Region.X, y, maxX, y);

			y += _height + (overPixels > 0 ? 1 : 0);
			overPixels--;
		}

		y--;
		context.DrawLine(_linePen, Container.Region.X, y, maxX, y);
	}

	private void DrawBarValues(RenderContext context, MaxValues maxValues, IndicatorCandle candle,
		int x, ref int y, ref int selectionY, int fullBarsWidth, bool showValues, int overPixelsSpace, int bar)
	{
		var overPixels = overPixelsSpace;

		for (var i = 0; i < RowsOrder.AvailableStrings.Count; i++)
		{
			var rowIndex = i;
			var type = RowsOrder.AvailableStrings.GetValueAtIndex(rowIndex);
			var isSelected = type == _pressedString;

			if (isSelected)
				selectionY = y;

			var rectHeight = _height + (overPixels > 0 ? 1 : 0);

			if (rowIndex == RowsOrder.AvailableStrings.SkipIdx && rowIndex != RowsOrder.AvailableStrings.Count - 1)
			{
				y += rectHeight;
				overPixels--;
				continue;
			}

			DrawValue(context, type, candle, maxValues, selectionY, x, y, bar, rectHeight, fullBarsWidth, showValues);

			y += rectHeight;
			overPixels--;
		}

		if (_pressedString is DataType.None)
			return;

		{
			var idx = RowsOrder.AvailableStrings.SkipIdx;
			var rectHeight = _height + (overPixels - 1 < idx ? 0 : 1);
			DrawValue(context, _pressedString, candle, maxValues, selectionY, x, y, bar, rectHeight, fullBarsWidth, showValues);
		}
	}

	private void DrawValue(RenderContext context, DataType type, IndicatorCandle candle, MaxValues maxValues,
		int selectionY, int x, int y, int bar, int rectHeight, int fullBarsWidth, bool showValues)
	{
		var rectY = type == _pressedString ? selectionY - _selectionOffset : y;

		if (type == _pressedString)
			rectY = Math.Max(Container.Region.Y, Math.Min(Container.Region.Bottom - rectHeight, rectY));

		var rect = new Rectangle(x, rectY, fullBarsWidth, rectHeight);
		var rate = GetRate(maxValues, type, candle, bar);

		var bgBrush = GetBrush(type, candle, bar, rate);

		context.FillRectangle(bgBrush, rect);

		if (showValues)
		{
			var text = GetValueText(type, candle, bar);

			var textRect = rect with
			{
				X = rect.X + _headerOffset
			};

			context.DrawString(text, Font.RenderObject, _textColor, textRect, _stringLeftFormat);
		}
	}

	private System.Drawing.Color GetBrush(DataType type, IndicatorCandle candle, int bar, decimal rate)
	{
		return type switch
		{
			DataType.Ask or DataType.Bid or DataType.Delta or DataType.DeltaVolume =>
				Blend(candle.Delta > 0 ? AskColor : BidColor, BackGroundColor, rate),

			DataType.Volume or DataType.VolumeSecond or DataType.SessionVolume or
				DataType.Trades or DataType.Height or DataType.Time or DataType.Duration => Blend(VolumeColor, BackGroundColor, rate),
			DataType.MaxDelta => Blend(candle.MaxDelta > 0 ?  AskColor : BidColor, BackGroundColor, rate),
			DataType.MinDelta => Blend(candle.MinDelta > 0 ?  AskColor : BidColor, BackGroundColor, rate),
            DataType.SessionDeltaVolume => Blend(_cDeltaPerVol[bar] > 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.SessionDelta => Blend(_cDelta[bar] > 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.DeltaChange => GetDeltaChangeBrush(bar, rate),
			DataType.DeltaSecond => Blend(_deltaPerSecond[bar] > 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.BuyImbalance => Blend(AskColor, BackGroundColor, rate),
			DataType.SellImbalance => Blend(BidColor, BackGroundColor, rate),
			DataType.NetImbalance => Blend(_netImbalance[bar] >= 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.StackedBuyImbalance => Blend(AskColor, BackGroundColor, rate),
			DataType.StackedSellImbalance => Blend(BidColor, BackGroundColor, rate),
			DataType.StackedNetImbalance => Blend(_stackedNetImbalance[bar] >= 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.PeakVolPerSec => Blend(VolumeColor, BackGroundColor, rate),
			DataType.PeakDeltaPerSec => Blend(_peakDeltaPerSec[bar] >= 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.PeakDeltaPerVol => Blend(_peakDeltaPerVol[bar] >= 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.None => System.Drawing.Color.Transparent,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private decimal GetRate(MaxValues maxValues, DataType type, IndicatorCandle candle, int bar)
	{
		return type switch
		{
			DataType.Ask => GetRate(candle.Ask, maxValues.MaxAsk),
			DataType.Bid => GetRate(candle.Bid, maxValues.MaxBid),
			DataType.Delta => GetRate(Math.Abs(candle.Delta), maxValues.MaxDelta),
			DataType.DeltaVolume => candle.Volume != 0 ? GetRate(Math.Abs(candle.Delta * 100.0m / candle.Volume), maxValues.MaxDeltaPerVolume) : 0,
			DataType.SessionDelta => GetRate(Math.Abs(_cDelta[bar]), maxValues.MaxSessionDelta),
			DataType.SessionDeltaVolume => GetRate(Math.Abs(_cDeltaPerVol[bar]), maxValues.MaxSessionDeltaPerVolume),
			DataType.MaxDelta => GetRate(Math.Abs(candle.MaxDelta), maxValues.MaxMaxDelta),
			DataType.MinDelta => GetRate(Math.Abs(candle.MinDelta), maxValues.MaxMinDelta),
			DataType.DeltaChange => GetRate(Math.Abs(_deltaChange[bar]), maxValues.MaxDeltaChange),
			DataType.Volume => GetRate(candle.Volume, maxValues.MaxVolume),
			DataType.VolumeSecond => GetRate(_volPerSecond[bar], maxValues.MaxVolumeSec),
			DataType.SessionVolume => GetRate(_cVolume[bar], maxValues.CumVolume),
			DataType.Trades => GetRate(candle.Ticks, maxValues.MaxTicks),
			DataType.Height => GetRate(_candleHeights[bar], maxValues.MaxHeight),
			DataType.Time => GetRate(_cVolume[bar], maxValues.CumVolume),
			DataType.Duration => GetRate(_candleDurations[bar], maxValues.MaxDuration),
			DataType.DeltaSecond => GetRate(Math.Abs(_deltaPerSecond[bar]), RowScale(type)),
			DataType.BuyImbalance => GetRate(_buyImbalance[bar], RowScale(type)),
			DataType.SellImbalance => GetRate(_sellImbalance[bar], RowScale(type)),
			DataType.NetImbalance => GetRate(Math.Abs(_netImbalance[bar]), RowScale(type)),
			DataType.StackedBuyImbalance => GetRate(_stackedBuyImbalance[bar], RowScale(type)),
			DataType.StackedSellImbalance => GetRate(_stackedSellImbalance[bar], RowScale(type)),
			DataType.StackedNetImbalance => GetRate(Math.Abs(_stackedNetImbalance[bar]), RowScale(type)),
			DataType.PeakVolPerSec or DataType.PeakDeltaPerSec or DataType.PeakDeltaPerVol => GetPeakRate(type, bar),
			DataType.None => 0,

			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private MaxValues CreateMaxValues()
	{
		decimal maxVolumeSec;
		var maxDelta = 0m;
		var maxAsk = 0m;
		var maxBid = 0m;
		var maxMaxDelta = 0m;
		var maxMinDelta = 0m;
		var maxVolume = 0m;
		var cumVolume = 0m;
		var maxDeltaChange = 0m;
		var maxSessionDelta = 0m;
		var maxSessionDeltaPerVolume = 0m;
		var maxDeltaPerVolume = 0m;
		var minDelta = decimal.MaxValue;
		var maxHeight = 0m;
		var maxTicks = 0m;
		var maxDuration = 0m;

		if (VisibleProportion)
		{
			for (var i = FirstVisibleBarNumber; i <= LastVisibleBarNumber; i++)
			{
				var candle = GetCandle(i);
				maxDelta = Math.Max(Math.Abs(candle.Delta), maxDelta);
				maxVolume = Math.Max(candle.Volume, maxVolume);
				minDelta = Math.Min(candle.MinDelta, minDelta);
				maxAsk = Math.Max(candle.Ask, maxAsk);
				maxBid = Math.Max(candle.Bid, maxBid);
				maxMaxDelta = Math.Max(Math.Abs(candle.MaxDelta), maxMaxDelta);
				maxMinDelta = Math.Max(Math.Abs(candle.MinDelta), maxMinDelta);
				maxSessionDelta = Math.Max(Math.Abs(_cDelta[i]), maxSessionDelta);

				if (candle.Volume is not 0)
					maxDeltaPerVolume = Math.Max(Math.Abs(100 * candle.Delta / candle.Volume), maxDeltaPerVolume);

				maxSessionDeltaPerVolume = Math.Max(Math.Abs(_cDeltaPerVol[i]), maxSessionDeltaPerVolume);
				cumVolume += candle.Volume;

				if (i == 0)
					continue;

				maxDeltaChange = Math.Max(Math.Abs(_deltaChange[i]), maxDeltaChange);
				maxHeight = Math.Max(candle.High - candle.Low, maxHeight);
				maxTicks = Math.Max(candle.Ticks, maxTicks);
				maxDuration = Math.Max(_candleDurations[i], maxDuration);
			}

			maxVolumeSec = _volPerSecond.MAX(LastVisibleBarNumber - FirstVisibleBarNumber + 1, LastVisibleBarNumber);
		}
		else
		{
			maxAsk = _maxAsk;
			maxBid = _maxBid;
			maxSessionDelta = _maxSessionDelta;
			maxDeltaPerVolume = _maxDeltaPerVolume;
			maxSessionDeltaPerVolume = _maxSessionDeltaPerVolume;
			maxDelta = _maxDelta;
			minDelta = _minDelta;
			maxMaxDelta = _maxMaxDelta;
			maxMinDelta = _maxMinDelta;
			maxVolume = _maxVolume;
			maxTicks = _maxTicks;
			maxDuration = _maxDuration;
			cumVolume = _cumVolume;
			maxDeltaChange = _maxDeltaChange;
			maxHeight = _maxHeight;
			maxVolumeSec = CurrentBar > 0
				? Math.Max(_maxClosedVolumeSec, _volPerSecond[CurrentBar - 1])
				: 0;
		}

		// Scales of the rows added by this indicator.
		_rowScale.Clear();

		foreach (var (type, maximum) in _rowMaxima)
		{
			if (RowsOrder[type].Enabled)
				_rowScale[type] = VisibleProportion ? maximum.Visible(FirstVisibleBarNumber, LastVisibleBarNumber) : maximum.Value(CurrentBar);
		}

		return new MaxValues
		{
			MaxAsk = maxAsk,
			MaxBid = maxBid,
			MaxSessionDelta = maxSessionDelta,
			MaxDeltaPerVolume = maxDeltaPerVolume,
			MaxSessionDeltaPerVolume = maxSessionDeltaPerVolume,
			MaxDelta = maxDelta,
			MinDelta = minDelta,
			MaxMaxDelta = maxMaxDelta,
			MaxMinDelta = maxMinDelta,
			MaxVolume = maxVolume,
			MaxTicks = maxTicks,
			MaxDuration = maxDuration,
			CumVolume = cumVolume,
			MaxDeltaChange = maxDeltaChange,
			MaxHeight = maxHeight,
			MaxVolumeSec = maxVolumeSec
		};
	}

	private string GetValueText(DataType type, IndicatorCandle candle, int bar)
	{
		return type switch
		{
			DataType.Ask => ChartInfo.TryGetMinimizedVolumeString(candle.Ask),
			DataType.Bid => ChartInfo.TryGetMinimizedVolumeString(candle.Bid),
			DataType.Delta => ChartInfo.TryGetMinimizedVolumeString(candle.Delta),
			DataType.DeltaVolume => FormatRatio(candle.Volume == 0 ? 0 : candle.Delta * 100m / candle.Volume),
			DataType.SessionDelta => ChartInfo.TryGetMinimizedVolumeString(_cDelta[bar]),
			DataType.SessionDeltaVolume => FormatRatio(_cDeltaPerVol[bar]),
			DataType.MaxDelta => ChartInfo.TryGetMinimizedVolumeString(candle.MaxDelta),
			DataType.MinDelta => ChartInfo.TryGetMinimizedVolumeString(candle.MinDelta),
			DataType.DeltaChange => ChartInfo.TryGetMinimizedVolumeString(_deltaChange[bar]),
			DataType.Volume => ChartInfo.TryGetMinimizedVolumeString(candle.Volume),
			DataType.VolumeSecond => ChartInfo.TryGetMinimizedVolumeString(_volPerSecond[bar]),
			DataType.SessionVolume => ChartInfo.TryGetMinimizedVolumeString(_cVolume[bar]),
			DataType.Trades => candle.Ticks.ToString(CultureInfo.InvariantCulture),
			DataType.Height => _candleHeights[bar].ToString(CultureInfo.InvariantCulture),
			DataType.Time => candle.Time.Add(TimeOffset).ToString("HH:mm:ss"),
			DataType.Duration => ((int)(candle.LastTime - candle.Time).TotalSeconds).ToString(),
			DataType.DeltaSecond => ChartInfo.TryGetMinimizedVolumeString(_deltaPerSecond[bar]),
			DataType.BuyImbalance => _buyImbalance[bar].ToString("0", CultureInfo.InvariantCulture),
			DataType.SellImbalance => _sellImbalance[bar].ToString("0", CultureInfo.InvariantCulture),
			DataType.NetImbalance => _netImbalance[bar].ToString("+0;-0;0", CultureInfo.InvariantCulture),
			DataType.StackedBuyImbalance => _stackedBuyImbalance[bar].ToString("0", CultureInfo.InvariantCulture),
			DataType.StackedSellImbalance => _stackedSellImbalance[bar].ToString("0", CultureInfo.InvariantCulture),
			DataType.StackedNetImbalance => _stackedNetImbalance[bar].ToString("+0;-0;0", CultureInfo.InvariantCulture),
			DataType.PeakVolPerSec => ChartInfo.TryGetMinimizedVolumeString(Math.Round(_peakVolPerSec[bar])),
			DataType.PeakDeltaPerSec => ChartInfo.TryGetMinimizedVolumeString(Math.Round(_peakDeltaPerSec[bar])),
			DataType.PeakDeltaPerVol => FormatRatio(_peakDeltaPerVol[bar]),
			DataType.None => string.Empty,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private void DrawToolTip(RenderContext g, Point location, string text, System.Drawing.Color bgColor)
	{
		var bounds = g.ClipBounds;
		g.ResetClip();

		const int offset = 15;

		var x = location.X;
		var y = location.Y;

		var size = g.MeasureString(text, Font.RenderObject);
		var height = size.Height + 10;
		var rect = new Rectangle(x + offset, y - height - 20, size.Width + 20, height);

		var center = rect.Y + rect.Height / 2;

		Point[] points =
		[
			new(x, y),
			new(x + offset, center - (int)(0.3 * height)),
			new(x + offset, center + (int)(0.3 * height))
		];

		g.FillPolygon(_textColor, points);

		var pen = new RenderPen(_textColor, 2);
		g.DrawRectangle(pen, rect, 2);
		g.FillRectangle(bgColor, rect);
		g.DrawString(text, Font.RenderObject, _textColor, rect, _tipFormat);

		g.SetClip(bounds);
	}

	private void CacheChanged()
	{
		if (_pressedString is DataType.None)
		{
			RowsOrder.AvailableStrings.SkipIdx = -1;
			return;
		}

		var idx = RowsOrder.AvailableStrings.IndexOfValue(_pressedString);

		if (idx is -1)
			throw new KeyNotFoundException("Type " + _pressedString + " not found at cache");

		RowsOrder.AvailableStrings.SkipIdx = idx;
	}

	private void OnFontPropertyChanged(string propertyName)
	{
		_layoutChanged = true;
	}

	private System.Drawing.Color GetDeltaChangeBrush(int bar, decimal rate)
	{
		var change = _deltaChange[bar];
		var rectColor = change > 0 ? AskColor : BidColor;
		return Blend(rectColor, BackGroundColor, rate);
	}

	private string GetHeader(DataType type)
	{
		return type switch
		{
			DataType.Ask => "Ask",
			DataType.Bid => "Bid",
			DataType.Delta => "Delta",
			DataType.DeltaVolume => "Delta/Volume",
			DataType.SessionDelta => "Session Delta",
			DataType.SessionDeltaVolume => "Session Delta/Volume",
			DataType.MaxDelta => "Max.Delta",
			DataType.MinDelta => "Min.Delta",
			DataType.DeltaChange => "Delta Change",
			DataType.Volume => "Volume",
			DataType.VolumeSecond => "Volume/sec",
			DataType.SessionVolume => "Session Volume",
			DataType.Trades => "Trades",
			DataType.Height => "Height",
			DataType.Time => "Time",
			DataType.Duration => "Duration",
			DataType.DeltaSecond => "Delta/sec",
			DataType.BuyImbalance => "Buy Imb",
			DataType.SellImbalance => "Sell Imb",
			DataType.NetImbalance => "Net Imb",
			DataType.StackedBuyImbalance => "Buy Stk.",
			DataType.StackedSellImbalance => "Sell Stk.",
			DataType.StackedNetImbalance => "Net Stk.",
			DataType.PeakVolPerSec => "Max Vol/sec",
			DataType.PeakDeltaPerSec => "Delta at Max vol/sec",
			DataType.PeakDeltaPerVol => "Delta/Vol at Max vol/sec",
			DataType.None => string.Empty,

			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private bool ShouldComputeImbalances()
	{
		return RowsOrder[DataType.BuyImbalance].Enabled
			|| RowsOrder[DataType.SellImbalance].Enabled
			|| RowsOrder[DataType.NetImbalance].Enabled
			|| RowsOrder[DataType.StackedBuyImbalance].Enabled
			|| RowsOrder[DataType.StackedSellImbalance].Enabled
			|| RowsOrder[DataType.StackedNetImbalance].Enabled
			|| _useNetImbalanceAlert;
	}

	private void CalculateImbalances(int bar, IndicatorCandle candle)
	{
		_levels.Clear();

		foreach (var level in candle.GetAllPriceLevels())
			_levels.Add(new PriceLevel(level.Price, level.Ask, level.Bid));

		var counts = ImbalanceCounter.Count(_levels, _imbalance);

		_buyImbalance[bar] = counts.Buy;
		_sellImbalance[bar] = counts.Sell;
		_netImbalance[bar] = counts.Net;
		_stackedBuyImbalance[bar] = counts.StackedBuy;
		_stackedSellImbalance[bar] = counts.StackedSell;
		_stackedNetImbalance[bar] = counts.StackedNet;
	}

	private bool PeakRowsEnabled()
	{
		return RowsOrder[DataType.PeakVolPerSec].Enabled
			|| RowsOrder[DataType.PeakDeltaPerSec].Enabled
			|| RowsOrder[DataType.PeakDeltaPerVol].Enabled;
	}

	// The tape history is only requested while a peak row is shown: showing the first one (or
	// hiding the last) recalculates, and so does a change of the tape settings while shown.
	private void OnTapeUseChanged()
	{
		bool running;

		lock (_tapeSync)
			running = _tape != null;

		if (running != PeakRowsEnabled())
			RecalculateValues();
	}

	private void OnTapeSettingsChanged()
	{
		if (PeakRowsEnabled())
			RecalculateValues();
	}

	private void RequestTapeHistory()
	{
		lock (_tapeSync)
		{
			if (_tape == null || CurrentBar < 1)
			{
				_tapeHistoryLoaded = true;
				return;
			}
		}

		var first = FirstTapeBar();

		lock (_tapeSync)
			_firstTapeBar = first;

		var request = new CumulativeTradesRequest(GetCandle(first).Time, GetCandle(CurrentBar - 1).LastTime, 0, 0);

		lock (_tapeSync)
			_tapeRequestId = request.RequestId;

		this.LogInfo($"ClusterStatisticPro: tape history request from bar {first} of {CurrentBar}.");
		RequestForCumulativeTrades(request);
	}

	private int FirstTapeBar()
	{
		if (_tapeSessionsToLoad <= 0)
			return 0;

		var sessions = 0;

		for (var bar = CurrentBar - 1; bar > 0; bar--)
		{
			if (IsNewSession(bar) && ++sessions == _tapeSessionsToLoad)
				return bar;
		}

		return 0;
	}

	// The peak rows are scaled against the mean of the recent bar peaks (the auto filter) when it
	// is on and has bars, otherwise against their maximum like the other rows.
	private decimal GetPeakRate(DataType type, int bar)
	{
		decimal meanVol = 0, meanDelta = 0;

		lock (_tapeSync)
		{
			if (_peakAutoFilter && _meanVolPerSec != null)
			{
				meanVol = _meanVolPerSec.MeanFor(bar);
				meanDelta = _meanDeltaPerSec.MeanFor(bar);
			}
		}

		var value = type switch
		{
			DataType.PeakVolPerSec => _peakVolPerSec[bar],
			DataType.PeakDeltaPerSec => Math.Abs(_peakDeltaPerSec[bar]),
			_ => Math.Abs(_peakDeltaPerVol[bar])
		};

		var mean = type switch
		{
			DataType.PeakVolPerSec => meanVol,
			DataType.PeakDeltaPerSec => meanDelta,
			_ => meanVol == 0 ? 0 : meanDelta / meanVol * 100m
		};

		return mean > 0 ? GetRateByMean(value, mean) : GetRate(value, RowScale(type));
	}

	// 10 to 100 by how far the value is above or below the mean; the power widens the contrast
	// of the peaks above the mean.
	private static decimal GetRateByMean(decimal value, decimal mean)
	{
		var ratio = (decimal)Math.Pow((double)(value / mean), 1.35);
		const decimal low = 0.85m, high = 1.35m;

		if (ratio <= low)
			return 10;

		if (ratio >= high)
			return 100;

		return 10 + (ratio - low) * (90 / (high - low));
	}

	private void RebuildPeakMeans()
	{
		lock (_tapeSync)
			RebuildPeakMeansLocked();

		RedrawChart();
	}

	// Caller holds _tapeSync. The closed bars with prints, in order.
	private void RebuildPeakMeansLocked()
	{
		_meanVolPerSec = new PeakMean(_peakAutoFilterPeriod, _peakAutoFilterEma);
		_meanDeltaPerSec = new PeakMean(_peakAutoFilterPeriod, _peakAutoFilterEma);

		if (_tape == null || !_tapeHistoryLoaded || _firstTapeBar < 0)
			return;

		for (var bar = _firstTapeBar; bar < CurrentBar - 1; bar++)
		{
			_meanVolPerSec.AddClosedBar(bar, _peakVolPerSec[bar]);
			_meanDeltaPerSec.AddClosedBar(bar, _peakDeltaPerSec[bar]);
		}
	}

	private void AddClosedBarToPeakMeans(int bar)
	{
		lock (_tapeSync)
		{
			if (_tape == null || !_tapeHistoryLoaded || _meanVolPerSec == null || bar < _firstTapeBar || bar <= _meanVolPerSec.LastBar)
				return;

			_meanVolPerSec.AddClosedBar(bar, _peakVolPerSec[bar]);
			_meanDeltaPerSec.AddClosedBar(bar, _peakDeltaPerSec[bar]);
		}
	}

	// Caller holds _tapeSync.
	private void ProcessTick(int bar, Tick tick)
	{
		if (bar < 0 || !_tape.Add(bar, tick) || !_tape.TryGet(bar, out var peak))
			return;

		_peakVolPerSec[bar] = peak.VolumePerSecond;
		_peakDeltaPerSec[bar] = peak.DeltaPerSecond;
		_peakDeltaPerVol[bar] = peak.DeltaPerVolume * 100m;
	}

	// Last bar starting at or before the time (bar times never decrease).
	private int BarOfTime(DateTime time)
	{
		int lo = 0, hi = CurrentBar - 1, result = -1;

		while (lo <= hi)
		{
			var mid = lo + (hi - lo) / 2;

			if (GetCandle(mid).Time <= time)
			{
				result = mid;
				lo = mid + 1;
			}
			else
				hi = mid - 1;
		}

		return result;
	}

	private static int SideOf(TradeDirection direction)
	{
		return direction switch
		{
			TradeDirection.Buy => 1,
			TradeDirection.Sell => -1,
			_ => 0
		};
	}

	private void CheckNetImbalanceAlert(int bar)
	{
		var threshold = Math.Max(1, NetImbalanceAlertValue);

		if (_netImbalanceAlertOnClose)
		{
			// The bar that has just closed, once, on the first update of the next one.
			var closed = bar - 1;

			if (closed >= 0 && _netImbalanceAlert.OnBarClosed(closed, (int)_netImbalance[closed], closed > 0 ? (int)_netImbalance[closed - 1] : 0, threshold))
				AddAlert(NetImbalanceAlertFile, string.Format(Res.AlertNetImbalanceTemplate, (int)_netImbalance[closed], threshold));

			return;
		}

		var net = (int)_netImbalance[bar];

		if (_netImbalanceAlert.OnLiveUpdate(bar, net, threshold))
			AddAlert(NetImbalanceAlertFile, string.Format(Res.AlertNetImbalanceTemplate, net, threshold));
	}

	// The imbalances are only calculated while a row (or the alert) uses them: when one starts
	// using them, or their settings change, the history is calculated at once, without a full
	// recalculation of the indicator.
	private void OnImbalanceUseChanged()
	{
		if (ShouldComputeImbalances())
			RebuildImbalances();
	}

	private void RebuildImbalances()
	{
		if (!ShouldComputeImbalances() || CurrentBar <= 0)
			return;

		for (var bar = 0; bar < CurrentBar; bar++)
			CalculateImbalances(bar, GetCandle(bar));

		foreach (var maximum in _rowMaxima.Values)
			maximum.Update(CurrentBar - 1);

		RedrawChart();
	}

	// A ratio given in percent, shown as a percentage (25%) or as a fraction (0.25).
	private string FormatRatio(decimal percent)
	{
		if (!RatiosAsPercent)
			return (percent / 100m).ToString("0.00", CultureInfo.InvariantCulture);

		return (Math.Abs(percent) >= 1 ? percent.ToString("0.", CultureInfo.InvariantCulture) : percent.ToString("0.#", CultureInfo.InvariantCulture)) + "%";
	}

	private decimal RowScale(DataType type)
	{
		return _rowScale.TryGetValue(type, out var scale) ? scale : 0;
	}

	private decimal GetRate(decimal value, decimal maximumValue)
	{
		if (maximumValue == 0)
			return 10;

		var rate = value * 100.0m / (maximumValue * 0.6m);

		if (rate < 10)
			rate = 10;

		if (rate > 100)
			return 100;

		return rate;
	}

	private System.Drawing.Color Blend(Color color, Color backColor, decimal amount)
	{
		var r = (byte)(color.R + (backColor.R - color.R) * (1 - amount * 0.01m));
		var g = (byte)(color.G + (backColor.G - color.G) * (1 - amount * 0.01m));
		var b = (byte)(color.B + (backColor.B - color.B) * (1 - amount * 0.01m));
		return System.Drawing.Color.FromArgb(_bgAlpha, r, g, b);
	}

	private bool CheckStartBar(int bar)
	{
		switch (_sessionMode)
		{
			case SessionMode.None:
				return bar == 0;
			case SessionMode.DefaultSession:
				return IsNewSession(bar);
			case SessionMode.CustomSession:
				if (bar == 0)
					return true;

				var candle = GetCandle(bar);
				var prevCandle = GetCandle(bar - 1);

				// A session starts when the bar belongs to a later session day than the previous
				// one. Shifting the times by the start time makes each session one calendar day,
				// so a start at 00:00 or one that falls in a gap (overnight, weekend) is not missed.
				var start = CustomSessionStart.Value;
				var prevDay = (prevCandle.Time.Add(TimeOffset) - start).Date;
				var day = (candle.Time.Add(TimeOffset) - start).Date;

				return day > prevDay;
			default:
				return false;
		}
	}

	#endregion
}