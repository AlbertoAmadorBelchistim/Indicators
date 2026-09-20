namespace ATAS.Indicators.Technical.ClusterStatsCore;

using System;
using System.Collections.Generic;

/// <summary>A trade print: time, volume and side (1 buy, -1 sell, 0 unknown).</summary>
public readonly record struct Tick(DateTime Time, decimal Volume, int Side);

/// <summary>Highest volume rate of a bar and the delta rate at that moment.</summary>
public readonly record struct BarPeak(decimal VolumePerSecond, decimal DeltaPerSecond)
{
	/// <summary>Delta share of the volume at the peak, -1 to 1.</summary>
	public decimal DeltaPerVolume => VolumePerSecond == 0m ? 0m : DeltaPerSecond / VolumePerSecond;
}

/// <summary>
/// Speed of the tape: the volume traded in the last <c>window</c> seconds, divided by the window,
/// after every print. A bar's peak is the highest of these rates among the prints of the bar
/// (only while the window holds at least the minimum volume), with the delta rate of the same
/// moment. The window runs across bar boundaries. History and realtime feed the same prints in
/// time order, so they give the same peaks. Not synchronized.
/// </summary>
public sealed class PeakRateEngine
{
	private readonly Queue<Tick> _window = new();
	private readonly Dictionary<int, BarPeak> _peaks = new();
	private readonly decimal _seconds;
	private decimal _volume;
	private decimal _delta;

	public PeakRateEngine(int windowSeconds, decimal minVolume)
	{
		WindowSeconds = Math.Max(1, windowSeconds);
		MinVolume = minVolume;
		_seconds = WindowSeconds;
	}

	public int WindowSeconds { get; }

	public decimal MinVolume { get; }

	public void Clear()
	{
		_window.Clear();
		_peaks.Clear();
		_volume = 0m;
		_delta = 0m;
	}

	/// <summary>Adds a print of <paramref name="bar"/>. True when the bar's peak changed.</summary>
	public bool Add(int bar, Tick tick)
	{
		_window.Enqueue(tick);
		_volume += tick.Volume;
		_delta += tick.Side * tick.Volume;

		var cutoff = tick.Time.AddSeconds(-WindowSeconds);

		while (_window.Count > 0 && _window.Peek().Time <= cutoff)
		{
			var old = _window.Dequeue();
			_volume -= old.Volume;
			_delta -= old.Side * old.Volume;
		}

		if (_volume < MinVolume)
			return false;

		var rate = _volume / _seconds;

		if (_peaks.TryGetValue(bar, out var peak) && rate <= peak.VolumePerSecond)
			return false;

		_peaks[bar] = new BarPeak(rate, _delta / _seconds);
		return true;
	}

	public bool TryGet(int bar, out BarPeak peak) => _peaks.TryGetValue(bar, out peak);
}

/// <summary>
/// Mean of the recent bar peaks, used as the reference of the peak rows: an EMA or SMA of the
/// absolute peak of each closed bar. The reference of a bar is the mean of the bars before it,
/// so a bar is never compared with itself.
/// </summary>
public sealed class PeakMean
{
	private readonly bool _ema;
	private readonly int _period;
	private readonly Queue<decimal> _sma = new();
	private readonly SortedDictionary<int, decimal> _meanBefore = new();
	private decimal _sum;
	private decimal _value;
	private int _count;
	private int _lastBar = -1;

	public PeakMean(int period, bool ema)
	{
		_period = Math.Max(1, period);
		_ema = ema;
	}

	/// <summary>Mean of the closed bars added so far (0 before the first).</summary>
	public decimal Current => _count == 0 ? 0m : _value;

	/// <summary>Adds the final peak of a closed bar; bars must come in order, each once.</summary>
	public void AddClosedBar(int bar, decimal peak)
	{
		if (bar <= _lastBar)
			return;

		_meanBefore[bar] = Current;
		_lastBar = bar;
		var value = Math.Abs(peak);

		if (_ema)
		{
			var alpha = 2m / (_period + 1m);
			_value = _count == 0 ? value : alpha * value + (1m - alpha) * _value;
		}
		else
		{
			_sma.Enqueue(value);
			_sum += value;

			if (_sma.Count > _period)
				_sum -= _sma.Dequeue();

			_value = _sum / _sma.Count;
		}

		_count++;
	}

	/// <summary>The reference of a bar: the mean before it, or the current mean for the bar in progress.</summary>
	public decimal MeanFor(int bar) => _meanBefore.TryGetValue(bar, out var mean) ? mean : Current;

	public int LastBar => _lastBar;
}
