using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.GameObjects;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public static class IEnumerableExtensions
	{
		public static YAMLNode ExportYAML3(this IEnumerable<Vector4f> _this, IAssetsExporter exporter)
		{
			YAMLSequenceNode node = new YAMLSequenceNode();
			foreach (Vector4f vector in _this)
			{
				node.Add(vector.ExportYAML3(exporter));
			}
			return node;
		}
		
		public static YAMLNode ExportYAML(this IEnumerable<Float> _this, IAssetsExporter exporter)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Flow);
			foreach (Float value in _this)
			{
				node.Add(value.ExportYAML(exporter));
			}
			return node;
		}
		
		public static YAMLNode ExportYAML(this IEnumerable<ComponentPair> _this, IAssetsExporter exporter)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (ComponentPair pair in _this)
			{
				if(pair.Component.IsValid(exporter.File))
				{
					node.Add(pair.ExportYAML(exporter));
				}
			}
			return node;
		}
		
		public static YAMLNode ExportYAML<T>(this T[][] _this, IAssetsExporter exporter)
			where T: IYAMLExportable
		{
			return ((IEnumerable<IEnumerable<T>>)_this).ExportYAML(exporter);
		}
	}
}
