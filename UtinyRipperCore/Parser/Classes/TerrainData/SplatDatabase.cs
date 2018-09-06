using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.TerrainDatas
{
	public struct SplatDatabase : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 5.0.1 and greater
		/// </summary>
		public static bool IsReadColorSpace(Version version)
		{
			return version.IsGreaterEqual(5, 0, 1);
		}

		public void Read(AssetReader reader)
		{
			m_splats = reader.ReadArray<SplatPrototype>();
			m_alphaTextures = reader.ReadArray<PPtr<Texture2D>>();
			AlphamapResolution = reader.ReadInt32();
			BaseMapResolution = reader.ReadInt32();
			if (IsReadColorSpace(reader.Version))
			{
				ColorSpace = reader.ReadInt32();
				MaterialRequiresMetallic = reader.ReadBoolean();
				MaterialRequiresSmoothness = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (SplatPrototype prototype in Splats)
			{
				foreach(Object @object in prototype.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
			
			foreach (PPtr<Texture2D> alphaTexture in AlphaTextures)
			{
				yield return alphaTexture.FetchDependency(file, isLog, () => nameof(SplatDatabase), "m_AlphaTextures");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Splats", Splats.ExportYAML(container));
			node.Add("m_AlphaTextures", AlphaTextures.ExportYAML(container));
			node.Add("m_AlphamapResolution", AlphamapResolution);
			node.Add("m_BaseMapResolution", BaseMapResolution);
			node.Add("m_ColorSpace", ColorSpace);
			node.Add("m_MaterialRequiresMetallic", MaterialRequiresMetallic);
			node.Add("m_MaterialRequiresSmoothness", MaterialRequiresSmoothness);
			return node;
		}

		public IReadOnlyList<SplatPrototype> Splats => m_splats;
		public IReadOnlyList<PPtr<Texture2D>> AlphaTextures => m_alphaTextures;
		public int AlphamapResolution { get; private set; }
		public int BaseMapResolution { get; private set; }
		public int ColorSpace { get; private set; }
		public bool MaterialRequiresMetallic { get; private set; }
		public bool MaterialRequiresSmoothness { get; private set; }

		private SplatPrototype[] m_splats;
		private PPtr<Texture2D>[] m_alphaTextures;
	}
}
