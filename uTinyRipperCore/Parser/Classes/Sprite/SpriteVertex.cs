using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;

namespace uTinyRipper.Classes.Sprites
{
	public struct SpriteVertex : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 4.5.0 
		/// </summary>
		public static bool IsReadUV(Version version)
		{
			return version.IsLess(4, 5);
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public void Read(AssetReader reader)
		{
			Position.Read(reader);
			if(IsReadUV(reader.Version))
			{
				UV.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("pos", Position.ExportYAML(container));
			/*if(IsReadUV)
			{
				node.Add("uv", UV.ExportYAML());
			}*/
			return node;
		}

		public Vector3f Position;
		public Vector2f UV;
	}
}
