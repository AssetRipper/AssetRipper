using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Materials;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLightmapFlags(Version version)
		{
			return version.IsGreaterEqual(5);
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
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 6;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			Shader.Read(stream);
			if(IsReadKeywords(stream.Version))
			{
				if(IsKeywordsArray(stream.Version))
				{
					m_shaderKeywordsArray = stream.ReadStringArray();
				}
				else
				{
					ShaderKeywords = stream.ReadStringAligned();
				}
			}

			if(IsReadLightmapFlags(stream.Version))
			{
				LightmapFlags = stream.ReadUInt32();
				if (IsReadOtherFlags(stream.Version))
				{
					EnableInstancingVariants = stream.ReadBoolean();
					DoubleSidedGI = stream.ReadBoolean();
					stream.AlignStream(AlignType.Align4);
				}
			}

			if (IsReadCustomRenderQueue(stream.Version))
			{
				CustomRenderQueue = stream.ReadInt32();
			}

			if (IsReadStringTagMap(stream.Version))
			{
				m_stringTagMap = new Dictionary<string, string>();
				m_stringTagMap.Read(stream);
				if (IsReadDisabledShaderPasses(stream.Version))
				{
					m_disabledShaderPasses = stream.ReadStringArray();
				}
			}

			SavedProperties.Read(stream);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Shader.FetchDependency(file, isLog, ToLogString, "m_Shader");
			foreach(Object @object in SavedProperties.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.InsertSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Shader", Shader.ExportYAML(container));
			node.Add("m_ShaderKeywords", IsReadKeywords(container.Version) ? (IsKeywordsArray(container.Version) ? string.Join(" ", m_shaderKeywordsArray) : ShaderKeywords) : string.Empty);
			node.Add("m_LightmapFlags", LightmapFlags);
			node.Add("m_EnableInstancingVariants", EnableInstancingVariants);
			node.Add("m_DoubleSidedGI", DoubleSidedGI);
			node.Add("m_CustomRenderQueue", CustomRenderQueue);
			node.Add("stringTagMap", IsReadStringTagMap(container.Version) ? StringTagMap.ExportYAML() : YAMLMappingNode.Empty);
#warning TODO: untested
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
