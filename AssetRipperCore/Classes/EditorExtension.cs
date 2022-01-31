using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.PrefabInstance;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
{
	public abstract class EditorExtension : Object.Object, IEditorExtension
	{
		protected EditorExtension(LayoutInfo layout) : base(layout) { }

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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
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
				node.Add(PrefabInstanceInvariantName(container.ExportVersion, container.ExportFlags), PrefabInstance.ExportYAML(container));
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

		protected IEnumerable<PPtr<IUnityObjectBase>> FetchDependenciesObject(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
		}

		public static string CorrespondingSourceObjectInvariantName(UnityVersion version, TransferInstructionFlags flags)
		{
			return HasCorrespondingSourceObject(version, flags) ? CorrespondingSourceObjectName : PrefabParentObjectName;
		}
		public static string PrefabInstanceInvariantName(UnityVersion version, TransferInstructionFlags flags)
		{
			return HasPrefabInstance(version, flags) ? PrefabInstanceName : PrefabInternalName;
		}

		private PPtr<Object.Object> ExtensionPtr => new();
		private PPtr<EditorExtension> CorrespondingSourceObject => new();
		private PPtr<Prefab> PrefabAsset => new();

		public PPtr<IPrefabInstance> PrefabInstance { get; set; } = new();

		public const string ExtensionPtrName = "m_ExtensionPtr";
		public const string CorrespondingSourceObjectName = "m_CorrespondingSourceObject";
		public const string PrefabParentObjectName = "m_PrefabParentObject";
		public const string PrefabInstanceName = "m_PrefabInstance";
		public const string PrefabInternalName = "m_PrefabInternal";
		public const string PrefabAssetName = "m_PrefabAsset";
	}
}
