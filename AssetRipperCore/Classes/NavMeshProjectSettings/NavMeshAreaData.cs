using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.NavMeshProjectSettings
{
	/// <summary>
	/// NavMeshLayerData previously
	/// </summary>
	public sealed class NavMeshAreaData : IAssetReadable, IYamlExportable
	{
		public NavMeshAreaData() { }
		public NavMeshAreaData(string name, float cost, int editType)
		{
			Name = name;
			Cost = cost;
			EditType = editType;
		}

		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasEditType(UnityVersion version) => version.IsLess(5);

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			Cost = reader.ReadSingle();
			if (HasEditType(reader.Version))
			{
				EditType = reader.ReadInt32();
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(NameName, Name);
			node.Add(CostName, Cost);
			return node;
		}

		public string Name { get; set; }
		public float Cost { get; set; }
		public int EditType { get; set; }

		public const string NameName = "name";
		public const string CostName = "cost";
	}
}
