using AssetRipper.Core.Math;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Imported
{
	public class ImportedFrame
	{
		public string Name { get; set; }
		public Vector3f LocalRotation { get; set; }
		public Vector3f LocalPosition { get; set; }
		public Vector3f LocalScale { get; set; }
		public ImportedFrame Parent { get; set; }

		private List<ImportedFrame> children;

		public ImportedFrame this[int i] => children[i];

		public int Count => children.Count;

		public string Path
		{
			get
			{
				var frame = this;
				var path = frame.Name;
				while (frame.Parent != null)
				{
					frame = frame.Parent;
					path = frame.Name + "/" + path;
				}
				return path;
			}
		}

		public ImportedFrame(int childrenCount = 0)
		{
			children = new List<ImportedFrame>(childrenCount);
		}

		public void AddChild(ImportedFrame obj)
		{
			children.Add(obj);
			obj.Parent?.Remove(obj);
			obj.Parent = this;
		}

		public void Remove(ImportedFrame frame)
		{
			children.Remove(frame);
		}

		public ImportedFrame FindFrameByPath(string path)
		{
			var name = path.Substring(path.LastIndexOf('/') + 1);
			foreach (var frame in FindChilds(name))
			{
				if (frame.Path.EndsWith(path, StringComparison.Ordinal))
				{
					return frame;
				}
			}
			return null;
		}

		public ImportedFrame FindFrame(string name)
		{
			if (Name == name)
			{
				return this;
			}
			foreach (var child in children)
			{
				var frame = child.FindFrame(name);
				if (frame != null)
				{
					return frame;
				}
			}
			return null;
		}

		public ImportedFrame FindChild(string name, bool recursive = true)
		{
			foreach (var child in children)
			{
				if (recursive)
				{
					var frame = child.FindFrame(name);
					if (frame != null)
					{
						return frame;
					}
				}
				else
				{
					if (child.Name == name)
					{
						return child;
					}
				}
			}
			return null;
		}

		public IEnumerable<ImportedFrame> FindChilds(string name)
		{
			if (Name == name)
			{
				yield return this;
			}
			foreach (var child in children)
			{
				foreach (var item in child.FindChilds(name))
				{
					yield return item;
				}
			}
		}
	}
}
