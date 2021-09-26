using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes
	/// </summary>
	public class UnityAssetBase : IAssetNew
	{
		public void ReadDebug(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		public void ReadRelease(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		public void WriteDebug(AssetWriter writer)
		{
			throw new NotSupportedException();
		}

		public void WriteRelease(AssetWriter writer)
		{
			throw new NotSupportedException();
		}

		public YAMLNode ExportYAML(bool release)
		{
			if (release)
				return ExportYAMLRelease();
			else
				return ExportYAMLDebug();
		}

		public virtual YAMLNode ExportYAMLDebug()
		{
			throw new NotSupportedException();
		}

		public YAMLNode ExportYAMLRelease()
		{
			throw new NotSupportedException();
		}
	}
}
