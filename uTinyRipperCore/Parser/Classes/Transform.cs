using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public class Transform : Component
	{
		public Transform(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);
			
			LocalRotation.Read(reader);
			LocalPosition.Read(reader);
			LocalScale.Read(reader);
			m_children = reader.ReadArray<PPtr<Transform>>();
			Father.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach (PPtr<Transform> ptr in Children)
			{
				yield return ptr.GetAsset(file);
			}
			if (!Father.IsNull)
			{
				yield return Father.GetAsset(file);
			}
		}

		public string GetRootPath()
		{
			string pre = string.Empty;
			if(!Father.IsNull)
			{
				pre = Father.GetAsset(File).GetRootPath() + "/";
			}
			return pre + GameObject.GetAsset(File).Name;
		}

		public int GetSiblingIndex()
		{
			if(Father.IsNull)
			{
				return 0;
			}
			Transform father = Father.GetAsset(File);
			for(int i = 0; i < father.Children.Count; i++)
			{
				PPtr<Transform> child = father.Children[i];
				if (child.PathID == PathID)
				{
					return i;
				}
			}
			throw new Exception("Transorm hasn't been found among father's children");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_LocalRotation", LocalRotation.ExportYAML(container));
			node.Add("m_LocalPosition", LocalPosition.ExportYAML(container));
			node.Add("m_LocalScale", LocalScale.ExportYAML(container));
			node.Add("m_Children", Children.ExportYAML(container));
			node.Add("m_Father", Father.ExportYAML(container));
			node.Add("m_RootOrder", GetSiblingIndex());
			node.Add("m_LocalEulerAnglesHint", LocalRotation.ToEuler().ExportYAML(container));
			return node;
		}

		public IReadOnlyList<PPtr<Transform>> Children => m_children;

		public Quaternionf LocalRotation;
		public Vector3f LocalPosition;
		public Vector3f LocalScale;
		public PPtr<Transform> Father;

		private PPtr<Transform>[] m_children;
	}
}
