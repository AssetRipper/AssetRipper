using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Axes : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			PreQ.Read(reader);
			PostQ.Read(reader);
			if (IsVector3(reader.Version))
			{
				Sgn = reader.ReadAsset<Vector3f>();
			}
			else
			{
				Sgn.Read(reader);
			}
			Limit.Read(reader);
			Length = reader.ReadSingle();
			Type = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PreQName, PreQ.ExportYAML(container));
			node.Add(PostQName, PostQ.ExportYAML(container));
			node.Add(SgnName, Sgn.ExportYAML(container));
			node.Add(LimitName, Limit.ExportYAML(container));
			node.Add(LengthName, Length);
			node.Add(TypeName, Type);
			return node;
		}

		public float Length { get; set; }
		public uint Type { get; set; }

		public const string PreQName = "m_PreQ";
		public const string PostQName = "m_PostQ";
		public const string SgnName = "m_Sgn";
		public const string LimitName = "m_Limit";
		public const string LengthName = "m_Length";
		public const string TypeName = "m_Type";

		public Vector4f PreQ;
		public Vector4f PostQ;
		public Vector4f Sgn;
		public Limit Limit;
	}
}
