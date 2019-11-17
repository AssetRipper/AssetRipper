using SevenZip;
using System;
using System.Collections.Generic;
using uTinyRipper.Classes.GameObjects;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Layout;
using System.Linq;

namespace uTinyRipper.Classes
{
	public sealed class GameObject : EditorExtension
	{
		public GameObject(AssetLayout layout):
			base(layout)
		{
			GameObjectLayout classLayout = layout.GameObject;
			if (classLayout.IsComponentTuple)
			{
				ComponentTuple = Array.Empty<Tuple<ClassIDType, PPtr<Component>>>();
			}
			else
			{
				Component = Array.Empty<ComponentPair>();
			}
			Name = string.Empty;
			TagString = TagManager.UntaggedTag;
			IsActive = true;
		}

		public GameObject(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public T GetComponent<T>()
			where T : Component
		{
			T component = FindComponent<T>();
			if (component == null)
			{
				throw new Exception($"Component of type {nameof(T)} hasn't been found");
			}
			return component;
		}

		public T FindComponent<T>()
			where T : Component
		{
			foreach (PPtr<Component> ptr in FetchComponents())
			{
				// component could has not impelemented asset type
				Component comp = ptr.FindAsset(File);
				if (comp is T t)
				{
					return t;
				}
			}
			return null;
		}

		public Transform GetTransform()
		{
			foreach (PPtr<Component> ptr in FetchComponents())
			{
				Component comp = ptr.FindAsset(File);
				if (comp == null)
				{
					continue;
				}

				if (comp.ClassID.IsTransform())
				{
					return (Transform)comp;
				}
			}
			throw new Exception("Can't find transform component");
		}

		public GameObject GetRoot()
		{
			Transform root = GetTransform();
			while (true)
			{
				Transform parent = root.Father.TryGetAsset(root.File);
				if (parent == null)
				{
					break;
				}
				else
				{
					root = parent;
				}
			}
			return root.GameObject.GetAsset(root.File);
		}

		public int GetRootDepth()
		{
			Transform root = GetTransform();
			int depth = 0;
			while (true)
			{
				Transform parent = root.Father.TryGetAsset(root.File);
				if (parent == null)
				{
					break;
				}

				root = parent;
				depth++;
			}
			return depth;
		}

		public IEnumerable<EditorExtension> FetchHierarchy()
		{
			foreach (EditorExtension element in FetchHierarchy(this))
			{
				yield return element;
			}
		}

		public List<EditorExtension> CollectHierarchy()
		{
			List<EditorExtension> hierarchy = new List<EditorExtension>();
			hierarchy.AddRange(FetchHierarchy(this));
			return hierarchy;
		}

		public IReadOnlyDictionary<uint, string> BuildTOS()
		{
			Dictionary<uint, string> tos = new Dictionary<uint, string>() { { 0, string.Empty } };
			BuildTOS(this, string.Empty, tos);
			return tos;
		}

		public override Object Convert(IExportContainer container)
		{
			return GameObjectConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			GameObjectLayout layout = reader.Layout.GameObject;
			if (layout.IsComponentTuple)
			{
				ComponentTuple = reader.ReadTupleEnum32TArray<ClassIDType, PPtr<Component>>((t) => (ClassIDType)t);
			}
			else
			{
				Component = reader.ReadAssetArray<ComponentPair>();
			}

			if (layout.IsActiveFirst)
			{
				IsActive = reader.ReadBoolean();
			}
			Layer = reader.ReadUInt32();
			if (layout.IsNameFirst)
			{
				Name = reader.ReadString();
			}

			if (layout.HasTag)
			{
				Tag = reader.ReadUInt16();
			}
#if UNIVERSAL
			else
			{
				TagString = reader.ReadString();
			}
			if (layout.HasIcon && layout.IsIconFirst)
			{
				Icon.Read(reader);
			}
			if (layout.HasNavMeshLayer)
			{
				NavMeshLayer = reader.ReadUInt32();
				StaticEditorFlags = reader.ReadUInt32();
			}
#endif
			if (!layout.IsNameFirst)
			{
				Name = reader.ReadString();
			}
			if (!layout.IsActiveFirst)
			{
				IsActive = reader.ReadBoolean();
			}


#if UNIVERSAL
			if (layout.HasIsStatic)
			{
				IsStatic = reader.ReadBoolean();
			}
			if (layout.HasIcon && !layout.IsIconFirst)
			{
				Icon.Read(reader);
			}
#endif
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			GameObjectLayout layout = writer.Layout.GameObject;
			if (layout.IsComponentTuple)
			{
				ComponentTuple.Write(writer, (t) => (int)t);
			}
			else
			{
				Component.Write(writer);
			}

			if (layout.IsActiveFirst)
			{
				writer.Write(IsActive);
			}
			writer.Write(Layer);
			if (layout.IsNameFirst)
			{
				writer.Write(Name);
			}

			if (layout.HasTag)
			{
				writer.Write(Tag);
			}
#if UNIVERSAL
			else
			{
				writer.Write(TagString);
			}
			if (layout.HasIcon && layout.IsIconFirst)
			{
				Icon.Write(writer);
			}
			if (layout.HasNavMeshLayer)
			{
				writer.Write(NavMeshLayer);
				writer.Write(StaticEditorFlags);
			}
#endif
			if (!layout.IsNameFirst)
			{
				writer.Write(Name);
			}
			if (!layout.IsActiveFirst)
			{
				writer.Write(IsActive);
			}


#if UNIVERSAL
			if (layout.HasIsStatic)
			{
				writer.Write(IsStatic);
			}
			if (layout.HasIcon && !layout.IsIconFirst)
			{
				Icon.Write(writer);
			}
#endif
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			GameObjectLayout layout = context.Layout.GameObject;
			if (layout.IsComponentTuple)
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(ComponentTuple.Select(t => t.Item2), layout.ComponentName))
				{
					yield return asset;
				}
			}
			else
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(Component, layout.ComponentName))
				{
					yield return asset;
				}
			}
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
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			GameObjectLayout layout = container.ExportLayout.GameObject;
			node.AddSerializedVersion(layout.Version);
			if (layout.IsComponentTuple)
			{
				node.Add(layout.ComponentName, ComponentTuple.ExportYAML(container, (t) => (int)t));
			}
			else
			{
				node.Add(layout.ComponentName, Component.ExportYAML(container));
			}

			if (layout.IsActiveFirst)
			{
				node.Add(layout.IsActiveName, IsActive);
			}

			node.Add(layout.LayerName, Layer);
			if (layout.IsNameFirst)
			{
				node.Add(layout.NameName, Name);
			}
			if (layout.HasTag)
			{
				node.Add(layout.TagName, Tag);
			}
			else
			{
				node.Add(layout.TagStringName, TagString);
			}

			if (layout.HasIcon && layout.IsIconFirst)
			{
				node.Add(layout.IconName, Icon.ExportYAML(container));
			}
			if (layout.HasNavMeshLayer)
			{
				node.Add(layout.NavMeshLayerName, NavMeshLayer);
				node.Add(layout.StaticEditorFlagsName, StaticEditorFlags);
			}
			if (!layout.IsNameFirst)
			{
				node.Add(layout.NameName, Name);
			}
			if (!layout.IsActiveFirst)
			{
				node.Add(layout.IsActiveName, IsActive);
			}
			if (layout.HasIsStatic)
			{
				node.Add(layout.IsStaticName, IsStatic);
			}
			if (layout.HasIcon && !layout.IsIconFirst)
			{
				node.Add(layout.IconName, Icon.ExportYAML(container));
			}
			return node;
		}

		private static IEnumerable<EditorExtension> FetchHierarchy(GameObject root)
		{
			yield return root;

			Transform transform = null;
			foreach (PPtr<Component> ptr in root.FetchComponents())
			{
				Component component = ptr.FindAsset(root.File);
				if (component == null)
				{
					continue;
				}

				yield return component;
				if (component.ClassID.IsTransform())
				{
					transform = (Transform)component;
				}
			}

			foreach (PPtr<Transform> pchild in transform.Children)
			{
				Transform child = pchild.GetAsset(transform.File);
				GameObject childGO = child.GameObject.GetAsset(root.File);
				foreach (EditorExtension childElement in FetchHierarchy(childGO))
				{
					yield return childElement;
				}
			}
		}

		private IEnumerable<PPtr<Component>> FetchComponents()
		{
			if (File.Layout.GameObject.IsComponentTuple)
			{
				return ComponentTuple.Select(t => t.Item2);
			}
			else
			{
				return Component.Select(t => t.Component);
			}
		}

		private void BuildTOS(GameObject parent, string parentPath, Dictionary<uint, string> tos)
		{
			Transform transform = parent.GetTransform();
			foreach (PPtr<Transform> childPtr in transform.Children)
			{
				Transform childTransform = childPtr.GetAsset(File);
				GameObject child = childTransform.GameObject.GetAsset(File);
				string path = parentPath != string.Empty ? parentPath + Transform.PathSeparator + child.Name : child.Name;
				uint pathHash = CRC.CalculateDigestUTF8(path);
				tos[pathHash] = path;

				BuildTOS(child, path, tos);
			}
		}

		public override string ExportExtension => throw new NotSupportedException();
		
		public ComponentPair[] Component
		{
			get => (ComponentPair[])m_component;
			set => m_component = value;
		}
		public Tuple<ClassIDType, PPtr<Component>>[] ComponentTuple
		{
			get => (Tuple<ClassIDType, PPtr<Component>>[])m_component;
			set => m_component = value;
		}
		public uint Layer { get; set; }
		public string Name { get; set; }
		public ushort Tag { get; set; }
		public string TagString { get; set; }
#if UNIVERSAL
		public uint NavMeshLayer { get; set; }
		public uint StaticEditorFlags { get; set; }
#else
		private uint NavMeshLayer => 0;
		private uint StaticEditorFlags => 0;
#endif
		public bool IsActive { get; set; }
#if UNIVERSAL
		public bool IsStatic
		{
			get => StaticEditorFlags != 0;
			set => StaticEditorFlags = value ? uint.MaxValue : 0;
		}

		public PPtr<Texture2D> Icon;
#else
		private bool IsStatic => false;
		private PPtr<Texture2D> Icon => default;
#endif

		private object m_component;
	}
}
