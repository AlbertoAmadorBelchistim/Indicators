using ATAS.Indicators.Drawing;
using OFT.Attributes.Editors;
using OFT.Rendering.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
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

		private int _targetVolume = 2500;

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
			// The cache is populated by OnCumulativeTradesResponse for
			// historical bars and by OnNewTrade for live bars; OnCalculate
			// itself stays a no-op.
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
			}
			EnsureCategoryHooks();
		}

		protected override void OnFinishRecalculate()
		{
			bool needsFetch;
			lock (_stateLock)
			{
				needsFetch = !_historyLoaded;
			}

			if (!needsFetch) return;
			if (CurrentBar < 1) return;

			var firstCandle = GetCandle(0);
			var lastCandle = GetCandle(CurrentBar - 1);
			var sessionStart = firstCandle.Time;
			var sessionEnd = lastCandle?.LastTime ?? firstCandle.LastTime;

			this.LogInfo($"DeltaPatterns: fetch started range={sessionStart:yyyy-MM-dd HH:mm:ss}-{sessionEnd:yyyy-MM-dd HH:mm:ss}, target {TargetVolume} contracts");

			var request = new CumulativeTradesRequest(sessionStart, sessionEnd, 0, 0);
			RequestForCumulativeTrades(request);
		}

		protected override void OnCumulativeTradesResponse(CumulativeTradesRequest request, IEnumerable<CumulativeTrade> cumulativeTrades)
		{
			if (cumulativeTrades == null) return;

			int totalBars = CurrentBar - 1;
			if (totalBars < 0) return;

			int tradeCount = 0;
			int barsCached = 0;

			try
			{
				var target = (decimal)TargetVolume;
				int currentBarIndex = 0;

				lock (_stateLock)
				{
					_window.Reset();
					_metrics.Clear();
					_patterns.Clear();
					_sessionMaxAmp = 0m;

					foreach (var trade in cumulativeTrades)
					{
						tradeCount++;

						while (currentBarIndex <= totalBars)
						{
							var candle = GetCandle(currentBarIndex);
							if (candle != null && candle.LastTime < trade.Time)
							{
								SnapshotBar(currentBarIndex);
								currentBarIndex++;
							}
							else
							{
								break;
							}
						}

						int direction = trade.Direction == TradeDirection.Buy ? 1 : -1;
						_window.Push(trade.FirstPrice, trade.Volume, direction);
						_window.Trim(target);
					}

					while (currentBarIndex <= totalBars)
					{
						SnapshotBar(currentBarIndex);
						currentBarIndex++;
					}

					barsCached = _metrics.BarCount;
					_historyLoaded = true;
				}

				this.LogInfo($"DeltaPatterns: fetch completed {tradeCount} trades, {barsCached} bars cached");
			}
			catch (Exception ex)
			{
				this.LogError($"DeltaPatterns: fetch failed - {ex.Message}");
			}
		}

		protected override void OnNewTrade(MarketDataArg trade)
		{
			lock (_stateLock)
			{
				if (!_historyLoaded) return;

				int direction = trade.Direction == TradeDirection.Buy ? 1 : -1;
				_window.Push(trade.Price, trade.Volume, direction);
				_window.Trim(TargetVolume);

				int currentBar = CurrentBar - 1;
				if (currentBar >= 0)
					SnapshotBar(currentBar);
			}
		}

		protected override void OnRender(RenderContext context, DrawingLayouts layout)
		{
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

		private void OnCategoryPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
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
					// No visual effect; the alert layer arrives in commit 11.
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

		#endregion
	}
}
