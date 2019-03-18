using System.Collections.Generic;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.ShaderVariantCollections;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
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

			m_shaders = reader.ReadTTKVPArray<PPtr<Shader>, ShaderInfo>();
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (KeyValuePair<PPtr<Shader>, ShaderInfo> kvp in m_shaders)
			{
				yield return kvp.Key.FetchDependency(file, isLog, ToLogString, "m_Shaders");
			}
		}

		public IReadOnlyDictionary<PPtr<Shader>, ShaderInfo> GetShaders()
		{
			Dictionary<PPtr<Shader>, ShaderInfo> shaders = new Dictionary<PPtr<Shader>, ShaderInfo>();
			foreach (KeyValuePair<PPtr<Shader>, ShaderInfo> kvp in m_shaders)
			{
				if (!kvp.Key.IsNull)
				{
					shaders.Add(kvp.Key, kvp.Value);
				}
			}
			return shaders;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Shaders", GetShaders().ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "shadervariants";

		public ILookup<PPtr<Shader>, ShaderInfo> Shaders => m_shaders.ToLookup(t => t.Key, t => t.Value);

		private KeyValuePair<PPtr<Shader>, ShaderInfo>[] m_shaders;
	}
}
