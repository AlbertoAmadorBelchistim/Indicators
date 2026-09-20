namespace ATAS.Indicators.Technical.TradeRecorderCore;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// One moment of the book and the tape, which is the part that cannot be rebuilt afterwards.
/// </summary>
/// <remarks>
/// Everything a chart can recompute later — delta, footprint, ranges, the path of the price — is
/// deliberately absent. What is here is what exists only while it is happening: the sizes resting
/// on each side, the spread, and where the position stood at that instant.
/// </remarks>
public readonly record struct Sample(
	DateTime TimeUtc,
	decimal Last,
	decimal Bid,
	decimal Ask,
	decimal BidSize,
	decimal AskSize,
	decimal BidDepth,
	decimal AskDepth,
	decimal PositionVolume);

/// <summary>A window of the most recent samples, overwritten in place.</summary>
/// <remarks>
/// The seconds before an entry can only be had by someone who was already recording, so the
/// buffer runs from the moment the indicator loads and is dumped when a trade appears.
/// </remarks>
public sealed class SampleRing
{
	private Sample[] _items;
	private int _next;
	private int _count;

	public SampleRing(int capacity)
	{
		Capacity = capacity < 1 ? 1 : capacity;
		_items = new Sample[Capacity];
	}

	public int Capacity { get; private set; }

	public int Count => _count;

	public void Resize(int capacity)
	{
		Capacity = capacity < 1 ? 1 : capacity;
		_items = new Sample[Capacity];
		_next = 0;
		_count = 0;
	}

	public void Add(Sample sample)
	{
		_items[_next] = sample;
		_next = (_next + 1) % Capacity;

		if (_count < Capacity)
			_count++;
	}

	public void Clear()
	{
		_next = 0;
		_count = 0;
	}

	/// <summary>The samples from oldest to newest, taking only those at or after a moment.</summary>
	public List<Sample> Since(DateTime fromUtc)
	{
		var result = new List<Sample>(_count);
		var start = _count == Capacity ? _next : 0;

		for (var i = 0; i < _count; i++)
		{
			var sample = _items[(start + i) % Capacity];

			if (sample.TimeUtc >= fromUtc)
				result.Add(sample);
		}

		return result;
	}
}

/// <summary>What the recorder is doing.</summary>
public enum CasePhase
{
	/// <summary>Flat, and nothing being recorded beyond the rolling window.</summary>
	Idle,

	/// <summary>A position is open.</summary>
	Open,

	/// <summary>Flat again, still recording the minutes after the exit.</summary>
	Tail,
}

/// <summary>What just changed, which is what the indicator acts on.</summary>
public enum CaseEvent
{
	None,
	Opened,
	Changed,
	Closed,
	Finished,
}

/// <summary>
/// Turns a series of position sizes into cases to record: one starts when the position leaves
/// flat and ends a while after it returns to flat, so what happened after the exit is kept too.
/// </summary>
public sealed class CaseState
{
	private decimal _volume;

	public CasePhase Phase { get; private set; } = CasePhase.Idle;

	public DateTime OpenedUtc { get; private set; }

	public DateTime ClosedUtc { get; private set; }

	/// <summary>Largest size the position reached, in absolute terms.</summary>
	public decimal PeakVolume { get; private set; }

	/// <summary>1 long, -1 short, 0 flat. Taken from the first size of the case.</summary>
	public int Side { get; private set; }

	public decimal Volume => _volume;

	/// <summary>Seconds recorded after the position goes flat.</summary>
	public int TailSeconds { get; set; } = 120;

	public CaseEvent Update(decimal volume, DateTime nowUtc)
	{
		var wasFlat = _volume == 0m;
		var isFlat = volume == 0m;
		_volume = volume;

		if (Phase == CasePhase.Idle)
		{
			if (isFlat)
				return CaseEvent.None;

			Phase = CasePhase.Open;
			OpenedUtc = nowUtc;
			Side = volume > 0m ? 1 : -1;
			PeakVolume = Math.Abs(volume);
			return CaseEvent.Opened;
		}

		if (Phase == CasePhase.Open)
		{
			if (!isFlat)
			{
				var size = Math.Abs(volume);

				if (size > PeakVolume)
					PeakVolume = size;

				return wasFlat ? CaseEvent.Opened : CaseEvent.Changed;
			}

			Phase = CasePhase.Tail;
			ClosedUtc = nowUtc;
			return CaseEvent.Closed;
		}

		// In the tail: a new position starts another case, and the old one is already written.
		if (!isFlat)
		{
			Phase = CasePhase.Open;
			OpenedUtc = nowUtc;
			ClosedUtc = default;
			Side = volume > 0m ? 1 : -1;
			PeakVolume = Math.Abs(volume);
			return CaseEvent.Opened;
		}

		if ((nowUtc - ClosedUtc).TotalSeconds >= TailSeconds)
		{
			Phase = CasePhase.Idle;
			Side = 0;
			PeakVolume = 0m;
			return CaseEvent.Finished;
		}

		return CaseEvent.None;
	}

	public void Reset()
	{
		Phase = CasePhase.Idle;
		_volume = 0m;
		Side = 0;
		PeakVolume = 0m;
		OpenedUtc = default;
		ClosedUtc = default;
	}
}

/// <summary>
/// The lines written to disk. One object per line, so a file can grow for months, be read by any
/// tool, and survive a crash without losing what was already written.
/// </summary>
public static class JsonLines
{
	/// <summary>Milliseconds everywhere, UTC everywhere: three clocks are enough to line up already.</summary>
	public const string TimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";

	public static string Sample(string caseId, string instrument, Sample sample)
	{
		var text = new StringBuilder(220);

		text.Append('{');
		Text(text, "case", caseId);
		text.Append(',');
		Text(text, "sym", instrument);
		text.Append(',');
		Time(text, "t", sample.TimeUtc);
		text.Append(',');
		Number(text, "last", sample.Last);
		text.Append(',');
		Number(text, "bid", sample.Bid);
		text.Append(',');
		Number(text, "ask", sample.Ask);
		text.Append(',');
		Number(text, "bid_size", sample.BidSize);
		text.Append(',');
		Number(text, "ask_size", sample.AskSize);
		text.Append(',');
		Number(text, "bid_depth", sample.BidDepth);
		text.Append(',');
		Number(text, "ask_depth", sample.AskDepth);
		text.Append(',');
		Number(text, "pos", sample.PositionVolume);
		text.Append('}');

		return text.ToString();
	}

	public static string Trade(
		string caseId,
		string instrument,
		string account,
		int side,
		decimal peakVolume,
		DateTime openedUtc,
		DateTime closedUtc,
		decimal openPrice,
		decimal closePrice,
		decimal pnl,
		int samples)
	{
		var text = new StringBuilder(320);

		text.Append('{');
		Text(text, "case", caseId);
		text.Append(',');
		Text(text, "sym", instrument);
		text.Append(',');
		Text(text, "account", account);
		text.Append(',');
		Text(text, "side", side > 0 ? "long" : "short");
		text.Append(',');
		Number(text, "qty", peakVolume);
		text.Append(',');
		Time(text, "opened", openedUtc);
		text.Append(',');
		Time(text, "closed", closedUtc);
		text.Append(',');
		Number(text, "open_price", openPrice);
		text.Append(',');
		Number(text, "close_price", closePrice);
		text.Append(',');
		Number(text, "pnl", pnl);
		text.Append(',');
		Number(text, "samples", samples);
		text.Append('}');

		return text.ToString();
	}

	/// <summary>Identifier of a case: the instrument and the moment it opened, to the millisecond.</summary>
	public static string CaseId(string instrument, DateTime openedUtc)
		=> Clean(instrument) + "-" + openedUtc.ToString("yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);

	private static void Text(StringBuilder text, string name, string value)
	{
		text.Append('"').Append(name).Append("\":\"").Append(Escape(value)).Append('"');
	}

	private static void Number(StringBuilder text, string name, decimal value)
	{
		text.Append('"').Append(name).Append("\":").Append(value.ToString(CultureInfo.InvariantCulture));
	}

	private static void Time(StringBuilder text, string name, DateTime value)
	{
		text.Append('"').Append(name).Append("\":\"").Append(value.ToString(TimeFormat, CultureInfo.InvariantCulture)).Append('"');
	}

	private static string Escape(string value)
	{
		if (string.IsNullOrEmpty(value))
			return string.Empty;

		var text = new StringBuilder(value.Length + 8);

		foreach (var c in value)
		{
			switch (c)
			{
				case '"':
					text.Append("\\\"");
					break;

				case '\\':
					text.Append("\\\\");
					break;

				case '\n':
				case '\r':
				case '\t':
					text.Append(' ');
					break;

				default:
					text.Append(c);
					break;
			}
		}

		return text.ToString();
	}

	/// <summary>An instrument code with nothing a file name would object to.</summary>
	private static string Clean(string value)
	{
		if (string.IsNullOrEmpty(value))
			return "unknown";

		var text = new StringBuilder(value.Length);

		foreach (var c in value)
			text.Append(char.IsLetterOrDigit(c) ? c : '_');

		return text.ToString();
	}
}
