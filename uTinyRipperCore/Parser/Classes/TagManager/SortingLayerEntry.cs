using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TagManagers
{
	public struct SortingLayerEntry : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadUserID(Version version)
		{
			return version.IsLess(5);
		}

		public void Read(AssetReader reader)
		{
			Name = reader.ReadString();
			if(IsReadUserID(reader.Version))
			{
				UserID = reader.ReadUInt32();
			}
			UniqueID = reader.ReadUInt32();
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(NameName, Name);
			node.Add(UniqueIDName, UniqueID);
			node.Add(LockedName, false);
			return node;
		}

		public string Name { get; private set; }
		public uint UserID { get; private set; }
		public uint UniqueID { get; private set; }

		public const string NameName = "name";
		public const string UniqueIDName = "uniqueID";
		public const string LockedName = "locked";
	}
}
