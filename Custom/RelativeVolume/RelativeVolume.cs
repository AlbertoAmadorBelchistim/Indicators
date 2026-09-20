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
	using ATAS.Indicators.Technical.RelativeVolumeCore;

	using OFT.Rendering.Context;
	using OFT.Rendering.Settings;
	using OFT.Rendering.Tools;

	using Utils.Common.Logging;

	[Category("Custom")]
	[DisplayName("Relative Volume")]
	[Description("Today's activity against the same time of day of the past sessions.")]
	public sealed class RelativeVolume : Indicator
	{
		#region Nested types

		/// <summary>One bar of the session in progress, waiting to find out whether the session counts.</summary>
		private readonly struct PendingBar
		{
			public PendingBar(int minutesFromOpen, decimal cumulativeVolume, decimal volume, decimal range)
			{
				MinutesFromOpen = minutesFromOpen;
				CumulativeVolume = cumulativeVolume;
				Volume = volume;
				Range = range;
			}

			public int MinutesFromOpen { get; }

			public decimal CumulativeVolume { get; }

			public decimal Volume { get; }

			public decimal Range { get; }
		}

		#endregion

		#region Fields

		private readonly RvolEngine _engine = new();
		private readonly List<PendingBar> _pending = new(1024);
		private readonly List<int> _durations = new(64);

		private readonly ValueDataSeries _ratio = new("Ratio", "Relative volume")
		{
			VisualType = VisualMode.Histogram,
			ShowZeroValue = false,
			Width = 2,
		};

		private readonly ValueDataSeries _low = new("Low", "Usual low")
		{
			VisualType = VisualMode.Line,
			ShowZeroValue = false,
			Color = DefaultColors.Gray.Convert(),
			Width = 1,
			IgnoredByAlerts = true,
		};

		private readonly ValueDataSeries _high = new("High", "Usual high")
		{
			VisualType = VisualMode.Line,
			ShowZeroValue = false,
			Color = DefaultColors.Gray.Convert(),
			Width = 1,
			IgnoredByAlerts = true,
		};

		private readonly LineSeries _one = new("One", "Usual")
		{
			Color = DefaultColors.Gray.Convert(),
			LineDashStyle = LineDashStyle.Dash,
			Value = 1,
			Width = 1,
		};

		private RenderFont _font = new("Arial", 10);

		private RvolMetric _metric = RvolMetric.CumulativeVolume;
		private decimal _minSessionFraction = 0.6m;
		private bool _prorateCurrentBar = true;

		// State of the pass over the bars.
		private DateTime _sessionOpen;
		private decimal _sessionVolume;
		private int _lastBar = -1;
		private decimal _barMinutes = 1m;

		// What the last bar computed, for the readout.
		private decimal _lastRatio;
		private decimal _lastValue;
		private decimal _lastReference;
		private int _lastSessions;

		#endregion

		#region Reference

		[Display(Name = "Compare", GroupName = "Reference", Description = "Volume since the open is the steady read; volume or range of the bar is what is happening right now.", Order = 100)]
		public RvolMetric Metric
		{
			get => _metric;
			set
			{
				_metric = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Sessions", GroupName = "Reference", Description = "Past sessions the reference is built from. Only the most recent ones: the market of six months ago is another market.", Order = 110)]
		[Range(2, 250)]
		public int Sessions
		{
			get => _engine.MaxSessions;
			set
			{
				_engine.MaxSessions = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Minimum sessions", GroupName = "Reference", Description = "Below this, nothing is drawn for that moment of the day. A ratio against three sessions is noise with a number on it.", Order = 120)]
		[Range(1, 250)]
		public int MinSessions
		{
			get => _engine.MinSessions;
			set
			{
				_engine.MinSessions = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Band, lower %", GroupName = "Reference", Description = "Percentile drawn under the usual value.", Order = 130)]
		[Range(1, 49)]
		public decimal LowPercentile
		{
			get => _engine.LowPercentile;
			set
			{
				_engine.LowPercentile = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Band, upper %", GroupName = "Reference", Description = "Percentile drawn over the usual value.", Order = 140)]
		[Range(51, 99)]
		public decimal HighPercentile
		{
			get => _engine.HighPercentile;
			set
			{
				_engine.HighPercentile = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Shortest session, fraction", GroupName = "Reference", Description = "A session shorter than this fraction of the usual one is left out: a half day around a holiday would drag the reference down all afternoon. Zero keeps them all.", Order = 150)]
		[Range(0, 1)]
		public decimal MinSessionFraction
		{
			get => _minSessionFraction;
			set
			{
				_minSessionFraction = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Scale the bar in progress", GroupName = "Reference", Description = "The bar that is still forming has traded only part of what it will: it is compared against the same part of the reference.", Order = 160)]
		public bool ProrateCurrentBar
		{
			get => _prorateCurrentBar;
			set
			{
				_prorateCurrentBar = value;
				RecalculateValues();
			}
		}

		#endregion

		#region Visualization

		[Display(Name = "Usual", GroupName = "Visualization", Description = "Between the two bands: an ordinary moment of the day.", Order = 200)]
		public CrossColor NormalColor { get; set; } = DefaultColors.Gray.Convert();

		[Display(Name = "Above the band", GroupName = "Visualization", Order = 210)]
		public CrossColor HighColor { get; set; } = Rgb(0x26, 0xA6, 0x9A);

		[Display(Name = "Below the band", GroupName = "Visualization", Order = 220)]
		public CrossColor LowColor { get; set; } = Rgb(0xEF, 0x53, 0x50);

		[Display(Name = "Bands", GroupName = "Visualization", Description = "The usual spread of this moment of the day, as a ratio to its middle.", Order = 230)]
		public bool ShowBands
		{
			get => _showBands;
			set
			{
				_showBands = value;
				RecalculateValues();
			}
		}

		[Display(Name = "Readout", GroupName = "Visualization", Description = "The ratio, the two raw numbers behind it and how many sessions it was measured over.", Order = 240)]
		public bool ShowReadout { get; set; } = true;

		[Display(Name = "Readout text", GroupName = "Visualization", Order = 250)]
		public CrossColor ReadoutColor { get; set; } = DefaultColors.Gray.Convert();

		[Display(Name = "Font size", GroupName = "Visualization", Order = 260)]
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

		public RelativeVolume()
			: base(useCandles: true)
		{
			Panel = IndicatorDataProvider.NewPanel;
			DenyToChangePanel = false;
			EnableCustomDrawing = true;
			SubscribeToDrawingEvents(DrawingLayouts.Final);

			DataSeries[0] = _ratio;
			DataSeries.Add(_low);
			DataSeries.Add(_high);
			LineSeries.Add(_one);
		}

		#endregion

		#region Protected methods

		protected override void OnInitialize()
		{
			this.LogInfo($"RelativeVolume: initialized ({typeof(RelativeVolume).Assembly.GetName().Version}).");
		}

		protected override void OnRecalculate()
		{
			_engine.Clear();
			_pending.Clear();
			_durations.Clear();
			_sessionVolume = 0m;
			_lastBar = -1;
			_lastRatio = 0m;
			_lastValue = 0m;
			_lastReference = 0m;
			_lastSessions = 0;
		}

		protected override void OnCalculate(int bar, decimal value)
		{
			var candle = GetCandle(bar);

			if (bar == 0)
			{
				StartSession(candle.Time);
				_barMinutes = 1m;
			}
			else if (IsNewSession(bar))
			{
				// The session that just ended goes into the reference, if it was a full one.
				FlushSession();
				StartSession(candle.Time);
			}
			else if (bar != _lastBar)
			{
				// The distance between two bars, which is what a bar in progress is measured against.
				var minutes = (decimal)(candle.Time - GetCandle(bar - 1).Time).TotalMinutes;

				if (minutes > 0m && minutes < 24m * 60m)
					_barMinutes = minutes;
			}

			if (bar != _lastBar)
				_sessionVolume = 0m;

			_sessionVolume += candle.Volume;
			_lastBar = bar;

			var minutesFromOpen = (int)Math.Floor((candle.Time - _sessionOpen).TotalMinutes);

			if (minutesFromOpen < 0)
				minutesFromOpen = 0;

			// The reference holds the sessions before this one, so what is drawn on a bar never
			// knows anything that had not happened yet.
			Draw(bar, minutesFromOpen, candle);

			_pending.Add(new PendingBar(minutesFromOpen, _sessionVolume, candle.Volume, candle.High - candle.Low));
		}

		protected override void OnRender(RenderContext context, DrawingLayouts layout)
		{
			if (!ShowReadout || ChartInfo is null || _lastSessions == 0)
				return;

			var text = string.Format(
				CultureInfo.InvariantCulture,
				"RVOL {0:0.00} · {1} vs {2} (n={3})",
				_lastRatio,
				Compact(_lastValue),
				Compact(_lastReference),
				_lastSessions);

			var size = context.MeasureString(text, _font);
			context.DrawString(text, _font, ReadoutColor.Convert(), new Rectangle(4, 2, size.Width + 2, size.Height));
		}

		#endregion

		#region Private methods

		private bool _showBands = true;

		private static CrossColor Rgb(int r, int g, int b) => Color.FromArgb(r, g, b).Convert();

		private void StartSession(DateTime open)
		{
			_sessionOpen = open;
			_sessionVolume = 0m;
			_pending.Clear();
		}

		/// <summary>
		/// Moves the session that has just ended into the reference, unless it was too short to be
		/// one: a half day would drag the middle down at every moment of the afternoon.
		/// </summary>
		private void FlushSession()
		{
			if (_pending.Count == 0)
				return;

			var duration = _pending[_pending.Count - 1].MinutesFromOpen;

			if (IsFullSession(duration))
			{
				foreach (var pending in _pending)
					_engine.Add(pending.MinutesFromOpen, pending.CumulativeVolume, pending.Volume, pending.Range);

				_durations.Add(duration);
			}

			_pending.Clear();
		}

		private bool IsFullSession(int duration)
		{
			if (_minSessionFraction <= 0m || _durations.Count == 0)
				return true;

			var sorted = new List<int>(_durations);
			sorted.Sort();

			return duration >= sorted[sorted.Count / 2] * _minSessionFraction;
		}

		private void Draw(int bar, int minutesFromOpen, IndicatorCandle candle)
		{
			var stats = _engine.Get(_metric, minutesFromOpen);

			if (stats.IsEmpty)
			{
				_ratio[bar] = 0m;
				_low[bar] = 0m;
				_high[bar] = 0m;
				return;
			}

			var current = _metric switch
			{
				RvolMetric.BarVolume => candle.Volume,
				RvolMetric.BarRange => candle.High - candle.Low,
				_ => _sessionVolume,
			};

			// The last bar is still forming: it has traded only part of what it will.
			if (_prorateCurrentBar && bar == CurrentBar - 1 && _metric != RvolMetric.CumulativeVolume)
				stats = stats.Prorate(ElapsedFraction(candle));

			var ratio = RvolEngine.Ratio(current, stats.Median);
			_ratio[bar] = ratio;

			var lowRatio = RvolEngine.Ratio(stats.Low, stats.Median);
			var highRatio = RvolEngine.Ratio(stats.High, stats.Median);

			_low[bar] = _showBands ? lowRatio : 0m;
			_high[bar] = _showBands ? highRatio : 0m;

			var color = ratio > highRatio
				? HighColor
				: ratio < lowRatio ? LowColor : NormalColor;

			_ratio.Colors[bar] = color.Convert();

			if (bar != CurrentBar - 1)
				return;

			_lastRatio = ratio;
			_lastValue = current;
			_lastReference = stats.Median;
			_lastSessions = stats.Sessions;
		}

		/// <summary>How much of the bar in progress has already happened.</summary>
		private decimal ElapsedFraction(IndicatorCandle candle)
		{
			if (_barMinutes <= 0m)
				return 1m;

			var elapsed = (decimal)(MarketTime - candle.Time).TotalMinutes;

			if (elapsed <= 0m)
				return 0m;

			var fraction = elapsed / _barMinutes;

			return fraction > 1m ? 1m : fraction;
		}

		private static string Compact(decimal value)
		{
			if (value >= 1000000m)
				return (value / 1000000m).ToString("0.#", CultureInfo.InvariantCulture) + "M";

			return value >= 1000m
				? (value / 1000m).ToString("0.#", CultureInfo.InvariantCulture) + "k"
				: value.ToString("0.##", CultureInfo.InvariantCulture);
		}

		#endregion
	}
}
