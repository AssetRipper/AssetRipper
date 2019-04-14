using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class Skybox : Behaviour
	{
		public Skybox(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			CustomSkybox.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return CustomSkybox.FetchDependency(file, isLog, ToLogString, CustomSkyboxName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(CustomSkyboxName, CustomSkybox.ExportYAML(container));
			return node;
		}

		public const string CustomSkyboxName = "m_CustomSkybox";

		public PPtr<Material> CustomSkybox;
	}
}
