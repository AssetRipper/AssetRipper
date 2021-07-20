using AssetRipper.Converters.Project;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using System;
using Version = AssetRipper.Parser.Files.File.Version.Version;

namespace AssetRipper.Parser.Classes.AnimatorController.Constants
{
	public struct BlendTreeConstant : IAssetReadable, IYAMLExportable
	{
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

		public OffsetPtr<ValueArrayConstant> BlendEventArrayConstant;
	}
}
