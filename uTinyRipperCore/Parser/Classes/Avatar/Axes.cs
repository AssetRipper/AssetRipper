using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct Axes : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public void Read(AssetReader reader)
		{
			PreQ.Read(reader);
			PostQ.Read(reader);
			if (IsVector3(reader.Version))
			{
				Sgn.Read3(reader);
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
			node.Add("m_PreQ", PreQ.ExportYAML(container));
			node.Add("m_PostQ", PostQ.ExportYAML(container));
			node.Add("m_Sgn", Sgn.ExportYAML(container));
			node.Add("m_Limit", Limit.ExportYAML(container));
			node.Add("m_Length", Length);
			node.Add("m_Type", Type);
			return node;
		}

		public float Length { get; private set; }
		public uint Type { get; private set; }

		public Vector4f PreQ;
		public Vector4f PostQ;
		public Vector4f Sgn;
		public Limit Limit;
	}
}
