using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct Vector2f : IAssetReadable, IYAMLExportable
	{
		public Vector2f(float x, float y)
		{
			X = x;
			Y = y;
		}

		public void Read(AssetStream stream)
		{
			X = stream.ReadSingle();
			Y = stream.ReadSingle();
		}
		
		public float GetMember(int index)
		{
			if (index == 0)
			{
				return X;
			}
			if(index == 1)
			{
				return Y;
			}
			throw new ArgumentException($"Invalid index {index}", nameof(index));
		}
		
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Style = MappingStyle.Flow;
			node.Add("x", X);
			node.Add("y", Y);
			return node;
		}

		public override string ToString()
		{
			return $"[{X.ToString("0.00")}, {Y.ToString("0.00")}]";
		}

		public static Vector2f One { get; } = new Vector2f(1.0f, 1.0f);

		public float X { get; private set; }
		public float Y { get; private set; }
	}
}
