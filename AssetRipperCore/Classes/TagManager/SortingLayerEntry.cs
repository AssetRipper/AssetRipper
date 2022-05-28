using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.TagManager
{
	public sealed class SortingLayerEntry : IAssetReadable, IYamlExportable
	{
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasUserID(UnityVersion version) => version.IsLess(5);

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			if (HasUserID(reader.Version))
			{
				UserID = reader.ReadUInt32();
			}
			UniqueID = reader.ReadUInt32();
			reader.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(NameName, Name);
			node.Add(UniqueIDName, UniqueID);
			node.Add(LockedName, false);
			return node;
		}

		public string Name { get; set; }
		public uint UserID { get; set; }
		public uint UniqueID { get; set; }

		public const string NameName = "name";
		public const string UniqueIDName = "uniqueID";
		public const string LockedName = "locked";
	}
}
