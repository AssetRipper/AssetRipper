using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Sprite
{
	public sealed class SpriteVertex : IAssetReadable, IYAMLExportable
	{
		public static int ToSerializedVersion(UnityVersion version)
		{
#warning TODO:
			return 2;
		}

		/// <summary>
		/// Less than 4.5.0 
		/// </summary>
		public static bool HasUV(UnityVersion version) => version.IsLess(4, 5);

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

		public Vector3f Position = new();
		public Vector2f UV = new();
	}
}
