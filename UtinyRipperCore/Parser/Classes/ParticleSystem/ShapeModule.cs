using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.ParticleSystems
{
	public struct ShapeModule : IAssetReadable, IYAMLExportable
	{
		/*private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}*/

		public void Read(AssetStream stream)
		{
			Enabled = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			Type = stream.ReadInt32();
			Angle = stream.ReadSingle();
			Length = stream.ReadSingle();
			BoxThickness.Read(stream);
			RadiusThickness = stream.ReadSingle();
			DonutRadius = stream.ReadSingle();
			Position.Read(stream);
			Rotation.Read(stream);
			Scale.Read(stream);
			PlacementMode = stream.ReadInt32();
			MeshMaterialIndex = stream.ReadInt32();
			MeshNormalOffset = stream.ReadSingle();
			Mesh.Read(stream);
			MeshRenderer.Read(stream);
			SkinnedMeshRenderer.Read(stream);
			UseMeshMaterialIndex = stream.ReadBoolean();
			UseMeshColors = stream.ReadBoolean();
			AlignToDirection = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
			
			RandomDirectionAmount = stream.ReadSingle();
			SphericalDirectionAmount = stream.ReadSingle();
			RandomPositionAmount = stream.ReadSingle();
			Radius.Read(stream);
			Arc.Read(stream);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			//node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("enabled", Enabled);
			node.Add("type", Type);
			node.Add("angle", Angle);
			node.Add("length", Length);
			node.Add("boxThickness", BoxThickness.ExportYAML(exporter));
			node.Add("radiusThickness", RadiusThickness);
			node.Add("donutRadius", DonutRadius);
			node.Add("m_Position", Position.ExportYAML(exporter));
			node.Add("m_Rotation", Rotation.ExportYAML(exporter));
			node.Add("m_Scale", Scale.ExportYAML(exporter));
			node.Add("placementMode", PlacementMode);
			node.Add("m_MeshMaterialIndex", MeshMaterialIndex);
			node.Add("m_MeshNormalOffset", MeshNormalOffset);
			node.Add("m_Mesh", Mesh.ExportYAML(exporter));
			node.Add("m_MeshRenderer", MeshRenderer.ExportYAML(exporter));
			node.Add("m_SkinnedMeshRenderer", SkinnedMeshRenderer.ExportYAML(exporter));
			node.Add("m_UseMeshMaterialIndex", UseMeshMaterialIndex);
			node.Add("m_UseMeshColors", UseMeshColors);
			node.Add("alignToDirection", AlignToDirection);
			node.Add("randomDirectionAmount", RandomDirectionAmount);
			node.Add("sphericalDirectionAmount", SphericalDirectionAmount);
			node.Add("randomPositionAmount", RandomPositionAmount);
			node.Add("radius", Radius.ExportYAML(exporter));
			node.Add("arc", Arc.ExportYAML(exporter));
			return node;
		}

		public bool Enabled { get; private set; }
		public int Type { get; private set; }
		public float Angle { get; private set; }
		public float Length { get; private set; }
		public float RadiusThickness { get; private set; }
		public float DonutRadius { get; private set; }
		public int PlacementMode { get; private set; }
		public int MeshMaterialIndex { get; private set; }
		public float MeshNormalOffset { get; private set; }
		public bool UseMeshMaterialIndex { get; private set; }
		public bool UseMeshColors { get; private set; }
		public bool AlignToDirection { get; private set; }
		public float RandomDirectionAmount { get; private set; }
		public float SphericalDirectionAmount { get; private set; }
		public float RandomPositionAmount { get; private set; }

		public Vector3f BoxThickness;
		public Vector3f Position;
		public Vector3f Rotation;
		public Vector3f Scale;
		public PPtr<Mesh> Mesh;
		public PPtr<MeshRenderer> MeshRenderer;
		public PPtr<SkinnedMeshRenderer> SkinnedMeshRenderer;
		public MultiModeParameter Radius;
		public MultiModeParameter Arc;
	}
}
