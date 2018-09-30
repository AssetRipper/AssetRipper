using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public abstract class EditorExtension : Object
	{
		protected EditorExtension(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		protected EditorExtension(AssetInfo assetInfo, uint hideFlags):
			base(assetInfo, hideFlags)
		{
		}

		/// <summary>
		/// Not Release and Not Prefab
		/// </summary>
		public static bool IsReadPrefabParentObject(TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && !flags.IsForPrefab();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadPrefabParentObject(reader.Flags))
			{
				PrefabParentObject.Read(reader);
				PrefabInternal.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			if (!PrefabParentObject.IsNull)
			{
				yield return PrefabParentObject.GetAsset(file);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_PrefabParentObject", PrefabParentObject.ExportYAML(container));
			node.Add("m_PrefabInternal", GetPrefabInternal(container).ExportYAML(container));
			return node;
		}

		private PPtr<Prefab> GetPrefabInternal(IExportContainer container)
		{
			if(IsReadPrefabParentObject(container.Flags))
			{
				return PrefabInternal;
			}
			if(container.ExportFlags.IsForPrefab())
			{
				PrefabExportCollection prefabCollection = (PrefabExportCollection)container.CurrentCollection;
				return prefabCollection.Asset.File.CreatePPtr((Prefab)prefabCollection.Asset);
			}
			return default;
		}

		/// <summary>
		/// CorrespondingSourceObject later
		/// </summary>
		public PPtr<EditorExtension> PrefabParentObject;
		/// <summary>
		/// Prefab previously
		/// </summary>
		public PPtr<Prefab> PrefabInternal;
	}
}
