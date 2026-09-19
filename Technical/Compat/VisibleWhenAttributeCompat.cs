#if ATAS_STABLE
#nullable enable annotations
namespace OFT.Attributes;

using System;

/// <summary>
/// No-op stub for OFT.Attributes.VisibleWhenAttribute, which ships in every v8 flavor but not in
/// Stable (v7). On Stable the property is simply always visible.
/// Remove this file once Stable ships VisibleWhenAttribute.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
internal sealed class VisibleWhenAttribute : Attribute
{
	public VisibleWhenAttribute(string propertyName, params object[] values)
	{
		PropertyName = propertyName;
		Values = values;
	}

	public string PropertyName { get; }

	public object[] Values { get; }
}
#endif
