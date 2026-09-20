namespace ATAS.Indicators.Technical.PropRisk;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;

/// <summary>Everything PropRiskMonitor keeps for one account between sessions.</summary>
public sealed class AccountState
{
	public const int CurrentSchema = 1;

	public int SchemaVersion { get; set; } = CurrentSchema;

	public DateTime SavedUtc { get; set; }

	public TrailingState Trailing { get; set; } = new();

	public DailyState Daily { get; set; } = new();

	/// <summary>Trades of the day per instrument (security code): each chart keeps its own.</summary>
	public Dictionary<string, LedgerState> Ledgers { get; set; } = new();

	public AccountState Clone()
	{
		return new AccountState
		{
			SchemaVersion = SchemaVersion,
			SavedUtc = SavedUtc,
			Trailing = Trailing.Clone(),
			Daily = Daily.Clone(),
			Ledgers = Ledgers.ToDictionary(kv => kv.Key, kv => kv.Value.Clone())
		};
	}
}

/// <summary>
/// One JSON file per account. Writes go to a unique temporary file that replaces the old one
/// (keeping it as .bak), under a named mutex, so several charts or ATAS instances on the same
/// account do not collide. Before writing, the file on disk is merged in: peaks only go up, hits
/// stay hit, and each chart only writes its own instrument's trades. A file that cannot be read is
/// renamed to .corrupt-* (the backup is tried first) and never overwritten with an empty state.
/// </summary>
public sealed class StateStore
{
	private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

	private readonly string _directory;

	public StateStore(string directory)
	{
		_directory = directory;
	}

	/// <summary>Default folder: %APPDATA%\ATAS\PropRiskMonitor.</summary>
	public static string DefaultDirectory =>
		Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ATAS", "PropRiskMonitor");

	/// <summary>Messages about quarantined files, for the log.</summary>
	public event Action<string> Warning;

	public string PathOf(string accountKey) => Path.Combine(_directory, FileNameOf(accountKey));

	public AccountState Load(string accountKey)
	{
		using var gate = Lock(accountKey);
		return ReadUnlocked(accountKey) ?? new AccountState();
	}

	/// <summary>Saves the state merged with the file on disk; <paramref name="ownInstruments"/> are the ledgers this chart owns.</summary>
	public AccountState Save(string accountKey, AccountState state, ICollection<string> ownInstruments)
	{
		Directory.CreateDirectory(_directory);

		using var gate = Lock(accountKey);
		var onDisk = ReadUnlocked(accountKey);
		var merged = onDisk == null ? state.Clone() : Merge(onDisk, state, ownInstruments);
		merged.SchemaVersion = AccountState.CurrentSchema;
		merged.SavedUtc = DateTime.UtcNow;

		var path = PathOf(accountKey);
		var temp = $"{path}.{Guid.NewGuid():N}.tmp";

		File.WriteAllText(temp, JsonSerializer.Serialize(merged, Json), Encoding.UTF8);

		if (File.Exists(path))
			File.Replace(temp, path, path + ".bak", true);
		else
			File.Move(temp, path);

		return merged;
	}

	/// <summary>Merges this chart's state into the one on disk.</summary>
	public static AccountState Merge(AccountState onDisk, AccountState mine, ICollection<string> ownInstruments)
	{
		var result = mine.Clone();

		// Trailing: same start (no reset in between) -> monotonic merge; otherwise the latest reset wins.
		var t = onDisk.Trailing;

		if (t.Initialized && result.Trailing.Initialized && t.StartEquity == result.Trailing.StartEquity)
		{
			result.Trailing.PeakEquity = Math.Max(result.Trailing.PeakEquity, t.PeakEquity);
			result.Trailing.Breached |= t.Breached;

			if (t.LastDay > result.Trailing.LastDay)
			{
				result.Trailing.LastDay = t.LastDay;
				result.Trailing.LastEquity = t.LastEquity;
			}
		}
		else if (t.Initialized && !result.Trailing.Initialized)
			result.Trailing = t.Clone();

		// Daily: a later day on disk wins; the same day merges peaks and hits.
		var d = onDisk.Daily;

		if (d.Initialized && (!result.Daily.Initialized || d.Day > result.Daily.Day))
			result.Daily = d.Clone();
		else if (d.Initialized && d.Day == result.Daily.Day)
		{
			result.Daily.PeakEquity = Math.Max(result.Daily.PeakEquity, d.PeakEquity);
			result.Daily.LowEquity = Math.Min(result.Daily.LowEquity, d.LowEquity);
			result.Daily.StopHit |= d.StopHit;
			result.Daily.TargetHit |= d.TargetHit;
		}

		// Ledgers: keep the other charts' instruments as they are on disk.
		foreach (var (instrument, ledger) in onDisk.Ledgers)
		{
			if (!ownInstruments.Contains(instrument))
				result.Ledgers[instrument] = ledger.Clone();
		}

		return result;
	}

	public static string FileNameOf(string accountKey)
	{
		var name = new StringBuilder();

		foreach (var c in string.IsNullOrWhiteSpace(accountKey) ? "unknown" : accountKey.Trim())
			name.Append(char.IsLetterOrDigit(c) || c is '-' or '_' or '.' ? c : '_');

		return $"{name}.state.json";
	}

	private AccountState ReadUnlocked(string accountKey)
	{
		var path = PathOf(accountKey);

		if (!File.Exists(path))
			return null;

		if (TryRead(path, out var state))
			return state;

		Quarantine(path);

		var backup = path + ".bak";

		if (File.Exists(backup) && TryRead(backup, out state))
		{
			Warning?.Invoke($"state of {accountKey} was unreadable; restored from the backup.");
			return state;
		}

		Warning?.Invoke($"state of {accountKey} was unreadable and has no usable backup; starting a new state.");
		return null;
	}

	private static bool TryRead(string path, out AccountState state)
	{
		state = null;

		try
		{
			state = JsonSerializer.Deserialize<AccountState>(File.ReadAllText(path, Encoding.UTF8), Json);
			return state != null && state.SchemaVersion <= AccountState.CurrentSchema
				&& state.Trailing != null && state.Daily != null && state.Ledgers != null;
		}
		catch (Exception ex) when (ex is JsonException or IOException or NotSupportedException)
		{
			return false;
		}
	}

	private void Quarantine(string path)
	{
		var target = $"{path}.corrupt-{DateTime.UtcNow:yyyyMMddHHmmssfff}";

		try
		{
			File.Move(path, target);
			Warning?.Invoke($"unreadable state file kept as {Path.GetFileName(target)}.");
		}
		catch (IOException)
		{
		}
	}

	private static IDisposable Lock(string accountKey)
	{
		var mutex = new Mutex(false, "PropRiskMonitor." + FileNameOf(accountKey));

		try
		{
			mutex.WaitOne(TimeSpan.FromSeconds(10));
		}
		catch (AbandonedMutexException)
		{
			// The previous owner died holding it; the mutex is ours now.
		}

		return new Releaser(mutex);
	}

	private sealed class Releaser : IDisposable
	{
		private readonly Mutex _mutex;

		public Releaser(Mutex mutex) => _mutex = mutex;

		public void Dispose()
		{
			try
			{
				_mutex.ReleaseMutex();
			}
			catch (ApplicationException)
			{
				// Not owned (the wait timed out).
			}

			_mutex.Dispose();
		}
	}
}
