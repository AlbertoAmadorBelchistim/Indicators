namespace ATAS.Indicators.Technical
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using System.Drawing;
	using System.Globalization;
	using System.IO;

	using ATAS.DataFeedsCore;
	using ATAS.Indicators;
	using ATAS.Indicators.Drawing;
	using ATAS.Indicators.Technical.TradeRecorderCore;

	using OFT.Rendering.Context;
	using OFT.Rendering.Tools;

	using Utils.Common.Logging;

	[Category("Custom")]
	[DisplayName("Trade Recorder")]
	[Description("Records what cannot be rebuilt later around every trade: the book, the spread and the position, from before the entry to after the exit.")]
	public sealed class TradeRecorder : Indicator
	{
		#region Fields

		private static readonly TimeSpan Heartbeat = TimeSpan.FromSeconds(1);

		private readonly object _sync = new();
		private readonly Dictionary<decimal, decimal> _bids = new();
		private readonly Dictionary<decimal, decimal> _asks = new();
		private readonly List<string> _sampleLines = new(512);
		private readonly List<string> _tradeLines = new(8);

		private readonly SampleRing _ring = new(240);
		private readonly CaseState _case = new();

		private RenderFont _font = new("Arial", 10);

		private bool _enabled = true;
		private string _folder = string.Empty;
		private int _secondsBefore = 60;
		private int _secondsAfter = 120;
		private int _intervalMs = 250;
		private int _depthLevels = 5;
		private bool _writeSamples = true;

		private DateTime _lastSample = DateTime.MinValue;
		private decimal _last;
		private decimal _positionVolume;

		private string _caseId = string.Empty;
		private int _caseSamples;
		private decimal _openPrice;
		private decimal _openClosedPnl;
		private bool _timerSubscribed;

		private long _written;
		private string _status = "idle";

		#endregion

		#region Recording

		[Display(Name = "Recording", GroupName = "Recording", Description = "While it is off nothing is written and nothing is kept.", Order = 100)]
		public bool Enabled
		{
			get => _enabled;
			set
			{
				_enabled = value;

				if (!value)
					Reset();

				RedrawChart();
			}
		}

		[Display(Name = "Folder", GroupName = "Recording", Description = "Where the files are written. Empty writes to ATAS's own data folder, under TradeRecorder.", Order = 110)]
		public string Folder
		{
			get => _folder;
			set
			{
				_folder = value ?? string.Empty;
				RedrawChart();
			}
		}

		[Display(Name = "Seconds before", GroupName = "Recording", Description = "What is kept from before the entry. It can only be had by recording all the time, so the window runs from the moment the indicator loads.", Order = 120)]
		[Range(0, 3600)]
		public int SecondsBefore
		{
			get => _secondsBefore;
			set
			{
				_secondsBefore = value;
				ResizeRing();
			}
		}

		[Display(Name = "Seconds after", GroupName = "Recording", Description = "What is recorded after the position goes flat, to see whether the exit was a good one.", Order = 130)]
		[Range(0, 3600)]
		public int SecondsAfter
		{
			get => _secondsAfter;
			set
			{
				_secondsAfter = value;
				_case.TailSeconds = value;
			}
		}

		[Display(Name = "Milliseconds between samples", GroupName = "Recording", Description = "A sample is taken when the book or the tape move, and never faster than this.", Order = 140)]
		[Range(20, 5000)]
		public int IntervalMs
		{
			get => _intervalMs;
			set
			{
				_intervalMs = value;
				ResizeRing();
			}
		}

		[Display(Name = "Depth, levels", GroupName = "Recording", Description = "Levels of each side added up into the sample.", Order = 150)]
		[Range(1, 50)]
		public int DepthLevels
		{
			get => _depthLevels;
			set
			{
				_depthLevels = value;
				RedrawChart();
			}
		}

		[Display(Name = "Write the samples", GroupName = "Recording", Description = "Off, only one line per trade is written and the book is not kept.", Order = 160)]
		public bool WriteSamples
		{
			get => _writeSamples;
			set
			{
				_writeSamples = value;
				RedrawChart();
			}
		}

		#endregion

		#region Visualization

		[Display(Name = "Status", GroupName = "Visualization", Order = 200)]
		public bool ShowStatus { get; set; } = true;

		[Display(Name = "Status text", GroupName = "Visualization", Order = 210)]
		public CrossColor StatusColor { get; set; } = DefaultColors.Gray.Convert();

		[Display(Name = "Font size", GroupName = "Visualization", Order = 220)]
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

		public TradeRecorder()
			: base(useCandles: true)
		{
			DenyToChangePanel = true;
			DrawAbovePrice = true;
			EnableCustomDrawing = true;
			SubscribeToDrawingEvents(DrawingLayouts.Final);

			DataSeries[0].IsHidden = true;
			((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

			_case.TailSeconds = _secondsAfter;
		}

		#endregion

		#region Protected methods

		protected override void OnInitialize()
		{
			this.LogInfo($"TradeRecorder: initialized ({typeof(TradeRecorder).Assembly.GetName().Version}).");
			ResizeRing();

			SubscribeToTimer(Heartbeat, OnHeartbeat);
			_timerSubscribed = true;
		}

		protected override void OnDispose()
		{
			if (_timerSubscribed)
			{
				UnsubscribeFromTimer(Heartbeat, OnHeartbeat);
				_timerSubscribed = false;
			}

			Flush();
			base.OnDispose();
		}

		protected override void OnCalculate(int bar, decimal value)
		{
			if (bar == CurrentBar - 1)
				_last = GetCandle(bar).Close;
		}

		protected override void OnNewTrade(MarketDataArg trade)
		{
			if (trade == null)
				return;

			_last = trade.Price;
			Capture();
		}

		protected override void OnBestBidAskChanged(MarketDataArg depth)
		{
			Apply(depth);
			Capture();
		}

		protected override void MarketDepthChanged(MarketDataArg depth)
		{
			Apply(depth);
			Capture();
		}

		protected override void OnPositionChanged(Position position)
		{
			if (!IsChartPosition(position))
				return;

			var volume = position.IsInPosition ? position.Volume : 0m;

			UpdateCase(volume, DateTime.UtcNow, position);
		}

		protected override void OnRender(RenderContext context, DrawingLayouts layout)
		{
			if (!ShowStatus || ChartInfo is null)
				return;

			string status;

			lock (_sync)
				status = _status;

			long written;

			lock (_sync)
				written = _written;

			var text = "Trade Recorder · " + status;

			if (written > 0)
				text += " · " + written.ToString("N0", CultureInfo.InvariantCulture) + " samples written";
			var size = context.MeasureString(text, _font);

			context.DrawString(text, _font, StatusColor.Convert(), new Rectangle(4, 2, size.Width + 2, size.Height));
		}

		#endregion

		#region Private methods

		private void ResizeRing()
		{
			var interval = Math.Max(_intervalMs, 20);
			var capacity = (int)Math.Ceiling(_secondsBefore * 1000d / interval) + 8;

			lock (_sync)
				_ring.Resize(capacity);
		}

		private void Reset()
		{
			lock (_sync)
			{
				_ring.Clear();
				_case.Reset();
				_sampleLines.Clear();
				_caseId = string.Empty;
				_caseSamples = 0;
				_status = "idle";
			}
		}

		private bool IsChartPosition(Position position)
		{
			var portfolio = TradingManager?.Portfolio;
			var security = TradingManager?.Security;

			return position != null && portfolio != null && security != null
				&& string.Equals(position.AccountID, portfolio.AccountID, StringComparison.Ordinal)
				&& position.Security != null
				&& string.Equals(position.Security.Code, security.Code, StringComparison.Ordinal);
		}

		private void Apply(MarketDataArg depth)
		{
			if (depth == null)
				return;

			var side = depth.DataType == ATAS.Indicators.MarketDataType.Bid ? _bids : depth.DataType == ATAS.Indicators.MarketDataType.Ask ? _asks : null;

			if (side == null)
				return;

			lock (_sync)
			{
				if (depth.Volume <= 0m)
					side.Remove(depth.Price);
				else
					side[depth.Price] = depth.Volume;
			}
		}

		/// <summary>
		/// Takes a sample when the book or the tape moved, never faster than the interval. This runs
		/// on the data threads, so it only touches the state under the lock and never the chart.
		/// </summary>
		private void Capture()
		{
			if (!_enabled)
				return;

			var now = DateTime.UtcNow;

			lock (_sync)
			{
				if ((now - _lastSample).TotalMilliseconds < _intervalMs)
					return;

				_lastSample = now;

				var sample = Build(now);
				_ring.Add(sample);

				if (_case.Phase != CasePhase.Idle && _writeSamples && _caseId.Length > 0)
				{
					_sampleLines.Add(JsonLines.Sample(_caseId, InstrumentCode(), sample));
					_caseSamples++;
				}
			}
		}

		/// <summary>Caller holds the lock.</summary>
		private Sample Build(DateTime now)
		{
			var bid = 0m;
			var bidSize = 0m;
			var bidDepth = 0m;

			foreach (var level in Top(_bids, descending: true, out bidDepth))
			{
				bid = level.Key;
				bidSize = level.Value;
				break;
			}

			var ask = 0m;
			var askSize = 0m;
			var askDepth = 0m;

			foreach (var level in Top(_asks, descending: false, out askDepth))
			{
				ask = level.Key;
				askSize = level.Value;
				break;
			}

			return new Sample(now, _last, bid, ask, bidSize, askSize, bidDepth, askDepth, _positionVolume);
		}

		/// <summary>
		/// The best levels of one side, and the volume resting on them. Caller holds the lock.
		/// </summary>
		private List<KeyValuePair<decimal, decimal>> Top(Dictionary<decimal, decimal> side, bool descending, out decimal depth)
		{
			var levels = new List<KeyValuePair<decimal, decimal>>(side);

			levels.Sort((a, b) => descending ? b.Key.CompareTo(a.Key) : a.Key.CompareTo(b.Key));

			var count = Math.Min(_depthLevels, levels.Count);
			depth = 0m;

			for (var i = 0; i < count; i++)
				depth += levels[i].Value;

			if (levels.Count > count)
				levels.RemoveRange(count, levels.Count - count);

			return levels;
		}

		private void UpdateCase(decimal volume, DateTime now, Position position)
		{
			if (!_enabled)
				return;

			var instrument = InstrumentCode();
			string message = null;

			lock (_sync)
			{
				_positionVolume = volume;
				var change = _case.Update(volume, now);

				switch (change)
				{
					case CaseEvent.Opened:
						_caseId = JsonLines.CaseId(instrument, _case.OpenedUtc);
						_caseSamples = 0;
						_openPrice = position?.AveragePrice ?? _last;
						_openClosedPnl = TradingManager?.Portfolio?.ClosedPnL ?? 0m;

						// Everything the rolling window kept from before the entry belongs to the case.
						if (_writeSamples)
						{
							foreach (var sample in _ring.Since(_case.OpenedUtc.AddSeconds(-_secondsBefore)))
							{
								_sampleLines.Add(JsonLines.Sample(_caseId, instrument, sample));
								_caseSamples++;
							}
						}

						_status = $"recording {_caseId}";
						message = $"TradeRecorder: case {_caseId} opened, {_caseSamples} samples from before the entry.";
						break;

					case CaseEvent.Closed:
						_status = $"after the exit {_caseId}";
						break;

					case CaseEvent.Finished:
						message = CloseCase(instrument);
						break;
				}
			}

			if (message != null)
				this.LogInfo(message);

			RedrawChart();
		}

		/// <summary>Caller holds the lock. Writes the line of the trade and leaves the case behind.</summary>
		private string CloseCase(string instrument)
		{
			var closedPnl = TradingManager?.Portfolio?.ClosedPnL ?? 0m;
			var account = TradingManager?.Portfolio?.AccountID ?? string.Empty;

			_tradeLines.Add(JsonLines.Trade(
				_caseId,
				instrument,
				account,
				_case.Side == 0 ? 1 : _case.Side,
				_case.PeakVolume,
				_case.OpenedUtc,
				_case.ClosedUtc,
				_openPrice,
				_last,
				closedPnl - _openClosedPnl,
				_caseSamples));

			var message = $"TradeRecorder: case {_caseId} written, {_caseSamples} samples.";

			_caseId = string.Empty;
			_caseSamples = 0;
			_status = "idle";

			return message;
		}

		private void OnHeartbeat()
		{
			if (!_enabled)
				return;

			// A quiet market still deserves a row, and the case has to end even with nothing moving.
			Capture();

			var position = TradingManager?.Position;

			if (_case.Phase == CasePhase.Tail)
				UpdateCase(position != null && position.IsInPosition ? position.Volume : 0m, DateTime.UtcNow, position);

			Flush();
		}

		/// <summary>Writes what is buffered. One call per second, never from the chart threads.</summary>
		private void Flush()
		{
			List<string> samples = null;
			List<string> trades = null;

			lock (_sync)
			{
				if (_sampleLines.Count > 0)
				{
					samples = new List<string>(_sampleLines);
					_sampleLines.Clear();
				}

				if (_tradeLines.Count > 0)
				{
					trades = new List<string>(_tradeLines);
					_tradeLines.Clear();
				}
			}

			if (samples == null && trades == null)
				return;

			try
			{
				var folder = ResolveFolder();
				Directory.CreateDirectory(folder);

				var month = DateTime.UtcNow.ToString("yyyyMM", CultureInfo.InvariantCulture);

				if (samples != null)
				{
					File.AppendAllLines(Path.Combine(folder, $"samples-{month}.jsonl"), samples);

					lock (_sync)
						_written += samples.Count;
				}

				if (trades != null)
					File.AppendAllLines(Path.Combine(folder, $"trades-{month}.jsonl"), trades);
			}
			catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException)
			{
				lock (_sync)
					_status = "cannot write: " + e.Message;

				this.LogError("TradeRecorder: writing failed: " + e.Message);
			}
		}

		private string ResolveFolder()
		{
			if (_folder.Length > 0)
				return _folder;

			return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				"ATAS",
				"TradeRecorder");
		}

		private string InstrumentCode() => TradingManager?.Security?.Code ?? InstrumentInfo?.Instrument ?? "unknown";

		#endregion
	}
}
