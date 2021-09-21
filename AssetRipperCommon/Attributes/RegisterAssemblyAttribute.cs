using AssetRipper.Core.Parser.Files;
using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class RegisterAssemblyAttribute : Attribute
	{
		UnityVersion Version { get; }

		public RegisterAssemblyAttribute(UnityVersion version)
		{
			Version = version;
		}
	}
}
