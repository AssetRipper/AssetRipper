using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.Sprites
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

		public void Read(AssetStream stream)
		{
			Position.Read(stream);
			if(IsReadUV(stream.Version))
			{
				UV.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("pos", Position.ExportYAML(exporter));
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
