using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public class Transform : Component
	{
		public Transform(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			LocalRotation.Read(stream);
			LocalPosition.Read(stream);
			LocalScale.Read(stream);
			m_children = stream.ReadArray<PPtr<Transform>>();
			Father.Read(stream);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			foreach (PPtr<Transform> ptr in Children)
			{
				yield return ptr.GetObject(file);
			}
			if (!Father.IsNull)
			{
				yield return Father.GetObject(file);
			}
		}

		public string GetRootPath()
		{
			string pre = string.Empty;
			if(!Father.IsNull)
			{
				pre = Father.GetObject(File).GetRootPath() + "/";
			}
			return pre + GameObject.GetObject(File).Name;
		}

		public int GetSiblingIndex()
		{
			Transform father = Father.FindObject(File);
			if(father == null)
			{
				return 0;
			}

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
