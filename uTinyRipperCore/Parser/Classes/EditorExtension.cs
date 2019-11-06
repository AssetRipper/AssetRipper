using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.YAML;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public abstract class EditorExtension : Object
	{
		protected EditorExtension(Version version):
			base(version)
		{
		}

		protected EditorExtension(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		protected EditorExtension(AssetInfo assetInfo, HideFlags hideFlags):
			base(assetInfo, hideFlags)
		{
		}

		/// <summary>
		/// Less than 3.5.0 and Not Release and Not Prefab
		/// </summary>
		public static bool HasExtensionPtr(Version version, TransferInstructionFlags flags) => HasEditorPtrs(flags) && version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 3.5.0 and greater and Not Release and Not Prefab
		/// </summary>
		public static bool HasCorrespondingSourceObject(Version version, TransferInstructionFlags flags) => HasEditorPtrs(flags) && version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2018.3 and Not Release and Not Prefab
		/// </summary>
		public static bool HasPrefabAsset(Version version, TransferInstructionFlags flags) => HasEditorPtrs(flags) && version.IsGreaterEqual(2018, 3);

		/// <summary>
		/// Not Release and Not Prefab
		/// </summary>
		private static bool HasEditorPtrs(TransferInstructionFlags flags) => !flags.IsRelease() && !flags.IsForPrefab();
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsCorrespondingSourceObjectName(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		private static bool IsPrefabInstanceName(Version version) => version.IsGreaterEqual(2018, 3);

		protected new static void GenerateTypeTree(TypeTreeContext context)
		{
			Object.GenerateTypeTree(context);
			if (HasEditorPtrs(context.Flags))
			{
				if (HasCorrespondingSourceObject(context.Version, context.Flags))
				{
					context.AddPPtr(nameof(EditorExtension), CorrespondingSourceObjectName);
					context.AddPPtr(Classes.PrefabInstance.GetPrefabInstanceName(context.Version), PrefabInstanceName);
				}
				else
				{
					context.AddPPtr(nameof(Object), ExtensionPtrName);
				}
				if (HasPrefabAsset(context.Version, context.Flags))
				{
					context.AddPPtr(nameof(Prefab), PrefabAssetName);
				}
			}
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			if (HasEditorPtrs(reader.Flags))
			{
				if (HasCorrespondingSourceObject(reader.Version, reader.Flags))
				{
					CorrespondingSourceObject.Read(reader);
					PrefabInstance.Read(reader);
				}
				else
				{
					ExtensionPtr = reader.ReadAsset<PPtr<Object>>();
				}
				if (HasPrefabAsset(reader.Version, reader.Flags))
				{
					PrefabAsset.Read(reader);
				}
			}
#endif
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

#if UNIVERSAL
			if (HasEditorPtrs(writer.Flags))
			{
				if (HasCorrespondingSourceObject(writer.Version, writer.Flags))
				{
					CorrespondingSourceObject.Write(writer);
					PrefabInstance.Write(writer);
				}
				else
				{
					ExtensionPtr.Write(writer);
				}
				if (HasPrefabAsset(writer.Version, writer.Flags))
				{
					PrefabAsset.Write(writer);
				}
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
			if (HasEditorPtrs(context.Flags))
			{
				if (HasCorrespondingSourceObject(context.Version, context.Flags))
				{
					yield return context.FetchDependency(CorrespondingSourceObject, CorrespondingSourceObjectName);
					yield return context.FetchDependency(PrefabInstance, PrefabInstanceName);
				}
				else
				{
					yield return context.FetchDependency(ExtensionPtr, ExtensionPtrName);
				}
				if (HasPrefabAsset(context.Version, context.Flags))
				{
					yield return context.FetchDependency(PrefabAsset, PrefabAssetName);
				}
			}
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasCorrespondingSourceObject(container.ExportVersion, container.ExportFlags))
			{
				node.Add(GetCorrespondingSourceObjectName(container.ExportVersion), CorrespondingSourceObject.ExportYAML(container));
				node.Add(GetPrefabInstanceName(container.ExportVersion), GetPrefabInstance(container).ExportYAML(container));
			}
			else if (HasExtensionPtr(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ExtensionPtrName, ExtensionPtr.ExportYAML(container));
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

		protected IEnumerable<PPtr<Object>> FetchDependenciesObject(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}
		}

		private string GetCorrespondingSourceObjectName(Version version)
		{
			return IsCorrespondingSourceObjectName(version) ? CorrespondingSourceObjectName : PrefabParentObjectName;
		}
		private string GetPrefabInstanceName(Version version)
		{
			return IsPrefabInstanceName(version) ? PrefabInstanceName : PrefabInternalName;
		}

		private PPtr<PrefabInstance> GetPrefabInstance(IExportContainer container)
		{
#if UNIVERSAL
			if (HasCorrespondingSourceObject(container.Version, container.Flags))
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
#warning TODO: PPtr<Extension>
		public PPtr<Object> ExtensionPtr
		{
			get => CorrespondingSourceObject.CastTo<Object>();
			set => CorrespondingSourceObject = value.CastTo<EditorExtension>();
		}
#else
		private PPtr<Object> ExtensionPtr => default;
		private PPtr<EditorExtension> CorrespondingSourceObject => default;
		private PPtr<PrefabInstance> PrefabInstance => default;
		private PPtr<Prefab> PrefabAsset => default;
#endif

		public const string ExtensionPtrName = "m_ExtensionPtr";
		public const string CorrespondingSourceObjectName = "m_CorrespondingSourceObject";
		public const string CorrespondingObjectFromSourceName = "m_CorrespondingObjectFromSource";
		public const string PrefabParentObjectName = "m_PrefabParentObject";
		public const string PrefabInstanceName = "m_PrefabInstance";
		public const string PrefabInternalName = "m_PrefabInternal";
		public const string PrefabName = "m_Prefab";
		public const string PrefabAssetName = "m_PrefabAsset";

#if UNIVERSAL
		/// <summary>
		/// CorrespondingObjectFromSource previously
		/// PrefabParentObject previously
		/// </summary>
		public PPtr<EditorExtension> CorrespondingSourceObject;
		/// <summary>
		/// PrefabInternal previously
		/// Prefab previously
		/// </summary>
		public PPtr<PrefabInstance> PrefabInstance;
		public PPtr<Prefab> PrefabAsset;
#endif
	}
}
