using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Field)]
	public sealed class NotEditableAttribute : Attribute
	{
	}
}