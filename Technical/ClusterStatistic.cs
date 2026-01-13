using ATAS.DataFeedsCore;
using ATAS.Indicators;
using System;
using System.Xml.Linq;

namespace ATAS.Indicators.Technical;

using ATAS.Indicators.Drawing;
using ATAS.Indicators.Technical.Extensions;
using OFT.Attributes;
using OFT.Localization;
using OFT.Rendering;
using OFT.Rendering.Context;
using OFT.Rendering.Control;
using OFT.Rendering.Settings;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Utils.Common.Logging;
using Color = CrossColor;

[DisplayName("Cluster Statistic")]
[Category(IndicatorCategories.VolumeOrderFlow)]
[Display(ResourceType = typeof(Strings), Description = nameof(Strings.ClusterStatisticDescription))]
[HelpLink("https://help.atas.net/support/solutions/articles/72000602624")]
public class ClusterStatistic : Indicator
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
			// 1) Pace / activity
			Add(DataType.VolumeSecond, new RenderInfo(0));
			Add(DataType.DeltaSecond, new RenderInfo(1));
			Add(DataType.PeakVolPerSec, new RenderInfo(2));
			Add(DataType.PeakDeltaPerSec, new RenderInfo(3));
			Add(DataType.PeakDeltaPerVol, new RenderInfo(4));

			// 2) Directional pressure (incl. extremes)
			Add(DataType.Delta, new RenderInfo(5));
			Add(DataType.DeltaVolume, new RenderInfo(6));
			Add(DataType.DeltaChange, new RenderInfo(7));
			Add(DataType.MaxDelta, new RenderInfo(8));
			Add(DataType.MinDelta, new RenderInfo(9));

			// 3) Imbalances
			Add(DataType.BuyImbalance, new RenderInfo(10));
			Add(DataType.SellImbalance, new RenderInfo(11));
			Add(DataType.NetImbalance, new RenderInfo(12));
			Add(DataType.StackedBuyImbalance, new RenderInfo(13));
			Add(DataType.StackedSellImbalance, new RenderInfo(14));
			Add(DataType.StackedNetImbalance, new RenderInfo(15));

			// 4) Candle context
			Add(DataType.Volume, new RenderInfo(16));
			Add(DataType.Trades, new RenderInfo(17));
			Add(DataType.Height, new RenderInfo(18));
			Add(DataType.Duration, new RenderInfo(19));
			Add(DataType.Time, new RenderInfo(20));

			// 5) Session context
			Add(DataType.SessionVolume, new RenderInfo(21));
			Add(DataType.SessionDelta, new RenderInfo(22));
			Add(DataType.SessionDeltaVolume, new RenderInfo(23));

			// 6) Raw prints (least useful under pressure)
			Add(DataType.Ask, new RenderInfo(24));
			Add(DataType.Bid, new RenderInfo(25));
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

		public decimal MaxDeltaSec { get; set; }

		public decimal MaxPeakVolPerSec { get; set; }

		public decimal MaxPeakDeltaPerSec { get; set; }
		public decimal MaxPeakDeltaPerVol { get; set; }
		public int MaxBuyImb { get; set; }
		public int MaxSellImb { get; set; }
		public int MaxNetImb { get; set; }
		public int MaxStackedBuyImb { get; set; }
		public int MaxStackedSellImb { get; set; }
		public int MaxStackedNetImb { get; set; }

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
		DeltaSecond,
		SessionVolume,
		Trades,
		Height,
		Time,
		Duration,
		PeakVolPerSec,
		PeakDeltaPerSec,
		PeakDeltaPerVol,
		BuyImbalance,
		SellImbalance,
		NetImbalance,
		StackedBuyImbalance,
		StackedSellImbalance,
		StackedNetImbalance,
		None
	}

	public enum SessionMode
	{
		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.None))]
		None,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Default))]
		DefaultSession,

		[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CustomSession))]
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
	private readonly ValueDataSeries _peakVolPerSec = new("PeakVolPerSec");
	private readonly ValueDataSeries _peakDeltaPerSec = new("PeakDeltaPerSec");
	private readonly ValueDataSeries _peakDeltaPerVol = new("Delta/Vol at Max vol/sec");
	private readonly ValueDataSeries _peakVolAuto = new("MaxVol/sec AutoThr");
	private readonly ValueDataSeries _peakDeltaAuto = new("Delta@MaxVol AutoThr");
	private readonly ValueDataSeries _buyImbalance = new("BuyImbalance");
	private readonly ValueDataSeries _sellImbalance = new("SellImbalance");
	private readonly ValueDataSeries _netImbalance = new("NetImbalance");
	private readonly ValueDataSeries _stackedBuyImbalance = new("StackedBuyImbalance");
	private readonly ValueDataSeries _stackedSellImbalance = new("StackedSellImbalance");
	private readonly ValueDataSeries _stackedNetImbalance = new("StackedNetImbalance");


	// --- SoT core state (historical cumulative trades) ---
	private List<CumulativeTrade> _allCumulativeTrades;

	// --- SoT params (backing fields) ---
	private int _sotTimeWindowSec = 5;
	private int _sotMinVolume = 150;

	// --- Sliding window sample for RT computed in OnCalculate ---
	private sealed class Sample
	{
		public DateTime T;
		public decimal Vol;   // incremental volume since last sample
		public decimal Delta; // incremental delta since last sample
	}

	private readonly Queue<Sample> _win = new(); // RT rolling window across bars
	private decimal _winVol = 0m;
	private decimal _winDelta = 0m;

	private int _rtBar = -1;            // last observed live bar index
	private decimal _prevCumVol = 0m;   // candle.Volume seen on previous tick
	private decimal _prevCumDelta = 0m; // candle.Delta seen on previous tick

	private decimal _rtPeakVolPerSec = 0m;
	private decimal _rtPeakDeltaPerSec = 0m;

	private bool _seededLiveSoT = false;
	private bool _hasSoTSampleThisBar = false;

	// AutoFilter state
	private int _afCount;
	private decimal _afVol, _afDelta, _afVolEma, _afDeltaEma;
	private readonly Queue<decimal> _afVolSma = new();
	private readonly Queue<decimal> _afDeltaSma = new();
	private decimal _afVolSmaSum;
	private decimal _afDeltaSmaSum;
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

	// --- Outline pens (high-rate highlighting) ---
	private readonly RenderPen _outlinePenLight = new(System.Drawing.Color.White, 1);
	private readonly RenderPen _outlinePenDark = new(System.Drawing.Color.Black, 1);


	private int _height = 15;

	private int _lastBar = -1;
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
	private decimal _lastHeightValue;

	// --- Net Imbalance alert state (no spam; threshold crossing) ---
	private int _lastNetImbalanceAlertBar = -1;   // bar index for which alert was already evaluated/fired
	private int _prevNetImbalanceLive;           // previous observed net (live bar only)
	private bool _hasPrevNetImbalanceLive;       // whether _prevNetImbalanceLive is valid (live bar only)


	private RenderPen _linePen = new(System.Drawing.Color.Transparent);
	private decimal _maxAsk;
	private decimal _maxBid;
	private decimal _maxDelta;
	private decimal _maxDeltaChange;
	private decimal _maxDeltaSec;
	private decimal _maxDeltaPerVolume;
	private decimal _maxDuration;
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
    private bool _ratiosAsPercent = true;

    private System.Drawing.Color _textColor;
    private int _fontHeight;
    private SessionMode _sessionMode = SessionMode.DefaultSession;

    // --- Imbalances rebuild control (only when enabled/params changed) ---
    private bool _imbalancesDirty;

    [Browsable(false)]
	public RenderOrder RowsOrder = new();
    private FilterTimeSpan _customSessionStart;

	#endregion

	#region Properties

	private int StrCount => RowsOrder.AvailableStrings.Count;

    #region Rows

    // ============================================================
    // Rows (scalper-first order)
    // 1100–1190 Pace / Activity
    // 1200–1290 Directional Pressure
    // 1300–1390 Imbalances
    // 1500–1590 Candle Context
    // 1600–1690 Session Context
    // 1700–1790 Raw Prints
    // ============================================================

    // ----------------------------
    // Pace / Activity (1100–1190)
    // ----------------------------

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowVolumePerSecond),
        GroupName = nameof(Resources.RowsPaceGroup),
        Description = nameof(Resources.ShowVolumePerSecondDescription), Order = 1100)]
    public bool ShowVolumePerSecond
	{
		get => _showVolumePerSecond;
		set
		{
			_showVolumePerSecond = value;
			RowsOrder.SetEnabled(DataType.VolumeSecond, value);
		}
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowDeltaPerSecond),
        GroupName = nameof(Resources.RowsPaceGroup),
        Description = nameof(Resources.ShowDeltaPerSecondDescription), Order = 1110)]
    public bool ShowDeltaPerSecond
	{
		get => _showDeltaPerSecond;
		set
		{
			_showDeltaPerSecond = value;
			RowsOrder.SetEnabled(DataType.DeltaSecond, value);
		}
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowPeakVolPerSec),
        GroupName = nameof(Resources.RowsPaceGroup), Order = 1120)]
    public bool ShowPeakVolPerSec
	{
		get => RowsOrder.TryGetValue(DataType.PeakVolPerSec, out var ri) && ri.Enabled;
		set => RowsOrder.SetEnabled(DataType.PeakVolPerSec, value);
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowPeakDeltaPerSec),
        GroupName = nameof(Resources.RowsPaceGroup), Order = 1130)]
    public bool ShowPeakDeltaPerSec
	{
		get => RowsOrder.TryGetValue(DataType.PeakDeltaPerSec, out var ri) && ri.Enabled;
		set => RowsOrder.SetEnabled(DataType.PeakDeltaPerSec, value);
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowPeakDeltaPerVol),
        GroupName = nameof(Resources.RowsPaceGroup), Order = 1140)]
    public bool ShowPeakDeltaPerVol
	{
		get => RowsOrder.TryGetValue(DataType.PeakDeltaPerVol, out var ri) && ri.Enabled;
		set => RowsOrder.SetEnabled(DataType.PeakDeltaPerVol, value);
	}

    // ----------------------------
    // Directional Pressure (1200–1290)
    // ----------------------------

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowDelta),
        GroupName = nameof(Resources.RowsPressureGroup),
        Description = nameof(Resources.ShowDeltaDescription), Order = 1200)]
    public bool ShowDelta
    {
        get => _showDelta;
        set
        {
            _showDelta = value;
            RowsOrder.SetEnabled(DataType.Delta, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowDeltaPerVolume),
        GroupName = nameof(Resources.RowsPressureGroup),
        Description = nameof(Resources.ShowDeltaPerVolumeDescription), Order = 1210)]
    public bool ShowDeltaPerVolume
    {
        get => _showDeltaPerVolume;
        set
        {
            _showDeltaPerVolume = value;
            RowsOrder.SetEnabled(DataType.DeltaVolume, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowDeltaChange),
        GroupName = nameof(Resources.RowsPressureGroup),
        Description = nameof(Resources.ShowDeltaChangeDescription), Order = 1220)]
    public bool ShowDeltaChange
    {
        get => _showDeltaChange;
        set
        {
            _showDeltaChange = value;
            RowsOrder.SetEnabled(DataType.DeltaChange, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowMaximumDelta),
        GroupName = nameof(Resources.RowsPressureGroup),
        Description = nameof(Resources.ShowMaximumDeltaDescription), Order = 1230)]
    public bool ShowMaximumDelta
    {
        get => _showMaximumDelta;
        set
        {
            _showMaximumDelta = value;
            RowsOrder.SetEnabled(DataType.MaxDelta, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowMinimumDelta),
        GroupName = nameof(Resources.RowsPressureGroup),
        Description = nameof(Resources.ShowMinimumDeltaDescription), Order = 1240)]
    public bool ShowMinimumDelta
    {
        get => _showMinimumDelta;
        set
        {
            _showMinimumDelta = value;
            RowsOrder.SetEnabled(DataType.MinDelta, value);
            _layoutChanged = true;
        }
    }


    // ----------------------------
    // Imbalances (1300ï¿½1390)
    // ----------------------------

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowBuyImbalances),
        GroupName = nameof(Resources.RowsImbalanceRowsGroup), Order = 1300)]
    public bool ShowBuyImbalance
	{
		get => RowsOrder.TryGetValue(DataType.BuyImbalance, out var ri) && ri.Enabled;
		set
		{
			var wasEnabled = RowsOrder.TryGetValue(DataType.BuyImbalance, out var ri) && ri.Enabled;
			RowsOrder.SetEnabled(DataType.BuyImbalance, value);
			if (!wasEnabled && value)
				MarkImbalancesDirty();

            _layoutChanged = true;
        }
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowSellImbalances),
        GroupName = nameof(Resources.RowsImbalanceRowsGroup), Order = 1310)]
    public bool ShowSellImbalance
	{
		get => RowsOrder.TryGetValue(DataType.SellImbalance, out var ri) && ri.Enabled;
		set
		{
			var wasEnabled = RowsOrder.TryGetValue(DataType.SellImbalance, out var ri) && ri.Enabled;
			RowsOrder.SetEnabled(DataType.SellImbalance, value);
			if (!wasEnabled && value)
				MarkImbalancesDirty();

            _layoutChanged = true;
        }
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowNetImbalances),
        GroupName = nameof(Resources.RowsImbalanceRowsGroup), Order = 1320)]
    public bool ShowNetImbalance
	{
		get => RowsOrder.TryGetValue(DataType.NetImbalance, out var ri) && ri.Enabled;
		set
		{
			var wasEnabled = RowsOrder.TryGetValue(DataType.NetImbalance, out var ri) && ri.Enabled;
			RowsOrder.SetEnabled(DataType.NetImbalance, value);
			if (!wasEnabled && value)
				MarkImbalancesDirty();

			_layoutChanged = true;
		}
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowStackedBuyImbalances),
        GroupName = nameof(Resources.RowsImbalanceRowsGroup), Order = 1330)]
    public bool ShowStackedBuyImbalance
	{
		get => RowsOrder.TryGetValue(DataType.StackedBuyImbalance, out var ri) && ri.Enabled;
		set
		{
			var wasEnabled = RowsOrder.TryGetValue(DataType.StackedBuyImbalance, out var ri) && ri.Enabled;
			RowsOrder.SetEnabled(DataType.StackedBuyImbalance, value);
			if (!wasEnabled && value)
				MarkImbalancesDirty();

			_layoutChanged = true;
		}
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowStackedSellImbalances),
        GroupName = nameof(Resources.RowsImbalanceRowsGroup), Order = 1340)]
    public bool ShowStackedSellImbalance
	{
		get => RowsOrder.TryGetValue(DataType.StackedSellImbalance, out var ri) && ri.Enabled;
		set
		{
			var wasEnabled = RowsOrder.TryGetValue(DataType.StackedSellImbalance, out var ri) && ri.Enabled;
			RowsOrder.SetEnabled(DataType.StackedSellImbalance, value);
			if (!wasEnabled && value)
				MarkImbalancesDirty();

			_layoutChanged = true;
		}
	}

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowStackedNetImbalances),
        GroupName = nameof(Resources.RowsImbalanceRowsGroup), Order = 1350)]
    public bool ShowStackedNetImbalance
	{
		get => RowsOrder.TryGetValue(DataType.StackedNetImbalance, out var ri) && ri.Enabled;
		set
		{
			var wasEnabled = RowsOrder.TryGetValue(DataType.StackedNetImbalance, out var ri) && ri.Enabled;
			RowsOrder.SetEnabled(DataType.StackedNetImbalance, value);
			if (!wasEnabled && value)
				MarkImbalancesDirty();

			_layoutChanged = true;
		}
	}

    // ----------------------------
    // Candle Context (1500–1590)
    // ----------------------------

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowVolume),
    GroupName = nameof(Resources.RowsCandleContextGroup),
    Description = nameof(Resources.ShowVolumesDescription), Order = 1500)]
    public bool ShowVolume
    {
        get => _showVolume;
        set
        {
            _showVolume = value;
            RowsOrder.SetEnabled(DataType.Volume, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowTradesCount),
        GroupName = nameof(Resources.RowsCandleContextGroup),
        Description = nameof(Resources.ShowTradesCountDescription), Order = 1510)]
    public bool ShowTicks
    {
        get => _showTicks;
        set
        {
            _showTicks = value;
            RowsOrder.SetEnabled(DataType.Trades, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowHeight),
        GroupName = nameof(Resources.RowsCandleContextGroup),
        Description = nameof(Resources.ShowCandleHeightDescription), Order = 1520)]
    public bool ShowHighLow
    {
        get => _showHighLow;
        set
        {
            _showHighLow = value;
            RowsOrder.SetEnabled(DataType.Height, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowDuration),
        GroupName = nameof(Resources.RowsCandleContextGroup),
        Description = nameof(Resources.ShowCandleDurationDescription), Order = 1530)]
    public bool ShowDuration
    {
        get => _showDuration;
        set
        {
            _showDuration = value;
            RowsOrder.SetEnabled(DataType.Duration, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowTime),
        GroupName = nameof(Resources.RowsCandleContextGroup),
        Description = nameof(Resources.ShowCandleTimeDescription), Order = 1540)]
    public bool ShowTime
    {
        get => _showTime;
        set
        {
            _showTime = value;
            RowsOrder.SetEnabled(DataType.Time, value);
            _layoutChanged = true;
        }
    }


    // ----------------------------
    // Session Context (1600–1690)
    // ----------------------------

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowSessionVolume),
    GroupName = nameof(Resources.RowsSessionContextGroup),
    Description = nameof(Resources.ShowSessionVolumeDescription), Order = 1600)]
    public bool ShowSessionVolume
    {
        get => _showSessionVolume;
        set
        {
            _showSessionVolume = value;
            RowsOrder.SetEnabled(DataType.SessionVolume, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowSessionDelta),
        GroupName = nameof(Resources.RowsSessionContextGroup),
        Description = nameof(Resources.ShowSessionDeltaDescription), Order = 1610)]
    public bool ShowSessionDelta
    {
        get => _showSessionDelta;
        set
        {
            _showSessionDelta = value;
            RowsOrder.SetEnabled(DataType.SessionDelta, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowSessionDeltaPerVolume),
        GroupName = nameof(Resources.RowsSessionContextGroup),
        Description = nameof(Resources.ShowSessionDeltaPerVolumeDescription), Order = 1620)]
    public bool ShowSessionDeltaPerVolume
    {
        get => _showSessionDeltaPerVolume;
        set
        {
            _showSessionDeltaPerVolume = value;
            RowsOrder.SetEnabled(DataType.SessionDeltaVolume, value);
            _layoutChanged = true;
        }
    }


    // ----------------------------
    // Raw Prints (1700–1790)
    // ----------------------------

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowAsk),
        GroupName = nameof(Resources.RowsRawPrintsGroup),
        Description = nameof(Resources.ShowAsksDescription), Order = 1700)]
    public bool ShowAsk
    {
        get => _showAsk;
        set
        {
            _showAsk = value;
            RowsOrder.SetEnabled(DataType.Ask, value);
            _layoutChanged = true;
        }
    }

    [Display(ResourceType = typeof(Resources), Name = nameof(Resources.ShowBid),
        GroupName = nameof(Resources.RowsRawPrintsGroup),
        Description = nameof(Resources.ShowBidsDescription), Order = 1710)]
    public bool ShowBid
    {
        get => _showBid;
        set
        {
            _showBid = value;
            RowsOrder.SetEnabled(DataType.Bid, value);
            _layoutChanged = true;
        }
    }



    #endregion

    #region Max vol/sec settings

    // ============================================================
    // Max vol/sec (SoT) settings (9000ï¿½9090)
    // ============================================================

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.SotTimeWindowSecName),
        GroupName = nameof(Resources.MaxVolPerSecGroup),
        Order = 1800)]
    [Range(1, 600)]
	public int SotTimeWindowSec
	{
		get => _sotTimeWindowSec;
		set
		{
			var v = Math.Max(1, Math.Min(600, value));
			if (_sotTimeWindowSec == v)
				return;

			_sotTimeWindowSec = v;
			OnSoTParamsChanged();
		}
	}

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.SotMinVolumeName),
        GroupName = nameof(Resources.MaxVolPerSecGroup),
        Order = 1810)]
    [Range(1, 100000)]
	public int SotMinVolume
	{
		get => _sotMinVolume;
		set
		{
			var v = Math.Max(1, Math.Min(100000, value));
			if (_sotMinVolume == v)
				return;

			_sotMinVolume = v;
			OnSoTParamsChanged();
		}
	}

	private bool _sotUseAutoFilter = true;

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.SotUseAutoFilterName),
        GroupName = nameof(Resources.MaxVolPerSecGroup),
        Order = 1820)]
    public bool SotUseAutoFilter
	{
		get => _sotUseAutoFilter;
		set
		{
			if (_sotUseAutoFilter == value)
				return;

			_sotUseAutoFilter = value;
			ResetAutoFilter();
			RebuildHistoricalSoT();
		}
	}

	private int _sotAutoFilterPeriod = 3;

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.SotAutoFilterPeriodName),
        GroupName = nameof(Resources.MaxVolPerSecGroup),
        Order = 1830)]
    [Range(1, 200)]
	public int SotAutoFilterPeriod
	{
		get => _sotAutoFilterPeriod;
		set
		{
			var v = Math.Max(1, Math.Min(200, value));
			if (_sotAutoFilterPeriod == v)
				return;

			_sotAutoFilterPeriod = v;
			ResetAutoFilter();
			RebuildHistoricalSoT();
		}
	}

	private bool _sotAutoFilterUseEma = true;

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.SotAutoFilterUseEmaName),
        GroupName = nameof(Resources.MaxVolPerSecGroup),
        Order = 1840)]
    public bool SotAutoFilterUseEma
	{
		get => _sotAutoFilterUseEma;
		set
		{
			if (_sotAutoFilterUseEma == value)
				return;

			_sotAutoFilterUseEma = value;
			ResetAutoFilter();
			RebuildHistoricalSoT();
		}
	}

	#endregion

	#region Imbalance settings

	// ============================================================
	// Imbalance settings (2000ï¿½2090)
	// ============================================================

	private int _imbalanceThreshold = 300;

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.ImbalanceThresholdPercentName),
        GroupName = nameof(Resources.ImbalanceGroup),
        Order = 1900)]
    [Range(101, 999)]
	public int ImbalanceThreshold
	{
		get => _imbalanceThreshold;
		set
		{
			if (_imbalanceThreshold == value)
				return;

			_imbalanceThreshold = value;
			MarkImbalancesDirty();
		}
	}

	private int _imbalanceMinDominantVolume = 30;

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.ImbalanceMinDominantVolumeName),
        GroupName = nameof(Resources.ImbalanceGroup),
        Order = 1910)]
    [Range(1, 100000)]
	public int ImbalanceMinDominantVolume
	{
		get => _imbalanceMinDominantVolume;
		set
		{
			if (_imbalanceMinDominantVolume == value)
				return;

			_imbalanceMinDominantVolume = value;
			MarkImbalancesDirty();
		}
	}

	private int _imbalanceMinDifference = 30;

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.ImbalanceMinDifferenceName),
        GroupName = nameof(Resources.ImbalanceGroup),
        Order = 1920)]
    [Range(0, 100000)]
	public int ImbalanceMinDifference
	{
		get => _imbalanceMinDifference;
		set
		{
			if (_imbalanceMinDifference == value)
				return;

			_imbalanceMinDifference = value;
			MarkImbalancesDirty();
		}
	}


	private int _stackedImbalanceMinLevels = 3;

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.StackedImbalanceMinLevelsName),
        GroupName = nameof(Resources.ImbalanceGroup),
        Order = 1930)]
    [Range(2, 20)]
	public int StackedImbalanceMinLevels
	{
		get => _stackedImbalanceMinLevels;
		set
		{
			if (_stackedImbalanceMinLevels == value)
				return;

			_stackedImbalanceMinLevels = value;
			MarkImbalancesDirty();
		}
	}


	#endregion

    #region Session

    [Tab(TabName = nameof(Strings.Data), ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.SessionDeltaMode), GroupName = nameof(Strings.Session), Description = nameof(Strings.SessionModeDescription), Order = 100, ResourceType = typeof(Strings))]
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

    [Tab(TabName = nameof(Strings.Data), ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.CustomSessionStart), GroupName = nameof(Strings.Session), Description = nameof(Strings.CustomSessionStartDescription), Order = 110, ResourceType = typeof(Strings))]
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

    #region Colors

	// ============================================================
	// Visualization / Colors (6000ï¿½6090)
	// ============================================================

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BackGround), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.LabelFillColorDescription), Order = 6000)]
	public Color BackGroundColor { get; set; } = Color.FromArgb(120, 0, 0, 0);

	[Range(1, 10)]
	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Transparency), GroupName = nameof(Strings.Visualization), Order = 6010)]
	public int BgTransparency
	{
		get => _bgTransparency;
		set
		{
			_bgTransparency = value;
			_bgAlpha = (byte)(255 * value / 10);
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Grid), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.GridColorDescription), Order = 6020)]
	public Color GridColor
	{
		get => _linePen.Color.Convert();
		set
		{
			_linePen = new RenderPen(value.Convert());
			_selectionPen = new RenderPen(value.Convert(), 3);
		}
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.VisibleProportion), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.VisibleProportionDescription), Order = 6030)]
	public bool VisibleProportion { get; set; }

	[Display(Name = "Ratios as Percent", GroupName = nameof(Strings.Visualization), Order = 6040)]
    public bool RatiosAsPercent
	{
		get => _ratiosAsPercent;
		set => _ratiosAsPercent = value;
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Volume), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.VolumeColorDescription), Order = 6050)]
	public Color VolumeColor { get; set; } = CrossColors.DarkGray;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.AskColor), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.AskColorDescription), Order = 6060)]
	public Color AskColor { get; set; } = CrossColors.Green;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.BidColor), GroupName = nameof(Strings.Visualization),
		Description = nameof(Strings.BidColorDescription), Order = 6070)]
	public Color BidColor { get; set; } = CrossColors.Red;

	#endregion

	#region Text

	// ============================================================
	// Text (7000ï¿½7090)
	// ============================================================

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Text),
		Description = nameof(Strings.LabelTextColorDescription), Order = 7000)]
	public Color TextColor
	{
		get => _textColor.Convert();
		set => _textColor = value.Convert();
	}

    [Display(ResourceType = typeof(Strings), Name = nameof(Strings.Font), GroupName = nameof(Strings.Text),
        Description = nameof(Strings.FontSettingDescription), Order = 7010)]
    public FontSetting Font
    {
        get => _font;
        set => SetTrackedProperty(ref _font, value, OnFontPropertyChanged);
    }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.CenterAlign), GroupName = nameof(Strings.Text),
		Description = nameof(Strings.CenterAlignDescription), Order = 7020)]
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

	// ============================================================
	// Headers (8000ï¿½8090)
	// ============================================================

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Color), GroupName = nameof(Strings.Headers),
		Description = nameof(Strings.HeaderBackgroundDescription), Order = 8000)]
	public Color HeaderBackground
	{
		get => _headerBackground.Convert();
		set => _headerBackground = value.Convert();
	}

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.HideRowsDescription), GroupName = nameof(Strings.Headers),
		Description = nameof(Strings.HideHeadersDescription), Order = 8010)]
	public bool HideRowsDescription { get; set; }

	#endregion

	#region Volume Alert

	// ============================================================
	// Alerts (4000ï¿½4090 Volume, 5000ï¿½5090 Delta, 3000ï¿½3090 Net Imbalance)
	// ============================================================

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = nameof(Strings.VolumeAlert),
		Description = nameof(Strings.UseAlertDescription), Order = 4000)]
	public bool UseVolumeAlert { get; set; }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Filter), GroupName = nameof(Strings.VolumeAlert),
		Description = nameof(Strings.AlertFilterDescription), Order = 4010)]
	[Range(0, int.MaxValue)]
	public decimal VolumeAlertValue { get; set; }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.VolumeAlert),
		Description = nameof(Strings.AlertFileDescription), Order = 4020)]
	public string VolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Delta alert

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.Enabled),
        GroupName = nameof(Resources.DeltaAlert),
        Description = nameof(Resources.UseAlertDescription),
        Order = 5000)]
    public bool UseDeltaAlert { get; set; }

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.Filter),
        GroupName = nameof(Resources.DeltaAlert),
        Description = nameof(Resources.AlertFilterDescription),
        Order = 5010)]
    public decimal DeltaAlertValue { get; set; }

    [Display(ResourceType = typeof(Resources),
        Name = nameof(Resources.AlertFile),
        GroupName = nameof(Resources.DeltaAlert),
        Description = nameof(Resources.AlertFileDescription),
        Order = 5020)]
    public string DeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Ask Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.AskAlert), Description = nameof(Strings.UseAlertDescription), Order = 600, ResourceType = typeof(Strings))]
    public bool UseAskAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.AskAlert), Description = nameof(Strings.AlertFilterDescription), Order = 610, ResourceType = typeof(Strings))]
    [Range(0, int.MaxValue)]
    public decimal AskAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.AskAlert), Description = nameof(Strings.AlertFileDescription), Order = 620, ResourceType = typeof(Strings))]
    public string AskAlertFile { get; set; } = "alert1";

    #endregion

    #region Bid Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.BidAlert), Description = nameof(Strings.UseAlertDescription), Order = 700, ResourceType = typeof(Strings))]
    public bool UseBidAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.BidAlert), Description = nameof(Strings.AlertFilterDescription), Order = 710, ResourceType = typeof(Strings))]
    [Range(0, int.MaxValue)]
    public decimal BidAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.BidAlert), Description = nameof(Strings.AlertFileDescription), Order = 720, ResourceType = typeof(Strings))]
    public string BidAlertFile { get; set; } = "alert1";

    #endregion

    #region Delta Per Volume Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.DeltaPerVolumeAlert), Description = nameof(Strings.UseAlertDescription), Order = 800, ResourceType = typeof(Strings))]
    public bool UseDeltaPerVolumeAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.DeltaPerVolumeAlert), Description = nameof(Strings.AlertFilterDescription), Order = 810, ResourceType = typeof(Strings))]
    [Range(0, 100)]
    public decimal DeltaPerVolumeAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.DeltaPerVolumeAlert), Description = nameof(Strings.AlertFileDescription), Order = 820, ResourceType = typeof(Strings))]
    public string DeltaPerVolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Session Delta Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.SessionDeltaAlert), Description = nameof(Strings.UseAlertDescription), Order = 900, ResourceType = typeof(Strings))]
    public bool UseSessionDeltaAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.SessionDeltaAlert), Description = nameof(Strings.AlertFilterDescription), Order = 910, ResourceType = typeof(Strings))]
    public decimal SessionDeltaAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.SessionDeltaAlert), Description = nameof(Strings.AlertFileDescription), Order = 920, ResourceType = typeof(Strings))]
    public string SessionDeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Session Delta Per Volume Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.SessionDeltaPerVolumeAlert), Description = nameof(Strings.UseAlertDescription), Order = 1000, ResourceType = typeof(Strings))]
    public bool UseSessionDeltaPerVolumeAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.SessionDeltaPerVolumeAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1010, ResourceType = typeof(Strings))]
    public decimal SessionDeltaPerVolumeAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.SessionDeltaPerVolumeAlert), Description = nameof(Strings.AlertFileDescription), Order = 1020, ResourceType = typeof(Strings))]
    public string SessionDeltaPerVolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Max Delta Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.MaxDeltaAlert), Description = nameof(Strings.UseAlertDescription), Order = 1100, ResourceType = typeof(Strings))]
    public bool UseMaxDeltaAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.MaxDeltaAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1110, ResourceType = typeof(Strings))]
    public decimal MaxDeltaAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.MaxDeltaAlert), Description = nameof(Strings.AlertFileDescription), Order = 1120, ResourceType = typeof(Strings))]
    public string MaxDeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Min Delta Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.MinDeltaAlert), Description = nameof(Strings.UseAlertDescription), Order = 1200, ResourceType = typeof(Strings))]
    public bool UseMinDeltaAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.MinDeltaAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1210, ResourceType = typeof(Strings))]
    public decimal MinDeltaAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.MinDeltaAlert), Description = nameof(Strings.AlertFileDescription), Order = 1220, ResourceType = typeof(Strings))]
    public string MinDeltaAlertFile { get; set; } = "alert1";

    #endregion

    #region Delta Change Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.DeltaChangeAlert), Description = nameof(Strings.UseAlertDescription), Order = 1300, ResourceType = typeof(Strings))]
    public bool UseDeltaChangeAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.DeltaChangeAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1310, ResourceType = typeof(Strings))]
    public decimal DeltaChangeAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.DeltaChangeAlert), Description = nameof(Strings.AlertFileDescription), Order = 1320, ResourceType = typeof(Strings))]
    public string DeltaChangeAlertFile { get; set; } = "alert1";

    #endregion

    #region Volume Per Second Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.VolumePerSecondAlert), Description = nameof(Strings.UseAlertDescription), Order = 1400, ResourceType = typeof(Strings))]
    public bool UseVolumePerSecondAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.VolumePerSecondAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1410, ResourceType = typeof(Strings))]
    [Range(0, int.MaxValue)]
    public decimal VolumePerSecondAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.VolumePerSecondAlert), Description = nameof(Strings.AlertFileDescription), Order = 1420, ResourceType = typeof(Strings))]
    public string VolumePerSecondAlertFile { get; set; } = "alert1";

    #endregion

    #region Session Volume Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.SessionVolumeAlert), Description = nameof(Strings.UseAlertDescription), Order = 1500, ResourceType = typeof(Strings))]
    public bool UseSessionVolumeAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.SessionVolumeAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1510, ResourceType = typeof(Strings))]
    [Range(0, int.MaxValue)]
    public decimal SessionVolumeAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.SessionVolumeAlert), Description = nameof(Strings.AlertFileDescription), Order = 1520, ResourceType = typeof(Strings))]
    public string SessionVolumeAlertFile { get; set; } = "alert1";

    #endregion

    #region Trades Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.TradesAlert), Description = nameof(Strings.UseAlertDescription), Order = 1600, ResourceType = typeof(Strings))]
    public bool UseTradesAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.TradesAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1610, ResourceType = typeof(Strings))]
    [Range(0, int.MaxValue)]
    public decimal TradesAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.TradesAlert), Description = nameof(Strings.AlertFileDescription), Order = 1620, ResourceType = typeof(Strings))]
    public string TradesAlertFile { get; set; } = "alert1";

    #endregion

    #region Height Alert

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Enabled), GroupName = nameof(Strings.HeightAlert), Description = nameof(Strings.UseAlertDescription), Order = 1700, ResourceType = typeof(Strings))]
    public bool UseHeightAlert { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.Filter), GroupName = nameof(Strings.HeightAlert), Description = nameof(Strings.AlertFilterDescription), Order = 1710, ResourceType = typeof(Strings))]
    [Range(0, int.MaxValue)]
    public decimal HeightAlertValue { get; set; }

    [Tab(TabName = nameof(Strings.Alerts), TabOrder = 2, ResourceType = typeof(Strings))]
    [Display(Name = nameof(Strings.AlertFile), GroupName = nameof(Strings.HeightAlert), Description = nameof(Strings.AlertFileDescription), Order = 1720, ResourceType = typeof(Strings))]
    public string HeightAlertFile { get; set; } = "alert1";
    #endregion

    #region Net Imbalance alert

    private bool _useNetImbalanceAlert;

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.Enabled), GroupName = "Net Imbalance Alert", Order = 3000)]
    public bool UseNetImbalanceAlert
	{
		get => _useNetImbalanceAlert;
		set
		{
			if (_useNetImbalanceAlert == value)
				return;

			_useNetImbalanceAlert = value;

			if (value)
				MarkImbalancesDirty();
		}
	}


	[Display(Name = "Alert Threshold (abs)", GroupName = "Net Imbalance Alert", Order = 3010)]
    [Range(1, 100000)]
	public int NetImbalanceAlertValue { get; set; } = 6;

	[Display(Name = "Use Closed Candle", GroupName = "Net Imbalance Alert", Order = 3020)]
    public bool UseClosedCandleForNetImbalanceAlert { get; set; }

	[Display(ResourceType = typeof(Strings), Name = nameof(Strings.AlertFile), GroupName = "Net Imbalance Alert", Order = 3030)]
	public string NetImbalanceAlertFile { get; set; } = "alert1";

	#endregion


	#endregion

	#region ctor

	public ClusterStatistic()
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
			return base.ProcessMouseDown(e);

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

	protected override void OnApplyDefaultColors()
	{
		HeaderBackground = DefaultColors.Gray.Convert();
		TextColor = CrossColors.White;

		if (ChartInfo is null)
			return;

		static Color WithoutAlpha(Color c) => CrossColorExtensions.FromRgb(c.R, c.G, c.B);

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

		// Fallback rebuild (e.g., init phase where immediate rebuild failed).
		if (_imbalancesDirty && bar == CurrentBar - 1)
		{
			if (ShouldComputeImbalances())
			{
			RebuildHistoricalImbalances();
			_imbalancesDirty = false;
				RedrawChart();
		}
			else
			{
				// Nothing uses imbalances right now.
				_imbalancesDirty = false;
			}
		}

		var candleSeconds = Convert.ToDecimal((candle.LastTime - candle.Time).TotalSeconds);

		if (candleSeconds is 0)
			candleSeconds = 1;

		_volPerSecond[bar] = candle.Volume / candleSeconds;
		_deltaPerSecond[bar] = candle.Delta / candleSeconds;

		_maxDeltaSec = Math.Max(Math.Abs(_deltaPerSecond[bar]), _maxDeltaSec);

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

            // Reset live net-imbalance crossing state on new bar
            _hasPrevNetImbalanceLive = false;
            _prevNetImbalanceLive = 0;
            _lastNetImbalanceAlertBar = -1;
        }

		if (bar == CurrentBar - 1)
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
                    AddAlert(DeltaAlertFile, string.Format(Resources.AlertDeltaTemplate, candle.Delta));
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
                    AddAlert(VolumeAlertFile, string.Format(Resources.AlertVolumeTemplate, candle.Volume));
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

        // --- SoT realtime peak computation (live bar only) ---
        if (bar == CurrentBar - 1)
        {
            if (_rtBar != bar)
            {
                _rtBar = bar;
                _prevCumVol = 0m;
                _prevCumDelta = 0m;
                _rtPeakVolPerSec = 0m;
                _rtPeakDeltaPerSec = 0m;
                _hasSoTSampleThisBar = false;
                _seededLiveSoT = false;
            }

			if (!_seededLiveSoT && _allCumulativeTrades != null && _allCumulativeTrades.Count > 0)
				SeedLiveWindowFromHistory();

			var now = candle.LastTime;
			var incVol = candle.Volume - _prevCumVol;
			var incDelta = candle.Delta - _prevCumDelta;

			if (incVol > 0)
			{
				_win.Enqueue(new Sample
				{
					T = now,
					Vol = incVol,
					Delta = incDelta
				});

				_winVol += incVol;
				_winDelta += incDelta;
				_hasSoTSampleThisBar = true;

				var cutoff = now.AddSeconds(-SotTimeWindowSec);
				while (_win.Count > 0 && _win.Peek().T <= cutoff)
				{
					var old = _win.Dequeue();
					_winVol -= old.Vol;
					_winDelta -= old.Delta;
				}

				if (_winVol >= SotMinVolume)
				{
					var vps = _winVol / SotTimeWindowSec;
					var dps = _winDelta / SotTimeWindowSec;

					if (vps > _rtPeakVolPerSec)
					{
						_rtPeakVolPerSec = vps;
						_rtPeakDeltaPerSec = dps;
					}
				}
			}

			_prevCumVol = candle.Volume;
			_prevCumDelta = candle.Delta;

			_peakVolPerSec[bar] = Math.Round(_rtPeakVolPerSec, 0);
			_peakDeltaPerSec[bar] = Math.Round(_rtPeakDeltaPerSec, 0);
			_peakDeltaPerVol[bar] = _peakVolPerSec[bar] == 0m
				? 0m
				: (_peakDeltaPerSec[bar] / _peakVolPerSec[bar]);
		}

		// --- Basic footprint imbalances (Buy/Sell/Net) ---
		// Buy imbalance: Ask(upper) vs Bid(lower)
		// Sell imbalance: Bid(lower) vs Ask(upper)
		// --- Basic footprint imbalances (Buy/Sell/Net) + stacked ---
		int buy = 0, sell = 0;
		int stackedBuy = 0, stackedSell = 0;

		if (!ShouldComputeImbalances())
		{
			ClearImbalanceSeries(bar);
		}
		else
		{
			ComputeImbalances(candle, out buy, out sell, out stackedBuy, out stackedSell);
			WriteImbalanceSeries(bar, buy, sell, stackedBuy, stackedSell);
		}


		// --- Optional Net Imbalance alert (threshold crossing, no spam) ---
		if (UseNetImbalanceAlert && bar == CurrentBar - 1)
		{
			var thr = Math.Max(1, NetImbalanceAlertValue);

			if (UseClosedCandleForNetImbalanceAlert)
			{
				// Evaluate the last CLOSED bar once (on the first tick of the new live bar)
				var alertBar = bar - 1;
				if (alertBar >= 0 && _lastNetImbalanceAlertBar != alertBar)
				{
					var cur = (int)_netImbalance[alertBar];
					var prev = alertBar > 0 ? (int)_netImbalance[alertBar - 1] : 0;

					var prevOutside = Math.Abs(prev) >= thr;
					var curOutside = Math.Abs(cur) >= thr;

					// Fire only on crossing: inside -> outside
					if (curOutside && !prevOutside)
                        AddAlert(NetImbalanceAlertFile, string.Format(Resources.AlertNetImbalanceTemplate, cur, thr));

					_lastNetImbalanceAlertBar = alertBar;
				}
			}
			else
			{
				// Live bar: crossing detection within the bar; fire at most once per bar
				var cur = (int)_netImbalance[bar];
				var prev = _hasPrevNetImbalanceLive ? _prevNetImbalanceLive : 0;

				var prevInside = Math.Abs(prev) < thr;
				var curOutside = Math.Abs(cur) >= thr;

				if (curOutside && prevInside && _lastNetImbalanceAlertBar != bar)
				{
                    AddAlert(NetImbalanceAlertFile, string.Format(Resources.AlertNetImbalanceTemplate, cur, thr));
                    _lastNetImbalanceAlertBar = bar;
				}

				_prevNetImbalanceLive = cur;
				_hasPrevNetImbalanceLive = true;
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

            // Also consider pressed row header (during drag)
            if (_pressedString is not DataType.None)
            {
                var size = context.MeasureString(GetHeader(_pressedString), Font.RenderObject);

                if (size.Width > maxWidth)
                {
                    maxWidth = size.Width;
                    _fontHeight = size.Height; // keep height consistent with current font
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

							var headerTextColor = GetContrastTextColor(_headerBackground);
							context.DrawString(text, Font.RenderObject, headerTextColor, textRect, _stringLeftFormat);
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


	protected override void OnFinishRecalculate()
	{
		ResetAutoFilter();

		// Reset RT sliding-window state after a full rebuild
		ResetSoTRuntimeState();
		_seededLiveSoT = false;
		_hasSoTSampleThisBar = false;

		if (CurrentBar <= 0)
			return;

		// Request historical cumulative trades for the loaded range (closed bars)
		var startTime = GetCandle(0).Time;
		var endTime = GetCandle(Math.Max(CurrentBar - 2, 0)).LastTime;

		var request = new CumulativeTradesRequest(startTime, endTime, 0, 0);
		RequestForCumulativeTrades(request);
	}

	protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
	{
		_allCumulativeTrades = cumulativeTrades?.ToList() ?? new List<CumulativeTrade>();
		RebuildHistoricalSoT();
		SeedLiveWindowFromHistory();
		RedrawChart();
	}


	#endregion

	#region Private methods

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

		// High-rate outline for better pop
		if (rate >= 90m && ShouldOutline(type))
		{
			var outlineColor = GetContrastTextColor(bgBrush);
			var pen = outlineColor == System.Drawing.Color.White ? _outlinePenLight : _outlinePenDark;
			context.DrawRectangle(pen, rect);
		}

		if (showValues)
		{
			var text = GetValueText(type, candle, bar);

			var textRect = rect with
			{
				X = rect.X + _headerOffset
			};

			var valueTextColor = GetContrastTextColor(bgBrush);
			context.DrawString(text, Font.RenderObject, valueTextColor, textRect, _stringLeftFormat);
		}

	}

	private System.Drawing.Color GetBrush(DataType type, IndicatorCandle candle, int bar, decimal rate)
	{
		return type switch
		{
			DataType.Ask or DataType.Bid or DataType.Delta or DataType.DeltaVolume or DataType.DeltaSecond =>
				Blend(candle.Delta > 0 ? AskColor : BidColor, BackGroundColor, rate),

			DataType.Volume or DataType.VolumeSecond or DataType.SessionVolume or
				DataType.Trades or DataType.Height or DataType.Time or DataType.Duration => Blend(VolumeColor, BackGroundColor, rate),
			DataType.PeakVolPerSec => Blend(VolumeColor, BackGroundColor, rate),
			DataType.PeakDeltaPerSec or DataType.PeakDeltaPerVol => Blend(_peakDeltaPerSec[bar] >= 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.MaxDelta => Blend(candle.MaxDelta > 0 ?  AskColor : BidColor, BackGroundColor, rate),
			DataType.MinDelta => Blend(candle.MinDelta > 0 ?  AskColor : BidColor, BackGroundColor, rate),
			DataType.SessionDeltaVolume => Blend(_cDeltaPerVol[bar] > 0 ? AskColor : BidColor, BackGroundColor, rate),
			DataType.SessionDelta => Blend(_cDelta[bar] > 0 ? AskColor : BidColor, BackGroundColor, rate),
            DataType.DeltaChange => GetDeltaChangeBrush(bar, rate),
            DataType.BuyImbalance => Blend(AskColor, BackGroundColor, rate),
            DataType.SellImbalance => Blend(BidColor, BackGroundColor, rate),
            DataType.NetImbalance => Blend(_netImbalance[bar] >= 0 ? AskColor : BidColor, BackGroundColor, rate),
            DataType.StackedBuyImbalance => Blend(AskColor, BackGroundColor, rate),
            DataType.StackedSellImbalance => Blend(BidColor, BackGroundColor, rate),
            DataType.StackedNetImbalance => Blend(_stackedNetImbalance[bar] >= 0 ? AskColor : BidColor, BackGroundColor, rate),
            DataType.None => System.Drawing.Color.Transparent,
			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private decimal GetRate(MaxValues maxValues, DataType type, IndicatorCandle candle, int bar)
	{

		if (type == DataType.PeakVolPerSec)
		{
			var v = Math.Abs(_peakVolPerSec[bar]);
			if (SotUseAutoFilter && _afCount > 0)
				return GetRateByMean(v, _peakVolAuto[bar]); // v vs threshold
			return GetRate(v, maxValues.MaxPeakVolPerSec); // fallback: visible max scaling
		}

		if (type == DataType.PeakDeltaPerSec)
		{
			var v = Math.Abs(_peakDeltaPerSec[bar]);
			if (SotUseAutoFilter && _afCount > 0)
				return GetRateByMean(v, _peakDeltaAuto[bar]);
			return GetRate(v, maxValues.MaxPeakDeltaPerSec);
		}

		if (type == DataType.PeakDeltaPerVol)
		{
			var v = Math.Abs(_peakDeltaPerVol[bar]);

			if (SotUseAutoFilter && _afCount > 0)
			{
				var meanV = _peakVolAuto[bar];
				var meanD = _peakDeltaAuto[bar];
				var meanRatio = meanV == 0m ? 0m : (meanD / meanV);
				return GetRateByMean(v, meanRatio);
			}

			return GetRate(v, maxValues.MaxPeakDeltaPerVol);
		}

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
			DataType.DeltaSecond => GetRate(Math.Abs(_deltaPerSecond[bar]), maxValues.MaxDeltaSec),
			DataType.VolumeSecond => GetRate(_volPerSecond[bar], maxValues.MaxVolumeSec),
			DataType.SessionVolume => GetRate(_cVolume[bar], maxValues.CumVolume),
			DataType.Trades => GetRate(candle.Ticks, maxValues.MaxTicks),
			DataType.Height => GetRate(_candleHeights[bar], maxValues.MaxHeight),
			DataType.Time => GetRate(_cVolume[bar], maxValues.CumVolume),
			DataType.Duration => GetRate(_candleDurations[bar], maxValues.MaxDuration),
			DataType.BuyImbalance => GetRate(_buyImbalance[bar], maxValues.MaxBuyImb),
			DataType.SellImbalance => GetRate(_sellImbalance[bar], maxValues.MaxSellImb),
			DataType.NetImbalance => GetRate(Math.Abs(_netImbalance[bar]), maxValues.MaxNetImb),
			DataType.StackedBuyImbalance => GetRate(_stackedBuyImbalance[bar], maxValues.MaxStackedBuyImb),
			DataType.StackedSellImbalance => GetRate(_stackedSellImbalance[bar], maxValues.MaxStackedSellImb),
			DataType.StackedNetImbalance => GetRate(Math.Abs(_stackedNetImbalance[bar]), maxValues.MaxStackedNetImb),
			DataType.None => 0,

			_ => throw new ArgumentOutOfRangeException()
		};
	}

	private MaxValues CreateMaxValues()
	{
		decimal maxVolumeSec;

		var maxAsk = 0m;
		var maxBid = 0m;
		var maxSessionDelta = 0m;
		var maxDeltaPerVolume = 0m;
		var maxSessionDeltaPerVolume = 0m;
		var maxDelta = 0m;
		var minDelta = decimal.MaxValue;
		var maxMaxDelta = 0m;
		var maxMinDelta = 0m;
		var maxVolume = 0m;
		var maxTicks = 0m;
		var maxDuration = 0m;
		var cumVolume = 0m;
		var maxDeltaChange = 0m;
		var maxHeight = 0m;

		var maxDeltaSec = 0m;
		var maxPeakVolPerSec = 0m;
		var maxPeakDeltaPerSec = 0m;
		var maxPeakDeltaPerVol = 0m;
		var maxBuy = 0;
		var maxSell = 0;
		var maxNet = 0;
		int maxStackedBuy = 0, maxStackedSell = 0, maxStackedNet = 0;

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
				maxSessionDeltaPerVolume = Math.Max(Math.Abs(_cDeltaPerVol[i]), maxSessionDeltaPerVolume);

				maxDeltaSec = Math.Max(Math.Abs(_deltaPerSecond[i]), maxDeltaSec);

				maxPeakVolPerSec = Math.Max(maxPeakVolPerSec, Math.Abs(_peakVolPerSec[i]));
				maxPeakDeltaPerSec = Math.Max(maxPeakDeltaPerSec, Math.Abs(_peakDeltaPerSec[i]));
				maxPeakDeltaPerVol = Math.Max(maxPeakDeltaPerVol, Math.Abs(_peakDeltaPerVol[i]));

				maxBuy = Math.Max(maxBuy, (int)_buyImbalance[i]);
				maxSell = Math.Max(maxSell, (int)_sellImbalance[i]);
				maxNet = Math.Max(maxNet, (int)Math.Abs(_netImbalance[i]));

				maxStackedBuy = Math.Max(maxStackedBuy, (int)_stackedBuyImbalance[i]);
				maxStackedSell = Math.Max(maxStackedSell, (int)_stackedSellImbalance[i]);
				maxStackedNet = Math.Max(maxStackedNet, (int)Math.Abs(_stackedNetImbalance[i]));

				if (candle.Volume != 0)
					maxDeltaPerVolume = Math.Max(Math.Abs(100m * candle.Delta / candle.Volume), maxDeltaPerVolume);

				cumVolume += candle.Volume;

				if (i == 0)
					continue;

				maxDeltaChange = Math.Max(Math.Abs(_deltaChange[i]), maxDeltaChange);
				maxHeight = Math.Max(candle.High - candle.Low, maxHeight);
				maxTicks = Math.Max(candle.Ticks, maxTicks);
				maxDuration = Math.Max(_candleDurations[i], maxDuration);
			}

			maxVolumeSec = _volPerSecond.MAX(LastVisibleBarNumber, FirstVisibleBarNumber);
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
			maxDeltaSec = _maxDeltaSec;
			maxHeight = _maxHeight;
			var last = CurrentBar - 1;
			maxVolumeSec = _volPerSecond.MAX(last, 0);
			maxPeakVolPerSec = _peakVolPerSec.MAX(last, 0);
			maxPeakDeltaPerSec = _peakDeltaPerSec.MAX(last, 0);
			maxPeakDeltaPerVol = _peakDeltaPerVol.MAX(last, 0);

			maxBuy = (int)_buyImbalance.MAX(last, 0);
			maxSell = (int)_sellImbalance.MAX(last, 0);
			maxNet = (int)_netImbalance.MAX(last, 0);
			maxStackedBuy = (int)_stackedBuyImbalance.MAX(last, 0);
			maxStackedSell = (int)_stackedSellImbalance.MAX(last, 0);
			maxStackedNet = (int)Math.Abs(_stackedNetImbalance.MAX(last, 0));

		}

		if (maxBuy == 0) maxBuy = 1;
		if (maxSell == 0) maxSell = 1;
		if (maxNet == 0) maxNet = 1;
		if (maxStackedBuy == 0) maxStackedBuy = 1;
		if (maxStackedSell == 0) maxStackedSell = 1;
		if (maxStackedNet == 0) maxStackedNet = 1;

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
			MaxVolumeSec = maxVolumeSec,
			MaxDeltaSec = maxDeltaSec,
			MaxPeakVolPerSec = maxPeakVolPerSec,
			MaxPeakDeltaPerSec = maxPeakDeltaPerSec,
			MaxPeakDeltaPerVol = maxPeakDeltaPerVol,
			MaxBuyImb = maxBuy,
			MaxSellImb = maxSell,
			MaxNetImb = maxNet,
			MaxStackedBuyImb = maxStackedBuy,
			MaxStackedSellImb = maxStackedSell,
			MaxStackedNetImb = maxStackedNet,
		};
	}

	private string GetValueText(DataType type, IndicatorCandle candle, int bar)
	{
		return type switch
		{
			DataType.Ask => ChartInfo.TryGetMinimizedVolumeString(candle.Ask),
			DataType.Bid => ChartInfo.TryGetMinimizedVolumeString(candle.Bid),
			DataType.Delta => ChartInfo.TryGetMinimizedVolumeString(candle.Delta),
			DataType.DeltaVolume => FormatRatio(_deltaPerVol[bar] / 100m, _ratiosAsPercent),
			DataType.SessionDelta => ChartInfo.TryGetMinimizedVolumeString(_cDelta[bar]),
			DataType.SessionDeltaVolume => FormatRatio(_cDeltaPerVol[bar] / 100m, _ratiosAsPercent),
			DataType.MaxDelta => ChartInfo.TryGetMinimizedVolumeString(candle.MaxDelta),
			DataType.MinDelta => ChartInfo.TryGetMinimizedVolumeString(candle.MinDelta),
			DataType.DeltaChange => ChartInfo.TryGetMinimizedVolumeString(_deltaChange[bar]),
			DataType.Volume => ChartInfo.TryGetMinimizedVolumeString(candle.Volume),
			DataType.VolumeSecond => ChartInfo.TryGetMinimizedVolumeString(_volPerSecond[bar]),
			DataType.SessionVolume => ChartInfo.TryGetMinimizedVolumeString(_cVolume[bar]),
			DataType.Trades => candle.Ticks.ToString(CultureInfo.InvariantCulture),
			DataType.Height => _candleHeights[bar].ToString(CultureInfo.InvariantCulture),
			DataType.Time => candle.Time.AddHours(InstrumentInfo.TimeZone).ToString("HH:mm:ss"),
			DataType.Duration => ((int)(candle.LastTime - candle.Time).TotalSeconds).ToString(),
			DataType.DeltaSecond => ChartInfo.TryGetMinimizedVolumeString(_deltaPerSecond[bar]),
			DataType.PeakVolPerSec => ChartInfo.TryGetMinimizedVolumeString(_peakVolPerSec[bar]),
			DataType.PeakDeltaPerSec => ChartInfo.TryGetMinimizedVolumeString(_peakDeltaPerSec[bar]),
			DataType.PeakDeltaPerVol => FormatRatio(_peakDeltaPerVol[bar], _ratiosAsPercent),
			DataType.BuyImbalance => _buyImbalance[bar].ToString(CultureInfo.InvariantCulture),
			DataType.SellImbalance => _sellImbalance[bar].ToString(CultureInfo.InvariantCulture),
			DataType.NetImbalance => _netImbalance[bar].ToString("+#;-#;0", CultureInfo.InvariantCulture),
			DataType.StackedBuyImbalance => _stackedBuyImbalance[bar].ToString(CultureInfo.InvariantCulture),
			DataType.StackedSellImbalance => _stackedSellImbalance[bar].ToString(CultureInfo.InvariantCulture),
			DataType.StackedNetImbalance => _stackedNetImbalance[bar].ToString("+#;-#;0", CultureInfo.InvariantCulture),
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
            DataType.Ask => Resources.Ask,
            DataType.Bid => Resources.Bid,
            DataType.Delta => Resources.Delta,

            DataType.DeltaVolume => _ratiosAsPercent
                ? Resources.DeltaVolumePercent
                : Resources.DeltaVolume,

            DataType.SessionDelta => Resources.SessionDelta,

            DataType.SessionDeltaVolume => _ratiosAsPercent
                ? Resources.SessionDeltaVolumePercent
                : Resources.SessionDeltaVolume,

            DataType.MaxDelta => Resources.MaxDelta,
            DataType.MinDelta => Resources.MinDelta,
            DataType.DeltaChange => Resources.DeltaChange,

            DataType.Volume => Resources.Volume,

            DataType.VolumeSecond => Resources.VolumeSecond,

            DataType.SessionVolume => Resources.SessionVolume,

            DataType.Trades => Resources.Trades,
            DataType.Height => Resources.Height,
            DataType.Time => Resources.Time,

            DataType.Duration => Resources.Duration,

            DataType.DeltaSecond => Resources.DeltaSecond,

            DataType.PeakVolPerSec => Resources.PeakVolPerSec,
            DataType.PeakDeltaPerSec => Resources.PeakDeltaPerSec,

            DataType.PeakDeltaPerVol => _ratiosAsPercent
                ? Resources.PeakDeltaPerVolPercent
                : Resources.PeakDeltaPerVol,

            DataType.BuyImbalance => Resources.BuyImbShort,
            DataType.SellImbalance => Resources.SellImbShort,
            DataType.NetImbalance => Resources.NetImbShort,

            DataType.StackedBuyImbalance => Resources.BuyStkShort,
            DataType.StackedSellImbalance => Resources.SellStkShort,
            DataType.StackedNetImbalance => Resources.NetStkShort,

            DataType.None => string.Empty,

            _ => string.Empty
        };
    }


    private static string FormatRatio(decimal ratio, bool asPercent)
	{
		// ratio is expected to be a true ratio (e.g., delta/volume), not already multiplied by 100.
		if (asPercent)
			return (ratio * 100m).ToString("+#0.00;-#0.00;0.00", CultureInfo.InvariantCulture) + "%";

		return ratio.ToString("+#0.0000;-#0.0000;0.0000", CultureInfo.InvariantCulture);
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
	private void ResetSoTRuntimeState()
	{
		_win.Clear();
		_winVol = 0m;
		_winDelta = 0m;

		_rtBar = -1;
		_prevCumVol = 0m;
		_prevCumDelta = 0m;

		_rtPeakVolPerSec = 0m;
		_rtPeakDeltaPerSec = 0m;
	}

	private void OnSoTParamsChanged()
	{
		ResetSoTRuntimeState();
		_seededLiveSoT = false;
		_hasSoTSampleThisBar = false;

		if (CurrentBar > 0)
		{
			for (int i = 0; i <= CurrentBar - 1; i++)
			{
				_peakVolPerSec[i] = 0m;
				_peakDeltaPerSec[i] = 0m;
			}
		}

		if (_allCumulativeTrades != null && _allCumulativeTrades.Count > 0)
		{
			RebuildHistoricalSoT();
			SeedLiveWindowFromHistory();
			RedrawChart();
			return;
		}

		if (CurrentBar > 0)
		{
			try
			{
				var startTime = GetCandle(0).Time;
				var endTime = GetCandle(CurrentBar - 1).LastTime;
				var req = new CumulativeTradesRequest(startTime, endTime, 0, 0);
				RequestForCumulativeTrades(req);
			}
			catch
			{
				// Candles not ready yet; next full recalc will request history.
			}
		}
	
		RedrawChart();
	}

	private void RebuildHistoricalSoT()
	{
		if (_allCumulativeTrades == null || _allCumulativeTrades.Count == 0 || CurrentBar <= 0)
			return;

		var lastClosedBar = CurrentBar - 2;
		if (lastClosedBar < 0)
			return;

		for (var i = 0; i <= lastClosedBar; i++)
		{
			_peakVolPerSec[i] = 0m;
			_peakDeltaPerSec[i] = 0m;
		}

		var queue = new Queue<CumulativeTrade>();
		var winVol = 0m;
		var winDelta = 0m;

		var tradeIndex = 0;

		for (var bar = 0; bar <= lastClosedBar; bar++)
		{
			var candle = GetCandle(bar);
			var barStart = candle.Time;
			var barEnd = candle.LastTime;

			var peakVolPerSec = 0m;
			var peakDeltaPerSec = 0m;

			while (tradeIndex < _allCumulativeTrades.Count &&
				   _allCumulativeTrades[tradeIndex].Time < barStart)
				tradeIndex++;

			var j = tradeIndex;

			while (j < _allCumulativeTrades.Count &&
				   _allCumulativeTrades[j].Time <= barEnd)
			{
				var trade = _allCumulativeTrades[j++];
				queue.Enqueue(trade);

				winVol += trade.Volume;
				winDelta += trade.Direction == TradeDirection.Buy
					? trade.Volume
					: -trade.Volume;

				var cutoff = trade.Time.AddSeconds(-SotTimeWindowSec);
				while (queue.Count > 0 && queue.Peek().Time <= cutoff)
				{
					var old = queue.Dequeue();
					winVol -= old.Volume;
					winDelta -= old.Direction == TradeDirection.Buy
						? old.Volume
						: -old.Volume;
				}

				if (winVol >= SotMinVolume)
				{
					var vps = winVol / SotTimeWindowSec;
					var dps = winDelta / SotTimeWindowSec;

					if (vps > peakVolPerSec)
					{
						peakVolPerSec = vps;
						peakDeltaPerSec = dps;
					}
				}
			}

			_peakVolPerSec[bar] = Math.Round(peakVolPerSec, 0);
			_peakDeltaPerSec[bar] = Math.Round(peakDeltaPerSec, 0);

			_peakDeltaPerVol[bar] = _peakVolPerSec[bar] == 0m
				? 0m
				: (_peakDeltaPerSec[bar] / _peakVolPerSec[bar]);

			UpdateAutoFilterWithClosedBar(bar);
		}
	}

	private void SeedLiveWindowFromHistory()
	{
		if (CurrentBar <= 0 || _allCumulativeTrades == null || _allCumulativeTrades.Count == 0)
			return;

		var liveBar = CurrentBar - 1;
		if (liveBar < 0)
			return;

		var candle = GetCandle(liveBar);
		var now = candle.LastTime;
		var cutoff = now.AddSeconds(-SotTimeWindowSec);

		_win.Clear();
		_winVol = 0m;
		_winDelta = 0m;
		_rtPeakVolPerSec = 0m;
		_rtPeakDeltaPerSec = 0m;

		var chunk = new List<CumulativeTrade>();

		for (var i = _allCumulativeTrades.Count - 1; i >= 0; i--)
		{
			var trade = _allCumulativeTrades[i];
			if (trade.Time <= cutoff)
				break;

			if (trade.Time <= now)
				chunk.Add(trade);
		}

		chunk.Reverse();

		foreach (var trade in chunk)
		{
			var delta = trade.Direction == TradeDirection.Buy
				? trade.Volume
				: -trade.Volume;

			_win.Enqueue(new Sample
			{
				T = trade.Time,
				Vol = trade.Volume,
				Delta = delta
			});

			_winVol += trade.Volume;
			_winDelta += delta;
		}

		while (_win.Count > 0 && _win.Peek().T <= cutoff)
		{
			var old = _win.Dequeue();
			_winVol -= old.Vol;
			_winDelta -= old.Delta;
		}

		_seededLiveSoT = true;
		_hasSoTSampleThisBar = _win.Count > 0;

		// Align accumulators so the next OnCalculate starts from zero increment
		_rtBar = liveBar;
		_prevCumVol = candle.Volume;
		_prevCumDelta = candle.Delta;
	}

    private void MarkImbalancesDirty()
    {
        _imbalancesDirty = true;

        // If we already have bars loaded, rebuild immediately so UI updates without waiting for a tick.
        if (CurrentBar <= 0)
            return;

		if (!ShouldComputeImbalances())
            return;

        try
        {
            RebuildHistoricalImbalances();
            _imbalancesDirty = false;
            RedrawChart();
        }
        catch
        {
            // Candles may not be ready yet (e.g., during init). Keep dirty and rebuild on next OnCalculate tick.
        }
    }


    private void ComputeImbalances(IndicatorCandle candle, out int buy, out int sell, out int stackedBuy, out int stackedSell)
	{
		buy = 0;
		sell = 0;
		stackedBuy = 0;
		stackedSell = 0;

		int buyStreak = 0;
		int sellStreak = 0;

		decimal ratio = ImbalanceThreshold / 100m;
		int minDom = ImbalanceMinDominantVolume;
		int minDiff = ImbalanceMinDifference;
		int stackedMin = Math.Max(2, StackedImbalanceMinLevels);

		for (decimal price = candle.High; price >= candle.Low + InstrumentInfo.TickSize; price -= InstrumentInfo.TickSize)
		{
			var upper = candle.GetPriceVolumeInfo(price);
			var lower = candle.GetPriceVolumeInfo(price - InstrumentInfo.TickSize);

			if (upper == null || lower == null)
				continue;

			var upperAsk = upper.Ask;
			var lowerBid = lower.Bid;

			bool isBuy = false;
			bool isSell = false;

			// BUY: upper ask dominates lower bid
			if (lowerBid > 0m)
			{
				if (upperAsk / lowerBid >= ratio &&
					upperAsk >= minDom &&
					upperAsk - lowerBid >= minDiff)
				{
					isBuy = true;
				}
			}

			// SELL: lower bid dominates upper ask (only if not buy at same level)
			if (!isBuy && upperAsk > 0m)
			{
				if (lowerBid / upperAsk >= ratio &&
					lowerBid >= minDom &&
					lowerBid - upperAsk >= minDiff)
				{
					isSell = true;
				}
			}

			if (isBuy)
			{
				buy++;
				buyStreak++;
				sellStreak = 0;

				if (buyStreak >= stackedMin)
					stackedBuy++;
			}
			else if (isSell)
			{
				sell++;
				sellStreak++;
				buyStreak = 0;

				if (sellStreak >= stackedMin)
					stackedSell++;
			}
			else
			{
				buyStreak = 0;
				sellStreak = 0;
			}
		}
	}

	private void RebuildHistoricalImbalances()
	{
		if (CurrentBar <= 0)
			return;

		for (int i = 0; i < CurrentBar; i++)
		{
			var c = GetCandle(i);
			if (c == null)
				continue;

			ComputeImbalances(c, out var buy, out var sell, out var stackedBuy, out var stackedSell);

			_buyImbalance[i] = buy;
			_sellImbalance[i] = sell;
			_netImbalance[i] = buy - sell;

			_stackedBuyImbalance[i] = stackedBuy;
			_stackedSellImbalance[i] = stackedSell;
			_stackedNetImbalance[i] = stackedBuy - stackedSell;
		}
	}

	private bool AreImbalanceRowsEnabled()
	{
		return
			(RowsOrder.TryGetValue(DataType.BuyImbalance, out var buyRi) && buyRi.Enabled) ||
			(RowsOrder.TryGetValue(DataType.SellImbalance, out var sellRi) && sellRi.Enabled) ||
			(RowsOrder.TryGetValue(DataType.NetImbalance, out var netRi) && netRi.Enabled) ||
			(RowsOrder.TryGetValue(DataType.StackedBuyImbalance, out var sbRi) && sbRi.Enabled) ||
			(RowsOrder.TryGetValue(DataType.StackedSellImbalance, out var ssRi) && ssRi.Enabled) ||
			(RowsOrder.TryGetValue(DataType.StackedNetImbalance, out var snRi) && snRi.Enabled);
	}

	private bool ShouldComputeImbalances()
	{
		// Compute if any imbalance row is visible OR net-imbalance alert is enabled.
		return AreImbalanceRowsEnabled() || UseNetImbalanceAlert;
	}

	private void ClearImbalanceSeries(int bar)
	{
		_buyImbalance[bar] = 0;
		_sellImbalance[bar] = 0;
		_netImbalance[bar] = 0;
		_stackedBuyImbalance[bar] = 0;
		_stackedSellImbalance[bar] = 0;
		_stackedNetImbalance[bar] = 0;
	}

	private void WriteImbalanceSeries(int bar, int buy, int sell, int stackedBuy, int stackedSell)
	{
		_buyImbalance[bar] = buy;
		_sellImbalance[bar] = sell;
		_netImbalance[bar] = buy - sell;

		_stackedBuyImbalance[bar] = stackedBuy;
		_stackedSellImbalance[bar] = stackedSell;
		_stackedNetImbalance[bar] = stackedBuy - stackedSell;
	}



	#endregion

	#region AutoFilter helpers
	private void UpdateAutoFilterWithClosedBar(int bar)
	{
		if (!SotUseAutoFilter) return;

		var vAbs = Math.Abs(_peakVolPerSec[bar]);
		var dAbs = Math.Abs(_peakDeltaPerSec[bar]);

		if (SotAutoFilterUseEma)
		{
			var p = Math.Max(1, SotAutoFilterPeriod);
			var alpha = 2m / (p + 1m);
			_afVolEma = _afCount == 0 ? vAbs : (alpha * vAbs + (1m - alpha) * _afVolEma);
			_afDeltaEma = _afCount == 0 ? dAbs : (alpha * dAbs + (1m - alpha) * _afDeltaEma);
			_afVol = _afVolEma;
			_afDelta = _afDeltaEma;
		}
		else
		{
			var p = Math.Max(1, SotAutoFilterPeriod);

			_afVolSma.Enqueue(vAbs);
			_afVolSmaSum += vAbs;
			if (_afVolSma.Count > p)
				_afVolSmaSum -= _afVolSma.Dequeue();

			_afDeltaSma.Enqueue(dAbs);
			_afDeltaSmaSum += dAbs;
			if (_afDeltaSma.Count > p)
				_afDeltaSmaSum -= _afDeltaSma.Dequeue();

			_afVol = _afVolSma.Count > 0 ? _afVolSmaSum / _afVolSma.Count : 0m;
			_afDelta = _afDeltaSma.Count > 0 ? _afDeltaSmaSum / _afDeltaSma.Count : 0m;
		}

		_afCount++;
		_peakVolAuto[bar] = _afVol;
		_peakDeltaAuto[bar] = _afDelta;
	}

	private void ResetAutoFilter()
	{
		_afCount = 0;
		_afVol = _afDelta = _afVolEma = _afDeltaEma = 0m;
		_afVolSma.Clear();
		_afDeltaSma.Clear();
		_afVolSmaSum = 0m;
		_afDeltaSmaSum = 0m;
		if (CurrentBar > 0)
		{
			for (int i = 0; i <= CurrentBar - 1; i++)
			{
				_peakVolAuto[i] = 0m;
				_peakDeltaAuto[i] = 0m;
			}
		}
	}

	// Scale to 10..100 based on how far the value is above/below the mean threshold.
	private decimal GetRateByMean(decimal value, decimal mean)
	{
		if (mean <= 0m) return 10m;
		var r = value / mean;              // >= 0
		var gamma = 1.35m;                 // >1 amplifies higher-than-mean peaks for better visual contrast
		r = (decimal)Math.Pow((double)r, (double)gamma);

		const decimal lo = 0.85m, hi = 1.35m;
		if (r <= lo) return 10m;
		if (r >= hi) return 100m;
		return 10m + (r - lo) * (90m / (hi - lo));
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

				return prevCandle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay < CustomSessionStart.Value
					&& candle.Time.AddHours(InstrumentInfo.TimeZone).TimeOfDay >= CustomSessionStart.Value;
			default:
				return false;
		}
	}

	#endregion
}