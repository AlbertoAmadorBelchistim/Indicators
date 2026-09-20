#if !TAB_ATTRIBUTE_AVAILABLE
namespace OFT.Attributes;

using System;

/// <summary>
/// Stable has no settings tabs. This stand-in lets the same source compile there; the platform
/// does not know it and shows the settings without tabs.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class TabAttribute : Attribute
{
	public string TabName { get; set; }

	public int TabOrder { get; set; }

	public Type ResourceType { get; set; }
}
#endif
