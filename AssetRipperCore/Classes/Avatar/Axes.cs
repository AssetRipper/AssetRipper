using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Avatar
{
	public struct Axes : IAssetReadable, IYAMLExportable
	{
		public Vector4f m_PreQ;
		public Vector4f m_PostQ;
		public Vector4f m_Sgn;
		public Limit m_Limit;
		public float m_Length { get; set; }
		public uint m_Type { get; set; }

		public Axes(ObjectReader reader)
		{
			var version = reader.version;
			m_PreQ = reader.ReadVector4f();
			m_PostQ = reader.ReadVector4f();
			if (version[0] > 5 || (version[0] == 5 && version[1] >= 4)) //5.4 and up
			{
				m_Sgn = reader.ReadVector3f();
			}
			else
			{
				m_Sgn = reader.ReadVector4f();
			}
			m_Limit = new Limit(reader);
			m_Length = reader.ReadSingle();
			m_Type = reader.ReadUInt32();
		}

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
