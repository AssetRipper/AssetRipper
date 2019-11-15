using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public abstract class EditorExtension : Object
	{
		protected EditorExtension(AssetLayout layout) :
			base(layout)
		{
		}

		protected EditorExtension(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			EditorExtensionLayout layout = reader.Layout.EditorExtension;
			if (layout.HasExtensionPtr)
			{
				ExtensionPtr = reader.ReadAsset<PPtr<Object>>();
			}
			if (layout.HasCorrespondingSourceObject)
			{
				CorrespondingSourceObject.Read(reader);
				PrefabInstance.Read(reader);
			}
			if (layout.HasPrefabAsset)
			{
				PrefabAsset.Read(reader);
			}
#endif
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

#if UNIVERSAL
			EditorExtensionLayout layout = writer.Layout.EditorExtension;
			if (layout.HasExtensionPtr)
			{
				ExtensionPtr.Write(writer);
			}
			if (layout.HasCorrespondingSourceObject)
			{
				CorrespondingSourceObject.Write(writer);
				PrefabInstance.Write(writer);
			}
			if (layout.HasPrefabAsset)
			{
				PrefabAsset.Write(writer);
			}
#endif
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

#if UNIVERSAL
			EditorExtensionLayout layout = context.Layout.EditorExtension;
			if (layout.HasExtensionPtr)
			{
				yield return context.FetchDependency(ExtensionPtr, layout.ExtensionPtrName);
			}
			if (layout.HasCorrespondingSourceObject)
			{
				yield return context.FetchDependency(CorrespondingSourceObject, layout.CorrespondingSourceObjectInvariantName);
				yield return context.FetchDependency(PrefabInstance, layout.PrefabInstanceInvariantName);
			}
			if (layout.HasPrefabAsset)
			{
				yield return context.FetchDependency(PrefabAsset, layout.PrefabAssetName);
			}
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			EditorExtensionLayout layout = container.ExportLayout.EditorExtension;
			if (layout.HasExtensionPtr)
			{
				node.Add(layout.ExtensionPtrName, ExtensionPtr.ExportYAML(container));
			}
			if (layout.HasCorrespondingSourceObject)
			{
				node.Add(layout.CorrespondingSourceObjectInvariantName, CorrespondingSourceObject.ExportYAML(container));
				node.Add(layout.PrefabInstanceInvariantName, GetPrefabInstance(container).ExportYAML(container));
			}
			if (layout.HasPrefabAsset)
			{
				node.Add(layout.PrefabAssetName, PrefabAsset.ExportYAML(container));
			}
			return node;
		}

		protected void ReadObject(AssetReader reader)
		{
			base.Read(reader);
		}

		protected void WriteObject(AssetWriter writer)
		{
			base.Write(writer);
		}

		protected YAMLMappingNode ExportYAMLRootObject(IExportContainer container)
		{
			return base.ExportYAMLRoot(container);
		}

		protected IEnumerable<PPtr<Object>> FetchDependenciesObject(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
		}

		private PPtr<PrefabInstance> GetPrefabInstance(IExportContainer container)
		{
#if UNIVERSAL
			if (container.ExportLayout.EditorExtension.HasPrefabInstanceInvariant)
			{
				return PrefabInstance;
			}
#endif
#warning TODO: set PrefabInstance for all assets in PrefabContainer
			if (container.ExportFlags.IsForPrefab())
			{
				PrefabExportCollection prefabCollection = (PrefabExportCollection)container.CurrentCollection;
				return prefabCollection.Asset.File.CreatePPtr((PrefabInstance)prefabCollection.Asset);
			}
			return default;
		}

#if UNIVERSAL
		public PPtr<EditorExtension> PrefabParentObject
		{
			get => CorrespondingSourceObject;
			set => CorrespondingSourceObject = value;
		}
		public PPtr<PrefabInstance> PrefabInternal
		{
			get => PrefabInstance;
			set => PrefabInstance = value;
		}

#warning TODO: PPtr<EditorExtensionImpl>
		public PPtr<Object> ExtensionPtr;
		public PPtr<EditorExtension> CorrespondingSourceObject;
		public PPtr<PrefabInstance> PrefabInstance;
		public PPtr<Prefab> PrefabAsset;
#else
		private PPtr<Object> ExtensionPtr => default;
		private PPtr<EditorExtension> CorrespondingSourceObject => default;
		private PPtr<Prefab> PrefabAsset => default;
#endif
	}
}
