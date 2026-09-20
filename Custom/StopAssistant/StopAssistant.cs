namespace ATAS.Indicators.Technical
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using System.Drawing;
	using System.Globalization;

	using ATAS.Indicators;
	using ATAS.Indicators.Drawing;
	using ATAS.Indicators.Technical.StopAssistantCore;

	using OFT.Rendering.Context;
	using OFT.Rendering.Settings;
	using OFT.Rendering.Tools;

	using Utils.Common.Logging;

	/// <summary>Where the entry price comes from.</summary>
	public enum EntrySource
	{
		[Display(Name = "Last price")]
		LastPrice,

		[Display(Name = "Average price of the position")]
		Position,

		[Display(Name = "Written by hand")]
		Manual,
	}

	/// <summary>Which side the plan is for.</summary>
	public enum SideSource
	{
		[Display(Name = "Long")]
		Long,

		[Display(Name = "Short")]
		Short,

		[Display(Name = "The open position")]
		Position,
	}

	[Category("Custom")]
	[DisplayName("Stop Assistant")]
	[Description("Stop candidates, what each one costs per contract, how many contracts fit and where the R multiples land.")]
	public sealed class StopAssistant : Indicator
	{
		#region Fields

		private readonly List<Bar> _bars = new(512);
		private readonly List<StopCandidate> _candidates = new(8);
		private readonly List<(string Label, decimal Price)> _levels = new(3);

		private readonly RenderStringFormat _rightFormat = new()
		{
			Alignment = StringAlignment.Far,
			LineAlignment = StringAlignment.Center,
		};

		private RenderFont _font = new("Arial", 10);

		private SideSource _side = SideSource.Long;
		private EntrySource _entrySource = EntrySource.LastPrice;
		private StopKind _stopKind = StopKind.Atr;

		private decimal _manualEntry;
		private decimal _manualStop;
		private int _atrPeriod = 14;
		private decimal _atrMultiplier = 1.5m;
		private int _swingLookback = 20;
		private int _paddingTicks = 4;
		private bool _useLevels = true;

		private decimal _budget = 200m;
		private decimal _dailyLimit;
		private decimal _manualLoss;
		private decimal _tickCost;
		private int _maxContracts;
		private int _targets = 3;

		// Last computed plan, for the render.
		private decimal _atr;
		private decimal _swing;
		private decimal _lastPrice;
		private RiskPlan _plan;
		private StopCandidate _stop;
		private bool _hasPlan;

		private DateTime _day;
		private decimal _dayStartClosedPnl;
		private bool _dayCaptured;

		#endregion

		#region Trade

		[Display(Name = "Side", GroupName = "Trade", Order = 100)]
		public SideSource Side
		{
			get => _side;
			set
			{
				_side = value;
				RedrawChart();
			}
		}

		[Display(Name = "Entry", GroupName = "Trade", Order = 110)]
		public EntrySource Entry
		{
			get => _entrySource;
			set
			{
				_entrySource = value;
				RedrawChart();
			}
		}

		[Display(Name = "Entry price", GroupName = "Trade", Description = "Used when the entry is written by hand.", Order = 120)]
		public decimal ManualEntry
		{
			get => _manualEntry;
			set
			{
				_manualEntry = value;
				RedrawChart();
			}
		}

		#endregion

		#region Stop

		[Display(Name = "Stop", GroupName = "Stop", Description = "Which candidate the numbers are computed for. When that one is not available, the nearest is used.", Order = 200)]
		public StopKind Stop
		{
			get => _stopKind;
			set
			{
				_stopKind = value;
				RedrawChart();
			}
		}

		[Display(Name = "ATR period", GroupName = "Stop", Order = 210)]
		[Range(1, 500)]
		public int AtrPeriod
		{
			get => _atrPeriod;
			set
			{
				_atrPeriod = value;
				RecalculateValues();
			}
		}

		[Display(Name = "ATR multiplier", GroupName = "Stop", Order = 220)]
		[Range(0.1, 20)]
		public decimal AtrMultiplier
		{
			get => _atrMultiplier;
			set
			{
				_atrMultiplier = value;
				RedrawChart();
			}
		}

		[Display(Name = "Swing, bars", GroupName = "Stop", Description = "Bars looked back for the low, or the high, the stop hides behind.", Order = 230)]
		[Range(2, 1000)]
		public int SwingLookback
		{
			get => _swingLookback;
			set
			{
				_swingLookback = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Padding, ticks", GroupName = "Stop", Description = "Ticks left beyond the swing or the level, so the stop is not exactly where everyone else's is.", Order = 240)]
		[Range(0, 200)]
		public int PaddingTicks
		{
			get => _paddingTicks;
			set
			{
				_paddingTicks = value;
				RedrawChart();
			}
		}

		[Display(Name = "Levels of the session", GroupName = "Stop", Description = "POC, value area high and value area low of the day as candidates.", Order = 250)]
		public bool UseLevels
		{
			get => _useLevels;
			set
			{
				_useLevels = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Stop price", GroupName = "Stop", Description = "A stop written by hand, for the Manual candidate.", Order = 260)]
		public decimal ManualStop
		{
			get => _manualStop;
			set
			{
				_manualStop = value;
				RedrawChart();
			}
		}

		#endregion

		#region Risk

		[Display(Name = "Risk of the trade", GroupName = "Risk", Description = "What this trade may lose, in the currency of the account.", Order = 300)]
		[Range(0, 1000000)]
		public decimal Budget
		{
			get => _budget;
			set
			{
				_budget = value;
				RedrawChart();
			}
		}

		[Display(Name = "Daily loss limit", GroupName = "Risk", Description = "The wall of the day. What is left of it caps the size of the trade. Zero leaves it out.", Order = 310)]
		[Range(0, 1000000)]
		public decimal DailyLimit
		{
			get => _dailyLimit;
			set
			{
				_dailyLimit = value;
				RedrawChart();
			}
		}

		[Display(Name = "Lost today", GroupName = "Risk", Description = "Negative, and only to correct the platform: zero reads the closed result of the day from the account.", Order = 320)]
		public decimal ManualLoss
		{
			get => _manualLoss;
			set
			{
				_manualLoss = value;
				RedrawChart();
			}
		}

		[Display(Name = "Value of the tick", GroupName = "Risk", Description = "Zero takes it from the instrument of the connection. Write it when the chart has no trading account behind it.", Order = 330)]
		[Range(0, 100000)]
		public decimal TickCost
		{
			get => _tickCost;
			set
			{
				_tickCost = value;
				RedrawChart();
			}
		}

		[Display(Name = "Maximum contracts", GroupName = "Risk", Description = "Zero leaves the risk as the only ceiling.", Order = 340)]
		[Range(0, 1000)]
		public int MaxContracts
		{
			get => _maxContracts;
			set
			{
				_maxContracts = value;
				RedrawChart();
			}
		}

		#endregion

		#region Visualization

		[Display(Name = "Targets, R", GroupName = "Visualization", Description = "How many multiples of the risk are drawn. Zero draws none.", Order = 400)]
		[Range(0, 10)]
		public int Targets
		{
			get => _targets;
			set
			{
				_targets = value;
				RedrawChart();
			}
		}

		[Display(Name = "Other candidates", GroupName = "Visualization", Description = "The stops that were not chosen, drawn thin.", Order = 410)]
		public bool ShowCandidates { get; set; } = true;

		[Display(Name = "Entry", GroupName = "Visualization", Order = 420)]
		public PenSettings EntryPen { get; set; } = new()
			{ Color = Rgb(0xBD, 0xBD, 0xBD), Width = 1, LineDashStyle = LineDashStyle.Dash };

		[Display(Name = "Stop line", GroupName = "Visualization", Order = 430)]
		public PenSettings StopPen { get; set; } = new()
			{ Color = Rgb(0xEF, 0x53, 0x50), Width = 2 };

		[Display(Name = "Candidate", GroupName = "Visualization", Order = 440)]
		public PenSettings CandidatePen { get; set; } = new()
			{ Color = Rgb(0x8D, 0x6E, 0x63), Width = 1, LineDashStyle = LineDashStyle.Dot };

		[Display(Name = "Target", GroupName = "Visualization", Order = 450)]
		public PenSettings TargetPen { get; set; } = new()
			{ Color = Rgb(0x26, 0xA6, 0x9A), Width = 1, LineDashStyle = LineDashStyle.Dash };

		[Display(Name = "Readout", GroupName = "Visualization", Order = 460)]
		public bool ShowReadout { get; set; } = true;

		[Display(Name = "Readout text", GroupName = "Visualization", Order = 470)]
		public CrossColor ReadoutColor { get; set; } = DefaultColors.Gray.Convert();

		[Display(Name = "Font size", GroupName = "Visualization", Order = 480)]
		[Range(5, 30)]
		public float FontSize
		{
			get => _font.Size;
			set
			{
				_font = new RenderFont("Arial", value);
				RedrawChart();
			}
		}

		#endregion

		#region ctor

		public StopAssistant()
			: base(useCandles: true)
		{
			DenyToChangePanel = true;
			DrawAbovePrice = true;
			EnableCustomDrawing = true;
			SubscribeToDrawingEvents(DrawingLayouts.Final);

			DataSeries[0].IsHidden = true;
			((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
		}

		#endregion

		#region Protected methods

		protected override void OnInitialize()
		{
			this.LogInfo($"StopAssistant: initialized ({typeof(StopAssistant).Assembly.GetName().Version}).");
		}

		protected override void OnRecalculate()
		{
			_bars.Clear();
			_levels.Clear();
			_hasPlan = false;
		}

		protected override void OnCalculate(int bar, decimal value)
		{
			var candle = GetCandle(bar);

			if (bar == 0)
			{
				_bars.Clear();

				if (_useLevels)
					RequestLevels();
			}

			if (bar < _bars.Count)
				_bars[bar] = new Bar(candle.High, candle.Low, candle.Close);
			else
				_bars.Add(new Bar(candle.High, candle.Low, candle.Close));

			if (bar != CurrentBar - 1)
				return;

			_lastPrice = candle.Close;
			_atr = StopFinder.Atr(_bars, _atrPeriod);
			_swing = StopFinder.Swing(_bars, _swingLookback, SideOf() == TradeSide.Long ? TradeSide.Long : TradeSide.Short);

			Build();
		}

#pragma warning disable CS0672
		protected override void OnFixedProfilesResponse(IndicatorCandle fixedProfileScaled, IndicatorCandle fixedProfileOriginScale, FixedProfilePeriods period)
#pragma warning restore CS0672
		{
			var candle = fixedProfileScaled;

			if (candle == null)
				return;

			var levels = new List<(string, decimal)>(3);

			if (candle.MaxVolumePriceInfo != null && candle.MaxVolumePriceInfo.Price > 0m)
				levels.Add(("POC", candle.MaxVolumePriceInfo.Price));

			if (candle.ValueArea != null && candle.ValueArea.ValueAreaHigh > 0m && candle.ValueArea.ValueAreaLow > 0m)
			{
				levels.Add(("VAH", candle.ValueArea.ValueAreaHigh));
				levels.Add(("VAL", candle.ValueArea.ValueAreaLow));
			}

			// Replaced as a whole: the render thread never walks a list that is being filled.
			lock (_levels)
			{
				_levels.Clear();
				_levels.AddRange(levels);
			}

			RedrawChart();
		}

		protected override void OnRender(RenderContext context, DrawingLayouts layout)
		{
			if (ChartInfo is null || InstrumentInfo is null || !_hasPlan)
				return;

			var region = ChartInfo.PriceChartContainer.Region;

			if (ShowCandidates)
			{
				foreach (var candidate in _candidates)
				{
					if (candidate.Price != _stop.Price)
						DrawLevel(context, region, candidate.Price, CandidatePen, candidate.Label);
				}
			}

			DrawLevel(context, region, _plan.Entry, EntryPen, "Entry");
			DrawLevel(context, region, _stop.Price, StopPen, $"Stop {_stop.Label} · {_plan.Ticks} ticks");

			var side = SideOf();

			for (var r = 1; r <= _targets; r++)
			{
				var target = RiskMath.Target(_plan.Entry, _stop.Price, side, r);
				DrawLevel(context, region, target, TargetPen, $"{r}R");
			}

			if (ShowReadout)
				DrawReadout(context);
		}

		#endregion

		#region Private methods

		private static CrossColor Rgb(int r, int g, int b) => Color.FromArgb(r, g, b).Convert();

		private void RequestLevels()
		{
#pragma warning disable CS0618
			GetFixedProfile(new FixedProfileRequest(FixedProfilePeriods.CurrentDay));
#pragma warning restore CS0618
		}

		private TradeSide SideOf()
		{
			if (_side == SideSource.Short)
				return TradeSide.Short;

			if (_side != SideSource.Position)
				return TradeSide.Long;

			var position = TradingManager?.Position;

			return position != null && position.IsInPosition && position.Volume < 0m
				? TradeSide.Short
				: TradeSide.Long;
		}

		private decimal EntryPrice()
		{
			switch (_entrySource)
			{
				case EntrySource.Manual:
					return _manualEntry;

				case EntrySource.Position:
					var position = TradingManager?.Position;

					return position != null && position.IsInPosition && position.AveragePrice > 0m
						? position.AveragePrice
						: _lastPrice;

				default:
					return _lastPrice;
			}
		}

		/// <summary>
		/// Value of one tick: the instrument of the connection knows it, and the setting is there
		/// for a chart with no trading account behind it.
		/// </summary>
		private decimal TickValue()
		{
			if (_tickCost > 0m)
				return _tickCost;

			var security = TradingManager?.Security;

			return security != null && security.TickCost > 0m ? security.TickCost : 0m;
		}

		/// <summary>
		/// The closed result of the day, negative when money was lost. The platform reports the
		/// closed result of the session, so what the day has done is measured from its first read.
		/// </summary>
		private decimal RealizedToday()
		{
			if (_manualLoss != 0m)
				return _manualLoss;

			var portfolio = TradingManager?.Portfolio;

			if (portfolio == null)
				return 0m;

			var today = MarketTime.Date;

			if (!_dayCaptured || today != _day)
			{
				_day = today;
				_dayStartClosedPnl = portfolio.ClosedPnL;
				_dayCaptured = true;
			}

			return portfolio.ClosedPnL - _dayStartClosedPnl;
		}

		private void Build()
		{
			var tickSize = InstrumentInfo?.TickSize ?? 0m;
			var entry = EntryPrice();
			var side = SideOf();

			if (tickSize <= 0m || entry <= 0m)
			{
				_hasPlan = false;
				return;
			}

			List<(string Label, decimal Price)> levels = null;

			if (_useLevels)
			{
				lock (_levels)
					levels = new List<(string, decimal)>(_levels);
			}

			_candidates.Clear();
			_candidates.AddRange(StopFinder.Build(
				entry,
				side,
				tickSize,
				_atr,
				_atrMultiplier,
				_swing,
				_paddingTicks,
				levels,
				_manualStop));

			if (!StopFinder.TryPick(_candidates, _stopKind, out _stop))
			{
				_hasPlan = false;
				return;
			}

			_plan = RiskMath.Plan(
				entry,
				_stop.Price,
				side,
				tickSize,
				TickValue(),
				_budget,
				_dailyLimit,
				RealizedToday(),
				_maxContracts);

			_hasPlan = true;
		}

		private void DrawLevel(RenderContext context, Rectangle region, decimal price, PenSettings pen, string text)
		{
			if (price <= 0m)
				return;

			var y = ChartInfo.GetYByPrice(price, false);

			if (y < 0 || y > region.Height)
				return;

			context.DrawLine(pen.RenderObject, 0, y, region.Width, y);

			var size = context.MeasureString(text, _font);
			var rect = new Rectangle(region.Width - size.Width - 6, y - size.Height, size.Width + 2, size.Height);

			context.DrawString(text, _font, pen.RenderObject.Color, rect, _rightFormat);
		}

		private void DrawReadout(RenderContext context)
		{
			var lines = new List<string>(3)
			{
				string.Format(
					CultureInfo.InvariantCulture,
					"{0} · stop {1} ticks · {2} per contract",
					SideOf() == TradeSide.Long ? "Long" : "Short",
					_plan.Ticks,
					Money(_plan.RiskPerContract)),
			};

			lines.Add(_plan.Contracts > 0
				? string.Format(CultureInfo.InvariantCulture, "{0} contracts · risk {1}", _plan.Contracts, Money(_plan.Risk))
				: _plan.Blocked
					? "no contracts: the daily limit is spent"
					: "no contracts: the stop does not fit in the risk allowed");

			if (_dailyLimit > 0m)
				lines.Add(string.Format(CultureInfo.InvariantCulture, "left of the day {0}", Money(_plan.BudgetLeft)));

			var color = ReadoutColor.Convert();
			var y = 2;

			foreach (var line in lines)
			{
				var size = context.MeasureString(line, _font);
				context.DrawString(line, _font, color, new Rectangle(4, y, size.Width + 2, size.Height));
				y += size.Height;
			}
		}

		private static string Money(decimal value) => value.ToString("N2", CultureInfo.InvariantCulture);

		#endregion
	}
}
