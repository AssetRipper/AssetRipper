using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Utils;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Mesh
{
	public sealed class MeshBlendShape : IAsset
	{
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool HasName(UnityVersion version) => version.IsLess(4, 3);
		/// <summary>
		/// Less than 4.3
		/// </summary>
		public static bool HasAABB(UnityVersion version) => version.IsLess(4, 3);

		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(4, 3);

		public void Read(AssetReader reader)
		{
			if (HasName(reader.Version))
			{
				Name = reader.ReadString();
			}

			FirstVertex = reader.ReadUInt32();
			VertexCount = reader.ReadUInt32();
			if (HasAABB(reader.Version))
			{
				AabbMinDelta.Read(reader);
				AabbMaxDelta.Read(reader);
			}

			HasNormals = reader.ReadBoolean();
			HasTangents = reader.ReadBoolean();
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}
		}

		public void Write(AssetWriter writer)
		{
			if (HasName(writer.Version))
			{
				writer.Write(Name);
			}

			writer.Write(FirstVertex);
			writer.Write(VertexCount);
			if (HasAABB(writer.Version))
			{
				AabbMinDelta.Write(writer);
				AabbMaxDelta.Write(writer);
			}

			writer.Write(HasNormals);
			writer.Write(HasTangents);
			if (IsAlign(writer.Version))
			{
				writer.AlignStream();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			if (HasName(container.ExportVersion))
			{
				node.Add(NameName, Name);
			}

			node.Add(FirstVertexName, FirstVertex);
			node.Add(VertexCountName, VertexCount);
			if (HasAABB(container.ExportVersion))
			{
				node.Add(AabbMinDeltaName, AabbMinDelta.ExportYAML(container));
				node.Add(AabbMaxDeltaName, AabbMaxDelta.ExportYAML(container));
			}

			node.Add(HasNormalsName, HasNormals);
			node.Add(HasTangentsName, HasTangents);
			return node;
		}

		public bool IsCRCMatch(uint crc)
		{
			return CrcUtils.VerifyDigestUTF8(Name, crc);
		}

		public string Name { get; set; }
		public uint FirstVertex { get; set; }
		public uint VertexCount { get; set; }
		public bool HasNormals { get; set; }
		public bool HasTangents { get; set; }

		public const string NameName = "name";
		public const string FirstVertexName = "firstVertex";
		public const string VertexCountName = "vertexCount";
		public const string AabbMinDeltaName = "aabbMinDelta";
		public const string AabbMaxDeltaName = "aabbMaxDelta";
		public const string HasNormalsName = "hasNormals";
		public const string HasTangentsName = "hasTangents";

		public Vector3f AabbMinDelta = new();
		public Vector3f AabbMaxDelta = new();
	}
}
