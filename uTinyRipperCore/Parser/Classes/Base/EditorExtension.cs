using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;

namespace uTinyRipper.Classes
{
	public abstract class EditorExtension : Object
	{
		protected EditorExtension(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		protected EditorExtension(AssetInfo assetInfo, HideFlags hideFlags):
			base(assetInfo, hideFlags)
		{
		}

		/// <summary>
		/// Not Release and Not Prefab
		/// </summary>
		public static bool HasCorrespondingSourceObject(TransferInstructionFlags flags) => !flags.IsRelease() && !flags.IsForPrefab();
		/// <summary>
		/// 2018.3 and Not Release and Not Prefab
		/// </summary>
		public static bool HasPrefabAsset(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsForPrefab() && version.IsGreaterEqual(2018, 3);
		}

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		private static bool IsCorrespondingSourceObjectName(Version version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		private static bool IsPrefabInstanceName(Version version) => version.IsGreaterEqual(2018, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

#if UNIVERSAL
			if (HasCorrespondingSourceObject(reader.Flags))
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
			if (HasCorrespondingSourceObject(writer.Flags))
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

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

#if UNIVERSAL
			yield return CorrespondingSourceObject.FetchDependency(file);
			yield return PrefabInstance.FetchDependency(file);
			yield return PrefabAsset.FetchDependency(file);
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasCorrespondingSourceObject(container.ExportFlags))
			{
				node.Add(GetCorrespondingSourceObjectName(container.ExportVersion), CorrespondingSourceObject.ExportYAML(container));
				node.Add(GetPrefabInstanceName(container.ExportVersion), GetPrefabInstance(container).ExportYAML(container));
			}
			if (HasPrefabAsset(container.ExportVersion, container.ExportFlags))
			{
				node.Add(PrefabAssetName, PrefabAsset.ExportYAML(container));
			}
			return node;
		}

		private string GetCorrespondingSourceObjectName(Version version)
		{
			return IsCorrespondingSourceObjectName(version) ? CorrespondingSourceObjectName : PrefabParentObjectName;
		}
		private string GetPrefabInstanceName(Version version)
		{
			return IsPrefabInstanceName(version) ? PrefabInstanceName : PrefabInternalName;
		}

		private PPtr<Prefab> GetPrefabInstance(IExportContainer container)
		{
#if UNIVERSAL
			if (HasCorrespondingSourceObject(container.Flags))
			{
				return PrefabInstance;
			}
#endif
#warning TODO: set PrefabInstance for all assets in PrefabContainer
			if (container.ExportFlags.IsForPrefab())
			{
				PrefabExportCollection prefabCollection = (PrefabExportCollection)container.CurrentCollection;
				return prefabCollection.Asset.File.CreatePPtr((Prefab)prefabCollection.Asset);
			}
			return default;
		}

		public const string ExtensionPtrName = "m_ExtensionPtr";
		public const string CorrespondingSourceObjectName = "m_CorrespondingSourceObject";
		public const string CorrespondingObjectFromSourceName = "m_CorrespondingObjectFromSource";
		public const string PrefabParentObjectName = "m_PrefabParentObject";
		public const string PrefabInstanceName = "m_PrefabInstance";
		public const string PrefabInternalName = "m_PrefabInternal";
		public const string PrefabName = "m_Prefab";
		public const string PrefabAssetName = "m_PrefabAsset";

#if !UNIVERSAL
		private PPtr<EditorExtension> CorrespondingSourceObject => default;
		private PPtr<EditorExtension> PrefabInstance => default;
		private PPtr<EditorExtension> PrefabAsset => default;
#endif

#if UNIVERSAL
#warning TODO: m_ExtensionPtr
		/// <summary>
		/// CorrespondingObjectFromSource previously
		/// PrefabParentObject previously
		/// </summary>
		public PPtr<EditorExtension> CorrespondingSourceObject;
		/// <summary>
		/// PrefabInternal previously
		/// Prefab previously
		/// </summary>
		public PPtr<Prefab> PrefabInstance;
		public PPtr<Object> PrefabAsset;
#endif
	}
}
