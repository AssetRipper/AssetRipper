using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public abstract class Component : EditorExtension, IComponent
	{
		protected Component(LayoutInfo layout) : base(layout) { }

		protected Component(AssetInfo assetInfo) : base(assetInfo) { }

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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(GameObject, GameObjectName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(GameObjectName, GameObject.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public PPtr<GameObject.GameObject> GameObject = new();

		public PPtr<GameObject.IGameObject> GameObjectPtr => GameObject.CastTo<GameObject.IGameObject>();

		public const string GameObjectName = "m_GameObject";
	}
}
