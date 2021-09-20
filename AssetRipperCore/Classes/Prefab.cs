using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Layout.Classes;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public sealed class Prefab : Object.Object
	{
		public Prefab(AssetLayout layout) : base(layout) { }

		public Prefab(AssetInfo assetInfo) : base(assetInfo) { }
		
		/// <summary>
		/// 2020 and greater
		/// </summary>
		public static bool HasHideFlagsBehavior(UnityVersion version) => version.IsGreaterEqual(2020);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			RootGameObject.Read(reader);

			if (HasHideFlagsBehavior(reader.Version))
			{
				HideFlagsBehavior = reader.ReadInt32();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			RootGameObject.Write(writer);
			
			if (HasHideFlagsBehavior(writer.Version))
			{
				writer.Write(HideFlagsBehavior);
			}
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
		public int HideFlagsBehavior { get; set; }
	}
}
