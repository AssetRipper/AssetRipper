using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class SkeletonBone : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			ParentName = reader.ReadString();
			Position.Read(reader);
			Rotation.Read(reader);
			Scale.Read(reader);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(NameName, Name);
			node.Add(ParentNameName, ParentName);
			node.Add(PositionName, Position.ExportYaml(container));
			node.Add(RotationName, Rotation.ExportYaml(container));
			node.Add(ScaleName, Scale.ExportYaml(container));
			return node;
		}

		public string Name { get; set; }
		public string ParentName { get; set; }

		public const string NameName = "m_Name";
		public const string ParentNameName = "m_ParentName";
		public const string PositionName = "m_Position";
		public const string RotationName = "m_Rotation";
		public const string ScaleName = "m_Scale";

		public Vector3f Position = new();
		public Quaternionf Rotation = new();
		public Vector3f Scale = new();
	}
}
