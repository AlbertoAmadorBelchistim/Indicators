namespace ATAS.Indicators.Technical.Heatmap;

using System;
using System.Collections.Generic;
using OFT.Rendering.Heatmap;

internal sealed class HeatmapSeriesBuffer<TValue>
{
	private readonly object _sync = new();
	private readonly List<HeatmapIndicatorSample<TValue>> _samples = new();
	private long _lastTimestampNanos;

	public bool Append(long timestampNanos, TValue value)
	{
		lock (_sync)
		{
			if (timestampNanos < _lastTimestampNanos)
				return false;

			if (timestampNanos == _lastTimestampNanos)
			{
				if (_samples.Count == 0)
					return false;

				_samples[^1] = new HeatmapIndicatorSample<TValue>(timestampNanos, value);
				return true;
			}

			_lastTimestampNanos = timestampNanos;
			_samples.Add(new HeatmapIndicatorSample<TValue>(timestampNanos, value));
			return true;
		}
	}

	public HeatmapIndicatorSeriesSnapshot<TValue> GetSnapshot(HeatmapIndicatorSnapshotRequest request, bool isTraining = false)
	{
		lock (_sync)
		{
			if (_samples.Count == 0)
				return new HeatmapIndicatorSeriesSnapshot<TValue>([], isTraining);

			var effectiveEnd = request.EffectiveEndTimeNanos;
			var endIndex = effectiveEnd > 0
				? UpperBound(effectiveEnd)
				: _samples.Count;
			if (endIndex <= 0)
				return new HeatmapIndicatorSeriesSnapshot<TValue>([], isTraining);

			var startIndex = request.ViewStartTimeNanos > 0
				? LowerBound(request.ViewStartTimeNanos)
				: 0;
			if (request.IncludeLeadingSample && startIndex > 0)
				startIndex--;

			var count = endIndex - startIndex;
			if (count <= 0)
				return new HeatmapIndicatorSeriesSnapshot<TValue>([], isTraining);

			if (request.MaxSamplesPerSeries is { } maxSamples && maxSamples > 0 && count > maxSamples)
			{
				startIndex = Math.Max(startIndex, endIndex - maxSamples);
				count = endIndex - startIndex;
			}

			var samples = new HeatmapIndicatorSample<TValue>[count];
			_samples.CopyTo(startIndex, samples, 0, count);

			return new HeatmapIndicatorSeriesSnapshot<TValue>(samples, isTraining);
		}
	}

	public HeatmapIndicatorSample<TValue>? GetLatestAtOrBefore(long cutoffTimeNanos)
	{
		lock (_sync)
		{
			for (var i = _samples.Count - 1; i >= 0; i--)
			{
				var sample = _samples[i];
				if (cutoffTimeNanos <= 0 || sample.TimestampNanos <= cutoffTimeNanos)
					return sample;
			}

			return null;
		}
	}

	public void Clear()
	{
		lock (_sync)
		{
			_samples.Clear();
			_lastTimestampNanos = 0;
		}
	}

	private int LowerBound(long timestampNanos)
	{
		var lo = 0;
		var hi = _samples.Count;
		while (lo < hi)
		{
			var mid = lo + ((hi - lo) / 2);
			if (_samples[mid].TimestampNanos < timestampNanos)
				lo = mid + 1;
			else
				hi = mid;
		}

		return lo;
	}

	private int UpperBound(long timestampNanos)
	{
		var lo = 0;
		var hi = _samples.Count;
		while (lo < hi)
		{
			var mid = lo + ((hi - lo) / 2);
			if (_samples[mid].TimestampNanos <= timestampNanos)
				lo = mid + 1;
			else
				hi = mid;
		}

		return lo;
	}
}
