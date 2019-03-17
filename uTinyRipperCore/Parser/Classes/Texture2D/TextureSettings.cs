using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Textures
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
			if (version.IsGreaterEqual(2017))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			FilterMode = (FilterMode)reader.ReadInt32();
			Aniso = reader.ReadInt32();
			MipBias = reader.ReadSingle();
			WrapU = (TextureWrapMode)reader.ReadInt32();
			if (IsReadWraps(reader.Version))
			{
				WrapV = (TextureWrapMode)reader.ReadInt32();
				WrapW = (TextureWrapMode)reader.ReadInt32();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(FilterModeName, (int)FilterMode);
			node.Add(AnisoName, Aniso);
			node.Add(MipBiasName, MipBias);
			if (IsReadWraps(container.ExportVersion))
			{
				node.Add(WrapUName, (int)WrapU);
				node.Add(WrapVName, (int)WrapV);
				node.Add(WrapWName, (int)WrapW);
			}
			else
			{
				node.Add(WrapModeName, (int)WrapU);
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

		public const string FilterModeName = "m_FilterMode";
		public const string AnisoName = "m_Aniso";
		public const string MipBiasName = "m_MipBias";
		public const string WrapModeName = "m_WrapMode";
		public const string WrapUName = "m_WrapU";
		public const string WrapVName = "m_WrapV";
		public const string WrapWName = "m_WrapW";
	}
}
