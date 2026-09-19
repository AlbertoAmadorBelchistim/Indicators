#if ATAS_LATEST || ATAS_STABLE
namespace ATAS.Indicators.Technical;

using System;

/// <summary>
/// Compat for IInstrumentInfo.TimeZoneOffset (minute-precision offset, upstream PLAT-298),
/// which only exists in the Beta/Alpha/ATAS X SDKs. Latest and Stable expose the whole-hour
/// IInstrumentInfo.TimeZone only, so the offset is derived from it (minutes are lost, which
/// matches what those platforms can represent anyway).
/// Uses a C# 14 extension member so the upstream call sites stay untouched.
/// Remove this file once Latest and Stable ship TimeZoneOffset.
/// </summary>
internal static class InstrumentInfoCompat
{
	extension(IInstrumentInfo info)
	{
		public TimeSpan TimeZoneOffset => TimeSpan.FromHours(info.TimeZone);
	}
}
#endif
