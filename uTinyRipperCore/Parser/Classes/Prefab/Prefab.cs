using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Prefabs;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// PrefabInstance later
	/// </summary>
	public sealed class Prefab : Object
	{
		public Prefab(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private Prefab(AssetInfo assetInfo, GameObject root) :
			base(assetInfo, HideFlags.HideInHierarchy)
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
			foreach (EditorExtension asset in root.FetchHierarchy())
			{
				yield return asset;
			}
		}

		private static int GetSerializedVersion(Version version)
		{
			// TODO:
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
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("m_Modification", Modification.ExportYAML(container));
			node.Add("m_ParentPrefab", ParentPrefab.ExportYAML(container));
			node.Add("m_RootGameObject", RootGameObject.ExportYAML(container));
			node.Add("m_IsPrefabParent", IsPrefabParent);
			return node;
		}

		public override string ExportExtension => PrefabKeyword;

		public bool IsPrefabParent { get; private set; }

#if DEBUG
		public string Name { get; private set; }
#endif
		
		public const string PrefabKeyword = "prefab";

		public PrefabModification Modification;
		/// <summary>
		/// SourcePrefab later
		/// Prefab previously
		/// Father previously
		/// </summary>
		public PPtr<Prefab> ParentPrefab;
		public PPtr<GameObject> RootGameObject;
	}
}
