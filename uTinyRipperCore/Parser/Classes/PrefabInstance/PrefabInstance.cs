using System.Collections.Generic;
using uTinyRipper.Classes.Prefabs;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// 2018.3 - Prefab has been renamed to PrefabInstance
	/// </summary>
	public sealed class PrefabInstance : Object
	{
		public PrefabInstance(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private PrefabInstance(AssetInfo assetInfo, GameObject root) :
			base(assetInfo, HideFlags.HideInHierarchy)
		{
			RootGameObject = root.File.CreatePPtr(root);
			IsPrefabParent = true;
#if DEBUG
			Name = root.Name;
#endif
		}

		public static PrefabInstance CreateVirtualInstance(VirtualSerializedFile virtualFile, GameObject root)
		{
			return virtualFile.CreateAsset((assetInfo) => new PrefabInstance(assetInfo, root));
		}

		public static int ToSerializedVersion(Version version)
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
			reader.AlignStream();
		}

		public IEnumerable<EditorExtension> FetchObjects(IAssetContainer file)
		{
			GameObject root = RootGameObject.GetAsset(file);
			foreach (EditorExtension asset in root.FetchHierarchy())
			{
				yield return asset;
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
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
			return $"{Name}({nameof(PrefabInstance)})";
#else
			return nameof(Prefab);
#endif
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ModificationName, Modification.ExportYAML(container));
			node.Add(ParentPrefabName, SourcePrefab.ExportYAML(container));
			node.Add(RootGameObjectName, RootGameObject.ExportYAML(container));
			node.Add(IsPrefabParentName, IsPrefabParent);
			return node;
		}

		public override string ExportExtension => PrefabKeyword;

		public bool IsPrefabParent { get; set; }

#if DEBUG
		public string Name { get; set; }
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
		public PPtr<PrefabInstance> SourcePrefab;
		public PPtr<GameObject> RootGameObject;
	}
}
