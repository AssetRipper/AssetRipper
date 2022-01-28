using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Material
{
#warning possible vector m_BuildTextureStacks 2020 and up
	public sealed class Material : NamedObject, IMaterial
	{
		public Material(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO:
			return 6;
		}

		/// <summary>
		/// 4.1.0b and greater
		/// </summary>
		public static bool HasKeywords(UnityVersion version) => version.IsGreaterEqual(4, 1, 0, UnityVersionType.Beta);
		/// <summary>
		/// Less 5.0.0
		/// </summary>
		public static bool IsKeywordsArray(UnityVersion version) => version.IsLess(5);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasCustomRenderQueue(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasLightmapFlags(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool HasOtherFlags(UnityVersion version) => version.IsGreaterEqual(5, 6);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasStringTagMap(UnityVersion version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 5.6.0b5 and greater
		/// </summary>
		public static bool HasDisabledShaderPasses(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 5);
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasBuildTextureStacks(UnityVersion version) => version.IsGreaterEqual(2020);

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

#warning Temp struct mismatch fix
			if (HasBuildTextureStacks(reader.Version))
			{
				reader.ReadInt32();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Shader, ShaderName);
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(SavedProperties, SavedPropertiesName))
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

		public PPtr<IShader> ShaderPtr => Shader.CastTo<IShader>();

		public const string ShaderName = "m_Shader";
		public const string ShaderKeywordsName = "m_ShaderKeywords";
		public const string LightmapFlagsName = "m_LightmapFlags";
		public const string EnableInstancingVariantsName = "m_EnableInstancingVariants";
		public const string DoubleSidedGIName = "m_DoubleSidedGI";
		public const string CustomRenderQueueName = "m_CustomRenderQueue";
		public const string StringTagMapName = "stringTagMap";
		public const string DisabledShaderPassesName = "disabledShaderPasses";
		public const string SavedPropertiesName = "m_SavedProperties";

		public PPtr<Shader.Shader> Shader = new();
		public UnityPropertySheet SavedProperties = new();
	}
}
