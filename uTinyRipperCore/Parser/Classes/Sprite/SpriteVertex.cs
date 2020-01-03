using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Sprites
{
	public struct SpriteVertex : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(Version version)
		{
			// TODO:
			return 2;
		}

		/// <summary>
		/// Less than 4.5.0 
		/// </summary>
		public static bool HasUV(Version version) => version.IsLess(4, 5);

		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			if (HasUV(reader.Version))
			{
				UV.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(PosName, Position.ExportYAML(container));
			if (HasUV(container.ExportVersion))
			{
				node.Add(UvName, UV.ExportYAML(container));
			}
			return node;
		}

		public const string PosName = "pos";
		public const string UvName = "uv";

		public Vector3f Position;
		public Vector2f UV;
	}
}
