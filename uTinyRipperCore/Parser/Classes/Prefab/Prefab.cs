using System.Collections.Generic;
using uTinyRipper.Classes.Prefabs;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

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
				GUID guid = ObjectUtils.CalculateAssetsGUID(FetchAssets(root));
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
			SourcePrefab.Read(reader);
			RootGameObject.Read(reader);
			IsPrefabParent = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align64);
		}

		public IEnumerable<EditorExtension> FetchObjects(IAssetContainer file)
		{
			GameObject root = RootGameObject.GetAsset(file);
			return FetchAssets(root);
		}

		public override IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			foreach (Object asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(SourcePrefab, ParentPrefabName);
			yield return context.FetchDependency(RootGameObject, RootGameObjectName);
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
			node.Add(ModificationName, Modification.ExportYAML(container));
			node.Add(ParentPrefabName, SourcePrefab.ExportYAML(container));
			node.Add(RootGameObjectName, RootGameObject.ExportYAML(container));
			node.Add(IsPrefabParentName, IsPrefabParent);
			return node;
		}

		public override string ExportExtension => PrefabKeyword;

		public bool IsPrefabParent { get; private set; }

#if DEBUG
		public string Name { get; private set; }
#endif
		
		public const string PrefabKeyword = "prefab";

		public const string ModificationName = "m_Modification";
		public const string SourcePrefabName = "m_SourcePrefab";
		public const string ParentPrefabName = "m_ParentPrefab";
		public const string RootGameObjectName = "m_RootGameObject";
		public const string IsPrefabParentName = "m_IsPrefabParent";

		public PrefabModification Modification;
		/// <summary>
		/// ParentPrefab previously
		/// Prefab previously
		/// Father previously
		/// </summary>
		public PPtr<Prefab> SourcePrefab;
		public PPtr<GameObject> RootGameObject;
	}
}
