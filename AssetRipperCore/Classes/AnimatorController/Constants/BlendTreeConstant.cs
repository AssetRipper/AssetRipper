using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System;


namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class BlendTreeConstant : IAssetReadable, IYamlExportable
	{
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool HasBlendEventArrayConstant(UnityVersion version) => version.IsLess(4, 5);

		public void Read(AssetReader reader)
		{
			NodeArray = reader.ReadAssetArray<OffsetPtr<BlendTreeNodeConstant>>();
			if (HasBlendEventArrayConstant(reader.Version))
			{
				BlendEventArrayConstant.Read(reader);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public OffsetPtr<BlendTreeNodeConstant>[] NodeArray { get; set; }

		public OffsetPtr<ValueArrayConstant> BlendEventArrayConstant = new();
	}
}
