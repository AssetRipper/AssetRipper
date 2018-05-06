using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Avatars
{
	public struct Axes : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.6.3 and greater
		/// </summary>
		public static bool IsVector3(Version version)
		{
			return version.IsGreaterEqual(5, 6, 1);
		}

		public void Read(AssetStream stream)
		{
			PreQ.Read(stream);
			PostQ.Read(stream);
			if (IsVector3(stream.Version))
			{
				Sgn.Read3(stream);
			}
			else
			{
				Sgn.Read(stream);
			}
			Limit.Read(stream);
			Length = stream.ReadSingle();
			Type = stream.ReadUInt32();
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
