using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Avatar
{
	public sealed class SkeletonBoneLimit : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			Min.Read(reader);
			Max.Read(reader);
			Value.Read(reader);
			Length = reader.ReadSingle();
			Modified = reader.ReadBoolean();
			reader.AlignStream();

		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(MinName, Min.ExportYaml(container));
			node.Add(MaxName, Max.ExportYaml(container));
			node.Add(ValueName, Value.ExportYaml(container));
			node.Add(LengthName, Length);
			node.Add(ModifiedName, Modified);
			return node;
		}

		public float Length { get; set; }
		public bool Modified { get; set; }

		public const string MinName = "m_Min";
		public const string MaxName = "m_Max";
		public const string ValueName = "m_Value";
		public const string LengthName = "m_Length";
		public const string ModifiedName = "m_Modified";

		public Vector3f Min = new();
		public Vector3f Max = new();
		public Vector3f Value = new();
	}
}
