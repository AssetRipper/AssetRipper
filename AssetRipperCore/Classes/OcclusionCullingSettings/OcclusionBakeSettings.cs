﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.OcclusionCullingSettings
{
	public sealed class OcclusionBakeSettings : IAsset
	{
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool HasViewCellSize(UnityVersion version) => version.IsLess(4, 3);

		public void Read(AssetReader reader)
		{
			if (HasViewCellSize(reader.Version))
			{
				ViewCellSize = reader.ReadSingle();
				BakeMode = reader.ReadUInt32();
				MemoryUsage = reader.ReadUInt32();
			}
			else
			{
				SmallestOccluder = reader.ReadSingle();
				SmallestHole = reader.ReadSingle();
				BackfaceThreshold = reader.ReadSingle();
			}
		}

		public void Write(AssetWriter writer)
		{
			if (HasViewCellSize(writer.Version))
			{
				writer.Write(ViewCellSize);
				writer.Write(BakeMode);
				writer.Write(MemoryUsage);
			}
			else
			{
				writer.Write(SmallestOccluder);
				writer.Write(SmallestHole);
				writer.Write(BackfaceThreshold);
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			if (HasViewCellSize(container.ExportVersion))
			{
				node.Add(ViewCellSizeName, ViewCellSize);
				node.Add(BakeModeName, BakeMode);
				node.Add(MemoryUsageName, MemoryUsage);
			}
			else
			{
				node.Add(SmallestOccluderName, SmallestOccluder);
				node.Add(SmallestHoleName, SmallestHole);
				node.Add(BackfaceThresholdName, BackfaceThreshold);
			}
			return node;
		}

		public float ViewCellSize { get; set; }
		public uint BakeMode { get; set; }
		public uint MemoryUsage { get; set; }
		public float SmallestOccluder { get; set; }
		public float SmallestHole { get; set; }
		public float BackfaceThreshold { get; set; }

		public const string ViewCellSizeName = "viewCellSize";
		public const string BakeModeName = "bakeMode";
		public const string MemoryUsageName = "memoryUsage";
		public const string SmallestOccluderName = "smallestOccluder";
		public const string SmallestHoleName = "smallestHole";
		public const string BackfaceThresholdName = "backfaceThreshold";
	}
}
