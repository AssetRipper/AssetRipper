using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.NavMeshProjectSettings
{
	/// <summary>
	/// NavMeshLayerData previously
	/// </summary>
	public sealed class NavMeshAreaData : IAssetReadable, IYAMLExportable
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

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
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
