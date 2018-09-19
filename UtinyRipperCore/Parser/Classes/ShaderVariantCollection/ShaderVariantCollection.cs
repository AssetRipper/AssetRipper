using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.ShaderVariantCollections;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class ShaderVariantCollection : NamedObject
	{
		public ShaderVariantCollection(AssetInfo assetInfo):
			base(assetInfo)
		{
		}
		
		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_shaders.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (PPtr <Shader> shader in Shaders.Keys)
			{
				yield return shader.FetchDependency(file, isLog, ToLogString, "m_Shaders");
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Shaders", Shaders.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "shadervariants";

		public IReadOnlyDictionary<PPtr<Shader>, ShaderInfo> Shaders => m_shaders;

		private Dictionary<PPtr<Shader>, ShaderInfo> m_shaders = new Dictionary<PPtr<Shader>, ShaderInfo>();
	}
}
