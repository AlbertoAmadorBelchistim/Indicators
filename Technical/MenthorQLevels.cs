using ATAS.Indicators;
using OFT.Rendering.Context;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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

            public ParsedEntry(decimal price, LevelLabel[] labels, string sourceId)
            {
                Price = price;
                Labels = labels ?? Array.Empty<LevelLabel>();
                SourceId = sourceId ?? string.Empty;
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

            public Level(decimal price, LevelLabel[] labels, LevelLabel winner, string displayText)
            {
                Price = price;
                Labels = labels ?? Array.Empty<LevelLabel>();
                Winner = winner;
                DisplayText = displayText ?? string.Empty;
            }
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
                        _owner.TextOffset,
                        applyOffset: true,
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
                        applyOffset: false,
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
                entries = _entries;
                warnings = Array.Empty<string>();
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
            Description = "Paste MenthorQ output for the Index ticker (SPX / NDX / RUT). Offset below is added to these prices.",
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

        #endregion

        #region Ctor

        public MenthorQLevels()
            : base(useCandles: true)
        {
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Final);

            // Construct with the (likely empty) default key so later code can
            // call _api without null checks. The setter rebuilds it on change.
            _api = new MqApi(_apiKey);
        }

        #endregion

        #region Overrides

        protected override void OnCalculate(int bar, decimal value)
        {
            RebuildParsedEntriesIfNeeded();

            // Shell: _parsedEntries is populated but nothing consumes it yet.
            // Engine (dedup within source) lands in a later commit, followed by the renderer.
        }

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            // Shell: no drawing yet.
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

        private void RebuildParsedEntriesIfNeeded()
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

            for (int w = 0; w < winnerWarnings.Length; w++)
                this.LogWarn($"[{winner?.SourceId ?? "none"}] {winnerWarnings[w]}");

            _dataDirty = false;

            if (winner != null)
            {
                this.LogInfo($"MenthorQLevels: rebuilt {_parsedEntries.Length} entries " +
                             $"from {winner.SourceId} ({winnerWarnings.Length} warnings)");
            }
            else
            {
                this.LogInfo("MenthorQLevels: rebuilt 0 entries (no active source)");
            }
        }

        #endregion

        #region Private methods: parsing

        private static ParseResult ParseMenthorQText(string raw, decimal offset, bool applyOffset, string sourceIdBase)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return new ParseResult(Array.Empty<ParsedEntry>(), Array.Empty<string>());

            raw = raw.Trim();

            // Optional prefix: "$TICKER:" — parsed only for traceability.
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

            // Parse as label/price pairs: (0,1), (2,3), ...
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

                if (applyOffset)
                    price += offset;

                if (!TryMapMenthorQLabel(labelRaw, out var mapped))
                {
                    // Unknown label — keep as Other and still plot it.
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

                entries.Add(new ParsedEntry(price, new[] { lvlLabel }, sourceId));
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

                        entries.Add(new ParsedEntry(
                            price: (decimal)value.Value,
                            labels: new[] { lvlLabel },
                            sourceId: "MenthorQ:Api"));
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

        #endregion
    }
}
