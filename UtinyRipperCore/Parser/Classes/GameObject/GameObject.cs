using SevenZip;
using System;
using System.Collections.Generic;
using System.Text;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.GameObjects;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class GameObject : EditorExtension
	{
		public GameObject(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static void CollectHierarchy(GameObject root, List<EditorExtension> heirarchy)
		{
			heirarchy.Add(root);

			Transform transform = null;
			foreach (ComponentPair cpair in root.Components)
			{
				Component component = cpair.Component.FindAsset(root.File);
				if(component == null)
				{
					continue;
				}

				heirarchy.Add(component);
				if (component.ClassID.IsTransform())
				{
					transform = (Transform)component;
				}
			}

			foreach (PPtr<Transform> pchild in transform.Children)
			{
				Transform child = pchild.GetAsset(root.File);
				GameObject childGO = child.GameObject.GetAsset(root.File);
				CollectHierarchy(childGO, heirarchy);
			}
		}

		/// <summary>
		/// Less than 4.0.0
		/// In earlier versions GameObject always has IsActive as false.
		/// </summary>
		private static bool IsAlwaysDeactivated(Version version)
		{
#warning unknown
			return version.IsLess(4);
		}

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 5;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Components = reader.ReadArray<ComponentPair>();

			Layer = reader.ReadInt32();
			Name = reader.ReadStringAligned();
			Tag = reader.ReadUInt16();
			IsActive = reader.ReadBoolean();
		}
		
		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			foreach(ComponentPair pair in Components)
			{
				foreach (Object asset in pair.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
			}
		}

		public Transform GetTransform()
		{
			foreach (ComponentPair pair in Components)
			{
				Component comp = pair.Component.FindAsset(File);
				if (comp == null)
				{
					continue;
				}

				if (comp.ClassID.IsTransform())
				{
					return (Transform)comp;
				}
			}
			return null;
		}

		public GameObject GetRoot()
		{
			Transform root = GetTransform();
			while (true)
			{
				Transform parent = root.Father.TryGetAsset(File);
				if (parent == null)
				{
					break;
				}
				else
				{
					root = parent;
				}
			}
			return root.GameObject.GetAsset(File);
		}

		public int GetRootDepth()
		{
			Transform root = GetTransform();
			int depth = 0;
			while (true)
			{
				Transform parent = root.Father.TryGetAsset(File);
				if (parent == null)
				{
					break;
				}

				root = parent;
				depth++;
			}
			return depth;
		}
		
		public IReadOnlyList<EditorExtension> CollectHierarchy()
		{
			List<EditorExtension> heirarchy = new List<EditorExtension>();
			CollectHierarchy(this, heirarchy);
			return heirarchy;
		}

		public IReadOnlyDictionary<uint, string> BuildTOS()
		{
			Dictionary<uint, string> tos = new Dictionary<uint, string>();
			tos.Add(0, string.Empty);

			BuildTOS(this, string.Empty, tos);
			return tos;
		}

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Name))
			{
				return base.ToString();
			}
			return $"{Name}({GetType().Name})";
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Component", Components.ExportYAML(container));
			node.Add("m_Layer", Layer);
			node.Add("m_Name", Name);
			node.Add("m_TagString", container.TagIDToName(Tag));
			node.Add("m_Icon", default(PPtr<Object>).ExportYAML(container));
			node.Add("m_NavMeshLayer", 0);
			node.Add("m_StaticEditorFlags", 0);
			node.Add("m_IsActive", GetExportIsActive(container.Version));
			return node;
		}

		private void BuildTOS(GameObject parent, string parentPath, Dictionary<uint, string> tos)
		{
			Transform transform = parent.GetTransform();
			foreach (PPtr<Transform> childPtr in transform.Children)
			{
				Transform childTransform = childPtr.GetAsset(File);
				GameObject child = childTransform.GameObject.GetAsset(File);
				string path = parentPath != string.Empty ? parentPath + "/" + child.Name : child.Name;
				CRC crc = new CRC();
				byte[] pathBytes = Encoding.UTF8.GetBytes(path);
				crc.Update(pathBytes, 0, (uint)pathBytes.Length);
				uint pathHash = crc.GetDigest();
				tos[pathHash] = path;

				BuildTOS(child, path, tos);
			}
		}

		private bool GetExportIsActive(Version version)
		{
#warning TODO: fix
			return IsAlwaysDeactivated(version) ? true : IsActive;
		}

		public override string ExportExtension => throw new NotSupportedException();
		
		public ComponentPair[] Components { get; private set; }
		public int Layer { get; private set; }
		public string Name { get; private set; } = string.Empty;
		public ushort Tag { get; private set; }
		public bool IsActive { get; private set; }
	}
}
