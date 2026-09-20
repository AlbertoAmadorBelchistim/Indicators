using ATAS.Indicators.Drawing;
using OFT.Attributes.Editors;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using Utils.Common.Logging;

namespace ATAS.Indicators.Technical
{
	[DisplayName("Delta Patterns")]
	[Category(IndicatorCategories.VolumeOrderFlow)]
	public class DeltaPatterns : Indicator
	{
		#region Nested Types: Pattern Model

		public enum DeltaPattern
		{
			None,
			AggressiveBuy,
			AggressiveSell,
			DominanceBuy,
			DominanceSell,
			DivergenceBullish,
			DivergenceBearish,
			ReversalBuy,
			ReversalSell,
			NeutralStruggle,
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class PatternCategory : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			protected void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
			{
				if (Equals(field, value)) return;
				field = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}

			private bool _enabled = true;
			[DisplayName("Enabled")]
			public bool Enabled
			{
				get => _enabled;
				set => Set(ref _enabled, value);
			}

			private bool _visible = true;
			[DisplayName("Visible")]
			public bool Visible
			{
				get => _visible;
				set => Set(ref _visible, value);
			}

			private decimal _minDeltaPercent;
			[DisplayName("Min Delta %")]
			[PostValueMode(PostValueModes.OnLostFocus)]
			public decimal MinDeltaPercent
			{
				get => _minDeltaPercent;
				set => Set(ref _minDeltaPercent, value);
			}

			private bool _enableAlert;
			[DisplayName("Enable Alert")]
			public bool EnableAlert
			{
				get => _enableAlert;
				set => Set(ref _enableAlert, value);
			}

			public override string ToString()
			{
				if (!Enabled)
					return "Disabled";

				var visibility = Visible ? "Visible" : "Hidden";
				var alert = EnableAlert ? " · Alert" : string.Empty;

				return MinDeltaPercent > 0m
					? $"{visibility} · {MinDeltaPercent:0.#}%{alert}"
					: $"{visibility}{alert}";
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class MonoPatternCategory : PatternCategory
		{
			private CrossColor _color;
			[DisplayName("Color")]
			public CrossColor Color
			{
				get => _color;
				set => Set(ref _color, value);
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class DirectionalPatternCategory : PatternCategory
		{
			private CrossColor _buyColor;
			[DisplayName("Buy Color")]
			public CrossColor BuyColor
			{
				get => _buyColor;
				set => Set(ref _buyColor, value);
			}

			private CrossColor _sellColor;
			[DisplayName("Sell Color")]
			public CrossColor SellColor
			{
				get => _sellColor;
				set => Set(ref _sellColor, value);
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class DominancePatternCategory : DirectionalPatternCategory
		{
			private decimal _wickTolerancePercent;
			[DisplayName("Wick Tolerance %")]
			[PostValueMode(PostValueModes.OnLostFocus)]
			public decimal WickTolerancePercent
			{
				get => _wickTolerancePercent;
				set => Set(ref _wickTolerancePercent, value);
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class ReversalPatternCategory : DirectionalPatternCategory
		{
			private decimal _closePercent;
			[DisplayName("Close Min %")]
			[PostValueMode(PostValueModes.OnLostFocus)]
			public decimal ClosePercent
			{
				get => _closePercent;
				set => Set(ref _closePercent, value);
			}
		}

		[TypeConverter(typeof(ExpandableObjectConverter))]
		public class NeutralPatternCategory : MonoPatternCategory
		{
			private decimal _strugglePercent;
			[DisplayName("Struggle %")]
			[PostValueMode(PostValueModes.OnLostFocus)]
			public decimal StrugglePercent
			{
				get => _strugglePercent;
				set => Set(ref _strugglePercent, value);
			}
		}

		public enum DebugOverlayMode
		{
			Off,
			Compact,
			Full,
		}

		public enum DebugOverlayCorner
		{
			TopRight,
			TopLeft,
			BottomRight,
			BottomLeft,
		}

		#endregion

		#region Nested Types: Engine

		public sealed class RollingDeltaWindow
		{
			private struct TickRecord
			{
				public decimal Price;
				public decimal Volume;
				public int Direction;
				public long Seq;
				public decimal PrefixSum;
			}

			private readonly Queue<TickRecord> _ticks = new Queue<TickRecord>(4096);

			// Monotonic deques: front holds the current extreme.
			//   _maxRunDeque: prefix sums strictly DECREASE from front to back.
			//   _minRunDeque: prefix sums strictly INCREASE from front to back.
			//   _maxPriceDeque: prices strictly DECREASE from front to back.
			//   _minPriceDeque: prices strictly INCREASE from front to back.
			private readonly LinkedList<TickRecord> _maxRunDeque = new LinkedList<TickRecord>();
			private readonly LinkedList<TickRecord> _minRunDeque = new LinkedList<TickRecord>();
			private readonly LinkedList<TickRecord> _maxPriceDeque = new LinkedList<TickRecord>();
			private readonly LinkedList<TickRecord> _minPriceDeque = new LinkedList<TickRecord>();

			private long _nextSeq;
			private decimal _runningPrefixSum;
			private decimal _startPrefixSum;

			public int TickCount => _ticks.Count;

			public decimal CurrentVolume { get; private set; }

			public decimal CurrentDelta => _runningPrefixSum - _startPrefixSum;

			public decimal MaxRunningDelta
			{
				get
				{
					if (_maxRunDeque.Count == 0) return 0m;
					var candidate = _maxRunDeque.First.Value.PrefixSum - _startPrefixSum;
					return candidate > 0m ? candidate : 0m;
				}
			}

			public decimal MinRunningDelta
			{
				get
				{
					if (_minRunDeque.Count == 0) return 0m;
					var candidate = _minRunDeque.First.Value.PrefixSum - _startPrefixSum;
					return candidate < 0m ? candidate : 0m;
				}
			}

			public decimal StartPrice => _ticks.Count > 0 ? _ticks.Peek().Price : 0m;

			public decimal EndPrice { get; private set; }

			public decimal HighPrice => _maxPriceDeque.Count > 0 ? _maxPriceDeque.First.Value.Price : 0m;

			public decimal LowPrice => _minPriceDeque.Count > 0 ? _minPriceDeque.First.Value.Price : 0m;

			public void Push(decimal price, decimal volume, int direction)
			{
				var delta = volume * direction;
				_runningPrefixSum += delta;

				var record = new TickRecord
				{
					Price = price,
					Volume = volume,
					Direction = direction,
					Seq = _nextSeq++,
					PrefixSum = _runningPrefixSum,
				};

				_ticks.Enqueue(record);
				CurrentVolume += volume;
				EndPrice = price;

				while (_maxRunDeque.Count > 0 && _maxRunDeque.Last.Value.PrefixSum <= record.PrefixSum)
					_maxRunDeque.RemoveLast();
				_maxRunDeque.AddLast(record);

				while (_minRunDeque.Count > 0 && _minRunDeque.Last.Value.PrefixSum >= record.PrefixSum)
					_minRunDeque.RemoveLast();
				_minRunDeque.AddLast(record);

				while (_maxPriceDeque.Count > 0 && _maxPriceDeque.Last.Value.Price <= price)
					_maxPriceDeque.RemoveLast();
				_maxPriceDeque.AddLast(record);

				while (_minPriceDeque.Count > 0 && _minPriceDeque.Last.Value.Price >= price)
					_minPriceDeque.RemoveLast();
				_minPriceDeque.AddLast(record);
			}

			public void Trim(decimal targetVolume)
			{
				while (_ticks.Count > 0)
				{
					var oldest = _ticks.Peek();
					if (CurrentVolume - oldest.Volume < targetVolume)
						break;

					_ticks.Dequeue();
					CurrentVolume -= oldest.Volume;
					_startPrefixSum = oldest.PrefixSum;

					if (_maxRunDeque.Count > 0 && _maxRunDeque.First.Value.Seq == oldest.Seq)
						_maxRunDeque.RemoveFirst();
					if (_minRunDeque.Count > 0 && _minRunDeque.First.Value.Seq == oldest.Seq)
						_minRunDeque.RemoveFirst();
					if (_maxPriceDeque.Count > 0 && _maxPriceDeque.First.Value.Seq == oldest.Seq)
						_maxPriceDeque.RemoveFirst();
					if (_minPriceDeque.Count > 0 && _minPriceDeque.First.Value.Seq == oldest.Seq)
						_minPriceDeque.RemoveFirst();
				}
			}

			public void Reset()
			{
				_ticks.Clear();
				_maxRunDeque.Clear();
				_minRunDeque.Clear();
				_maxPriceDeque.Clear();
				_minPriceDeque.Clear();
				CurrentVolume = 0m;
				EndPrice = 0m;
				_nextSeq = 0;
				_runningPrefixSum = 0m;
				_startPrefixSum = 0m;
			}
		}

		public struct BarMetrics
		{
			public decimal Delta;
			public decimal Volume;
			public decimal MaxRunningDelta;
			public decimal MinRunningDelta;
			public decimal OpenPrice;
			public decimal ClosePrice;
			public decimal HighPrice;
			public decimal LowPrice;
		}

		public sealed class BarMetricsCache
		{
			private BarMetrics[] _metrics = Array.Empty<BarMetrics>();
			private bool[] _populated = Array.Empty<bool>();

			public int BarCount { get; private set; }

			public void EnsureCapacity(int requiredCount)
			{
				if (requiredCount <= _metrics.Length) return;
				int newCapacity = Math.Max(requiredCount, Math.Max(_metrics.Length * 2, 1024));
				Array.Resize(ref _metrics, newCapacity);
				Array.Resize(ref _populated, newCapacity);
			}

			public void Set(int bar, BarMetrics value)
			{
				if (bar < 0) return;
				EnsureCapacity(bar + 1);
				_metrics[bar] = value;
				_populated[bar] = true;
				if (bar + 1 > BarCount) BarCount = bar + 1;
			}

			public bool TryGet(int bar, out BarMetrics value)
			{
				if (bar < 0 || bar >= _populated.Length || !_populated[bar])
				{
					value = default;
					return false;
				}
				value = _metrics[bar];
				return true;
			}

			public bool IsPopulated(int bar)
			{
				return bar >= 0 && bar < _populated.Length && _populated[bar];
			}

			public void Clear()
			{
				BarCount = 0;
				if (_populated.Length > 0)
					Array.Clear(_populated, 0, _populated.Length);
			}
		}

		public sealed class BarPatternCache
		{
			private DeltaPattern[] _patterns = Array.Empty<DeltaPattern>();

			public int BarCount { get; private set; }

			public void EnsureCapacity(int requiredCount)
			{
				if (requiredCount <= _patterns.Length) return;
				int newCapacity = Math.Max(requiredCount, Math.Max(_patterns.Length * 2, 1024));
				Array.Resize(ref _patterns, newCapacity);
			}

			public void Set(int bar, DeltaPattern pattern)
			{
				if (bar < 0) return;
				EnsureCapacity(bar + 1);
				_patterns[bar] = pattern;
				if (bar + 1 > BarCount) BarCount = bar + 1;
			}

			public DeltaPattern Get(int bar)
			{
				if (bar < 0 || bar >= BarCount) return DeltaPattern.None;
				return _patterns[bar];
			}

			public void Clear()
			{
				BarCount = 0;
				if (_patterns.Length > 0)
					Array.Clear(_patterns, 0, _patterns.Length);
			}
		}

		#endregion

		#region Fields

		private readonly object _stateLock = new object();
		private readonly RollingDeltaWindow _window = new RollingDeltaWindow();
		private readonly BarMetricsCache _metrics = new BarMetricsCache();
		private readonly BarPatternCache _patterns = new BarPatternCache();
		private bool _historyLoaded;

		// Id of the pending history request; responses to older requests are ignored.
		private int _requestId;

		// Realtime ticks received while the history is pending, replayed after it.
		private readonly List<MarketDataArg> _pendingTicks = new List<MarketDataArg>();

		// Last bar whose snapshot has been taken. Bars are snapshotted in order: a bar is
		// snapshotted after each of its ticks, and a bar without ticks with the window as it was.
		private int _lastSnapshotBar = -1;

		// Time of the last tick of the history response and how many response ticks have that
		// time. Ticks carry no identity: buffered ticks older than that time, and as many with that
		// time as the response has (minus those processed before buffering started), are already
		// in the history.
		private DateTime _historyEndTime;
		private int _boundaryTickCount;

		// Time of the last tick processed and how many had that time, and the same values when the
		// last recalculation started buffering.
		private DateTime _lastTickTime;
		private int _lastTickCount;
		private DateTime _bufferStartTickTime;
		private int _bufferStartTickCount;

		private int _targetVolume = 2500;
		private int _sessionsToCalculate = 5;

		// First bar of the calculated range. Bars before it have no snapshot.
		private int _firstBar;

		// Session-wide running maximum of per-bar raw amplitudes.
		// Drives the global scale anchor so a single outlier in the
		// session keeps the panel's vertical extent stable.
		private decimal _sessionMaxAmp;

		private const decimal ScaleAnchorMargin = 1.15m;
		private const int MarkerPriceGap = 10;

		private readonly CandleDataSeries _cAggressive = new CandleDataSeries("Aggressive") { IsHidden = true, ShowCurrentValue = false};
		private readonly CandleDataSeries _cDominance = new CandleDataSeries("Dominance") { IsHidden = true, ShowCurrentValue = false};
		private readonly CandleDataSeries _cDivergence = new CandleDataSeries("Divergence") { IsHidden = true, ShowCurrentValue = false};
		private readonly CandleDataSeries _cReversal = new CandleDataSeries("Reversal") { IsHidden = true, ShowCurrentValue = false};
		private readonly CandleDataSeries _cNeutral = new CandleDataSeries("Neutral") { IsHidden = true, ShowCurrentValue = false};
		private readonly CandleDataSeries _cNormal = new CandleDataSeries("Normal") { IsHidden = true, ShowCurrentValue = false};

		private readonly ValueDataSeries _scaleHigh = new ValueDataSeries("Scale High")
		{
			VisualType = VisualMode.Hide,
			IsHidden = true,
			ShowCurrentValue = false,
			ShowZeroValue = false,
			ScaleIt = true,
			IgnoredByAlerts = true,
		};

		private readonly ValueDataSeries _scaleLow = new ValueDataSeries("Scale Low")
		{
			VisualType = VisualMode.Hide,
			IsHidden = true,
			ShowCurrentValue = false,
			ShowZeroValue = false,
			ScaleIt = true,
			IgnoredByAlerts = true,
		};

		private static readonly Candle EmptyCandle = new Candle();

		private bool _showChartSignals = true;
		private int _signalSize = 10;

		private string _alertSoundFile = "alert1";
		private decimal _alertCooldownSeconds = 30m;
		private CrossColor _alertBackgroundColor = CrossColor.FromArgb(255, 75, 72, 72);
		private CrossColor _alertForegroundColor = CrossColor.FromArgb(255, 247, 249, 249);

		private DateTime _lastAlertTime = DateTime.MinValue;

		// Pattern of the rolling window after the last processed tick. Alerts fire when it changes,
		// so a pattern that persists into the next bar does not alert again.
		private DeltaPattern _lastObservedPattern = DeltaPattern.None;

		private DebugOverlayMode _debugOverlayMode = DebugOverlayMode.Off;
		private DebugOverlayCorner _debugOverlayCorner = DebugOverlayCorner.TopRight;
		private readonly RenderFont _hudFont = new RenderFont("Consolas", 11);
		private readonly RenderPen _hudPen = new RenderPen(System.Drawing.Color.LightGray, 1);
		private int _debugOverlayOffsetX;
		private int _debugOverlayOffsetY;

        private volatile bool _disposed;


        #endregion

        #region Properties: Patterns

        [Display(Name = "Aggressive", GroupName = "Patterns", Order = 10,
			Description = "Raw absolute delta of the rolling window over the threshold percentage.")]
		public DirectionalPatternCategory Aggressive { get; set; } = new DirectionalPatternCategory
		{
			BuyColor = System.Drawing.Color.Lime.Convert(),
			SellColor = System.Drawing.Color.Red.Convert(),
			MinDeltaPercent = 15m,
		};

		[Display(Name = "Dominance", GroupName = "Patterns", Order = 20,
			Description = "Sustained one-sided pressure with negligible counter-excursion in the rolling window.")]
		public DominancePatternCategory Dominance { get; set; } = new DominancePatternCategory
		{
			BuyColor = System.Drawing.Color.ForestGreen.Convert(),
			SellColor = System.Drawing.Color.DarkRed.Convert(),
			MinDeltaPercent = 12m,
			WickTolerancePercent = 0.1m,
		};

		[Display(Name = "Divergence", GroupName = "Patterns", Order = 30,
			Description = "The rolling window's price direction contradicts the net delta direction.")]
		public MonoPatternCategory Divergence { get; set; } = new MonoPatternCategory
		{
			Color = System.Drawing.Color.Yellow.Convert(),
			MinDeltaPercent = 10m,
		};

		[Display(Name = "Reversal", GroupName = "Patterns", Order = 40,
			Description = "Delta extreme was reached and the window then closed past the threshold in the opposite direction. MinDeltaPercent is the extreme reached; ClosePercent is the close-side confirmation.")]
		public ReversalPatternCategory Reversal { get; set; } = new ReversalPatternCategory
		{
			BuyColor = System.Drawing.Color.Cyan.Convert(),
			SellColor = System.Drawing.Color.Orange.Convert(),
			MinDeltaPercent = 10m,
			ClosePercent = 2m,
		};

		[Display(Name = "Neutral", GroupName = "Patterns", Order = 50,
			Description = "Small net delta combined with high internal struggle range. MinDeltaPercent is the maximum close range; StrugglePercent is the internal volatility floor.")]
		public NeutralPatternCategory Neutral { get; set; } = new NeutralPatternCategory
		{
			Color = System.Drawing.Color.Silver.Convert(),
			MinDeltaPercent = 1m,
			StrugglePercent = 12m,
		};

		[Display(Name = "Normal", GroupName = "Patterns", Order = 60,
			Description = "Background coloring for windows that did not match any other pattern.")]
		public MonoPatternCategory Normal { get; set; } = new MonoPatternCategory
		{
			Color = System.Drawing.Color.DimGray.Convert(),
			Visible = false,
		};

		#endregion

		#region Properties: Calculation

		[Display(Name = "Target Volume", GroupName = "Calculation", Order = 1,
			Description = "Rolling window size in contracts. Each snapshot summarises the last N traded contracts.")]
		[PostValueMode(PostValueModes.OnLostFocus)]
		public int TargetVolume
		{
			get => _targetVolume;
			set
			{
				var clamped = Math.Max(1, value);
				if (_targetVolume == clamped) return;
				_targetVolume = clamped;
				RecalculateValues();
			}
		}

		[Display(Name = "Sessions to Calculate", GroupName = "Calculation", Order = 2,
			Description = "Number of the most recent sessions whose trades are requested and classified. 0 calculates the whole chart, which can take long on charts with many days loaded.")]
		[Range(0, 1000)]
		[PostValueMode(PostValueModes.OnLostFocus)]
		public int SessionsToCalculate
		{
			get => _sessionsToCalculate;
			set
			{
				var clamped = Math.Max(0, value);
				if (_sessionsToCalculate == clamped) return;
				_sessionsToCalculate = clamped;
				RecalculateValues();
			}
		}

		#endregion

		#region Properties: Visuals

		[Display(Name = "Show Chart Signals", GroupName = "Visuals", Order = 1,
			Description = "Toggle the marker overlay on the price chart.")]
		public bool ShowChartSignals
		{
			get => _showChartSignals;
			set
			{
				if (_showChartSignals == value) return;
				_showChartSignals = value;
				RedrawChart();
			}
		}

		[Display(Name = "Signal Size", GroupName = "Visuals", Order = 2,
			Description = "Edge length in pixels of each marker on the price chart. Clamped to 2..50.")]
		[PostValueMode(PostValueModes.OnLostFocus)]
		public int SignalSize
		{
			get => _signalSize;
			set
			{
				var clamped = Math.Max(2, Math.Min(50, value));
				if (_signalSize == clamped) return;
				_signalSize = clamped;
				RedrawChart();
			}
		}

		#endregion

		#region Properties: Alerts

		[Display(Name = "Alert Sound File", GroupName = "Alerts", Order = 1,
			Description = "Name of the sound file ATAS plays when an alert fires. Files live in the ATAS sounds folder.")]
		public string AlertSoundFile
		{
			get => _alertSoundFile;
			set => _alertSoundFile = string.IsNullOrWhiteSpace(value) ? "alert1" : value;
		}

		[Display(Name = "Alert Cooldown (seconds)", GroupName = "Alerts", Order = 2,
			Description = "Minimum time between consecutive alerts. Applied globally across all pattern categories.")]
		[PostValueMode(PostValueModes.OnLostFocus)]
		public decimal AlertCooldownSeconds
		{
			get => _alertCooldownSeconds;
			set => _alertCooldownSeconds = Math.Max(0m, Math.Min(3600m, value));
		}

		[Display(Name = "Alert Background", GroupName = "Alerts", Order = 3,
			Description = "Background colour of the alert pop-up.")]
		public CrossColor AlertBackgroundColor
		{
			get => _alertBackgroundColor;
			set => _alertBackgroundColor = value;
		}

		[Display(Name = "Alert Foreground", GroupName = "Alerts", Order = 4,
			Description = "Text colour of the alert pop-up.")]
		public CrossColor AlertForegroundColor
		{
			get => _alertForegroundColor;
			set => _alertForegroundColor = value;
		}

		#endregion

		#region Properties: Diagnostics

		[Display(Name = "Overlay Mode", GroupName = "Diagnostics", Order = 1,
			Description = "Off hides the HUD. Compact shows the rolling window state and the live bar pattern. Full adds session-wide stats, cooldown remaining and per-category Enabled / Alert / Visible flags.")]
		public DebugOverlayMode DebugOverlay
		{
			get => _debugOverlayMode;
			set
			{
				if (_debugOverlayMode == value) return;
				_debugOverlayMode = value;
				RedrawChart();
			}
		}

		[Display(Name = "Overlay Corner", GroupName = "Diagnostics", Order = 2,
			Description = "Anchor corner of the HUD on the price chart container.")]
		public DebugOverlayCorner DebugOverlayCornerProperty
		{
			get => _debugOverlayCorner;
			set
			{
				if (_debugOverlayCorner == value) return;
				_debugOverlayCorner = value;
				RedrawChart();
			}
		}

		[Display(Name = "Overlay Offset X", GroupName = "Diagnostics", Order = 3,
			Description = "Horizontal nudge of the HUD anchor in pixels relative to the selected corner. Positive moves right, negative moves left. Useful for fine-tuning when the chosen corner overlaps another overlay.")]
		[PostValueMode(PostValueModes.OnLostFocus)]
		public int DebugOverlayOffsetX
		{
			get => _debugOverlayOffsetX;
			set
			{
				if (_debugOverlayOffsetX == value) return;
				_debugOverlayOffsetX = value;
				RedrawChart();
			}
		}

		[Display(Name = "Overlay Offset Y", GroupName = "Diagnostics", Order = 4,
			Description = "Vertical nudge of the HUD anchor in pixels relative to the selected corner. Positive moves down, negative moves up.")]
		[PostValueMode(PostValueModes.OnLostFocus)]
		public int DebugOverlayOffsetY
		{
			get => _debugOverlayOffsetY;
			set
			{
				if (_debugOverlayOffsetY == value) return;
				_debugOverlayOffsetY = value;
				RedrawChart();
			}
		}

		#endregion

		#region Ctor

		public DeltaPatterns()
			: base(useCandles: true)
		{
			Panel = IndicatorDataProvider.NewPanel;
			DenyToChangePanel = true;

			EnableCustomDrawing = true;
			SubscribeToDrawingEvents(DrawingLayouts.Final);

			// Hide the implicit DataSeries[0] so the Drawing panel
			// does not list a phantom 1px line for the indicator.
			DataSeries[0].IsHidden = true;
			((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

			// Insert the scale anchors at the start of DataSeries so they
			// take the priority slots ATAS' panel autoscale weighs first.
			// Pattern matches the diapason-high / diapason-low ordering in
			// ATAS' built-in Delta indicator.
			DataSeries.Insert(0, _scaleHigh);
			DataSeries.Insert(1, _scaleLow);

			DataSeries.Add(_cAggressive);
			DataSeries.Add(_cDominance);
			DataSeries.Add(_cDivergence);
			DataSeries.Add(_cReversal);
			DataSeries.Add(_cNeutral);
			DataSeries.Add(_cNormal);

			UpdateSeriesColors();
			EnsureCategoryHooks();
		}

		#endregion

		#region Protected Methods

		protected override void OnCalculate(int bar, decimal value)
		{
			// Bars are snapshotted from the history response and from the ticks. A new realtime bar
			// without ticks yet gets the snapshot of the window as it is, as the history does.
			if (_disposed || bar != CurrentBar - 1)
				return;

			lock (_stateLock)
			{
				if (_historyLoaded)
					SnapshotUpTo(bar);
			}
		}

		protected override void OnRecalculate()
		{
			lock (_stateLock)
			{
				_window.Reset();
				_metrics.Clear();
				_patterns.Clear();
				_sessionMaxAmp = 0m;
				_historyLoaded = false;
				_requestId = 0;
				_lastSnapshotBar = -1;
				_firstBar = 0;
				_pendingTicks.Clear();
				_bufferStartTickTime = _lastTickTime;
				_bufferStartTickCount = _lastTickCount;
			}
			EnsureCategoryHooks();
		}

		protected override void OnFinishRecalculate()
		{
            if (_disposed) return;

            bool needsFetch;
			lock (_stateLock)
			{
				needsFetch = !_historyLoaded;
			}

			if (!needsFetch) return;
			if (CurrentBar < 1) return;

			var firstBar = FirstCalculatedBar();

			lock (_stateLock)
				_firstBar = firstBar;

			var firstCandle = GetCandle(firstBar);
			var lastCandle = GetCandle(CurrentBar - 1);
			var sessionStart = firstCandle.Time;
			var sessionEnd = lastCandle?.LastTime ?? firstCandle.LastTime;

			this.LogInfo($"DeltaPatterns: fetch started range={sessionStart:yyyy-MM-dd HH:mm:ss}-{sessionEnd:yyyy-MM-dd HH:mm:ss} (bars {firstBar}-{CurrentBar - 1}, sessions {(_sessionsToCalculate > 0 ? _sessionsToCalculate.ToString() : "all")}), target {TargetVolume} contracts");

			var request = new CumulativeTradesRequest(sessionStart, sessionEnd, 0, 0);

			lock (_stateLock)
				_requestId = request.RequestId;

			RequestForCumulativeTrades(request);
		}

		protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
		{
			if (_disposed) return;

			if (cumulativeTrades == null) return;

			lock (_stateLock)
			{
				if (request.RequestId != _requestId)
					return;
			}

			int tickCount = 0;
			int barsCached = 0;

			try
			{
				// The history is rebuilt from the ticks of the cumulative trades, the same data the
				// realtime path receives, so the window holds the same prices and sizes in both.
				var ticks = cumulativeTrades
					.Where(t => t.Ticks != null)
					.SelectMany(t => t.Ticks)
					.OrderBy(t => t.Time)
					.ToList();

				tickCount = ticks.Count;

				lock (_stateLock)
				{
					_window.Reset();
					_metrics.Clear();
					_patterns.Clear();
					_sessionMaxAmp = 0m;
					_lastSnapshotBar = _firstBar - 1;

					_historyEndTime = ticks.Count > 0 ? ticks[ticks.Count - 1].Time : DateTime.MinValue;
					_boundaryTickCount = 0;

					for (var i = ticks.Count - 1; i >= 0 && ticks[i].Time == _historyEndTime; i--)
						_boundaryTickCount++;

					foreach (var tick in ticks)
						ProcessTick(tick);

					if (CurrentBar > 0)
						SnapshotUpTo(CurrentBar - 1);

					barsCached = _metrics.BarCount;
				}

				this.LogInfo($"DeltaPatterns: fetch completed {tickCount} ticks, {barsCached} bars cached");
			}
			catch (Exception ex)
			{
				this.LogError($"DeltaPatterns: fetch failed - {ex.Message}");
			}

			ReplayPendingTicks();
		}

		protected override void OnNewTrade(MarketDataArg trade)
		{
			if (_disposed) return;

			DeltaPattern oldPattern;
			DeltaPattern newPattern;

			lock (_stateLock)
			{
				if (!_historyLoaded)
				{
					_pendingTicks.Add(trade);
					return;
				}

				if (CurrentBar == 0) return;

				ProcessTick(trade);

				if (_lastSnapshotBar < 0) return;

				// Compare with the pattern after the previous tick, not with the one stored for the
				// bar: the first tick of a new bar would otherwise see no pattern and alert again
				// for a pattern that simply carries on.
				oldPattern = _lastObservedPattern;
				newPattern = _patterns.Get(_lastSnapshotBar);
				_lastObservedPattern = newPattern;
			}

			// Alert firing happens outside the lock so AddAlert's I/O does not stall the
			// market-data thread.
			if (newPattern != DeltaPattern.None && newPattern != oldPattern)
				TryFireAlert(newPattern);
		}

		protected override void OnRender(RenderContext context, DrawingLayouts layout)
		{
            if (_disposed) return;

            if (!ShowChartSignals) return;
			if (ChartInfo == null) return;
			if (layout != DrawingLayouts.Final) return;

			var priceContainer = ChartInfo.PriceChartContainer;
			if (priceContainer == null) return;

			var aggBuy = Aggressive.BuyColor.Convert();
			var aggSell = Aggressive.SellColor.Convert();
			var domBuy = Dominance.BuyColor.Convert();
			var domSell = Dominance.SellColor.Convert();
			var div = Divergence.Color.Convert();
			var revBuy = Reversal.BuyColor.Convert();
			var revSell = Reversal.SellColor.Convert();
			var neu = Neutral.Color.Convert();

			int firstBar = FirstVisibleBarNumber;
			int lastBar = LastVisibleBarNumber;

			for (int bar = firstBar; bar <= lastBar; bar++)
			{
				try
			   {
					if (!_metrics.TryGet(bar, out var m)) continue;
					var pattern = _patterns.Get(bar);
					if (pattern == DeltaPattern.None) continue;
					if (!IsPatternMarkerVisible(pattern)) continue;

					var candle = GetCandle(bar);
					if (candle == null) continue;

					var price = ResolveSignalPrice(pattern, candle);

					if (price <= 0m) continue;

					int x = priceContainer.GetXByBar(bar, false);
					int y = priceContainer.GetYByPrice(price, false);

					switch (pattern)
					{
						case DeltaPattern.AggressiveBuy:
							DrawDot(context, x, y, aggBuy, top: false);
							break;
						case DeltaPattern.AggressiveSell:
							DrawDot(context, x, y, aggSell, top: true);
							break;
						case DeltaPattern.DominanceBuy:
							DrawDot(context, x, y, domBuy, top: false);
							break;
						case DeltaPattern.DominanceSell:
							DrawDot(context, x, y, domSell, top: true);
							break;
						case DeltaPattern.DivergenceBullish:
							DrawDiamond(context, x, y, div, top: true);
							break;
						case DeltaPattern.DivergenceBearish:
							DrawDiamond(context, x, y, div, top: false);
							break;
						case DeltaPattern.ReversalBuy:
							DrawSquare(context, x, y, revBuy, top: false);
							break;
						case DeltaPattern.ReversalSell:
							DrawSquare(context, x, y, revSell, top: true);
							break;
						case DeltaPattern.NeutralStruggle:
							DrawDot(context, x, y, neu, top: true);
							break;
					}
				}
				catch (OverflowException)
				{
					return;
				}
			}

			if (_debugOverlayMode != DebugOverlayMode.Off)
				DrawDebugOverlay(context, priceContainer);
		}

        protected override void OnDispose()
        {
            // Mark first so any in-flight OnNewTrade / OnRender / category
            // PropertyChanged callbacks observe the flag and bail out
            // before they touch state that we are about to wipe.
            _disposed = true;

            UnhookCategoryHandlers();

            lock (_stateLock)
            {
                _window.Reset();
                _metrics.Clear();
                _patterns.Clear();
                _sessionMaxAmp = 0m;
                _historyLoaded = false;
            }

            this.LogInfo("DeltaPatterns: disposed");

            base.OnDispose();
        }

        #endregion

        #region Public Methods

        public DeltaPattern Classify(BarMetrics snapshot)
		{
			if (snapshot.Volume <= 0m) return DeltaPattern.None;

			return TryDetectDivergence(snapshot)
				?? TryDetectReversal(snapshot)
				?? TryDetectDominance(snapshot)
				?? TryDetectAggressive(snapshot)
				?? TryDetectNeutralStruggle(snapshot)
				?? DeltaPattern.None;
		}

		#endregion

		#region Private Methods: Classification

		private DeltaPattern? TryDetectDivergence(BarMetrics m)
		{
			if (!Divergence.Enabled) return null;

			decimal threshold = (decimal)TargetVolume * (Divergence.MinDeltaPercent / 100m);
			if (Math.Abs(m.Delta) <= threshold) return null;

			// A window that closes where it opened has no price direction to diverge from. Without
			// this check a flat window counted as "price down" and only positive delta diverged.
			if (m.ClosePrice == m.OpenPrice) return null;

			bool priceUp = m.ClosePrice > m.OpenPrice;
			bool deltaUp = m.Delta > 0m;
			if (priceUp == deltaUp) return null;

			return deltaUp ? DeltaPattern.DivergenceBullish : DeltaPattern.DivergenceBearish;
		}

		private DeltaPattern? TryDetectReversal(BarMetrics m)
		{
			if (!Reversal.Enabled) return null;

			decimal target = (decimal)TargetVolume;
			decimal extreme = target * (Reversal.MinDeltaPercent / 100m);
			decimal close = target * (Reversal.ClosePercent / 100m);

			if (m.MaxRunningDelta > extreme && m.Delta < -close)
				return DeltaPattern.ReversalSell;
			if (m.MinRunningDelta < -extreme && m.Delta > close)
				return DeltaPattern.ReversalBuy;
			return null;
		}

		private DeltaPattern? TryDetectDominance(BarMetrics m)
		{
			if (!Dominance.Enabled) return null;

			decimal target = (decimal)TargetVolume;
			decimal threshold = target * (Dominance.MinDeltaPercent / 100m);
			if (Math.Abs(m.Delta) <= threshold) return null;

			decimal wick = target * (Dominance.WickTolerancePercent / 100m);

			if (m.Delta > 0m && m.MinRunningDelta >= -wick)
				return DeltaPattern.DominanceBuy;
			if (m.Delta < 0m && m.MaxRunningDelta <= wick)
				return DeltaPattern.DominanceSell;
			return null;
		}

		private DeltaPattern? TryDetectAggressive(BarMetrics m)
		{
			if (!Aggressive.Enabled) return null;

			decimal threshold = (decimal)TargetVolume * (Aggressive.MinDeltaPercent / 100m);
			if (Math.Abs(m.Delta) <= threshold) return null;

			return m.Delta > 0m ? DeltaPattern.AggressiveBuy : DeltaPattern.AggressiveSell;
		}

		private DeltaPattern? TryDetectNeutralStruggle(BarMetrics m)
		{
			if (!Neutral.Enabled) return null;
			if (m.Volume <= 0m) return null;

			decimal deltaPercent = Math.Abs(m.Delta / m.Volume * 100m);
			if (deltaPercent > Neutral.MinDeltaPercent) return null;

			decimal struggle = (decimal)TargetVolume * (Neutral.StrugglePercent / 100m);
			if (m.MaxRunningDelta > struggle || m.MinRunningDelta < -struggle)
				return DeltaPattern.NeutralStruggle;
			return null;
		}

		#endregion

		#region Private Methods: Rendering

		// Caller must hold _stateLock. Adds a tick to the window in the bar that contains its time.
		// Bars before it that have not been snapshotted yet (no ticks of their own) get the window
		// as it was before the tick.
		private void ProcessTick(MarketDataArg tick)
		{
			if (tick.Time == _lastTickTime)
				_lastTickCount++;
			else
			{
				_lastTickTime = tick.Time;
				_lastTickCount = 1;
			}

			var bar = BarOfTime(tick.Time);

			if (bar < 0)
				return;

			SnapshotUpTo(bar - 1);

			// Trades without an aggressor side (Between) have no delta: they would otherwise be
			// counted as sells and skew every pattern towards selling.
			if (tick.Direction is not (TradeDirection.Buy or TradeDirection.Sell))
			{
				SnapshotUpTo(bar);
				return;
			}

			int direction = tick.Direction == TradeDirection.Buy ? 1 : -1;
			_window.Push(tick.Price, tick.Volume, direction);
			_window.Trim(TargetVolume);

			SnapshotBar(bar);
			_lastSnapshotBar = Math.Max(_lastSnapshotBar, bar);
		}

		// Caller must hold _stateLock. Snapshots the bars after the last snapshotted one up to bar
		// with the window as it is.
		private void SnapshotUpTo(int bar)
		{
			for (var b = _lastSnapshotBar + 1; b <= bar; b++)
				SnapshotBar(b);

			_lastSnapshotBar = Math.Max(_lastSnapshotBar, bar);
		}

		// First bar of the last SessionsToCalculate sessions, or 0 for the whole chart.
		private int FirstCalculatedBar()
		{
			if (_sessionsToCalculate <= 0)
				return 0;

			var sessions = 0;

			for (var bar = CurrentBar - 1; bar > 0; bar--)
			{
				if (IsNewSession(bar) && ++sessions == _sessionsToCalculate)
					return bar;
			}

			return 0;
		}

		// Bar that contains a time: the last bar opened at or before it, or -1 before the first bar.
		// Ticks arrive in time order, so the search starts from the last snapshotted bar and
		// normally moves forward by at most one bar.
		private int BarOfTime(DateTime time)
		{
			if (CurrentBar == 0)
				return -1;

			var bar = Math.Min(Math.Max(0, _lastSnapshotBar), CurrentBar - 1);

			while (bar > 0 && GetCandle(bar).Time > time)
				bar--;

			if (GetCandle(bar).Time > time)
				return -1;

			while (bar + 1 < CurrentBar && GetCandle(bar + 1).Time <= time)
				bar++;

			return bar;
		}

		// Replays the ticks buffered while the history was pending, then switches to realtime
		// under the lock once the buffer is empty. Ticks already in the history are skipped.
		private void ReplayPendingTicks()
		{
			int boundaryToSkip;
			var replayed = 0;
			var skipped = 0;

			lock (_stateLock)
			{
				boundaryToSkip = _boundaryTickCount -
					(_bufferStartTickTime == _historyEndTime ? _bufferStartTickCount : 0);
			}

			while (true)
			{
				List<MarketDataArg> batch;

				lock (_stateLock)
				{
					if (_pendingTicks.Count == 0)
					{
						_historyLoaded = true;
						_lastObservedPattern = _lastSnapshotBar >= 0 ? _patterns.Get(_lastSnapshotBar) : DeltaPattern.None;
						break;
					}

					batch = new List<MarketDataArg>(_pendingTicks);
					_pendingTicks.Clear();
				}

				foreach (var tick in batch)
				{
					lock (_stateLock)
					{
						if (tick.Time < _historyEndTime || (tick.Time == _historyEndTime && boundaryToSkip-- > 0))
						{
							skipped++;
							continue;
						}

						ProcessTick(tick);
						replayed++;
					}
				}
			}

			this.LogInfo($"DeltaPatterns: realtime started; buffered ticks: {replayed} replayed, {skipped} already in the history");
			RedrawChart();
		}

		// Caller must hold _stateLock.
		private void SnapshotBar(int bar)
		{
			var snapshot = new BarMetrics
			{
				Delta = _window.CurrentDelta,
				Volume = _window.CurrentVolume,
				MaxRunningDelta = _window.MaxRunningDelta,
				MinRunningDelta = _window.MinRunningDelta,
				OpenPrice = _window.StartPrice,
				ClosePrice = _window.EndPrice,
				HighPrice = _window.HighPrice,
				LowPrice = _window.LowPrice,
			};
			_metrics.Set(bar, snapshot);
			var pattern = Classify(snapshot);
			_patterns.Set(bar, pattern);
			ApplyBarToSeries(bar, snapshot, pattern);
		}

		// Caller must hold _stateLock.
		private void ApplyBarToSeries(int bar, BarMetrics m, DeltaPattern pattern)
		{
			_cAggressive[bar] = EmptyCandle;
			_cDominance[bar] = EmptyCandle;
			_cDivergence[bar] = EmptyCandle;
			_cReversal[bar] = EmptyCandle;
			_cNeutral[bar] = EmptyCandle;
			_cNormal[bar] = EmptyCandle;

			var entry = MapPatternToSeries(pattern);
			if (entry.Visible)
			{
				entry.Series[bar] = new Candle
				{
					Open = 0m,
					Close = m.Delta,
					High = m.MaxRunningDelta,
					Low = m.MinRunningDelta,
				};
			}

			var rawAmp = ComputeRawAmplitude(m);
			if (rawAmp > _sessionMaxAmp)
			{
				_sessionMaxAmp = rawAmp;
				BroadcastScaleAnchor();
			}
			else
			{
				var anchor = ComputeDisplayAnchor();
				_scaleHigh[bar] = anchor;
				_scaleLow[bar] = -anchor;
			}
		}

		private (CandleDataSeries Series, bool Visible) MapPatternToSeries(DeltaPattern p)
		{
			switch (p)
			{
				case DeltaPattern.AggressiveBuy:
				case DeltaPattern.AggressiveSell:
					return (_cAggressive, Aggressive.Visible);
				case DeltaPattern.DominanceBuy:
				case DeltaPattern.DominanceSell:
					return (_cDominance, Dominance.Visible);
				case DeltaPattern.DivergenceBullish:
				case DeltaPattern.DivergenceBearish:
					return (_cDivergence, Divergence.Visible);
				case DeltaPattern.ReversalBuy:
				case DeltaPattern.ReversalSell:
					return (_cReversal, Reversal.Visible);
				case DeltaPattern.NeutralStruggle:
					return (_cNeutral, Neutral.Visible);
				default:
					return (_cNormal, Normal.Visible);
			}
		}

		private static decimal ComputeRawAmplitude(BarMetrics m)
		{
			return Math.Max(Math.Abs(m.MaxRunningDelta), Math.Abs(m.MinRunningDelta));
		}

		// Caller must hold _stateLock.
		private decimal ComputeDisplayAnchor()
		{
			var floor = (decimal)TargetVolume * 0.1m;
			var amp = _sessionMaxAmp < floor ? floor : _sessionMaxAmp;
			return amp * ScaleAnchorMargin;
		}

		// Caller must hold _stateLock.
		private void BroadcastScaleAnchor()
		{
			var anchor = ComputeDisplayAnchor();
			for (int bar = 0; bar < _metrics.BarCount; bar++)
			{
				_scaleHigh[bar] = anchor;
				_scaleLow[bar] = -anchor;
			}
		}

		private void UpdateSeriesColors()
		{
			SetDirectionalCandleColor(_cAggressive, Aggressive);
			SetDirectionalCandleColor(_cDominance, Dominance);
			SetMonoCandleColor(_cDivergence, Divergence.Color);
			SetDirectionalCandleColor(_cReversal, Reversal);
			SetMonoCandleColor(_cNeutral, Neutral.Color);
			SetMonoCandleColor(_cNormal, Normal.Color);
		}

		private static void SetDirectionalCandleColor(CandleDataSeries series, DirectionalPatternCategory cat)
		{
			series.UpCandleColor = cat.BuyColor;
			series.DownCandleColor = cat.SellColor;
			series.BorderColor = cat.BuyColor;
		}

		private static void SetMonoCandleColor(CandleDataSeries series, CrossColor color)
		{
			series.UpCandleColor = color;
			series.DownCandleColor = color;
			series.BorderColor = color;
		}

		private void EnsureCategoryHooks()
		{
			EnsureHook(Aggressive);
			EnsureHook(Dominance);
			EnsureHook(Divergence);
			EnsureHook(Reversal);
			EnsureHook(Neutral);
			EnsureHook(Normal);
		}

		private void EnsureHook(PatternCategory cat)
		{
			if (cat == null) return;
			cat.PropertyChanged -= OnCategoryPropertyChanged;
			cat.PropertyChanged += OnCategoryPropertyChanged;
		}

        #region Private Methods: Lifecycle

        private void UnhookCategoryHandlers()
        {
            UnhookFrom(Aggressive);
            UnhookFrom(Dominance);
            UnhookFrom(Divergence);
            UnhookFrom(Reversal);
            UnhookFrom(Neutral);
            UnhookFrom(Normal);
        }

        private void UnhookFrom(PatternCategory cat)
        {
            if (cat == null) return;
            cat.PropertyChanged -= OnCategoryPropertyChanged;
        }

        #endregion

        private void OnCategoryPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            if (_disposed) return;

            switch (e.PropertyName)
			{
				case nameof(MonoPatternCategory.Color):
				case nameof(DirectionalPatternCategory.BuyColor):
				case nameof(DirectionalPatternCategory.SellColor):
					UpdateSeriesColors();
					RedrawChart();
					break;

				case nameof(PatternCategory.Visible):
					RebuildSeriesFromCache();
					break;

				case nameof(PatternCategory.Enabled):
				case nameof(PatternCategory.MinDeltaPercent):
				case nameof(DominancePatternCategory.WickTolerancePercent):
				case nameof(ReversalPatternCategory.ClosePercent):
				case nameof(NeutralPatternCategory.StrugglePercent):
					ReclassifyAndRender();
					break;

				case nameof(PatternCategory.EnableAlert):
					// The alert pipeline reads the flag at firing time; no cache rebuild needed.
					break;
			}
		}

		private void ReclassifyAndRender()
		{
			lock (_stateLock)
			{
				for (int bar = 0; bar < _metrics.BarCount; bar++)
				{
					if (_metrics.TryGet(bar, out var m))
					{
						var pattern = Classify(m);
						_patterns.Set(bar, pattern);
						ApplyBarToSeries(bar, m, pattern);
					}
				}

				// The last pattern may have changed with the settings: take it as the one already
				// observed so the next tick does not alert for the settings change.
				if (_historyLoaded && _lastSnapshotBar >= 0)
					_lastObservedPattern = _patterns.Get(_lastSnapshotBar);
			}
			RedrawChart();
		}

		private void RebuildSeriesFromCache()
		{
			lock (_stateLock)
			{
				for (int bar = 0; bar < _metrics.BarCount; bar++)
				{
					if (_metrics.TryGet(bar, out var m))
					{
						ApplyBarToSeries(bar, m, _patterns.Get(bar));
					}
				}
			}
			RedrawChart();
		}

		private bool IsPatternMarkerVisible(DeltaPattern pattern)
		{
			switch (pattern)
			{
				case DeltaPattern.AggressiveBuy:
				case DeltaPattern.AggressiveSell:
					return Aggressive.Visible;
				case DeltaPattern.DominanceBuy:
				case DeltaPattern.DominanceSell:
					return Dominance.Visible;
				case DeltaPattern.DivergenceBullish:
				case DeltaPattern.DivergenceBearish:
					return Divergence.Visible;
				case DeltaPattern.ReversalBuy:
				case DeltaPattern.ReversalSell:
					return Reversal.Visible;
				case DeltaPattern.NeutralStruggle:
					return Neutral.Visible;
				default:
					return false;
			}
		}

		private static decimal ResolveSignalPrice(DeltaPattern pattern, IndicatorCandle candle)
		{
			switch (pattern)
			{
				case DeltaPattern.AggressiveBuy:
				case DeltaPattern.DominanceBuy:
				case DeltaPattern.ReversalBuy:
				case DeltaPattern.DivergenceBearish:
					return candle.Low;
				case DeltaPattern.AggressiveSell:
				case DeltaPattern.DominanceSell:
				case DeltaPattern.ReversalSell:
				case DeltaPattern.DivergenceBullish:
					return candle.High;
				case DeltaPattern.NeutralStruggle:
					return candle.Close;
				default:
					return 0m;
			}
		}

		private void DrawDot(RenderContext ctx, int x, int y, System.Drawing.Color color, bool top)
		{
			int offset = top ? -SignalSize - MarkerPriceGap : MarkerPriceGap;
			var rect = new Rectangle(x - SignalSize / 2, y + offset, SignalSize, SignalSize);
			ctx.FillEllipse(color, rect);
		}

		private void DrawSquare(RenderContext ctx, int x, int y, System.Drawing.Color color, bool top)
		{
			int offset = top ? -SignalSize - MarkerPriceGap : MarkerPriceGap;
			var rect = new Rectangle(x - SignalSize / 2, y + offset, SignalSize, SignalSize);
			ctx.FillRectangle(color, rect);
		}

		private void DrawDiamond(RenderContext ctx, int x, int y, System.Drawing.Color color, bool top)
		{
			int s = SignalSize / 2 + 2;
			int offsetY = top ? -s - MarkerPriceGap : s + MarkerPriceGap;
			var p1 = new Point(x, y + offsetY - s);
			var p2 = new Point(x + s, y + offsetY);
			var p3 = new Point(x, y + offsetY + s);
			var p4 = new Point(x - s, y + offsetY);
			ctx.FillPolygon(color, new[] { p1, p2, p3, p4 });
		}

		private bool IsPatternAlertEnabled(DeltaPattern pattern)
		{
			switch (pattern)
			{
				case DeltaPattern.AggressiveBuy:
				case DeltaPattern.AggressiveSell:
					return Aggressive.EnableAlert;
				case DeltaPattern.DominanceBuy:
				case DeltaPattern.DominanceSell:
					return Dominance.EnableAlert;
				case DeltaPattern.DivergenceBullish:
				case DeltaPattern.DivergenceBearish:
					return Divergence.EnableAlert;
				case DeltaPattern.ReversalBuy:
				case DeltaPattern.ReversalSell:
					return Reversal.EnableAlert;
				case DeltaPattern.NeutralStruggle:
					return Neutral.EnableAlert;
				default:
					return false;
			}
		}

		private void TryFireAlert(DeltaPattern pattern)
		{
			if (!IsPatternAlertEnabled(pattern)) return;

			var now = DateTime.UtcNow;
			if ((now - _lastAlertTime).TotalSeconds < (double)AlertCooldownSeconds) return;

			_lastAlertTime = now;

			var instrument = InstrumentInfo?.Instrument ?? "?";
			var message = $"DeltaPatterns: {pattern}";
			AddAlert(AlertSoundFile, instrument, message, AlertBackgroundColor, AlertForegroundColor);

			this.LogInfo($"DeltaPatterns: alert fired pattern={pattern} instrument={instrument}");
		}

		#endregion

		#region Private Methods: Debug Overlay

		private void DrawDebugOverlay(RenderContext context, IChartContainer container)
		{
			List<string> lines;
			try
			{
				lines = BuildDebugHudSnapshot(_debugOverlayMode);
			}
			catch (Exception ex)
			{
				this.LogError($"DeltaPatterns: HUD snapshot failed - {ex.Message}");
				return;
			}

			const int padding = 8;
			const int lineHeight = 16;
			const int margin = 10;

			int maxTextWidth = 0;
			foreach (var line in lines)
			{
				if (string.IsNullOrEmpty(line)) continue;
				var size = context.MeasureString(line, _hudFont);
				if (size.Width > maxTextWidth) maxTextWidth = size.Width;
			}

			int width = maxTextWidth + padding * 2;
			int height = lines.Count * lineHeight + padding * 2;

			int x, y;
			switch (_debugOverlayCorner)
			{
				case DebugOverlayCorner.TopLeft:
					x = container.Region.X + margin;
					y = container.Region.Y + margin;
					break;
				case DebugOverlayCorner.BottomLeft:
					x = container.Region.X + margin;
					y = container.Region.Y + container.Region.Height - height - margin;
					break;
				case DebugOverlayCorner.BottomRight:
					x = container.Region.X + container.Region.Width - width - margin;
					y = container.Region.Y + container.Region.Height - height - margin;
					break;
				case DebugOverlayCorner.TopRight:
				default:
					x = container.Region.X + container.Region.Width - width - margin;
					y = container.Region.Y + margin;
					break;
			}

			x += _debugOverlayOffsetX;
			y += _debugOverlayOffsetY;

			var bgRect = new Rectangle(x, y, width, height);
			context.FillRectangle(System.Drawing.Color.FromArgb(210, 18, 18, 22), bgRect);
			context.DrawRectangle(_hudPen, bgRect);

			int textY = y + padding;
			foreach (var line in lines)
			{
				context.DrawString(line, _hudFont, System.Drawing.Color.White, x + padding, textY);
				textY += lineHeight;
			}
		}

		private List<string> BuildDebugHudSnapshot(DebugOverlayMode mode)
		{
			int liveBar;
			DeltaPattern livePattern;
			bool historyLoaded;
			int barsCount;
			int tickCount;
			decimal volume, delta, maxRun, minRun;
			decimal sessionMaxAmp;
			DateTime lastAlertTime;

			lock (_stateLock)
			{
				liveBar = CurrentBar - 1;
				livePattern = liveBar >= 0 ? _patterns.Get(liveBar) : DeltaPattern.None;
				historyLoaded = _historyLoaded;
				barsCount = _metrics.BarCount;
				tickCount = _window.TickCount;
				volume = _window.CurrentVolume;
				delta = _window.CurrentDelta;
				maxRun = _window.MaxRunningDelta;
				minRun = _window.MinRunningDelta;
				sessionMaxAmp = _sessionMaxAmp;
				lastAlertTime = _lastAlertTime;
			}

			var lines = new List<string>(24);
			lines.Add("== DELTA PATTERNS ==");

			if (mode == DebugOverlayMode.Full)
			{
				lines.Add(string.Empty);
				lines.Add($"TargetVolume:  {TargetVolume}");
				lines.Add($"HistoryLoaded: {historyLoaded}");
				lines.Add($"BarsCached:    {barsCount}");
				lines.Add($"SessionMaxAmp: {sessionMaxAmp:0}");
			}

			lines.Add(string.Empty);
			lines.Add("Window");
			lines.Add($"  Ticks:   {tickCount}");
			lines.Add($"  Volume:  {volume:0}");
			lines.Add($"  Delta:   {delta:+0;-0;0}");
			lines.Add($"  MaxRun:  {maxRun:+0;-0;0}");
			lines.Add($"  MinRun:  {minRun:+0;-0;0}");
			lines.Add(string.Empty);
			lines.Add($"Live bar:  {liveBar}");
			lines.Add($"Pattern:   {livePattern}");
			lines.Add(string.Empty);

			if (mode == DebugOverlayMode.Full)
			{				
				string cooldownStatus;
				if (lastAlertTime == DateTime.MinValue)
				{
					cooldownStatus = "ready";
				}
				else
				{
					double sinceLast = (DateTime.UtcNow - lastAlertTime).TotalSeconds;
					double cooldown = (double)AlertCooldownSeconds;
					cooldownStatus = sinceLast >= cooldown
						? "ready"
						: $"{cooldown - sinceLast:0.0}s left";
				}

				lines.Add($"Cooldown: {AlertCooldownSeconds}s ({cooldownStatus})");
				lines.Add($"  Aggressive  {FormatCategoryFlags(Aggressive)}");
				lines.Add($"  Dominance   {FormatCategoryFlags(Dominance)}");
				lines.Add($"  Divergence  {FormatCategoryFlags(Divergence)}");
				lines.Add($"  Reversal    {FormatCategoryFlags(Reversal)}");
				lines.Add($"  Neutral     {FormatCategoryFlags(Neutral)}");
				lines.Add(string.Empty);
			}

			return lines;
		}

		private static string FormatCategoryFlags(PatternCategory cat)
		{
			var enabled = cat.Enabled ? "ON " : "OFF";
			var alert = cat.EnableAlert ? "ALRT" : "    ";
			var visible = cat.Visible ? "VIS" : "HID";
			return $"{enabled} {alert} {visible}";
		}

		#endregion

		#region Private Methods: Debug Hooks (temporary — removed in c15)

		// Bypass IsPatternAlertEnabled and the cooldown gate.
		// Use this to verify that the AddAlert dispatcher itself works
		// (sound file path, popup colors, instrument tag) independent
		// of category configuration.
		[Browsable(false)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void DebugFireAlertRaw(DeltaPattern pattern)
		{
            if (_disposed) return;

            var instrument = InstrumentInfo?.Instrument ?? "?";
			var message = $"DeltaPatterns[DBG-RAW]: {pattern}";
			try
			{
				AddAlert(AlertSoundFile, instrument, message, AlertBackgroundColor, AlertForegroundColor);
				this.LogInfo($"DeltaPatterns: debug raw alert fired pattern={pattern} instrument={instrument}");
			}
			catch (Exception ex)
			{
				this.LogError($"DeltaPatterns: debug raw alert dispatch failed - {ex.Message}");
			}
		}

		// Goes through the same gating as a real transition: respects
		// category Enabled/EnableAlert and the global cooldown.
		[Browsable(false)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void DebugFireAlertGated(DeltaPattern pattern)
		{
            if (_disposed) return;

            TryFireAlert(pattern);
		}

		// Forces the cooldown timer back to MinValue so the next call
		// to DebugFireAlertGated / a real transition can fire immediately.
		[Browsable(false)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void DebugResetAlertCooldown()
		{
            _lastAlertTime = DateTime.MinValue;
			this.LogInfo("DeltaPatterns: debug cooldown reset");
		}

		// Simulates the OnNewTrade transition path end-to-end: rewrites
		// the live bar's cached pattern to oldPattern, then invokes the
		// gated alert with newPattern. Mirrors what would happen if the
		// window classifier produced a transition naturally.
		[Browsable(false)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public void DebugSimulateTransition(DeltaPattern oldPattern, DeltaPattern newPattern)
		{
            if (_disposed) return;

            int currentBar = CurrentBar - 1;
			if (currentBar < 0)
			{
				this.LogError("DeltaPatterns: debug simulate transition skipped - no current bar");
				return;
			}

			lock (_stateLock)
			{
				_patterns.Set(currentBar, oldPattern);
			}

			if (oldPattern != newPattern)
				TryFireAlert(newPattern);

			this.LogInfo($"DeltaPatterns: debug simulated transition {oldPattern} -> {newPattern}");
		}

		// Inspector helper: returns the current alert state as a single
		// string for the Immediate Window. Avoids hand-evaluating
		// private fields one at a time.
		[Browsable(false)]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
		public string DebugAlertSnapshot()
		{
			return
				$"sound='{AlertSoundFile}' " +
				$"cooldown={AlertCooldownSeconds}s " +
				$"lastAlertUtc={_lastAlertTime:yyyy-MM-dd HH:mm:ss.fff} " +
				$"sinceLast={(DateTime.UtcNow - _lastAlertTime).TotalSeconds:0.000}s " +
				$"agg={Aggressive.Enabled}/{Aggressive.EnableAlert} " +
				$"dom={Dominance.Enabled}/{Dominance.EnableAlert} " +
				$"div={Divergence.Enabled}/{Divergence.EnableAlert} " +
				$"rev={Reversal.Enabled}/{Reversal.EnableAlert} " +
				$"neu={Neutral.Enabled}/{Neutral.EnableAlert}";
		}

        #endregion
    }
}
