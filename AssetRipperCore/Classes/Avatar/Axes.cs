using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class Axes : IAssetReadable, IYAMLExportable
	{
		public Vector4f m_PreQ = new();
		public Vector4f m_PostQ = new();
		public Vector4f m_Sgn = new();
		public Limit m_Limit = new();
		public float m_Length { get; set; }
		public uint m_Type { get; set; }

		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		public static bool IsVector3f(UnityVersion version) => version.IsGreaterEqual(5, 4);

		public void Read(AssetReader reader)
		{
			m_PreQ.Read(reader);
			m_PostQ.Read(reader);
			if (IsVector3f(reader.Version))
			{
				m_Sgn = reader.ReadAsset<Vector3f>();
			}
			else
			{
				m_Sgn.Read(reader);
			}
			m_Limit.Read(reader);
			m_Length = reader.ReadSingle();
			m_Type = reader.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(PreQName, m_PreQ.ExportYAML(container));
			node.Add(PostQName, m_PostQ.ExportYAML(container));
			node.Add(SgnName, m_Sgn.ExportYAML(container));
			node.Add(LimitName, m_Limit.ExportYAML(container));
			node.Add(LengthName, m_Length);
			node.Add(TypeName, m_Type);
			return node;
		}

		public const string PreQName = "m_PreQ";
		public const string PostQName = "m_PostQ";
		public const string SgnName = "m_Sgn";
		public const string LimitName = "m_Limit";
		public const string LengthName = "m_Length";
		public const string TypeName = "m_Type";
	}
}
