using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Classes.ShaderVariantCollection
{
	public sealed class ShaderVariantCollection : NamedObject
	{
		public ShaderVariantCollection(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			m_shaders = reader.ReadKVPTTArray<PPtr<Shader.Shader>, ShaderInfo>();
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object.Object> asset in context.FetchDependencies(m_shaders.Select(t => t.Key), ShadersName))
			{
				yield return asset;
			}
		}

		public IReadOnlyDictionary<PPtr<Shader.Shader>, ShaderInfo> GetShaders()
		{
			Dictionary<PPtr<Shader.Shader>, ShaderInfo> shaders = new Dictionary<PPtr<Shader.Shader>, ShaderInfo>();
			foreach (KeyValuePair<PPtr<Shader.Shader>, ShaderInfo> kvp in m_shaders)
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

		public ILookup<PPtr<Shader.Shader>, ShaderInfo> Shaders => m_shaders.ToLookup(t => t.Key, t => t.Value);

		public const string ShadersName = "m_Shaders";

		private KeyValuePair<PPtr<Shader.Shader>, ShaderInfo>[] m_shaders;
	}
}
