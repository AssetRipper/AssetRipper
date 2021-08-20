using AssetRipper.Core.Project;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Layout.Classes;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Structure.Collections;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public abstract class EditorExtension : Object.Object
	{
		protected EditorExtension(AssetLayout layout) : base(layout) { }

		protected EditorExtension(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			EditorExtensionLayout layout = reader.Layout.EditorExtension;
			if (layout.HasExtensionPtr)
			{
				ExtensionPtr = reader.ReadAsset<PPtr<Object.Object>>();
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
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

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
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

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

		protected IEnumerable<PPtr<Object.Object>> FetchDependenciesObject(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
		}

		private PPtr<PrefabInstance.PrefabInstance> GetPrefabInstance(IExportContainer container)
		{
			if (container.ExportLayout.EditorExtension.HasPrefabInstanceInvariant)
			{
				return PrefabInstance;
			}

#warning TODO: set PrefabInstance for all assets in PrefabContainer
			if (container.ExportFlags.IsForPrefab())
			{
				PrefabExportCollection prefabCollection = (PrefabExportCollection)container.CurrentCollection;
				return prefabCollection.Asset.File.CreatePPtr((PrefabInstance.PrefabInstance)prefabCollection.Asset);
			}
			return default;
		}

		/// <summary>
		/// Editor Only
		/// </summary>
		public PPtr<EditorExtension> PrefabParentObject
		{
			get => CorrespondingSourceObject;
			set => CorrespondingSourceObject = value;
		}
		/// <summary>
		/// Editor Only
		/// </summary>
		public PPtr<PrefabInstance.PrefabInstance> PrefabInternal
		{
			get => PrefabInstance;
			set => PrefabInstance = value;
		}

#warning TODO: PPtr<EditorExtensionImpl>
		/// <summary>
		/// Editor Only
		/// </summary>
		public PPtr<Object.Object> ExtensionPtr { get; set; }
		/// <summary>
		/// Editor Only
		/// </summary>
		public PPtr<EditorExtension> CorrespondingSourceObject { get; set; }
		/// <summary>
		/// Editor Only
		/// </summary>
		public PPtr<PrefabInstance.PrefabInstance> PrefabInstance { get; set; }
		/// <summary>
		/// Editor Only
		/// </summary>
		public PPtr<Prefab> PrefabAsset { get; set; }
	}
}
