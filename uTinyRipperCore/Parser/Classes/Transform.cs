using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public class Transform : Component
	{
		public Transform(AssetLayout layout):
			base(layout)
		{
			Children = Array.Empty<PPtr<Transform>>();
		}

		public Transform(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public string GetRootPath()
		{
			string pre = string.Empty;
			if (!Father.IsNull)
			{
				pre = Father.GetAsset(File).GetRootPath() + PathSeparator;
			}
			return pre + GameObject.GetAsset(File).Name;
		}

		public int GetSiblingIndex()
		{
			if (Father.IsNull)
			{
				return 0;
			}
			Transform father = Father.GetAsset(File);
			for (int i = 0; i < father.Children.Length; i++)
			{
				PPtr<Transform> child = father.Children[i];
				if (child.PathID == PathID)
				{
					return i;
				}
			}
			throw new Exception("Transorm hasn't been found among father's children");
		}

		public Transform FindChild(string path)
		{
			if (path.Length == 0)
			{
				return this;
			}
			return FindChild(path, 0);
		}

		public override Object Convert(IExportContainer container)
		{
			return TransformConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			LocalRotation.Read(reader);
			LocalPosition.Read(reader);
			LocalScale.Read(reader);
			Children = reader.ReadAssetArray<PPtr<Transform>>();
			Father.Read(reader);
#if UNIVERSAL
			TransformLayout layout = reader.Layout.Transform;
			if (layout.HasRootOrder)
			{
				RootOrder = reader.ReadInt32();
			}
			if (layout.HasLocalEulerAnglesHint)
			{
				LocalEulerAnglesHint.Read(reader);
			}
#endif
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			LocalRotation.Write(writer);
			LocalPosition.Write(writer);
			LocalScale.Write(writer);
			Children.Write(writer);
			Father.Write(writer);
#if UNIVERSAL
			TransformLayout layout = writer.Layout.Transform;
			if (layout.HasRootOrder)
			{
				writer.Write(RootOrder);
			}
			if (layout.HasLocalEulerAnglesHint)
			{
				LocalEulerAnglesHint.Write(writer);
			}
#endif
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			TransformLayout layout = context.Layout.Transform;
			foreach (PPtr<Object> asset in context.FetchDependencies(Children, layout.ChildrenName))
			{
				yield return asset;
			}
			yield return context.FetchDependency(Father, layout.FatherName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			TransformLayout layout = container.Layout.Transform;
			node.Add(layout.LocalRotationName, LocalRotation.ExportYAML(container));
			node.Add(layout.LocalPositionName, LocalPosition.ExportYAML(container));
			node.Add(layout.LocalScaleName, LocalScale.ExportYAML(container));
			node.Add(layout.ChildrenName, Children.ExportYAML(container));
			node.Add(layout.FatherName, Father.ExportYAML(container));
			node.Add(layout.RootOrderName, RootOrder);
			node.Add(layout.LocalEulerAnglesHintName, LocalEulerAnglesHint.ExportYAML(container));
			return node;
		}

		private Transform FindChild(string path, int startIndex)
		{
			int separatorIndex = path.IndexOf(PathSeparator, startIndex);
			string childName = separatorIndex == -1 ?
				path.Substring(startIndex, path.Length - startIndex) :
				path.Substring(startIndex, separatorIndex - startIndex);
			foreach (PPtr<Transform> childPtr in Children)
			{
				Transform child = childPtr.GetAsset(File);
				GameObject childGO = child.GameObject.GetAsset(File);
				if (childGO.Name == childName)
				{
					return separatorIndex == -1 ? child : child.FindChild(path, separatorIndex + 1);
				}
			}
			return null;
		}

		public PPtr<Transform>[] Children { get; set; }
#if UNIVERSAL
		public int RootOrder { get; set; }
#else
		private int RootOrder => GetSiblingIndex();
		private Vector3f LocalEulerAnglesHint => LocalRotation.ToEuler();
#endif

		public const char PathSeparator = '/';

		public Quaternionf LocalRotation;
		public Vector3f LocalPosition;
		public Vector3f LocalScale;
		public PPtr<Transform> Father;
#if UNIVERSAL
		public Vector3f LocalEulerAnglesHint;
#endif
	}
}
