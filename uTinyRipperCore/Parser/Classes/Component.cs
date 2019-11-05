using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public abstract class Component : EditorExtension
	{
		protected Component(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// Not Prefab
		/// </summary>
		public static bool HasGameObject(TransferInstructionFlags flags) => !flags.IsForPrefab();

		protected new static void GenerateTypeTree(TypeTreeContext context)
		{
			EditorExtension.GenerateTypeTree(context);
			if (HasGameObject(context.Flags))
			{
				context.AddPPtr(nameof(Classes.GameObject), GameObjectName);
			}
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

			if (HasGameObject(reader.Flags))
			{
				GameObject.Read(reader);
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasGameObject(writer.Flags))
			{
				GameObject.Write(writer);
			}
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

			yield return context.FetchDependency(GameObject, GameObjectName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(GameObjectName, GameObject.ExportYAML(container));
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public const string GameObjectName = "m_GameObject";

		public PPtr<GameObject> GameObject;
	}
}
