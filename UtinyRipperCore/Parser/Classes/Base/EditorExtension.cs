using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
		/// Prefab
		/// </summary>
		public static bool IsReadPrefab(TransferInstructionFlags flags)
		{
			return !flags.IsUnknown1() && !flags.IsForPrefab();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadPrefab(reader.Flags))
			{
				PrefabParentObject.Read(reader);
				PrefabInternal.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (IsWritePrefab)
			{
				node.Add("m_PrefabParentObject", PrefabParentObject.ExportYAML(container));
				node.Add("m_PrefabInternal", PrefabInternal.ExportYAML(container));
			}
			return node;
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

		private bool IsWritePrefab => ClassID != ClassIDType.Prefab;
		
		public PPtr<EditorExtension> PrefabParentObject;
		public PPtr<Prefab> PrefabInternal;
	}
}
