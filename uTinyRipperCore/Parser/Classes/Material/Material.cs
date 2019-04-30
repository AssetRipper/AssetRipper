using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Materials;
using uTinyRipper.SerializedFiles;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class Material : NamedObject
	{
		public Material(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 4.1.0b and greater
		/// </summary>
		public static bool IsReadKeywords(Version version)
		{
			return version.IsGreaterEqual(4, 1, 0, VersionType.Beta);
		}
		/// <summary>
		/// Less 5.0.0
		/// </summary>
		public static bool IsKeywordsArray(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadCustomRenderQueue(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadLightmapFlags(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadOtherFlags(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool IsReadStringTagMap(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadDisabledShaderPasses(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
			return 6;
		}

		public string FindPropertyNameByCRC28(uint crc)
		{
			return SavedProperties.FindPropertyNameByCRC28(crc);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Shader.Read(reader);
			if (IsReadKeywords(reader.Version))
			{
				if (IsKeywordsArray(reader.Version))
				{
					m_shaderKeywordsArray = reader.ReadStringArray();
				}
				else
				{
					ShaderKeywords = reader.ReadString();
				}
			}

			if (IsReadLightmapFlags(reader.Version))
			{
				LightmapFlags = reader.ReadUInt32();
				if (IsReadOtherFlags(reader.Version))
				{
					EnableInstancingVariants = reader.ReadBoolean();
					DoubleSidedGI = reader.ReadBoolean();
					reader.AlignStream(AlignType.Align4);
				}
			}

			if (IsReadCustomRenderQueue(reader.Version))
			{
				CustomRenderQueue = reader.ReadInt32();
			}

			if (IsReadStringTagMap(reader.Version))
			{
				m_stringTagMap = new Dictionary<string, string>();
				m_stringTagMap.Read(reader);
				if (IsReadDisabledShaderPasses(reader.Version))
				{
					m_disabledShaderPasses = reader.ReadStringArray();
				}
			}

			SavedProperties.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return Shader.FetchDependency(file, isLog, ToLogString, "m_Shader");
			foreach (Object asset in SavedProperties.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO:
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("m_Shader", Shader.ExportYAML(container));
			node.Add("m_ShaderKeywords", IsReadKeywords(container.Version) ? (IsKeywordsArray(container.Version) ? string.Join(" ", m_shaderKeywordsArray) : ShaderKeywords) : string.Empty);
			node.Add("m_LightmapFlags", LightmapFlags);
			node.Add("m_EnableInstancingVariants", EnableInstancingVariants);
			node.Add("m_DoubleSidedGI", DoubleSidedGI);
			node.Add("m_CustomRenderQueue", CustomRenderQueue);
			node.Add("stringTagMap", IsReadStringTagMap(container.Version) ? StringTagMap.ExportYAML() : YAMLMappingNode.Empty);
			node.Add("disabledShaderPasses", IsReadDisabledShaderPasses(container.Version) ? DisabledShaderPasses.ExportYAML() : YAMLSequenceNode.Empty);
			node.Add("m_SavedProperties", SavedProperties.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => "mat";

		public IReadOnlyList<string> ShaderKeywordsArray => m_shaderKeywordsArray;
		public string ShaderKeywords { get; private set; } = string.Empty;
		public int CustomRenderQueue { get; private set; }
		public uint LightmapFlags { get; private set; }
		public bool EnableInstancingVariants { get; private set; }
		public bool DoubleSidedGI { get; private set; }
		public IReadOnlyList<string> DisabledShaderPasses => m_disabledShaderPasses;
		public IReadOnlyDictionary<string, string> StringTagMap => m_stringTagMap;

		public PPtr<Shader> Shader;
		public UnityPropertySheet SavedProperties;

		private string[] m_shaderKeywordsArray = null;
		private string[] m_disabledShaderPasses = null;
		private Dictionary<string, string> m_stringTagMap;
	}
}
