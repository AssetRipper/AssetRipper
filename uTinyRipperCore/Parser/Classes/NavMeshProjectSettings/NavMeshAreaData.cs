using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.NavMeshProjectSettingss
{
	/// <summary>
	/// NavMeshLayerData previously
	/// </summary>
	public struct NavMeshAreaData : IAssetReadable, IYAMLExportable
	{
		public NavMeshAreaData(string name, float cost, int editType)
		{
			Name = name;
			Cost = cost;
			EditType = editType;
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadEditType(Version version)
		{
			return version.IsLess(5);
		}

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Cost = reader.ReadSingle();
			if(IsReadEditType(reader.Version))
			{
				EditType = reader.ReadInt32();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("name", Name);
			node.Add("cost", Cost);
			return node;
		}

		public string Name { get; private set; }
		public float Cost { get; private set; }
		public int EditType { get; private set; }
	}
}
