using AssetRipper.Core.Project;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;
using Version = AssetRipper.Core.Parser.Files.Version;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public class BlendTreeConstant : IAssetReadable, IYAMLExportable
	{
		public BlendTreeConstant() { }
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool HasBlendEventArrayConstant(Version version) => version.IsLess(4, 5);

		public void Read(AssetReader reader)
		{
			NodeArray = reader.ReadAssetArray<OffsetPtr<BlendTreeNodeConstant>>();
			if (HasBlendEventArrayConstant(reader.Version))
			{
				BlendEventArrayConstant.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public OffsetPtr<BlendTreeNodeConstant>[] NodeArray { get; set; }

		public OffsetPtr<ValueArrayConstant> BlendEventArrayConstant = new();
	}
}
