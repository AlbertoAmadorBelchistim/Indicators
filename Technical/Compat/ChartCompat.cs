#if ATAS_STABLE
namespace ATAS.Indicators.Technical;

using System;

/// <summary>
/// Compat for the v8 extension IChart.GetTimeFrameTypeChartPeriod(), missing in Stable (v7).
/// Mirrors the platform behaviour: only "TimeFrame" charts, with periods "M&lt;n&gt;", "H&lt;n&gt;",
/// "Daily" and "Weekly"; anything else throws ArgumentException like the SDK does.
/// Remove this file once Stable ships the extension.
/// </summary>
internal static class ChartCompat
{
	public static TimeSpan GetTimeFrameTypeChartPeriod(this IChart chartInfo)
	{
		ArgumentNullException.ThrowIfNull(chartInfo);

		if (chartInfo.ChartType != "TimeFrame")
			throw new ArgumentException("Chart type must be TimeFrame.", nameof(chartInfo));

		var timeFrame = chartInfo.TimeFrame ?? string.Empty;

		if (timeFrame.Contains('M', StringComparison.OrdinalIgnoreCase))
			return TimeSpan.FromMinutes(int.Parse(timeFrame.Replace("M", string.Empty, StringComparison.OrdinalIgnoreCase)));

		if (timeFrame.Contains('H', StringComparison.OrdinalIgnoreCase))
			return TimeSpan.FromHours(int.Parse(timeFrame.Replace("H", string.Empty, StringComparison.OrdinalIgnoreCase)));

		if (timeFrame.Equals("Daily", StringComparison.OrdinalIgnoreCase))
			return TimeSpan.FromDays(1);

		if (timeFrame.Equals("Weekly", StringComparison.OrdinalIgnoreCase))
			return TimeSpan.FromDays(7);

		throw new ArgumentException("Unsupported time frame: " + timeFrame, nameof(chartInfo));
	}
}
#endif
