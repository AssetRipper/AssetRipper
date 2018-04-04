using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Textures
{
	public struct TextureSettings : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadWraps(Version version)
		{
			return version.IsGreaterEqual(2017);
		}

		public static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(2017))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetStream stream)
		{
			FilterMode = (FilterMode)stream.ReadInt32();
			Aniso = stream.ReadInt32();
			MipBias = stream.ReadSingle();
			WrapU = (TextureWrapMode)stream.ReadInt32();
			if (IsReadWraps(stream.Version))
			{
				WrapV = (TextureWrapMode)stream.ReadInt32();
				WrapW = (TextureWrapMode)stream.ReadInt32();
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			int version = GetSerializedVersion(exporter.Version);
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(version);
			node.Add("m_FilterMode", (int)FilterMode);
			node.Add("m_Aniso", Aniso);
			node.Add("m_MipBias", MipBias);
			if (version == 1)
			{
				node.Add("m_WrapMode", (int)WrapU);
			}
			else
			{
				node.Add("m_WrapU", (int)WrapU);
				node.Add("m_WrapV", (int)WrapV);
				node.Add("m_WrapW", (int)WrapW);
			}
			return node;
		}

		public FilterMode FilterMode { get; private set; }
		public int Aniso { get; private set; }
		public float MipBias { get; private set; }
		/// <summary>
		/// WrapMode previously
		/// </summary>
		public TextureWrapMode WrapU { get; private set; }
		public TextureWrapMode WrapV { get; private set; }
		public TextureWrapMode WrapW { get; private set; }
	}
}
