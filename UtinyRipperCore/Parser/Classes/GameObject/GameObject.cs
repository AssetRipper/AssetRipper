using System;
using System.Collections.Generic;
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

		private static int GetSerializedVersion(Version version)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			return 5;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Components = stream.ReadArray<ComponentPair>();

			Layer = stream.ReadInt32();
			Name = stream.ReadStringAligned();
			Tag = stream.ReadUInt16();
			IsActive = stream.ReadBoolean();
		}
		
		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			foreach(ComponentPair pair in Components)
			{
				foreach (Object @object in pair.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
		}

		public GameObject GetRoot()
		{
			Transform root = GetTransform();
			while (true)
			{
				Transform parent = root.Father.TryGetObject(File);
				if (parent == null)
				{
					break;
				}
				else
				{
					root = parent;
				}
			}
			return root.GameObject.GetObject(File);
		}

		public int GetRootDepth()
		{
			Transform root = GetTransform();
			int depth = 0;
			while (true)
			{
				Transform parent = root.Father.TryGetObject(File);
				if (parent == null)
				{
					break;
				}

				root = parent;
				depth++;
			}
			return depth;
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Component", Components.ExportYAML(exporter));
			node.Add("m_Layer", Layer);
			node.Add("m_Name", Name);
#warning TODO: tag index to string name
			node.Add("m_TagString", "Untagged");
#warning what are those 3 params???
			node.Add("m_Icon", default(PPtr<Object>).ExportYAML(exporter));
			node.Add("m_NavMeshLayer", 0);
			node.Add("m_StaticEditorFlags", 0);
			node.Add("m_IsActive", IsActive);
			return node;
		}

		public Transform GetTransform()
		{
			foreach (ComponentPair pair in Components)
			{
				Component comp = pair.Component.FindObject(File);
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

		public override string ToString()
		{
			if (string.IsNullOrEmpty(Name))
			{
				return base.ToString();
			}
			return $"{Name}({GetType().Name})";
		}

		public override string ExportExtension => throw new NotSupportedException();
		
		public ComponentPair[] Components { get; private set; }
		public int Layer { get; private set; }
		public string Name { get; private set; } = string.Empty;
		public ushort Tag { get; private set; }
		public bool IsActive { get; private set; }
	}
}
