using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class RegisterAssetTypeAttribute : Attribute
	{
		public string TypeName { get; }
		public int IdNumber { get; }
		public Type Type { get; }

		public RegisterAssetTypeAttribute(string typeName, int idNumber, Type type)
		{
			TypeName = typeName;
			IdNumber = idNumber;
			Type = type;
		}
	}
}
