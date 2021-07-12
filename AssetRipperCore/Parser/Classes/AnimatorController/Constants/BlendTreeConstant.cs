using System;
using AssetRipper.Classes.Misc;
using AssetRipper.Converters;
using AssetRipper.YAML;

namespace AssetRipper.Classes.AnimatorControllers
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
