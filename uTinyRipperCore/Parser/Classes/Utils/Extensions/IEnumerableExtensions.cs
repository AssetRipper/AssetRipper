using System;
using System.Collections.Generic;
using uTinyRipper.Classes.GameObjects;
using uTinyRipper.Converters;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Misc;

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
	}
}
