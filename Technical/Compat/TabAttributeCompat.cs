#if !TAB_ATTRIBUTE_AVAILABLE
namespace OFT.Attributes;

using System;

/// <summary>
/// Stub for OFT.Attributes.TabAttribute, not yet available in Beta/Latest/Stable/ATAS_X builds.
/// Remove this file (or set TAB_ATTRIBUTE_AVAILABLE in csproj) once all target ATAS versions
/// include TabAttribute in OFT.Attributes.dll.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
internal sealed class TabAttribute : Attribute
{
	public string TabName { get; set; } = string.Empty;
	public int TabOrder { get; set; }
	public Type? ResourceType { get; set; }
}
#endif
