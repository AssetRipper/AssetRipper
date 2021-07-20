using AssetRipper.Converters.Project;
using AssetRipper.Layout;
using AssetRipper.Layout.Classes;
using AssetRipper.Parser.Asset;
using AssetRipper.Parser.Classes.Misc;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Asset.Writer;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Parser.Classes
{
	public sealed class Prefab : Object.Object
	{
		public Prefab(AssetLayout layout) :
			base(layout)
		{
		}

		public Prefab(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			RootGameObject.Read(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			RootGameObject.Write(writer);
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			PrefabLayout layout = context.Layout.Prefab;
			yield return context.FetchDependency(RootGameObject, layout.RootGameObjectName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			PrefabLayout layout = container.ExportLayout.Prefab;
			node.Add(layout.RootGameObjectName, RootGameObject.ExportYAML(container));
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.Prefab;

		public PPtr<GameObject.GameObject> RootGameObject;
	}
}
