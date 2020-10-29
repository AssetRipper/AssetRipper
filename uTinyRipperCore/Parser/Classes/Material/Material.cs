using System.Collections.Generic;
using uTinyRipper.Classes.Materials;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class Material : NamedObject
	{
		public Material(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 6;
		}

		/// <summary>
		/// 4.1.0b and greater
		/// </summary>
		public static bool HasKeywords(Version version) => version.IsGreaterEqual(4, 1, 0, VersionType.Beta);
		/// <summary>
		/// Less 5.0.0
		/// </summary>
		public static bool IsKeywordsArray(Version version) => version.IsLess(5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasCustomRenderQueue(Version version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasLightmapFlags(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasOtherFlags(Version version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasStringTagMap(Version version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasDisabledShaderPasses(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 5);

		public string FindPropertyNameByCRC28(uint crc)
		{
			return SavedProperties.FindPropertyNameByCRC28(crc);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Shader.Read(reader);
			if (HasKeywords(reader.Version))
			{
				if (IsKeywordsArray(reader.Version))
				{
					ShaderKeywordsArray = reader.ReadStringArray();
				}
				else
				{
					ShaderKeywords = reader.ReadString();
				}
			}

			if (HasLightmapFlags(reader.Version))
			{
				LightmapFlags = reader.ReadUInt32();
				if (HasOtherFlags(reader.Version))
				{
					EnableInstancingVariants = reader.ReadBoolean();
					DoubleSidedGI = reader.ReadBoolean();
					reader.AlignStream();
				}
			}

			if (HasCustomRenderQueue(reader.Version))
			{
				CustomRenderQueue = reader.ReadInt32();
			}

			if (HasStringTagMap(reader.Version))
			{
				StringTagMap = new Dictionary<string, string>();
				StringTagMap.Read(reader);
			}
			if (HasDisabledShaderPasses(reader.Version))
			{
				DisabledShaderPasses = reader.ReadStringArray();
			}

			SavedProperties.Read(reader);
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Shader, ShaderName);
			foreach (PPtr<Object> asset in context.FetchDependencies(SavedProperties, SavedPropertiesName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO:
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ShaderName, Shader.ExportYAML(container));
			node.Add(ShaderKeywordsName, HasKeywords(container.Version) ? (IsKeywordsArray(container.Version) ? string.Join(" ", ShaderKeywordsArray) : ShaderKeywords) : string.Empty);
			node.Add(LightmapFlagsName, LightmapFlags);
			node.Add(EnableInstancingVariantsName, EnableInstancingVariants);
			node.Add(DoubleSidedGIName, DoubleSidedGI);
			node.Add(CustomRenderQueueName, CustomRenderQueue);
			node.Add(StringTagMapName, HasStringTagMap(container.Version) ? StringTagMap.ExportYAML() : YAMLMappingNode.Empty);
			node.Add(DisabledShaderPassesName, HasDisabledShaderPasses(container.Version) ? DisabledShaderPasses.ExportYAML() : YAMLSequenceNode.Empty);
			node.Add(SavedPropertiesName, SavedProperties.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "mat";

		public string[] ShaderKeywordsArray { get; set; }
		public string ShaderKeywords { get; set; } = string.Empty;
		public int CustomRenderQueue { get; set; }
		public uint LightmapFlags { get; set; }
		public bool EnableInstancingVariants { get; set; }
		public bool DoubleSidedGI { get; set; }
		public string[] DisabledShaderPasses { get; set; }
		public Dictionary<string, string> StringTagMap { get; set; }

		public const string ShaderName = "m_Shader";
		public const string ShaderKeywordsName = "m_ShaderKeywords";
		public const string LightmapFlagsName = "m_LightmapFlags";
		public const string EnableInstancingVariantsName = "m_EnableInstancingVariants";
		public const string DoubleSidedGIName = "m_DoubleSidedGI";
		public const string CustomRenderQueueName = "m_CustomRenderQueue";
		public const string StringTagMapName = "stringTagMap";
		public const string DisabledShaderPassesName = "disabledShaderPasses";
		public const string SavedPropertiesName = "m_SavedProperties";

		public PPtr<Shader> Shader;
		public UnityPropertySheet SavedProperties;
	}
}
