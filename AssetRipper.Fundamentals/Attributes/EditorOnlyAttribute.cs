using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field)]
	public sealed class EditorOnlyAttribute : Attribute
	{
	}
}
