using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public struct RectOffset : IScriptStructure
	{
		public RectOffset(RectOffset copy)
		{
			Left = copy.Left;
			Right = copy.Right;
			Top = copy.Top;
			Bottom = copy.Bottom;
		}

		public IScriptStructure CreateCopy()
		{
			return new RectOffset(this);
		}

		public void Read(AssetStream stream)
		{
			Left = stream.ReadSingle();
			Right = stream.ReadSingle();
			Top = stream.ReadSingle();
			Bottom = stream.ReadSingle();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("m_Left", Left);
			node.Add("m_Right", Right);
			node.Add("m_Top", Top);
			node.Add("m_Bottom", Bottom);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}

		public float Left { get; private set; }
		public float Right { get; private set; }
		public float Top { get; private set; }
		public float Bottom { get; private set; }
	}
}
