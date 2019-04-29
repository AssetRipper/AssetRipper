using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Avatars
{
	public struct SkeletonBoneLimit : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			Min.Read(reader);
			Max.Read(reader);
			Value.Read(reader);
			Length = reader.ReadSingle();
			Modified = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(MinName, Min.ExportYAML(container));
			node.Add(MaxName, Max.ExportYAML(container));
			node.Add(ValueName, Value.ExportYAML(container));
			node.Add(LengthName, Length);
			node.Add(ModifiedName, Modified);
			return node;
		}

		public float Length { get; private set; }
		public bool Modified { get; private set; }

		public const string MinName = "m_Min";
		public const string MaxName = "m_Max";
		public const string ValueName = "m_Value";
		public const string LengthName = "m_Length";
		public const string ModifiedName = "m_Modified";

		public Vector3f Min;
		public Vector3f Max;
		public Vector3f Value;
	}
}
