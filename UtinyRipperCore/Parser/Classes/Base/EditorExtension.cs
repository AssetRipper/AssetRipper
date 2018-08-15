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
			PrefabInternal = new PPtr<Prefab>();
		}
		
		/// <summary>
		/// Engine Package
		/// </summary>
		public static bool IsReadPrefab(TransferInstructionFlags flags)
		{
			return !flags.IsUnknown1() && !flags.IsSerializeForPrefabSystem();
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadPrefab(stream.Flags))
			{
				PrefabParentObject.Read(stream);
				PrefabInternal.Read(stream);
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
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			if (!PrefabParentObject.IsNull)
			{
				yield return PrefabParentObject.GetAsset(file);
			}
		}

		public IPPtr<Prefab> PrefabInternal { get; set; }

		private bool IsWritePrefab => ClassID != ClassIDType.Prefab;

		public PPtr<EditorExtension> PrefabParentObject;
	}
}
