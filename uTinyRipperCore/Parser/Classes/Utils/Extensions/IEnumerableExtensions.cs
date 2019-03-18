using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.GameObjects;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public static class IEnumerableExtensions
	{
		public static int IndexOf<T>(this IEnumerable<T> _this, Func<T, bool> predicate)
		{
			int index = 0;
			foreach(T t in _this)
			{
				if(predicate(t))
				{
					return index;
				}
				index++;
			}
			return -1;
		}

		public static YAMLNode ExportYAML3(this IEnumerable<Vector4f> _this, IExportContainer container)
		{
			YAMLSequenceNode node = new YAMLSequenceNode();
			foreach (Vector4f vector in _this)
			{
				node.Add(vector.ExportYAML3(container));
			}
			return node;
		}
		
		public static YAMLNode ExportYAML(this IEnumerable<Float> _this, IExportContainer container)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Flow);
			foreach (Float value in _this)
			{
				node.Add(value.ExportYAML(container));
			}
			return node;
		}
		
		public static YAMLNode ExportYAML(this IEnumerable<ComponentPair> _this, IExportContainer container)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (ComponentPair pair in _this)
			{
				if(pair.Component.IsValid(container))
				{
					node.Add(pair.ExportYAML(container));
				}
			}
			return node;
		}
		
		public static YAMLNode ExportYAML<T>(this T[][] _this, IExportContainer container)
			where T: IYAMLExportable
		{
			return ((IEnumerable<IEnumerable<T>>)_this).ExportYAML(container);
		}
	}
}
