using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.Prefabs;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class Prefab : Object
	{
		public Prefab(AssetInfo assetInfo):
			base(assetInfo)
		{
			ThisPrefab = new PrefabPtr(this);
		}

		public Prefab(GameObject root) :
			base(CreateAssetsInfo(root))
		{
			RootGameObject = new PPtr<GameObject>(root);
			ThisPrefab = new PrefabPtr(this);
			IsPrefabParent = true;
			ObjectHideFlags = 1;
		}

		private static AssetInfo CreateAssetsInfo(GameObject root)
		{
			AssetInfo assetInfo = new AssetInfo(root.File, 0, ClassIDType.Prefab);
			if(Config.IsGenerateGUIDByContent)
			{
				assetInfo.GUID = ObjectUtils.CalculateObjectsGUID(FetchObjects(root));
			}
			return assetInfo;
		}
		
		private static IEnumerable<EditorExtension> FetchObjects(GameObject root, bool isLog = false)
		{
			List<Object> dependencies = ObjectUtils.CollectDependencies(root, isLog);
			IEnumerable<Object> deps = dependencies.Where(t => !t.IsAsset);
#warning dependency on other prefab's component?

#warning: TODO: OrderBy inside exporter
			IEnumerable<Object> gos = deps.Where(t => t.ClassID == ClassIDType.GameObject);
			foreach (Object go in gos)
			{
				yield return (GameObject)go;
			}
			IEnumerable<Object> comps = deps.Where(t => t.ClassID != ClassIDType.GameObject).OrderBy(t => t.ClassID);
			foreach (Object comp in comps)
			{
				yield return (Component)comp;
			}
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 2;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			ParentPrefab.Read(stream);
			RootGameObject.Read(stream);
			IsPrefabParent = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align64);
			throw new NotSupportedException("Currently EditorExtension's PrefabInternal field doesn't support Engine's prefabs");
		}

		public IEnumerable<EditorExtension> FetchObjects(bool isLog = false)
		{
			return FetchObjects(File, isLog);
		}

		public IEnumerable<EditorExtension> FetchObjects(ISerializedFile file, bool isLog = false)
		{
			GameObject root = RootGameObject.GetObject(file);
			return FetchObjects(root);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			if(!ParentPrefab.IsNull)
			{
				yield return ParentPrefab.GetObject(file);
			}
			yield return RootGameObject.GetObject(file);
		}

		public string GetName()
		{
			return RootGameObject.GetObject(File).Name;
		}

		public override string ToString()
		{
			return $"{GetName()}(Prefab)";
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Modification", Modification.ExportYAML(exporter));
			node.Add("m_ParentPrefab", ParentPrefab.ExportYAML(exporter));
			node.Add("m_RootGameObject", RootGameObject.ExportYAML(exporter));
			node.Add("m_IsPrefabParent", IsPrefabParent);
			return node;
		}

		public override string ExportExtension => "prefab";
		
		public InnerPPtr<Prefab> ThisPrefab { get; }

		public bool IsPrefabParent { get; private set; }

		public PrefabModification Modification;
		public PPtr<Prefab> ParentPrefab;
		public PPtr<GameObject> RootGameObject;
	}
}
