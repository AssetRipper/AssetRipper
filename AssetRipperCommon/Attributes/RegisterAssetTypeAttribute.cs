using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class RegisterAssetTypeAttribute : Attribute
	{
		public string TypeName { get; }
		public int IdNumber { get; }
		public string FullName { get; } //TODO: Replace with System.Type?

		public RegisterAssetTypeAttribute(string typeName, int idNumber, string fullName)
		{
			TypeName = typeName;
			IdNumber = idNumber;
			FullName = fullName;
		}
	}
}
