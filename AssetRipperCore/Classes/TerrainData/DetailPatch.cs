using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes.TerrainData
{
	public sealed class DetailPatch : IAsset
	{
		/// <summary>
		/// Less than 2020.2
		/// </summary>
		public static bool HasBounds(UnityVersion version) => version.IsLess(2020, 2);
		public void Read(AssetReader reader)
		{
			if (HasBounds(reader.Version))
			{
				Bounds.Read(reader);
			}

			LayerIndices = reader.ReadByteArray();
			reader.AlignStream();
			NumberOfObjects = reader.ReadByteArray();
			reader.AlignStream();
		}

		public void Write(AssetWriter writer)
		{
			if (HasBounds(writer.Version))
			{
				Bounds.Write(writer);
			}

			LayerIndices.Write(writer);
			writer.AlignStream();
			NumberOfObjects.Write(writer);
			writer.AlignStream();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			if (HasBounds(container.ExportVersion))
			{
				node.Add(BoundsName, Bounds.ExportYaml(container));
			}

			node.Add(LayerIndicesName, LayerIndices.ExportYaml());
			node.Add(NumberOfObjectsName, NumberOfObjects.ExportYaml());
			return node;
		}

		public byte[] LayerIndices { get; set; }
		public byte[] NumberOfObjects { get; set; }

		public const string BoundsName = "bounds";
		public const string LayerIndicesName = "layerIndices";
		public const string NumberOfObjectsName = "numberOfObjects";

		public AABB Bounds = new AABB();
	}
}
