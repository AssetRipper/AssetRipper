using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Prefabs;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class Prefab : Object
	{
		public Prefab(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private Prefab(AssetInfo assetInfo, GameObject root) :
			base(assetInfo, 1)
		{
			RootGameObject = root.File.CreatePPtr(root);
			IsPrefabParent = true;
#if DEBUG
			Name = root.Name;
#endif
		}

		public static Prefab CreateVirtualInstance(VirtualSerializedFile virtualFile, GameObject root)
		{
			if (Config.IsGenerateGUIDByContent)
			{
				EngineGUID guid = ObjectUtils.CalculateAssetsGUID(FetchAssets(root));
				return virtualFile.CreateAsset(guid, (assetInfo) => new Prefab(assetInfo, root));
			}
			return virtualFile.CreateAsset((assetInfo) => new Prefab(assetInfo, root));
		}

		private static IEnumerable<EditorExtension> FetchAssets(GameObject root, bool isLog = false)
		{
			IReadOnlyList<EditorExtension> hierarchy = root.CollectHierarchy();
			foreach (EditorExtension asset in hierarchy)
			{
				yield return asset;
			}
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Modification.Read(reader);
			ParentPrefab.Read(reader);
			RootGameObject.Read(reader);
			IsPrefabParent = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align64);
		}

		public IEnumerable<EditorExtension> FetchObjects(ISerializedFile file, bool isLog = false)
		{
			GameObject root = RootGameObject.GetAsset(file);
			return FetchAssets(root);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return ParentPrefab.GetAsset(file);
			yield return RootGameObject.GetAsset(file);
		}

		public string GetName(ISerializedFile file)
		{
			return RootGameObject.GetAsset(file).Name;
		}

		public override string ToString()
		{
#if DEBUG
			return $"{Name}({nameof(Prefab)})";
#else
			return nameof(Prefab);
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Modification", Modification.ExportYAML(container));
			node.Add("m_ParentPrefab", ParentPrefab.ExportYAML(container));
			node.Add("m_RootGameObject", RootGameObject.ExportYAML(container));
			node.Add("m_IsPrefabParent", IsPrefabParent);
			return node;
		}

		public override string ExportExtension => "prefab";

		public bool IsPrefabParent { get; private set; }

#if DEBUG
		public string Name { get; private set; }
#endif

		public PrefabModification Modification;
		public PPtr<Prefab> ParentPrefab;
		public PPtr<GameObject> RootGameObject;
	}
}
