#if !TAB_ATTRIBUTE_AVAILABLE
#nullable enable annotations
namespace OFT.Attributes;

using System;

/// <summary>
/// Stub for OFT.Attributes.TabAttribute, not available in Stable (v7). Every v8 flavor ships it since the 2026-09 builds.
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
