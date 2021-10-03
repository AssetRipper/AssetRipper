using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public abstract class EditorExtension : Object.Object
	{
		protected EditorExtension(AssetLayout layout) : base(layout) { }

		protected EditorExtension(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Less than 3.5.0 and Not Release
		/// </summary>
		public static bool HasExtensionPtr(UnityVersion version, TransferInstructionFlags flags) => version.IsLess(3, 5) && !flags.IsRelease();
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasCorrespondingSourceObjectInvariant(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && !flags.IsRelease();
		/// <summary>
		/// 2018.2 and greater and Not Release
		/// </summary>
		public static bool HasCorrespondingSourceObject(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2018, 2) && !flags.IsRelease();
		/// <summary>
		/// 3.5.0 to 2018.2 exclusive and Not Release
		/// </summary>
		public static bool HasPrefabParentObject(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && version.IsLess(2018, 2) && !flags.IsRelease();
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasPrefabInstanceInvariant(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && !flags.IsRelease();
		/// <summary>
		/// 2018.3 and greater and Not Release
		/// </summary>
		public static bool HasPrefabInstance(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2018, 3) && !flags.IsRelease();
		/// <summary>
		/// 3.5.0 to 2018.3 exclusive and Not Release
		/// </summary>
		public static bool HasPrefabInternal(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && version.IsLess(2018, 3) && !flags.IsRelease();
		/// <summary>
		/// 2018.3 and greater and Not Release
		/// </summary>
		public static bool HasPrefabAsset(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2018, 3) && !flags.IsRelease();

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			if (HasExtensionPtr(reader.Version, reader.Flags))
			{
				ExtensionPtr = reader.ReadAsset<PPtr<Object.Object>>();
			}
			if (HasCorrespondingSourceObject(reader.Version, reader.Flags))
			{
				CorrespondingSourceObject.Read(reader);
				PrefabInstance.Read(reader);
			}
			if (HasPrefabAsset(reader.Version, reader.Flags))
			{
				PrefabAsset.Read(reader);
			}
#endif
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

#if UNIVERSAL
			if (HasExtensionPtr(writer.Version, writer.Flags))
			{
				ExtensionPtr.Write(writer);
			}
			if (HasCorrespondingSourceObject(writer.Version, writer.Flags))
			{
				CorrespondingSourceObject.Write(writer);
				PrefabInstance.Write(writer);
			}
			if (HasPrefabAsset(writer.Version, writer.Flags))
			{
				PrefabAsset.Write(writer);
			}
#endif
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

#if UNIVERSAL
			if (HasExtensionPtr(context.Version, context.Flags))
			{
				yield return context.FetchDependency(ExtensionPtr, ExtensionPtrName);
			}
			if (HasCorrespondingSourceObject(context.Version, context.Flags))
			{
				yield return context.FetchDependency(CorrespondingSourceObject, CorrespondingSourceObjectInvariantName(context.Version, context.Flags));
				yield return context.FetchDependency(PrefabInstance, PrefabInstanceInvariantName(context.Version, context.Flags));
			}
			if (HasPrefabAsset(context.Version, context.Flags))
			{
				yield return context.FetchDependency(PrefabAsset, PrefabAssetName);
			}
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasExtensionPtr(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ExtensionPtrName, ExtensionPtr.ExportYAML(container));
			}
			if (HasCorrespondingSourceObject(container.ExportVersion, container.ExportFlags))
			{
				node.Add(CorrespondingSourceObjectInvariantName(container.ExportVersion, container.ExportFlags), CorrespondingSourceObject.ExportYAML(container));
				node.Add(PrefabInstanceInvariantName(container.ExportVersion, container.ExportFlags), GetPrefabInstance(container).ExportYAML(container));
			}
			if (HasPrefabAsset(container.ExportVersion, container.ExportFlags))
			{
				node.Add(PrefabAssetName, PrefabAsset.ExportYAML(container));
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
#if UNIVERSAL
			if (HasPrefabInstanceInvariant(container.ExportVersion, container.ExportFlags))
			{
				return PrefabInstance;
			}
#endif
#warning TODO: set PrefabInstance for all assets in PrefabContainer
			if (container.ExportFlags.IsForPrefab())
			{
				PrefabExportCollection prefabCollection = (PrefabExportCollection)container.CurrentCollection;
				return prefabCollection.Asset.File.CreatePPtr((PrefabInstance.PrefabInstance)prefabCollection.Asset);
			}
			return default;
		}

		public static string CorrespondingSourceObjectInvariantName(UnityVersion version, TransferInstructionFlags flags)
		{
			return HasCorrespondingSourceObject(version, flags) ? CorrespondingSourceObjectName : PrefabParentObjectName;
		}
		public static string PrefabInstanceInvariantName(UnityVersion version, TransferInstructionFlags flags)
		{
			return HasPrefabInstance(version, flags) ? PrefabInstanceName : PrefabInternalName;
		}

#if UNIVERSAL
		public PPtr<EditorExtension> PrefabParentObject
		{
			get => CorrespondingSourceObject;
			set => CorrespondingSourceObject = value;
		}
		public PPtr<PrefabInstance.PrefabInstance> PrefabInternal
		{
			get => PrefabInstance;
			set => PrefabInstance = value;
		}

#warning TODO: PPtr<EditorExtensionImpl>
		public PPtr<Object.Object> ExtensionPtr;
		public PPtr<EditorExtension> CorrespondingSourceObject;
		public PPtr<PrefabInstance.PrefabInstance> PrefabInstance;
		public PPtr<Prefab> PrefabAsset;
#else
		private PPtr<Object.Object> ExtensionPtr => default;
		private PPtr<EditorExtension> CorrespondingSourceObject => default;
		private PPtr<Prefab> PrefabAsset => default;
#endif

		public const string ExtensionPtrName = "m_ExtensionPtr";
		public const string CorrespondingSourceObjectName = "m_CorrespondingSourceObject";
		public const string PrefabParentObjectName = "m_PrefabParentObject";
		public const string PrefabInstanceName = "m_PrefabInstance";
		public const string PrefabInternalName = "m_PrefabInternal";
		public const string PrefabAssetName = "m_PrefabAsset";
	}
}
