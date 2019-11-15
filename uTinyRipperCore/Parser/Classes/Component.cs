using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public abstract class Component : EditorExtension
	{
		protected Component(AssetLayout layout) :
			base(layout)
		{
		}

		protected Component(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public GameObject GetRoot()
		{
			GameObject go = GameObject.GetAsset(File);
			return go.GetRoot();
		}

		public int GetRootDepth()
		{
			GameObject go = GameObject.GetAsset(File);
			return go.GetRootDepth();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			GameObject.Read(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			GameObject.Write(writer);
		}

		public sealed override void ExportBinary(IExportContainer container, Stream stream)
		{
			base.ExportBinary(container, stream);
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			ComponentLayout layout = context.Layout.Component;
			yield return context.FetchDependency(GameObject, layout.GameObjectName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			ComponentLayout layout = container.ExportLayout.Component;
			node.Add(layout.GameObjectName, GameObject.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public PPtr<GameObject> GameObject;
	}
}
