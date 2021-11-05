using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Converters.GameObject;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using SevenZip;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Classes.GameObject
{
	public sealed class GameObject : EditorExtension, IHasName, IGameObject
	{
		public GameObject(AssetLayout layout) : base(layout)
		{
			if (IsComponentTuple(layout.Info.Version))
			{
				ComponentTuple = Array.Empty<Tuple<ClassIDType, PPtr<Component>>>();
			}
			else
			{
				Component = Array.Empty<ComponentPair>();
			}
			Name = string.Empty;
			TagString = TagManager.TagManagerConstants.UntaggedTag;
			IsActive = true;
		}

		public GameObject(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5, 5))
			{
				// unknown
				return 5;
			}
			else if (version.IsGreaterEqual(4))
			{
				// active state inheritance
				return 4;
			}
			else
			{
				// min is 3
				// tag is ushort for Release, otherwise string. For later versions for yaml only string left
				return 3;

				// tag is string
				// Version = 2;
				// tag is ushort
				// Version = 1;
			}
		}

		public T GetComponent<T>() where T : Component
		{
			T component = FindComponent<T>();
			if (component == null)
			{
				throw new Exception($"Component of type {nameof(T)} hasn't been found");
			}
			return component;
		}

		public T FindComponent<T>() where T : Component
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

		public override IUnityObjectBase Convert(IExportContainer container)
		{
			return GameObjectConverter.Convert(container, this);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsComponentTuple(reader.Version))
			{
				ComponentTuple = reader.ReadTupleEnum32TArray<ClassIDType, PPtr<Component>>((t) => (ClassIDType)t);
			}
			else
			{
				Component = reader.ReadAssetArray<ComponentPair>();
			}

			Layer = reader.ReadUInt32();
			Name = reader.ReadString();

			if (HasTag(reader.Version, reader.Flags))
			{
				Tag = reader.ReadUInt16();
			}
			IsActive = reader.ReadBoolean();
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (IsComponentTuple(writer.Version))
			{
				ComponentTuple.Write(writer, (t) => (int)t);
			}
			else
			{
				Component.Write(writer);
			}

			writer.Write(Layer);
			writer.Write(Name);

			if (HasTag(writer.Version, writer.Flags))
			{
				writer.Write(Tag);
			}
			writer.Write(IsActive);
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (IsComponentTuple(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(ComponentTuple.Select(t => t.Item2), ComponentName))
				{
					yield return asset;
				}
			}
			else
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Component, ComponentName))
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
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (IsComponentTuple(container.ExportVersion))
			{
				node.Add(ComponentName, ComponentTuple.ExportYAML(container, (t) => (int)t));
			}
			else
			{
				node.Add(ComponentName, ExportYAML(Component, container));
			}

			node.Add(LayerName, Layer);
			node.Add(NameName, Name);
			if (HasTag(container.ExportVersion, container.ExportFlags))
			{
				node.Add(TagName, Tag);
			}
			else
			{
				node.Add(TagStringName, TagString);
			}

			if (HasIcon(container.ExportVersion, container.ExportFlags) && IsIconFirst(container.ExportVersion))
			{
				node.Add(IconName, Icon.ExportYAML(container));
			}
			if (HasNavMeshLayer(container.ExportVersion, container.ExportFlags))
			{
				node.Add(NavMeshLayerName, NavMeshLayer);
				node.Add(StaticEditorFlagsName, StaticEditorFlags);
			}
			node.Add(IsActiveName, IsActive);
			if (HasIsStatic(container.ExportVersion, container.ExportFlags))
			{
				node.Add(IsStaticName, IsStatic);
			}
			if (HasIcon(container.ExportVersion, container.ExportFlags) && !IsIconFirst(container.ExportVersion))
			{
				node.Add(IconName, Icon.ExportYAML(container));
			}
			return node;
		}

		private static YAMLNode ExportYAML(IEnumerable<ComponentPair> _this, IExportContainer container)
		{
			YAMLSequenceNode node = new YAMLSequenceNode(SequenceStyle.Block);
			foreach (ComponentPair pair in _this)
			{
				if (pair.Component.IsValid(container))
				{
					node.Add(pair.ExportYAML(container));
				}
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
			if (IsComponentTuple(File.Version))
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

		/// <summary>
		/// Release or less than 2.1.0
		/// </summary>
		public static bool HasTag(UnityVersion version, TransferInstructionFlags flags) => flags.IsRelease() || version.IsLess(2, 1);
		/// <summary>
		/// 2.1.0 and greater and Not Release
		/// </summary>
		public static bool HasTagString(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2, 1) && !flags.IsRelease();
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasNavMeshLayer(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && !flags.IsRelease();
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasStaticEditorFlags(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 5) && !flags.IsRelease();
		/// <summary>
		/// 3.0.0 to 3.5.0 exclusive and Not Release
		/// </summary>
		public static bool HasIsStatic(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3) && version.IsLess(3, 5) && !flags.IsRelease();
		/// <summary>
		/// At least 3.4.0 and Not Release
		/// </summary>
		public static bool HasIcon(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(3, 4) && !flags.IsRelease();
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsComponentTuple(UnityVersion version) => version.IsLess(5, 5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsIconFirst(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// Less than 4.0.0
		/// </summary>
		public static bool IsActiveInherited(UnityVersion version) => version.IsLess(4);

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

		private uint NavMeshLayer => 0;
		private uint StaticEditorFlags => 0;
		public bool IsActive { get; set; }

		private bool IsStatic => false;
		private PPtr<Texture2D.Texture2D> Icon => default;

		private object m_component;

		public const string ComponentName = "m_Component";
		public const string LayerName = "m_Layer";
		public const string NameName = "m_Name";
		public const string TagName = "m_Tag";
		public const string TagStringName = "m_TagString";
		public const string NavMeshLayerName = "m_NavMeshLayer";
		public const string StaticEditorFlagsName = "m_StaticEditorFlags";
		public const string IsActiveName = "m_IsActive";
		public const string IsStaticName = "m_IsStatic";
		public const string IconName = "m_Icon";
	}
}
