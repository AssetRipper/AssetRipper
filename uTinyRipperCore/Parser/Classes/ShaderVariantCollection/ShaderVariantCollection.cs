using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes.ShaderVariantCollections;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

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

			m_shaders = reader.ReadKVPTTArray<PPtr<Shader>, ShaderInfo>();
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(m_shaders.Select(t => t.Key), ShadersName))
			{
				yield return asset;
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
			node.Add(ShadersName, GetShaders().ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "shadervariants";

		public ILookup<PPtr<Shader>, ShaderInfo> Shaders => m_shaders.ToLookup(t => t.Key, t => t.Value);

		public const string ShadersName = "m_Shaders";

		private KeyValuePair<PPtr<Shader>, ShaderInfo>[] m_shaders;
	}
}
