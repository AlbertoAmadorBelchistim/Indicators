using ATAS.Indicators;
using OFT.Rendering.Context;
using OFT.Rendering.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Utils.Common.Logging;

namespace ATAS.Indicators.Technical
{
	[Category("Custom")]
	[DisplayName("MenthorQLevels")]
	public sealed class MenthorQLevels : Indicator
	{
		#region Nested types: model

		internal enum LevelCategory
		{
			// Dense enum (no gaps) so it can be used as an index into pen caches
			// later. `Other` stays at 100 as a fallback bucket — renderer guards
			// bounds when it sees it.
			CallResistance = 0,   // CR
			PutSupport = 1,   // PS
			HighVolatilityLevel = 2,   // HVL
			GammaExposure = 3,   // GEX (ranked: rank 1 strongest, higher = weaker)
			GammaWall = 4,   // GW  — pivot / magnet (max total gamma, single per context)
			BlindSpot = 5,   // BL (ranked)
			DayMin = 6,   // 1D Min
			DayMax = 7,   // 1D Max
			RiskTrigger = 8,   // RT MM-DD
			LowerBand = 9,   // LB MM-DD
			UpperBand = 10,  // UB MM-DD
			Swing = 11,  // generic swing levels not matching UB/LB/RT

			Other = 100
		}

		internal readonly struct LevelLabel
		{
			public readonly LevelCategory Category;

			// Rank is 0 for flagship levels (CallWall, PutWall, HVL, Gamma Wall,
			// 1D Min/Max). For numbered families like GEX N / BL N, a lower
			// number means a stronger level (rank 1 is the strongest). Used by
			// the renderer to map to tiers (Thick/Medium/Thin).
			public readonly int Rank;

			public readonly bool Is0Dte;

			// Raw text exactly as it came from the source (API response or paste).
			// Kept for logging and debug overlay.
			public readonly string RawLabel;

			// Canonical display text the renderer draws on chart
			// (e.g., "CR", "PS", "GEX 3", "BL 4", "UB 01-13", "GW").
			public readonly string DisplayLabel;

			public LevelLabel(LevelCategory category, int rank, bool is0Dte, string rawLabel, string displayLabel)
			{
				Category = category;
				Rank = rank;
				Is0Dte = is0Dte;
				RawLabel = rawLabel ?? string.Empty;
				DisplayLabel = displayLabel ?? string.Empty;
			}
		}

		internal readonly struct ParsedEntry
		{
			public readonly decimal Price;

			// One entry may carry several labels when the same price is tagged
			// by more than one concept within the same source (rare, but e.g.
			// "Call Resistance 0DTE / Gamma Wall 0DTE" at the same price).
			public readonly LevelLabel[] Labels;

			// Stable identifier used for logging and debug (e.g. "MenthorQ:Api",
			// "MenthorQ:Text:Index", "MenthorQ:Text:Futures").
			public readonly string SourceId;

			// Source ticker as parsed from the feed (e.g. "SPX", "ES", "QQQ"),
			// or empty when the source did not carry one. Consumed by the
			// renderer to annotate labels when the source ticker differs from
			// the chart instrument (BuildLevels compares against ResolveTicker).
			public readonly string SourceTicker;

			// Pre-transform price, set only when the source applied a multiplier
			// and/or offset different from identity. Null when no transform was
			// applied. Lets the renderer surface "(SPX 5870.50)" alongside the
			// transformed chart price.
			public readonly decimal? OriginalPrice;

			public ParsedEntry(decimal price, LevelLabel[] labels, string sourceId,
							   string sourceTicker = null, decimal? originalPrice = null)
			{
				Price = price;
				Labels = labels ?? Array.Empty<LevelLabel>();
				SourceId = sourceId ?? string.Empty;
				SourceTicker = sourceTicker ?? string.Empty;
				OriginalPrice = originalPrice;
			}
		}

		internal readonly struct Level
		{
			public readonly decimal Price;

			// All labels that resolved to this price after dedup / conflict
			// resolution within the active source.
			public readonly LevelLabel[] Labels;

			// The label chosen to drive the visual style (category / tier / 0DTE).
			public readonly LevelLabel Winner;

			// Pre-built display text (e.g. "CW / LG2"). Computed once at build
			// time so OnRender stays allocation-free.
			public readonly string DisplayText;

			// Optional parenthetical appended to DisplayText at draw time when
			// the source ticker differs from the chart instrument (e.g.
			// "  (SPX 5870.50)" or "  (ES)" when no transform was applied).
			// Empty when the source matches the chart, when the source has no
			// ticker, or when no chart ticker is resolvable yet. Kept separate
			// from DisplayText so alerts (which reuse DisplayText verbatim) stay
			// free of the suffix.
			public readonly string TickerSuffix;

			public Level(decimal price, LevelLabel[] labels, LevelLabel winner,
						 string displayText, string tickerSuffix = null)
			{
				Price = price;
				Labels = labels ?? Array.Empty<LevelLabel>();
				Winner = winner;
				DisplayText = displayText ?? string.Empty;
				TickerSuffix = tickerSuffix ?? string.Empty;
			}
		}

		internal enum RenderTier
		{
			Thick = 0,   // flagship walls (GW, CR, PS, HVL)
			Medium = 1,  // date-anchored bands/triggers, top-rank GEX/BL
			Thin = 2     // everything weaker
		}

		#endregion

		#region Nested types: sources

		internal interface ILevelsSource
		{
			// Stable id for logging / debug (e.g., "MenthorQ:Text:Index").
			string SourceId { get; }

			// Whether the source contributes entries.
			bool IsEnabled { get; }

			// Fetch entries produced by the source. Parsing warnings are returned
			// so the indicator can log or surface them later.
			bool TryGetEntries(out ParsedEntry[] entries, out string[] warnings);
		}

		private sealed class ManualTextSource : ILevelsSource
		{
			private readonly MenthorQLevels _owner;

			public ManualTextSource(MenthorQLevels owner)
			{
				_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			}

			public string SourceId => "MenthorQ:Text";

			public bool IsEnabled => _owner.EnableManualText;

			public bool TryGetEntries(out ParsedEntry[] entries, out string[] warnings)
			{
				if (!IsEnabled)
				{
					entries = Array.Empty<ParsedEntry>();
					warnings = Array.Empty<string>();
					return true;
				}

				var rawIndex = _owner.IndexTextRaw;
				var rawFut = _owner.FuturesTextRaw;

				if (string.IsNullOrWhiteSpace(rawIndex) && string.IsNullOrWhiteSpace(rawFut))
				{
					entries = Array.Empty<ParsedEntry>();
					warnings = Array.Empty<string>();
					return true;
				}

				var allEntries = new List<ParsedEntry>(64);
				var allWarnings = new List<string>(16);

				if (!string.IsNullOrWhiteSpace(rawIndex))
				{
					var r = ParseMenthorQText(
						rawIndex,
						_owner.TextMultiplier,
						_owner.TextOffset,
						applyTransform: true,
				sourceIdBase: "MenthorQ:Text:Index");

					if (r.Entries.Length > 0)
						allEntries.AddRange(r.Entries);

					if (r.Warnings.Length > 0)
						allWarnings.AddRange(r.Warnings);
				}

				if (!string.IsNullOrWhiteSpace(rawFut))
				{
					var r = ParseMenthorQText(
						rawFut,
						_owner.TextOffset,
						_owner.TextOffset,
						applyTransform: false,
						sourceIdBase: "MenthorQ:Text:Futures");

					if (r.Entries.Length > 0)
						allEntries.AddRange(r.Entries);

					if (r.Warnings.Length > 0)
						allWarnings.AddRange(r.Warnings);
				}

				entries = allEntries.Count == 0 ? Array.Empty<ParsedEntry>() : allEntries.ToArray();
				warnings = allWarnings.Count == 0 ? Array.Empty<string>() : allWarnings.ToArray();
				return true;
			}
		}

		private sealed class ApiSource : ILevelsSource
		{
			private readonly MenthorQLevels _owner;
			private ParsedEntry[] _entries = Array.Empty<ParsedEntry>();

			public ApiSource(MenthorQLevels owner)
			{
				_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			}

			public string SourceId => "MenthorQ:Api";

			// Self-gating: inactive while the user hasn't filled key + userId OR while
			// the last fetch returned nothing. The engine commit will add explicit
			// priority logic on top of this; for now this cleanly falls back to text.
			public bool IsEnabled =>
				!string.IsNullOrEmpty(_owner._apiKey)
				&& !string.IsNullOrEmpty(_owner._userId)
				&& _entries.Length > 0;

			public bool TryGetEntries(out ParsedEntry[] entries, out string[] warnings)
			{
				warnings = Array.Empty<string>();

				var multiplier = _owner._apiMultiplier;
				var offset = _owner._apiOffset;

				// Fast path — no transform, hand the raw array back by reference.
				if (multiplier == 1m && offset == 0m)
				{
					entries = _entries;
					return true;
				}

				// Slow path — allocate a transformed copy. Runs at engine cadence
				// (RebuildLevelsIfNeeded), not per-render frame, so allocation is rare.
				// Preserve any SourceTicker the upstream fetch set, and stamp the
				// pre-transform price as OriginalPrice so the renderer can surface
				// "(SPX 5870.50)" alongside the transformed chart price.
				var result = new ParsedEntry[_entries.Length];
				for (int i = 0; i < _entries.Length; i++)
				{
					var raw = _entries[i];
					var transformed = (raw.Price * multiplier) + offset;
					result[i] = new ParsedEntry(
						transformed,
						raw.Labels,
						raw.SourceId,
						sourceTicker: raw.SourceTicker,
						originalPrice: raw.Price);
				}
				entries = result;
				return true;
			}

			public void SetEntries(ParsedEntry[] entries)
			{
				_entries = entries ?? Array.Empty<ParsedEntry>();
			}

			public void Clear() => _entries = Array.Empty<ParsedEntry>();
		}

		#endregion

		#region Nested types: parsing

		internal readonly struct ParseResult
		{
			public readonly ParsedEntry[] Entries;
			public readonly string[] Warnings;

			public ParseResult(ParsedEntry[] entries, string[] warnings)
			{
				Entries = entries ?? Array.Empty<ParsedEntry>();
				Warnings = warnings ?? Array.Empty<string>();
			}
		}

		private readonly struct MappedLabel
		{
			public MappedLabel(string displayLabel, LevelCategory category, int rank, bool is0Dte)
			{
				DisplayLabel = displayLabel;
				Category = category;
				Rank = rank;
				Is0Dte = is0Dte;
			}

			public string DisplayLabel { get; }
			public LevelCategory Category { get; }
			public int Rank { get; }
			public bool Is0Dte { get; }
		}

		#endregion

		#region Nested types: API

		// Six types taken from the decompiled reference. The full set supported
		// by the real API lands in commit 5.
		[JsonConverter(typeof(JsonStringEnumConverter))]
		internal enum LevelType
		{
			blindspots,
			swing_levels,
			gamma_levels,
			gamma_scalping,
			gamma_levels_intraday,
			gamma_scalping_intraday
		}

		private sealed class MqLevelValueDto
		{
			[JsonPropertyName("name")]
			public string Name { get; set; }

			[JsonPropertyName("value")]
			public double Value { get; set; }
		}

		private sealed class MqLevelDto
		{
			[JsonPropertyName("level_type")]
			public LevelType LevelType { get; set; }

			[JsonPropertyName("date")]
			public DateTime Date { get; set; }

			[JsonPropertyName("level_values")]
			public List<MqLevelValueDto> LevelValues { get; set; }
		}

		private sealed class MqNotExistingLevelDto
		{
			[JsonPropertyName("level_type")]
			public LevelType LevelType { get; set; }
		}

		private sealed class MqLevelsDto
		{
			[JsonPropertyName("ticker")]
			public string Ticker { get; set; }

			[JsonPropertyName("ticker_mq")]
			public string TickerMq { get; set; }

			[JsonPropertyName("levels")]
			public List<MqLevelDto> Levels { get; set; }

			[JsonPropertyName("not_existing_levels")]
			public List<MqNotExistingLevelDto> NotExistingLevels { get; set; }
		}

		private sealed class MqApi
		{
			private const string ApiUrl = "https://api.menthorq.io/getDailyLevels";
			private const int DefaultRetryDelayMs = 1000;

			private readonly HttpClient _httpClient;
			private readonly string _apiKey;

			private string _status;
			private DateTime? _lastApiCall;

			public string Status => _status;
			public DateTime? LastApiCall => _lastApiCall;

			public MqApi(string apiKey)
			{
				_apiKey = apiKey ?? string.Empty;
				_httpClient = new HttpClient();

				if (!string.IsNullOrEmpty(_apiKey))
					_httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
			}

			public async Task<MqLevelsDto> GetLevelsAsync(string ticker, LevelType[] levelTypes, string userId)
			{
				if (string.IsNullOrEmpty(_apiKey))
				{
					_status = "API Key not set";
					return null;
				}

				if (string.IsNullOrEmpty(userId))
				{
					_status = "User ID (email) not set";
					return null;
				}

				if (string.IsNullOrEmpty(ticker))
				{
					_status = "Ticker not available";
					return null;
				}

				var levelTypeString = string.Join(",", levelTypes);
				var queryString =
					"?platform=atas" +
					$"&ticker={ticker}" +
					$"&level_type={levelTypeString}" +
					$"&user_id={Uri.EscapeDataString(userId)}";

				var requestUrl = ApiUrl + queryString;

				try
				{
					HttpResponseMessage response;
					do
					{
						_lastApiCall = DateTime.Now;
						response = await _httpClient.GetAsync(requestUrl).ConfigureAwait(false);
						_status = response.StatusCode.ToString();

						if (response.StatusCode != HttpStatusCode.TooManyRequests)
							break;

						var delayMs = DefaultRetryDelayMs;
						if (response.Headers.TryGetValues("Retry-After", out var values)
							&& int.TryParse(values.FirstOrDefault(), out var retryAfterSeconds)
							&& retryAfterSeconds > 0)
						{
							delayMs = retryAfterSeconds * 1000;
						}

						await Task.Delay(delayMs).ConfigureAwait(false);
					}
					while (true);

					if (!response.IsSuccessStatusCode)
						return null;

					var jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					return JsonSerializer.Deserialize<MqLevelsDto>(jsonResponse);
				}
				catch (Exception ex)
				{
					_status = "Error: " + ex.Message;
					return null;
				}
			}
		}

		#endregion

		#region Nested types: settings

		// Per-category style settings exposed as a single expandable row in
		// the property panel. ExpandableObjectConverter is the cross-flavor
		// safe equivalent of OHLCPlus's custom WPF editor — it gives the same
		// "click to expand" UX without the UserControl dependency that breaks
		// ATAS X. The class implements INotifyPropertyChanged so the
		// indicator can subscribe to changes and invalidate caches / redraw
		// without polling.
		[TypeConverter(typeof(ExpandableObjectConverter))]
		public sealed class CategoryStyle : INotifyPropertyChanged
		{
			private bool _isVisible;
			private Color _color;

			public event PropertyChangedEventHandler PropertyChanged;

			// Required by ATAS serialisation (parameterless ctor).
			public CategoryStyle() { }

			public CategoryStyle(bool isVisible, Color color)
			{
				_isVisible = isVisible;
				_color = color;
			}

			[Display(Name = "Visible", Order = 10)]
			public bool IsVisible
			{
				get => _isVisible;
				set
				{
					if (_isVisible == value) return;
					_isVisible = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
				}
			}

			[Display(Name = "Color", Order = 20)]
			public Color Color
			{
				get => _color;
				set
				{
					if (_color == value) return;
					_color = value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
				}
			}

			// Collapsed-row label. Default ToString would show the full type
			// name; this gives the user the visibility state at a glance.
			public override string ToString() => _isVisible ? "Visible" : "Hidden";
		}

		#endregion

		#region Nested types: alerts

		private readonly struct AlertRecord
		{
			public readonly DateTime FiredAt;
			public readonly bool DirectionAbove;

			public AlertRecord(DateTime firedAt, bool directionAbove)
			{
				FiredAt = firedAt;
				DirectionAbove = directionAbove;
			}
		}

		#endregion

		#region Nested types: debug overlay

		public enum DebugOverlayLocation
		{
			TopLeft,
			TopCenter,
			TopRight,
			BottomLeft,
			BottomCenter,
			BottomRight
		}

		#endregion

		#region Fields

		// Sources — plugins that emit ParsedEntry[]. Populated in EnsureSourcesInitialized.
		private readonly List<ILevelsSource> _sources = new List<ILevelsSource>(2);

		// API source — populated on every successful fetch.
		private ApiSource _apiSource;

		private bool _sourcesInitialized;

		// Full request set. The MenthorQ backend accepts all six on a single call
		// and will simply omit missing blocks, so we always ask for everything.
		private static readonly LevelType[] AllLevelTypes = new[]
		{
	LevelType.blindspots,
	LevelType.swing_levels,
	LevelType.gamma_levels,
	LevelType.gamma_scalping,
	LevelType.gamma_levels_intraday,
	LevelType.gamma_scalping_intraday,
};

		// Union of all enabled sources. Consumed by the engine in a later commit.
		private ParsedEntry[] _parsedEntries = Array.Empty<ParsedEntry>();

		// Final consumable output of the engine: one Level per unique price,
		// with deduplicated labels, a winner that drives the visual style, and
		// a pre-built display text. Consumed by OnRender in a later commit.
		private Level[] _levels = Array.Empty<Level>();

		// Render pen cache, keyed by (category, tier). Uses RenderPen — the
		// canonical OFT drawing primitive — rather than System.Drawing.Pen,
		// which would still work via ATAS X auto-conversion but is not the
		// idiomatic choice in the SDK. Worst-case size is the cartesian
		// product (categories × tiers), ~39 entries.
		private readonly Dictionary<(LevelCategory, RenderTier), RenderPen> _penCache
			= new Dictionary<(LevelCategory, RenderTier), RenderPen>();

		// Halo pen cache. Same shape as _penCache but the entries draw the
		// glow under 0DTE levels — wider stroke, lower alpha. Halo width is
		// per-tier (a 0DTE flagship deserves a bigger glow than a 0DTE
		// swing), but the colour is derived from the category palette so the
		// halo always reads as "the same level, in 0DTE mode".
		private readonly Dictionary<(LevelCategory, RenderTier), RenderPen> _haloPenCache
			= new Dictionary<(LevelCategory, RenderTier), RenderPen>();

		// Default font for level labels. Static because it's invariant in this
		// commit — user customisation will land via a FontSetting wrapper in a
		// later commit. RenderFont is the canonical OFT drawing primitive for
		// fonts, same cross-flavor story as RenderPen.
		private static readonly RenderFont LabelFont = new RenderFont("Arial", 10);

		// Halo opacity. 80/255 ≈ 31% — strong enough to read on dark themes,
		// soft enough that the main line is still the dominant visual.
		private const int HaloAlpha = 80;

		// Dirty flag — set by setters, consumed by RebuildParsedEntriesIfNeeded.
		private bool _dataDirty = true;

		// UI: Manual text source
		private bool _enableManualText;
		private string _indexTextRaw = string.Empty;
		private string _futuresTextRaw = string.Empty;
		private decimal _textOffset;

		// UI: API
		private string _apiKey = string.Empty;
		private string _userId = string.Empty;

		// Runtime: API client, reconstructed on ApiKey change.
		// Non-null from ctor on — callers never need a null-check.
		private MqApi _api;

		// UI: Ticker override. When non-empty, takes precedence over InstrumentInfo.
		private string _tickerOverride = string.Empty;

		// UI: Alerts
		private bool _enableAlerts = false;
		private string _alertSoundFile = "alert1.wav";
		private int _alertCooldownSeconds = 60;

		// UI: reversal toggle
		private bool _enableReversalAlerts = true;

		// Pending alerts, keyed by level price. An entry exists from the
		// moment a cross alert fires until either (a) the cooldown expires
		// and the entry is processed (potentially emitting a reversal) and
		// removed, or (b) the indicator is destroyed. Replaces the simpler
		// _lastAlertTime from the previous commit because we now need to
		// remember the alerted direction, not just the timestamp.
		private readonly Dictionary<decimal, AlertRecord> _pendingAlerts
			= new Dictionary<decimal, AlertRecord>();

		// Last close observed by DetectAndFireAlerts. Used to detect
		// transitions on a per-tick basis instead of per-bar basis. Reset to
		// the current close on first call ever (or after a recalc that
		// re-runs OnCalculate from bar 0).
		private bool _alertStateInitialised = false;
		private decimal _lastObservedClose;

		// Multiplier for the Index text path. Applied BEFORE TextOffset.
		// Default 1 = no scaling, preserves the additive-only behaviour of
		// previous commits.
		private decimal _textMultiplier = 1m;

		// API: multiplier and offset, mirroring the Manual text pair.
		private decimal _apiMultiplier = 1m;
		private decimal _apiOffset;

		// UI: debug overlay
		private bool _enableDebugOverlay = false;
		private DebugOverlayLocation _debugOverlayLocation = DebugOverlayLocation.TopRight;

		// Tracks which source produced the current _levels. Set inside
		// RebuildLevelsIfNeeded; consumed by the debug overlay so the user
		// can see at a glance which feed is active without reading the log.
		private string _lastActiveSourceId = "(none)";

		// Static font for the debug overlay. Monospace so multi-line numeric
		// state aligns cleanly. Cross-flavor like the label font.
		private static readonly RenderFont DebugFont = new RenderFont("Consolas", 9);

		// UI: auto-refresh
		private bool _enableAutoRefresh;

		// Per-day fired-slot ledger to ensure each slot fires once per day.
		// Keyed by (Hour, Minute) in EST. Reset when nowEst.Date changes.
		private DateTime _autoRefreshLastDate = DateTime.MinValue;
		private readonly HashSet<(int Hour, int Minute)> _autoRefreshFiredSlotsToday = new();

		// Cadence: a 30-second timer is enough to catch any slot's
		// post-stabilisation window. The timer instance is also our
		// subscribe/unsubscribe handle.
		private static readonly TimeSpan AutoRefreshTimerInterval = TimeSpan.FromSeconds(30);

		// Eastern Time zone — handles EST/EDT transitions automatically
		// despite the Windows ID being labelled "Standard". Falls back to
		// IANA name on non-Windows runtimes.
		private static readonly TimeZoneInfo EasternTimeZone = ResolveEasternTimeZone();

		// MenthorQ intraday update schedule (EST). Source: MenthorQ
		// documentation. 1 pre-market slot at 08:00 plus 13 regular-session
		// slots every 30 minutes from 09:50 to 15:50.
		private static readonly (int Hour, int Minute)[] IntradayScheduleEst = new[]
		{
	(8, 0),
	(9, 50), (10, 20), (10, 50),
	(11, 20), (11, 50),
	(12, 20), (12, 50),
	(13, 20), (13, 50),
	(14, 20), (14, 50),
	(15, 20), (15, 50)
};

		#endregion

		#region Properties: Manual text

		[Display(Name = "Enabled",
			GroupName = "Manual text",
			Description = "Enable pasting MenthorQ levels as comma-separated text. Ignored when the API is configured and reachable.",
			Order = 10)]
		public bool EnableManualText
		{
			get => _enableManualText;
			set
			{
				if (_enableManualText == value)
					return;

				_enableManualText = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Index text",
			GroupName = "Manual text",
			Description = "Paste MenthorQ output for an Index or ETF ticker (SPX / NDX / RUT, or SPY / QQQ / IWM). Multiplier and offset below are applied to these prices in that order.",
			Order = 20)]
		public string IndexTextRaw
		{
			get => _indexTextRaw;
			set
			{
				value ??= string.Empty;
				if (string.Equals(_indexTextRaw, value, StringComparison.Ordinal))
					return;

				_indexTextRaw = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Futures text",
			GroupName = "Manual text",
			Description = "Paste MenthorQ output for the Futures ticker (ES / NQ / RTY). Offset is not applied to these prices.",
			Order = 30)]
		public string FuturesTextRaw
		{
			get => _futuresTextRaw;
			set
			{
				value ??= string.Empty;
				if (string.Equals(_futuresTextRaw, value, StringComparison.Ordinal))
					return;

				_futuresTextRaw = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Multiplier (Index to Chart)",
			GroupName = "Manual text",
			Description = "Applied to every Index-text price BEFORE the offset. Most common case: ETF data → Futures chart. Approx ratios at current index levels: SPY → ES ≈ 10, QQQ → NQ ≈ 41, IWM → RTY ≈ 10. Ratios drift with index level — recalibrate periodically. Default 1 (no scaling). Futures text is never transformed.",
			Order = 35)]
		public decimal TextMultiplier
		{
			get => _textMultiplier;
			set
			{
				if (_textMultiplier == value) return;
				_textMultiplier = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Offset (Index to Chart)",
			GroupName = "Manual text",
			Description = "Added to every price parsed from the Index text, to align SPX levels to ES, NDX to NQ, etc. Futures text is never offset.",
			Order = 40)]
		public decimal TextOffset
		{
			get => _textOffset;
			set
			{
				if (_textOffset == value)
					return;

				_textOffset = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Clear text fields",
			GroupName = "Manual text",
			Description = "Toggle to clear both Index and Futures text fields. Self-resets.",
			Order = 50)]
		public bool ClearManualTextNow
		{
			get => false;
			set
			{
				if (!value)
					return;

				if (_indexTextRaw.Length == 0 && _futuresTextRaw.Length == 0)
					return;

				_indexTextRaw = string.Empty;
				_futuresTextRaw = string.Empty;

				// Force UI textboxes to refresh (matches legacy LevelsLolo behavior).
				RaisePropertyChanged(nameof(IndexTextRaw));
				RaisePropertyChanged(nameof(FuturesTextRaw));

				_dataDirty = true;
				RecalculateValues();
			}
		}

		#endregion

		#region Properties: API

		[Display(Name = "API Key",
			GroupName = "API",
			Description = "MenthorQ API key. When set together with User ID, the API becomes the active data source and manual text is ignored.",
			Order = 110)]
		public string ApiKey
		{
			get => _apiKey;
			set
			{
				value ??= string.Empty;
				if (string.Equals(_apiKey, value, StringComparison.Ordinal))
					return;

				_apiKey = value;
				_api = new MqApi(_apiKey);
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "User ID (email)",
			GroupName = "API",
			Description = "Email associated with your MenthorQ account. Sent as the user_id query parameter on every request.",
			Order = 120)]
		public string UserId
		{
			get => _userId;
			set
			{
				value ??= string.Empty;
				if (string.Equals(_userId, value, StringComparison.Ordinal))
					return;

				_userId = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Ticker override",
	GroupName = "API",
	Description = "Optional. Leave empty to use the chart instrument (micros are auto-stripped: MES→ES, MNQ→NQ). Set a value to force a specific ticker (e.g. SPX on ES, or ES when the feed exposes ESH24).",
	Order = 130)]
		public string TickerOverride
		{
			get => _tickerOverride;
			set
			{
				value ??= string.Empty;
				if (string.Equals(_tickerOverride, value, StringComparison.Ordinal))
					return;

				_tickerOverride = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Multiplier (API to Chart)",
			GroupName = "API",
			Description = "Applied to every API-fetched price BEFORE the offset. Most common case: ETF API feed → Futures chart. Approx ratios at current index levels: SPY → ES ≈ 10, QQQ → NQ ≈ 41, IWM → RTY ≈ 10. Ratios drift with index level — recalibrate periodically. Default 1 (no scaling). Applied at engine time — does not trigger an API refetch.",
			Order = 132)]
		public decimal ApiMultiplier
		{
			get => _apiMultiplier;
			set
			{
				if (_apiMultiplier == value) return;
				_apiMultiplier = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Offset (API to Chart)",
			GroupName = "API",
			Description = "Added to every API-fetched price AFTER the multiplier. Combined formula: (raw_price × multiplier) + offset. Applied at engine time — changing it does not trigger an API refetch.",
			Order = 135)]
		public decimal ApiOffset
		{
			get => _apiOffset;
			set
			{
				if (_apiOffset == value) return;
				_apiOffset = value;
				_dataDirty = true;
				RecalculateValues();
			}
		}

		[Display(Name = "Update Levels",
	GroupName = "API",
	Description = "Fetch latest levels from the MenthorQ API. Self-resets.",
	Order = 140)]
		public bool UpdateLevels
		{
			get => false;
			set
			{
				if (!value)
					return;

				_ = FetchAndParseLevelsAsync();
			}
		}

		[Display(Name = "Auto refresh", GroupName = "API",
	Description = "Automatically refresh API levels following MenthorQ's documented intraday schedule (14 fixed EST update slots: pre-market at 08:00 plus every 30 minutes from 09:50 to 15:50). A small post-slot delay lets the data settle on MenthorQ's end before the fetch fires. Toggling this on triggers an immediate catch-up fetch. EOD slots (18:30 / 23:00 EST) are not auto-refreshed; press Update Levels manually after market close if needed.",
	Order = 142)]
		public bool EnableAutoRefresh
		{
			get => _enableAutoRefresh;
			set
			{
				if (_enableAutoRefresh == value) return;
				_enableAutoRefresh = value;

				// Catch-up: when the user toggles on mid-session, fetch
				// immediately so the chart reflects the latest levels
				// without waiting for the next scheduled slot.
				if (value)
					_ = FetchAndParseLevelsAsync();
			}
		}

		#endregion

		#region Properties: Levels

		// Each row is a single expandable property. The Display attribute
		// here governs the row label and ordering inside the "Levels" group;
		// the inner CategoryStyle's [Display] attributes govern the sub-row
		// labels (Visible, Color) when the row is expanded.

		[Display(Name = "Gamma Wall", GroupName = "Levels", Order = 200,
			Description = "Visibility and colour for GW lines, halos and labels.")]
		public CategoryStyle GammaWall { get; }

		[Display(Name = "Call Resistance", GroupName = "Levels", Order = 201,
			Description = "Visibility and colour for CR lines, halos and labels.")]
		public CategoryStyle CallResistance { get; }

		[Display(Name = "Put Support", GroupName = "Levels", Order = 202,
			Description = "Visibility and colour for PS lines, halos and labels.")]
		public CategoryStyle PutSupport { get; }

		[Display(Name = "High Volatility Level", GroupName = "Levels", Order = 203,
			Description = "Visibility and colour for HVL lines, halos and labels.")]
		public CategoryStyle HighVolatilityLevel { get; }

		[Display(Name = "Risk Trigger", GroupName = "Levels", Order = 204,
			Description = "Visibility and colour for RT lines, halos and labels.")]
		public CategoryStyle RiskTrigger { get; }

		[Display(Name = "Upper Band", GroupName = "Levels", Order = 205,
			Description = "Visibility and colour for UB lines, halos and labels.")]
		public CategoryStyle UpperBand { get; }

		[Display(Name = "Lower Band", GroupName = "Levels", Order = 206,
			Description = "Visibility and colour for LB lines, halos and labels.")]
		public CategoryStyle LowerBand { get; }

		[Display(Name = "Gamma Exposure", GroupName = "Levels", Order = 207,
			Description = "Visibility and colour for GEX N lines, halos and labels.")]
		public CategoryStyle GammaExposure { get; }

		[Display(Name = "Blind Spot", GroupName = "Levels", Order = 208,
			Description = "Visibility and colour for BL N lines, halos and labels.")]
		public CategoryStyle BlindSpot { get; }

		[Display(Name = "Day Max", GroupName = "Levels", Order = 209,
			Description = "Visibility and colour for 1D Max lines, halos and labels.")]
		public CategoryStyle DayMax { get; }

		[Display(Name = "Day Min", GroupName = "Levels", Order = 210,
			Description = "Visibility and colour for 1D Min lines, halos and labels.")]
		public CategoryStyle DayMin { get; }

		[Display(Name = "Swing", GroupName = "Levels", Order = 211,
			Description = "Visibility and colour for Swing lines, halos and labels.")]
		public CategoryStyle Swing { get; }

		[Display(Name = "Other", GroupName = "Levels", Order = 212,
			Description = "Visibility and colour for fallback / unrecognised labels.")]
		public CategoryStyle Other { get; }

		#endregion

		#region Properties: Alerts

		[Display(Name = "Enable alerts", GroupName = "Alerts",
			Description = "Master switch for cross-level alerts. When off, nothing is emitted regardless of category visibility.",
			Order = 400)]
		public bool EnableAlerts
		{
			get => _enableAlerts;
			set => _enableAlerts = value;
		}

		[Display(Name = "Sound file", GroupName = "Alerts",
			Description = "Filename of the sound played when an alert fires. ATAS resolves bundled names like 'alert1.wav' from its sound directory; absolute paths work too.",
			Order = 410)]
		public string AlertSoundFile
		{
			get => _alertSoundFile;
			set => _alertSoundFile = value ?? string.Empty;
		}

		[Display(Name = "Alert on reversal", GroupName = "Alerts",
	Description = "When a cross alert fires and price reverts back across the level within the cooldown window, emit a follow-up alert at cooldown expiry.",
	Order = 415)]
		public bool EnableReversalAlerts
		{
			get => _enableReversalAlerts;
			set => _enableReversalAlerts = value;
		}

		[Display(Name = "Cooldown (seconds)", GroupName = "Alerts",
			Description = "Minimum time between consecutive alerts on the same level price. Prevents alert storms when price oscillates around a level.",
			Order = 420)]
		[Range(1, 3600)]
		public int AlertCooldownSeconds
		{
			get => _alertCooldownSeconds;
			set => _alertCooldownSeconds = Math.Max(1, value);
		}

		#endregion

		#region Properties: Diagnostics

		[Display(Name = "Show debug overlay",
			GroupName = "Diagnostics",
			Description = "Renders a small status box in the top-left corner of the chart showing internal indicator state: resolved ticker, active source, level/entry counts, pending alerts, API status, and current text/API multiplier+offset values. Default off — turn on when diagnosing why levels are missing or misaligned.",
			Order = 500)]
		public bool EnableDebugOverlay
		{
			get => _enableDebugOverlay;
			set
			{
				if (_enableDebugOverlay == value) return;
				_enableDebugOverlay = value;
				RedrawChart();
			}
		}

		[Display(Name = "Overlay location",
	GroupName = "Diagnostics",
	Description = "Corner of the chart where the debug overlay is anchored. Default TopRight to avoid colliding with the OHLC info that ATAS draws in the top-left under the cursor.",
	Order = 510)]
		public DebugOverlayLocation OverlayLocation
		{
			get => _debugOverlayLocation;
			set
			{
				if (_debugOverlayLocation == value) return;
				_debugOverlayLocation = value;
				RedrawChart();
			}
		}

		#endregion

		#region ctor

		public MenthorQLevels()
			: base(useCandles: true)
		{
			DenyToChangePanel = true;
			DrawAbovePrice = true;
			EnableCustomDrawing = true;
			SubscribeToDrawingEvents(DrawingLayouts.Final);

			// Hide the default DataSeries[0] from the chart panel and from
			// the indicator's draw list. The indicator renders entirely
			// through OnRender against _levels; the base-class series has
			// no values to plot and would otherwise show up under "Dibujado"
			// as an empty 1px solid line entry.
			DataSeries[0].IsHidden = true;
			((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;

			_api = new MqApi(_apiKey);

			// Initialise the 13 styles with their default colour and Visible=true,
			// then wire each one to OnStyleChanged so any sub-field edit
			// propagates to the rendering layer without polling.
			GammaWall = NewStyle(LevelCategory.GammaWall);
			CallResistance = NewStyle(LevelCategory.CallResistance);
			PutSupport = NewStyle(LevelCategory.PutSupport);
			HighVolatilityLevel = NewStyle(LevelCategory.HighVolatilityLevel);
			RiskTrigger = NewStyle(LevelCategory.RiskTrigger);
			UpperBand = NewStyle(LevelCategory.UpperBand);
			LowerBand = NewStyle(LevelCategory.LowerBand);
			GammaExposure = NewStyle(LevelCategory.GammaExposure);
			BlindSpot = NewStyle(LevelCategory.BlindSpot);
			DayMax = NewStyle(LevelCategory.DayMax);
			DayMin = NewStyle(LevelCategory.DayMin);
			Swing = NewStyle(LevelCategory.Swing);
			Other = NewStyle(LevelCategory.Other);
		}

		private CategoryStyle NewStyle(LevelCategory c)
		{
			var s = new CategoryStyle(isVisible: true, color: DefaultColorFor(c));
			s.PropertyChanged += OnStyleChanged;
			return s;
		}

		private void OnStyleChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(CategoryStyle.Color))
				InvalidatePenCaches();

			RedrawChart();
		}

		#endregion

		#region Overrides

		protected override void OnCalculate(int bar, decimal value)
		{
			RebuildLevelsIfNeeded();
			DetectAndFireAlerts(bar);
		}

		protected override void OnRender(RenderContext context, DrawingLayouts layout)
		{
			if (_levels == null || _levels.Length == 0)
				return;

			if (ChartInfo == null)
				return;

			var visible = ChartInfo.PriceChartContainer;
			if (visible == null)
				return;

			int xRight = Container.Region.Right;

			// Pass 1 — halos for 0DTE levels.
			for (int i = 0; i < _levels.Length; i++)
			{
				var level = _levels[i];

				if (level.Price < visible.Low || level.Price > visible.High)
					continue;

				if (!IsCategoryVisible(level.Winner.Category))
					continue;

				if (!level.Winner.Is0Dte)
					continue;

				var tier = ClassifyTier(level.Winner);
				var halo = GetHaloPen(level.Winner.Category, tier);
				int y = ChartInfo.GetYByPrice(level.Price, false);

				context.DrawLine(halo, 0, y, xRight, y);
			}

			// Pass 2 — main lines on top of all halos.
			for (int i = 0; i < _levels.Length; i++)
			{
				var level = _levels[i];

				if (level.Price < visible.Low || level.Price > visible.High)
					continue;

				if (!IsCategoryVisible(level.Winner.Category))
					continue;

				var tier = ClassifyTier(level.Winner);
				var pen = GetPen(level.Winner.Category, tier);
				int y = ChartInfo.GetYByPrice(level.Price, false);

				context.DrawLine(pen, 0, y, xRight, y);
			}

			// Pass 3 — labels.
			for (int i = 0; i < _levels.Length; i++)
			{
				var level = _levels[i];

				if (level.Price < visible.Low || level.Price > visible.High)
					continue;

				if (!IsCategoryVisible(level.Winner.Category))
					continue;

				// Concatenate display text + optional ticker suffix. When the
				// source ticker matches the chart instrument, TickerSuffix is
				// empty and the concat is a no-op (returns the interned string).
				var text = level.DisplayText;
				if (!string.IsNullOrEmpty(level.TickerSuffix))
					text += level.TickerSuffix;

				if (string.IsNullOrEmpty(text))
					continue;

				var color = GetColorFor(level.Winner.Category);
				int y = ChartInfo.GetYByPrice(level.Price, false);

				var textSize = context.MeasureString(text, LabelFont);
				int x = xRight - textSize.Width - 5;

				context.DrawString(text, LabelFont, color, x, y - textSize.Height - 3);
			}

			// Pass 4 — debug overlay (opt-in). Drawn last so it sits on top of
			// every other layer, including labels. Anchored to chart-frame
			// coordinates rather than price coordinates, so it doesn't move
			// with scrolling or zoom.
			if (_enableDebugOverlay)
				RenderDebugOverlay(context);
		}

		protected override void OnInitialize()
		{
			base.OnInitialize();
			SubscribeToTimer(AutoRefreshTimerInterval, OnAutoRefreshTick);
		}

		protected override void OnDispose()
		{
			UnsubscribeFromTimer(AutoRefreshTimerInterval, OnAutoRefreshTick);
			base.OnDispose();
		}

		#endregion

		#region Private methods: sources

		private void EnsureSourcesInitialized()
		{
			if (_sourcesInitialized)
				return;

			_sources.Clear();

			// Registration order defines priority. The first enabled source in the
			// list wins — see RebuildParsedEntriesIfNeeded. ApiSource is registered
			// first because MenthorQ API is the intended primary feed when creds
			// are configured; manual text is the offline / demo fallback.
			_apiSource = new ApiSource(this);
			_sources.Add(_apiSource);

			_sources.Add(new ManualTextSource(this));

			_sourcesInitialized = true;
		}

		private void RebuildLevelsIfNeeded()
		{
			if (!_dataDirty)
				return;

			EnsureSourcesInitialized();

			ILevelsSource winner = null;
			ParsedEntry[] winnerEntries = Array.Empty<ParsedEntry>();
			string[] winnerWarnings = Array.Empty<string>();

			for (int i = 0; i < _sources.Count; i++)
			{
				var src = _sources[i];
				if (!src.IsEnabled)
					continue;

				if (!src.TryGetEntries(out var entries, out var warnings))
					continue;

				winner = src;
				winnerEntries = entries ?? Array.Empty<ParsedEntry>();
				winnerWarnings = warnings ?? Array.Empty<string>();
				break;
			}

			_parsedEntries = winnerEntries;
			// ResolveTicker() returns the chart-side ticker normalized the same
			// way the API path is (override first, micros stripped). Empty when
			// InstrumentInfo isn't available yet — BuildTickerSuffix treats that
			// as "don't suppress", which is fine: levels render with their
			// ticker annotation until the chart instrument loads, then rebuild.
			_levels = BuildLevels(_parsedEntries, ResolveTicker());

			for (int w = 0; w < winnerWarnings.Length; w++)
				this.LogWarn($"[{winner?.SourceId ?? "none"}] {winnerWarnings[w]}");

			_dataDirty = false;

			_lastActiveSourceId = winner?.SourceId ?? "(none)";

			if (winner != null)
			{
				this.LogInfo($"MenthorQLevels: rebuilt {_parsedEntries.Length} entries " +
							 $"→ {_levels.Length} levels from {winner.SourceId} " +
							 $"({winnerWarnings.Length} warnings)");
			}
			else
			{
				this.LogInfo("MenthorQLevels: rebuilt 0 entries (no active source)");
			}
		}

		#endregion

		#region Private methods: parsing

		private static ParseResult ParseMenthorQText(
	string raw,
	decimal multiplier,
	decimal offset,
	bool applyTransform,
	string sourceIdBase)
		{
			if (string.IsNullOrWhiteSpace(raw))
				return new ParseResult(Array.Empty<ParsedEntry>(), Array.Empty<string>());

			raw = raw.Trim();

			string ticker = string.Empty;
			var colon = raw.IndexOf(':');
			if (colon > 0 && raw[0] == '$')
			{
				ticker = raw.Substring(1, colon - 1).Trim();
				raw = raw.Substring(colon + 1);
			}

			var parts = raw.Split(',');
			if (parts.Length < 2)
				return new ParseResult(
					Array.Empty<ParsedEntry>(),
					new[] { "Expected comma-separated label/price pairs." });

			var warnings = new List<string>(8);
			var entries = new List<ParsedEntry>(parts.Length / 2);

			for (int i = 0; i + 1 < parts.Length; i += 2)
			{
				var labelRaw = (parts[i] ?? string.Empty).Trim();
				var priceRaw = (parts[i + 1] ?? string.Empty).Trim();

				if (labelRaw.Length == 0)
				{
					warnings.Add($"Empty label at pair index {i / 2 + 1}.");
					continue;
				}

				if (!TryParseDecimalInvariant(priceRaw, out var price))
				{
					warnings.Add($"Invalid price '{priceRaw}' for label '{labelRaw}'.");
					continue;
				}

				// Capture the pre-transform price BEFORE we mutate `price`, so we
				// can surface it in the chart label when the source ticker differs
				// from the chart instrument. Only emit OriginalPrice when the
				// transform is non-identity — otherwise the original equals the
				// displayed price and the parenthetical would be redundant.
				decimal? originalPrice = null;
				if (applyTransform)
				{
					if (multiplier != 1m || offset != 0m)
						originalPrice = price;
					price = (price * multiplier) + offset;
				}

				if (!TryMapMenthorQLabel(labelRaw, out var mapped))
				{
					mapped = new MappedLabel(
						displayLabel: NormalizeWhitespace(labelRaw),
						category: LevelCategory.Other,
						rank: 999,
						is0Dte: Contains0Dte(labelRaw));
				}

				var sourceId = string.IsNullOrEmpty(ticker)
					? (sourceIdBase ?? "MenthorQ:Text")
					: $"{(sourceIdBase ?? "MenthorQ:Text")}:{ticker}";

				var lvlLabel = new LevelLabel(
					mapped.Category,
					mapped.Rank,
					mapped.Is0Dte,
					rawLabel: labelRaw,
					displayLabel: mapped.DisplayLabel);

				entries.Add(new ParsedEntry(
					price,
					new[] { lvlLabel },
					sourceId,
					sourceTicker: ticker,
					originalPrice: originalPrice));
			}

			if (entries.Count == 0 && warnings.Count == 0)
				warnings.Add("No valid label/price pairs found.");

			return new ParseResult(
				entries.Count == 0 ? Array.Empty<ParsedEntry>() : entries.ToArray(),
				warnings.Count == 0 ? Array.Empty<string>() : warnings.ToArray());
		}

		private static bool TryMapMenthorQLabel(string labelRaw, out MappedLabel mapped)
		{
			mapped = default;

			var s = NormalizeWhitespace(labelRaw);
			if (s.Length == 0)
				return false;

			bool is0Dte = Contains0Dte(s);
			var sNo0 = Remove0DteSuffix(s);

			// Flagship single-instance concepts — rank 0
			if (EqualsIgnoreCase(sNo0, "Call Resistance"))
			{
				mapped = new MappedLabel("CR" + (is0Dte ? " 0DTE" : string.Empty),
					LevelCategory.CallResistance, 0, is0Dte);
				return true;
			}

			if (EqualsIgnoreCase(sNo0, "Put Support"))
			{
				mapped = new MappedLabel("PS" + (is0Dte ? " 0DTE" : string.Empty),
					LevelCategory.PutSupport, 0, is0Dte);
				return true;
			}

			if (EqualsIgnoreCase(sNo0, "HVL"))
			{
				mapped = new MappedLabel("HVL" + (is0Dte ? " 0DTE" : string.Empty),
					LevelCategory.HighVolatilityLevel, 0, is0Dte);
				return true;
			}

			if (EqualsIgnoreCase(sNo0, "Gamma Wall"))
			{
				// Pivot / magnet — kept separate from GEX per MenthorQ semantics.
				mapped = new MappedLabel("GW" + (is0Dte ? " 0DTE" : string.Empty),
					LevelCategory.GammaWall, 0, is0Dte);
				return true;
			}

			if (EqualsIgnoreCase(sNo0, "1D Min"))
			{
				mapped = new MappedLabel("1D Min", LevelCategory.DayMin, 0, is0Dte);
				return true;
			}

			if (EqualsIgnoreCase(sNo0, "1D Max"))
			{
				mapped = new MappedLabel("1D Max", LevelCategory.DayMax, 0, is0Dte);
				return true;
			}

			// Numbered families: "GEX N", "BL N". Lower N = stronger.
			if (TryParsePrefixNumber(sNo0, "GEX", out var gexN))
			{
				mapped = new MappedLabel($"GEX {gexN}" + (is0Dte ? " 0DTE" : string.Empty),
					LevelCategory.GammaExposure, gexN, is0Dte);
				return true;
			}

			if (TryParsePrefixNumber(sNo0, "BL", out var blN))
			{
				mapped = new MappedLabel($"BL {blN}" + (is0Dte ? " 0DTE" : string.Empty),
					LevelCategory.BlindSpot, blN, is0Dte);
				return true;
			}

			// Date-ranged families: "LB MM-DD", "UB MM-DD", "RT MM-DD"
			if (TryParseBandOrTrigger(sNo0, out var bandType, out var mmdd))
			{
				var cat = bandType switch
				{
					"LB" => LevelCategory.LowerBand,
					"UB" => LevelCategory.UpperBand,
					"RT" => LevelCategory.RiskTrigger,
					_ => LevelCategory.Other
				};

				mapped = new MappedLabel($"{bandType} {mmdd}" + (is0Dte ? " 0DTE" : string.Empty),
					cat, 0, is0Dte);
				return true;
			}

			return false;
		}

		private static bool TryParseDecimalInvariant(string s, out decimal value)
		{
			return decimal.TryParse(
				s,
				NumberStyles.Number | NumberStyles.AllowLeadingSign,
				CultureInfo.InvariantCulture,
				out value);
		}

		private static string NormalizeWhitespace(string s)
		{
			if (s == null)
				return string.Empty;

			s = s.Trim();
			if (s.Length == 0)
				return string.Empty;

			var sb = new StringBuilder(s.Length);
			bool prevSpace = false;

			for (int i = 0; i < s.Length; i++)
			{
				char ch = s[i];
				if (char.IsWhiteSpace(ch))
				{
					if (!prevSpace)
						sb.Append(' ');
					prevSpace = true;
				}
				else
				{
					sb.Append(ch);
					prevSpace = false;
				}
			}

			return sb.ToString();
		}

		private static bool Contains0Dte(string s)
		{
			return s.IndexOf("0DTE", StringComparison.OrdinalIgnoreCase) >= 0;
		}

		private static string Remove0DteSuffix(string s)
		{
			// Remove trailing "0DTE" if present (case-insensitive). Keep internal occurrences.
			var idx = s.LastIndexOf("0DTE", StringComparison.OrdinalIgnoreCase);
			if (idx < 0)
				return s;

			for (int i = idx + 4; i < s.Length; i++)
			{
				if (!char.IsWhiteSpace(s[i]))
					return s;
			}

			return s.Substring(0, idx).TrimEnd();
		}

		private static bool EqualsIgnoreCase(string a, string b)
		{
			return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
		}

		private static bool TryParsePrefixNumber(string s, string prefix, out int n)
		{
			n = 0;

			if (!s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				return false;

			var rest = s.Substring(prefix.Length).TrimStart();
			if (rest.Length == 0)
				return false;

			int i = 0;
			while (i < rest.Length && char.IsDigit(rest[i]))
				i++;

			if (i == 0)
				return false;

			return int.TryParse(
				rest.Substring(0, i),
				NumberStyles.Integer,
				CultureInfo.InvariantCulture,
				out n);
		}

		private static bool TryParseBandOrTrigger(string s, out string bandType, out string mmdd)
		{
			bandType = null;
			mmdd = null;

			if (s.Length < 3)
				return false;

			var prefix = s.Substring(0, 2).ToUpperInvariant();
			if (prefix != "LB" && prefix != "RT" && prefix != "UB")
				return false;

			var rest = s.Substring(2).TrimStart();
			if (rest.Length < 5)
				return false;

			// NN-NN validation only — no date parsing in MVP.
			if (!char.IsDigit(rest[0]) || !char.IsDigit(rest[1])
				|| rest[2] != '-'
				|| !char.IsDigit(rest[3]) || !char.IsDigit(rest[4]))
				return false;

			bandType = prefix;
			mmdd = rest.Substring(0, 5);
			return true;
		}

		#endregion

		#region Private methods: API

		// Resolves the ticker sent to the MenthorQ API, honouring the manual
		// override first, then falling back to the chart instrument with the
		// micro-future prefix stripped.
		private string ResolveTicker()
		{
			if (!string.IsNullOrWhiteSpace(_tickerOverride))
				return _tickerOverride.Trim().ToUpperInvariant();

			var instrument = InstrumentInfo?.Instrument;
			if (string.IsNullOrEmpty(instrument))
				return string.Empty;

			// Micros: strip leading "M" on 3-character tickers.
			// MES→ES, MNQ→NQ, MYM→YM, M2K→2K.
			if (instrument.Length == 3 && instrument.StartsWith("M", StringComparison.Ordinal))
				return instrument.Substring(1);

			return instrument;
		}

		private async Task FetchAndParseLevelsAsync()
		{
			try
			{
				var ticker = ResolveTicker();
				if (string.IsNullOrWhiteSpace(ticker) || string.IsNullOrWhiteSpace(_apiKey))
				{
					this.LogWarn("MenthorQLevels: fetch skipped — ticker or API key missing.");
					return;
				}

				EnsureSourcesInitialized();

				this.LogInfo($"MenthorQLevels: fetching ticker='{ticker}' userId='{_userId}'.");

				var response = await _api
					.GetLevelsAsync(ticker, AllLevelTypes, _userId)
					.ConfigureAwait(false);

				if (response?.Levels == null || response.Levels.Count == 0)
				{
					_apiSource.Clear();
					_dataDirty = true;
					this.LogWarn($"MenthorQLevels: empty or null response (status='{_api.Status}').");
					RecalculateValues();
					return;
				}

				var entries = new List<ParsedEntry>(64);
				var unmapped = 0;

				// Resolve the ticker we tag entries with. Prefer the API's own
				// canonical name (TickerMq, e.g. "SPX") then the requested ticker
				// (e.g. "ES"), then finally the value we sent to the endpoint.
				// Stripped/uppercased for stable equality against ResolveTicker().
				var sourceTicker = (response.TickerMq
									?? response.Ticker
									?? ticker
									?? string.Empty).Trim().ToUpperInvariant();

				foreach (var block in response.Levels)
				{
					if (block?.LevelValues == null)
						continue;

					var blockIs0Dte = IsIntradayBlock(block.LevelType);

					foreach (var value in block.LevelValues)
					{
						if (value == null || string.IsNullOrWhiteSpace(value.Name))
							continue;

						if (!TryMapMenthorQLabel(value.Name, out var mapped))
						{
							unmapped++;
							mapped = new MappedLabel(
								displayLabel: value.Name,
								category: LevelCategory.Other,
								rank: 999,
								is0Dte: blockIs0Dte);
						}

						var lvlLabel = new LevelLabel(
							category: mapped.Category,
							rank: mapped.Rank,
							is0Dte: blockIs0Dte || mapped.Is0Dte,
							rawLabel: value.Name,
							displayLabel: mapped.DisplayLabel);

						// OriginalPrice is left null here: any ApiMultiplier/
						// ApiOffset transform happens later in ApiSource.TryGetEntries,
						// which stamps OriginalPrice = raw.Price at that point.
						entries.Add(new ParsedEntry(
							price: (decimal)value.Value,
							labels: new[] { lvlLabel },
							sourceId: "MenthorQ:Api",
							sourceTicker: sourceTicker));
					}
				}

				_apiSource.SetEntries(entries.ToArray());
				_dataDirty = true;

				this.LogInfo($"MenthorQLevels: API returned {entries.Count} entries " +
							 $"({unmapped} unmapped) across {response.Levels.Count} blocks.");

				RecalculateValues();
			}
			catch (Exception ex)
			{
				this.LogError($"MenthorQLevels: fetch failed — {ex.Message}");
			}
		}

		private static bool IsIntradayBlock(LevelType type)
			=> type == LevelType.gamma_levels_intraday
			|| type == LevelType.gamma_scalping_intraday;

		private void OnAutoRefreshTick()
		{
			if (!_enableAutoRefresh) return;
			if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_userId)) return;

			var nowEst = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EasternTimeZone);
			var today = nowEst.Date;

			// Day rollover: clear the per-day fired ledger so today's slots
			// can fire fresh.
			if (today != _autoRefreshLastDate)
			{
				_autoRefreshFiredSlotsToday.Clear();
				_autoRefreshLastDate = today;
			}

			for (int i = 0; i < IntradayScheduleEst.Length; i++)
			{
				var slot = IntradayScheduleEst[i];
				if (_autoRefreshFiredSlotsToday.Contains(slot)) continue;

				var slotTime = today + new TimeSpan(slot.Hour, slot.Minute, 0);

				// Fire window: 1 minute after the slot (server stabilisation
				// buffer) up to 5 minutes after (catch-up tolerance for
				// missed timer ticks, e.g. system suspend or a previous fetch
				// still in flight).
				var windowStart = slotTime + TimeSpan.FromMinutes(1);
				var windowEnd = slotTime + TimeSpan.FromMinutes(5);

				if (nowEst >= windowStart && nowEst < windowEnd)
				{
					_autoRefreshFiredSlotsToday.Add(slot);
					_ = FetchAndParseLevelsAsync();
					this.LogInfo($"MenthorQLevels: scheduled refresh — slot {slot.Hour:D2}:{slot.Minute:D2} EST");
					return;
				}
			}
		}

		private static TimeZoneInfo ResolveEasternTimeZone()
		{
			// Windows ID first because ATAS runs on Windows. Falls back to
			// the IANA name if the host is .NET 6+ on Linux/macOS.
			try { return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); }
			catch (TimeZoneNotFoundException) { }

			try { return TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); }
			catch (TimeZoneNotFoundException) { }

			// Last-ditch fallback. Loses DST handling but the indicator
			// still functions; the trader can switch off auto-refresh and
			// press the manual button at the right times.
			return TimeZoneInfo.Utc;
		}

		#endregion

		#region Private methods: engine

		private static Level[] BuildLevels(ParsedEntry[] entries, string chartTicker)
		{
			if (entries == null || entries.Length == 0)
				return Array.Empty<Level>();

			// Group by exact decimal price. Source-level rounding (API double →
			// decimal cast) was applied at parse time, so we trust equality here.
			var byPrice = new Dictionary<decimal, List<LevelLabel>>(entries.Length);

			// Parallel map of (sourceTicker, originalPrice) keyed by price. We
			// capture the values from the FIRST entry that contributes to a
			// price; subsequent entries at the same price (rare) are ignored
			// for the purpose of the ticker annotation. This keeps the visual
			// mapping stable even if two sources happen to land on the same
			// round number (e.g. SPX 5870 and ES 5870 — we annotate with the
			// first one parsed).
			var tickerByPrice = new Dictionary<decimal, (string ticker, decimal? orig)>(entries.Length);

			for (int i = 0; i < entries.Length; i++)
			{
				var e = entries[i];
				if (e.Labels == null || e.Labels.Length == 0)
					continue;

				if (!byPrice.TryGetValue(e.Price, out var bucket))
				{
					bucket = new List<LevelLabel>(2);
					byPrice[e.Price] = bucket;
					tickerByPrice[e.Price] = (e.SourceTicker, e.OriginalPrice);
				}

				for (int k = 0; k < e.Labels.Length; k++)
					bucket.Add(e.Labels[k]);
			}

			if (byPrice.Count == 0)
				return Array.Empty<Level>();

			var result = new Level[byPrice.Count];
			int idx = 0;

			foreach (var kv in byPrice)
			{
				var price = kv.Key;
				var deduped = DedupLabels(kv.Value);
				var winner = PickWinner(deduped);
				var displayText = BuildDisplayText(deduped, winner);

				var tickerInfo = tickerByPrice[price];
				var tickerSuffix = BuildTickerSuffix(tickerInfo.ticker, tickerInfo.orig, chartTicker);

				result[idx++] = new Level(price, deduped, winner, displayText, tickerSuffix);
			}

			return result;
		}

		// Build the parenthetical "  (TICKER PRICE)" / "  (TICKER)" appended to
		// a level's DisplayText when the source ticker differs from the chart
		// instrument. Returns empty string when the suffix should not be shown:
		//   - source carries no ticker, or
		//   - source ticker equals chart ticker (case-insensitive), or
		//   - chart ticker hasn't been resolved yet.
		// The leading two-space gap matches the visual rhythm of the existing
		// "X / Y" multi-label format ("CR / GW  (SPX 5870.50)").
		private static string BuildTickerSuffix(string sourceTicker, decimal? originalPrice, string chartTicker)
		{
			if (string.IsNullOrWhiteSpace(sourceTicker))
				return string.Empty;

			if (!string.IsNullOrWhiteSpace(chartTicker)
				&& string.Equals(sourceTicker, chartTicker, StringComparison.OrdinalIgnoreCase))
				return string.Empty;

			if (originalPrice.HasValue)
			{
				// Decimal.ToString preserves the source scale (e.g. 5870.50m
				// renders as "5870.50"), which is what we want — the user
				// pasted that precision deliberately.
				var priceStr = originalPrice.Value.ToString(CultureInfo.InvariantCulture);
				return $"  ({sourceTicker} {priceStr})";
			}

			return $"  ({sourceTicker})";
		}

		// Dedup by the (Category, Rank, Is0Dte) tuple. Two labels that differ
		// only in RawLabel or DisplayLabel collapse into one — the parser
		// canonicalises display text, so any leftover difference is cosmetic.
		private static LevelLabel[] DedupLabels(List<LevelLabel> labels)
		{
			if (labels == null || labels.Count == 0)
				return Array.Empty<LevelLabel>();
			if (labels.Count == 1)
				return new[] { labels[0] };

			var seen = new HashSet<(LevelCategory, int, bool)>();
			var keep = new List<LevelLabel>(labels.Count);

			for (int i = 0; i < labels.Count; i++)
			{
				var l = labels[i];
				var key = (l.Category, l.Rank, l.Is0Dte);
				if (seen.Add(key))
					keep.Add(l);
			}

			return keep.ToArray();
		}

		// Pick the label that drives the visual style. Three tie-breakers,
		// in order: higher CategoryPriority wins; if equal, 0DTE wins over
		// non-0DTE; if equal, lower Rank wins (rank 1 beats rank 5 in
		// GEX/BL families, and flagship rank 0 beats numbered ranks within
		// its family).
		private static LevelLabel PickWinner(LevelLabel[] labels)
		{
			if (labels == null || labels.Length == 0)
				return default;

			var best = labels[0];
			var bestPri = CategoryPriority(best.Category);

			for (int i = 1; i < labels.Length; i++)
			{
				var cand = labels[i];
				var candPri = CategoryPriority(cand.Category);

				if (candPri > bestPri) { best = cand; bestPri = candPri; continue; }
				if (candPri < bestPri) continue;

				if (cand.Is0Dte && !best.Is0Dte) { best = cand; continue; }
				if (!cand.Is0Dte && best.Is0Dte) continue;

				if (cand.Rank < best.Rank) { best = cand; continue; }
			}

			return best;
		}

		// Concatenate distinct DisplayLabel values with " / ", winner first.
		// Inputs are assumed deduped, so the equality check below is structural,
		// not semantic.
		private static string BuildDisplayText(LevelLabel[] labels, LevelLabel winner)
		{
			if (labels == null || labels.Length == 0)
				return string.Empty;
			if (labels.Length == 1)
				return labels[0].DisplayLabel ?? string.Empty;

			var sb = new StringBuilder(labels.Length * 8);
			sb.Append(winner.DisplayLabel ?? string.Empty);

			for (int i = 0; i < labels.Length; i++)
			{
				var l = labels[i];
				if (l.Category == winner.Category
					&& l.Rank == winner.Rank
					&& l.Is0Dte == winner.Is0Dte)
					continue;

				sb.Append(" / ");
				sb.Append(l.DisplayLabel ?? string.Empty);
			}

			return sb.ToString();
		}

		// Category priority drives the visual tier. Higher = wins.
		// Ordering encodes MenthorQ semantics: GammaWall is the magnet (price
		// of max total gamma), CR/PS the flagship walls, HVL the volatility
		// pivot, then date-anchored RiskTrigger and Bands, then the ranked
		// GEX/BlindSpot families, then daily extremes, then Swing, then Other
		// as the catch-all.
		private static int CategoryPriority(LevelCategory c) => c switch
		{
			LevelCategory.GammaWall => 100,
			LevelCategory.CallResistance => 90,
			LevelCategory.PutSupport => 90,
			LevelCategory.HighVolatilityLevel => 80,
			LevelCategory.RiskTrigger => 70,
			LevelCategory.UpperBand => 60,
			LevelCategory.LowerBand => 60,
			LevelCategory.GammaExposure => 50,
			LevelCategory.BlindSpot => 40,
			LevelCategory.DayMax => 30,
			LevelCategory.DayMin => 30,
			LevelCategory.Swing => 20,
			LevelCategory.Other => 10,
			_ => 0
		};

		#endregion

		#region Private methods: rendering

		private RenderPen GetPen(LevelCategory category, RenderTier tier)
		{
			var key = (category, tier);
			if (_penCache.TryGetValue(key, out var pen))
				return pen;

			var color = GetColorFor(category);
			var width = DefaultWidthFor(tier);

			pen = new RenderPen(color, width);
			_penCache[key] = pen;
			return pen;
		}

		private RenderPen GetHaloPen(LevelCategory category, RenderTier tier)
		{
			var key = (category, tier);
			if (_haloPenCache.TryGetValue(key, out var pen))
				return pen;

			var baseColor = GetColorFor(category);
			var haloColor = Color.FromArgb(HaloAlpha, baseColor.R, baseColor.G, baseColor.B);
			var haloWidth = DefaultHaloWidthFor(tier);

			pen = new RenderPen(haloColor, haloWidth);
			_haloPenCache[key] = pen;
			return pen;
		}

		// Halo widths are an absolute table rather than "main + delta" so the
		// thinnest tier still gets a halo wide enough to read as a glow rather
		// than as a thicker line. The numbers are tuned to feel proportional
		// to DefaultWidthFor without forcing a multiplicative formula that
		// would collapse Thin into invisibility.
		private static float DefaultHaloWidthFor(RenderTier t) => t switch
		{
			RenderTier.Thick => 8f,
			RenderTier.Medium => 6f,
			RenderTier.Thin => 4f,
			_ => 4f
		};

		// Map a Level (via its Winner) to a render tier. Tier drives line
		// thickness; later commits will let it influence opacity and dash too.
		private static RenderTier ClassifyTier(LevelLabel winner)
		{
			switch (winner.Category)
			{
				case LevelCategory.GammaWall:
				case LevelCategory.CallResistance:
				case LevelCategory.PutSupport:
				case LevelCategory.HighVolatilityLevel:
					return RenderTier.Thick;

				case LevelCategory.RiskTrigger:
				case LevelCategory.UpperBand:
				case LevelCategory.LowerBand:
					return RenderTier.Medium;

				case LevelCategory.GammaExposure:
				case LevelCategory.BlindSpot:
					// Top-2 ranks of GEX/BL are still meaningful walls; lower
					// ranks fade into the noise floor.
					return winner.Rank <= 2 ? RenderTier.Medium : RenderTier.Thin;

				case LevelCategory.DayMin:
				case LevelCategory.DayMax:
				case LevelCategory.Swing:
				case LevelCategory.Other:
				default:
					return RenderTier.Thin;
			}
		}

		// Default palette. Tuned for visibility on both light and dark themes
		// without going overboard on saturation. Per-category overrides will
		// land in the user-customisation commit.
		private static Color DefaultColorFor(LevelCategory c) => c switch
		{
			LevelCategory.GammaWall => Color.Gold,
			LevelCategory.CallResistance => Color.OrangeRed,
			LevelCategory.PutSupport => Color.LimeGreen,
			LevelCategory.HighVolatilityLevel => Color.Cyan,
			LevelCategory.RiskTrigger => Color.Magenta,
			LevelCategory.UpperBand => Color.DodgerBlue,
			LevelCategory.LowerBand => Color.MediumSeaGreen,
			LevelCategory.GammaExposure => Color.Goldenrod,
			LevelCategory.BlindSpot => Color.LightGray,
			LevelCategory.DayMax => Color.Salmon,
			LevelCategory.DayMin => Color.LightGreen,
			LevelCategory.Swing => Color.MediumPurple,
			LevelCategory.Other => Color.White,
			_ => Color.Gray
		};

		private static float DefaultWidthFor(RenderTier t) => t switch
		{
			RenderTier.Thick => 2.5f,
			RenderTier.Medium => 1.5f,
			RenderTier.Thin => 1.0f,
			_ => 1.0f
		};

		// Single switch instead of two parallel ones — the StyleFor lookup is
		// the authoritative mapping from LevelCategory to its UI-bound style
		// object.
		private CategoryStyle StyleFor(LevelCategory c) => c switch
		{
			LevelCategory.GammaWall => GammaWall,
			LevelCategory.CallResistance => CallResistance,
			LevelCategory.PutSupport => PutSupport,
			LevelCategory.HighVolatilityLevel => HighVolatilityLevel,
			LevelCategory.RiskTrigger => RiskTrigger,
			LevelCategory.UpperBand => UpperBand,
			LevelCategory.LowerBand => LowerBand,
			LevelCategory.GammaExposure => GammaExposure,
			LevelCategory.BlindSpot => BlindSpot,
			LevelCategory.DayMax => DayMax,
			LevelCategory.DayMin => DayMin,
			LevelCategory.Swing => Swing,
			LevelCategory.Other => Other,
			_ => null
		};

		private bool IsCategoryVisible(LevelCategory c)
			=> StyleFor(c)?.IsVisible ?? true;

		private Color GetColorFor(LevelCategory c)
			=> StyleFor(c)?.Color ?? Color.Gray;

		// Drop both pen caches so the next OnRender rebuilds them with the
		// fresh palette. Called from every Color setter. Race window with
		// OnRender is bounded by a single dictionary clear; the worst case
		// is one transient frame with a stale pen, which is invisible at
		// 60fps.
		private void InvalidatePenCaches()
		{
			_penCache.Clear();
			_haloPenCache.Clear();
		}

		private void RenderDebugOverlay(RenderContext context)
		{
			if (Container?.Region == null) return;

			var ticker = string.IsNullOrEmpty(_tickerOverride)
				? (InstrumentInfo?.Instrument ?? "(no instrument)")
				: $"{_tickerOverride} (override)";

			var lastApiCall = _api?.LastApiCall?.ToString("HH:mm:ss") ?? "(never)";
			var apiStatus = _api?.Status ?? "(uninit)";

			var lines = new[]
			{
		$"Ticker     : {ResolveTicker()}  ({ticker})",
		$"Source     : {_lastActiveSourceId}",
		$"Entries    : {_parsedEntries.Length}  →  Levels: {_levels.Length}",
		$"Alerts     : {(_enableAlerts ? "on" : "off")}  reversal={(_enableReversalAlerts ? "on" : "off")}  pending={_pendingAlerts.Count}  cooldown={_alertCooldownSeconds}s",
		$"API        : status={apiStatus}  last={lastApiCall}",
		$"Text xform : x {_textMultiplier}  +  {_textOffset}",
		$"API xform  : x {_apiMultiplier}  +  {_apiOffset}"
	};

			// Measure widest line for the background rectangle.
			int maxWidth = 0;
			int lineHeight = 0;
			for (int i = 0; i < lines.Length; i++)
			{
				var size = context.MeasureString(lines[i], DebugFont);
				if (size.Width > maxWidth) maxWidth = size.Width;
				if (size.Height > lineHeight) lineHeight = size.Height;
			}
			if (lineHeight == 0) lineHeight = 12; // safety fallback
			lineHeight += 2; // small leading

			const int padding = 6;
			const int margin = 10;
			int boxWidth = maxWidth + padding * 2;
			int boxHeight = lineHeight * lines.Length + padding * 2;

			GetOverlayAnchor(boxWidth, boxHeight, margin, padding, out int x, out int y);

			var bgRect = new Rectangle(
				x - padding,
				y - padding,
				boxWidth,
				boxHeight);

			// Semi-transparent black background — readable on both light and
			// dark themes.
			context.FillRectangle(Color.FromArgb(180, 0, 0, 0), bgRect);

			// Lime green text — high contrast against the background, doesn't
			// collide with any of the level palette colours.
			var textColor = Color.LimeGreen;
			for (int i = 0; i < lines.Length; i++)
			{
				context.DrawString(lines[i], DebugFont, textColor, x, y + i * lineHeight);
			}
		}

		private void GetOverlayAnchor(int boxWidth, int boxHeight, int margin, int padding,
	out int x, out int y)
		{
			var region = Container.Region;

			int leftX = region.Left + margin + padding;
			int rightX = region.Right - boxWidth - margin + padding;
			int centerX = region.Left + (region.Width - boxWidth) / 2 + padding;
			int topY = region.Top + margin + padding;
			int bottomY = region.Bottom - boxHeight - margin + padding;

			switch (_debugOverlayLocation)
			{
				case DebugOverlayLocation.TopLeft: x = leftX; y = topY; break;
				case DebugOverlayLocation.TopCenter: x = centerX; y = topY; break;
				case DebugOverlayLocation.TopRight: x = rightX; y = topY; break;
				case DebugOverlayLocation.BottomLeft: x = leftX; y = bottomY; break;
				case DebugOverlayLocation.BottomCenter: x = centerX; y = bottomY; break;
				case DebugOverlayLocation.BottomRight: x = rightX; y = bottomY; break;
				default: x = leftX; y = topY; break;
			}
		}

		#endregion

		#region Private methods: alerts

		private void DetectAndFireAlerts(int bar)
		{
			if (!_enableAlerts) return;
			if (_levels == null || _levels.Length == 0) return;
			if (bar < CurrentBar - 1) return;

			var current = GetCandle(bar);
			var currentClose = current.Close;
			var now = DateTime.Now;
			var ticker = InstrumentInfo?.Instrument ?? string.Empty;

			// Process pending alerts whose cooldown has expired. May emit
			// reversal notifications as a side effect. Done first so the
			// _pendingAlerts table is in its post-expiry state when the
			// cross-detection loop below consults it.
			ProcessExpiredAlerts(currentClose, ticker, now);

			// First call ever — seed and exit. No previous tick to compare.
			if (!_alertStateInitialised)
			{
				_lastObservedClose = currentClose;
				_alertStateInitialised = true;
				return;
			}

			var prevObserved = _lastObservedClose;
			_lastObservedClose = currentClose;

			if (currentClose == prevObserved) return;

			var lo = Math.Min(prevObserved, currentClose);
			var hi = Math.Max(prevObserved, currentClose);
			var directionAbove = currentClose > prevObserved;

			for (int i = 0; i < _levels.Length; i++)
			{
				var level = _levels[i];

				if (!IsCategoryVisible(level.Winner.Category))
					continue;

				if (level.Price <= lo || level.Price >= hi)
					continue;

				// Cooldown gate. ContainsKey is now equivalent to "cooldown
				// active" because ProcessExpiredAlerts above has already
				// dropped any entries whose cooldown expired this tick.
				if (_pendingAlerts.ContainsKey(level.Price))
					continue;

				var priceStr = level.Price.ToString("0.##", CultureInfo.InvariantCulture);
				var direction = directionAbove ? "above" : "below";
				var message = $"crossed {direction} {level.DisplayText} @ {priceStr}";

				try
				{
					AddAlert(_alertSoundFile, ticker, message,
						Color.Black.Convert(),
						Color.White.Convert());

					_pendingAlerts[level.Price] = new AlertRecord(now, directionAbove);

					this.LogInfo($"MenthorQLevels: alert — {ticker} {message}");
				}
				catch (Exception ex)
				{
					this.LogWarn($"MenthorQLevels: alert failed for '{message}' — {ex.Message}");
				}
			}
		}

		private void ProcessExpiredAlerts(decimal currentClose, string ticker, DateTime now)
		{
			if (_pendingAlerts.Count == 0) return;

			List<decimal> toRemove = null;

			foreach (var kv in _pendingAlerts)
			{
				var levelPrice = kv.Key;
				var record = kv.Value;

				// Still inside the cooldown window — leave it alone.
				if ((now - record.FiredAt).TotalSeconds < _alertCooldownSeconds)
					continue;

				// Cooldown expired. Decide whether to emit a reversal before
				// dropping the entry. Equality with the level price is treated
				// as ambiguous (neither above nor below) and skipped silently.
				toRemove ??= new List<decimal>();
				toRemove.Add(levelPrice);

				if (!_enableReversalAlerts) continue;
				if (currentClose == levelPrice) continue;

				var nowAbove = currentClose > levelPrice;
				var reverted = record.DirectionAbove != nowAbove;

				if (!reverted) continue;

				// Look up the level. It may have been removed (user re-pegó
				// text or refreshed API) since the alert fired, in which case
				// we have no DisplayText / category and silently skip.
				Level? matched = null;
				for (int i = 0; i < _levels.Length; i++)
				{
					if (_levels[i].Price == levelPrice)
					{
						matched = _levels[i];
						break;
					}
				}

				if (!matched.HasValue) continue;

				var lvl = matched.Value;

				// Visibility may have been toggled off in the meantime — alerting
				// on a hidden level is the same kind of confusion we gate the
				// forward path against.
				if (!IsCategoryVisible(lvl.Winner.Category)) continue;

				var priceStr = levelPrice.ToString("0.##", CultureInfo.InvariantCulture);
				var newSide = nowAbove ? "above" : "below";
				var message = $"back {newSide} {lvl.DisplayText} @ {priceStr}";

				try
				{
					AddAlert(_alertSoundFile, ticker, message,
						Color.Black.Convert(),
						Color.White.Convert());

					this.LogInfo($"MenthorQLevels: alert — {ticker} {message}");
				}
				catch (Exception ex)
				{
					this.LogWarn($"MenthorQLevels: reversal alert failed for '{message}' — {ex.Message}");
				}
			}

			if (toRemove != null)
			{
				for (int i = 0; i < toRemove.Count; i++)
					_pendingAlerts.Remove(toRemove[i]);
			}
		}

		#endregion
	}
}
