﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.Misc
{
	public struct OffsetPtr<T> : IAsset where T : IAssetReadable, IYAMLExportable, new()
	{
		public OffsetPtr(T instance)
		{
			Instance = instance;
		}

		public void Read(AssetReader reader)
		{
			Instance.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			if (Instance is IAssetWritable writable)
			{
				writable.Write(writer);
			}
			else
			{
				string typeName = Instance?.GetType().ToString() ?? typeof(T).ToString();
				throw new System.NotSupportedException($"Writing not supported for {typeName}");
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(DataName, Instance.ExportYAML(container));
			return node;
		}

		public const string DataName = "data";

		public T Instance = new();
	}
}
