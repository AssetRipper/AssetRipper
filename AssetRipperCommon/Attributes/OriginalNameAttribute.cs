using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(
		AttributeTargets.Class |
		AttributeTargets.Struct |
		AttributeTargets.Enum |
		AttributeTargets.Method |
		AttributeTargets.Property |
		AttributeTargets.Field |
		AttributeTargets.Event |
		AttributeTargets.Interface |
		AttributeTargets.Parameter |
		AttributeTargets.Delegate,
		AllowMultiple = false)]
	public class OriginalNameAttribute : Attribute
	{
		public OriginalNameAttribute(string originalName)
		{
			OriginalName = originalName;
		}

		public string OriginalName { get; }

	}
}
