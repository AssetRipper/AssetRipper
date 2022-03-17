using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Texture2D
{
	public sealed class GLTextureSettings : UnityAssetBase, IGLTextureSettings
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
			// WrapMode has been replaced by WrapU
			if (version.IsGreaterEqual(2017))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		private static bool HasWraps(UnityVersion version) => version.IsGreaterEqual(2017);

		public bool HasWrapMode => !HasWraps(this.AssetUnityVersion);

		public override void Read(AssetReader reader)
		{
			FilterMode = (FilterMode)reader.ReadInt32();
			Aniso = reader.ReadInt32();
			MipBias = reader.ReadSingle();
			if (HasWraps(reader.Version))
			{
				WrapU = (TextureWrapMode)reader.ReadInt32();
				WrapV = (TextureWrapMode)reader.ReadInt32();
				WrapW = (TextureWrapMode)reader.ReadInt32();
			}
			else
			{
				WrapMode = (TextureWrapMode)reader.ReadInt32();
			}
		}

		public override void Write(AssetWriter writer)
		{
			writer.Write((int)FilterMode);
			writer.Write(Aniso);
			writer.Write(MipBias);
			if (HasWraps(writer.Version))
			{
				writer.Write((int)WrapU);
				writer.Write((int)WrapV);
				writer.Write((int)WrapW);
			}
			else
			{
				writer.Write((int)WrapMode);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(FilterModeName, (int)FilterMode);
			node.Add(AnisoName, Aniso);
			node.Add(MipBiasName, MipBias);
			if (HasWraps(container.ExportVersion))
			{
				node.Add(WrapUName, (int)WrapU);
				node.Add(WrapVName, (int)WrapV);
				node.Add(WrapWName, (int)WrapW);
			}
			else
			{
				node.Add(WrapModeName, (int)WrapMode);
			}
			return node;
		}

		public FilterMode FilterMode { get; set; }
		public int Aniso { get; set; }
		public float MipBias { get; set; }
		public TextureWrapMode WrapMode
		{
			get => WrapU;
			set => WrapU = value;
		}
		public TextureWrapMode WrapU { get; set; }
		public TextureWrapMode WrapV { get; set; }
		public TextureWrapMode WrapW { get; set; }

		public const string FilterModeName = "m_FilterMode";
		public const string AnisoName = "m_Aniso";
		public const string MipBiasName = "m_MipBias";
		public const string WrapModeName = "m_WrapMode";
		public const string WrapUName = "m_WrapU";
		public const string WrapVName = "m_WrapV";
		public const string WrapWName = "m_WrapW";
	}
}
