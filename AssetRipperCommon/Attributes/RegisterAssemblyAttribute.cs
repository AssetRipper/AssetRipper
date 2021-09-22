using AssetRipper.Core.Parser.Files;
using System;

namespace AssetRipper.Core.Attributes
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class RegisterAssemblyAttribute : Attribute
	{
		string Version { get; }

		public RegisterAssemblyAttribute(string version)
		{
			Version = version;
		}

		public UnityVersion GetParsedVersion() => UnityVersion.Parse(Version);
	}
}
