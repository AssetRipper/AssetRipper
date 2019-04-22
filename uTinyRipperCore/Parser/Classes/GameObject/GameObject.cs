using SevenZip;
using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.GameObjects;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class GameObject : EditorExtension
	{
		public GameObject(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		private static IEnumerable<EditorExtension> FetchHierarchy(GameObject root)
		{
			yield return root;

			Transform transform = null;
			foreach (ComponentPair cpair in root.Components)
			{
				Component component = cpair.Component.FindAsset(root.File);
				if(component == null)
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

		/// <summary>
		/// Less than 3.5 or Not Prefab
		/// </summary>
		public static bool IsReadComponents(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsForPrefab() || version.IsLess(3, 5);
		}
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		public static bool IsReadIsActiveFirst(Version version)
		{
			return version.IsLess(2, 1);
		}
		/// <summary>
		/// Release
		/// </summary>
		public static bool IsReadTag(TransferInstructionFlags flags)
		{
			return flags.IsRelease();
		}
		/// <summary>
		/// 3.4.0 and greater and Not Release
		/// </summary>
		public static bool IsReadIcon(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 4);
		}
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool IsReadNavMeshLayer(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 3.0.0 to 3.5.0 exclusive
		/// </summary>
		public static bool IsReadIsStatic(Version version)
		{
			return version.IsLess(3, 5) && version.IsGreaterEqual(3);
		}

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		private static bool IsReadIconFirst(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// Less than 4.0.0
		/// SerializedVersion less than 4
		/// </summary>
		private static bool IsActiveInherited(Version version)
		{
			return version.IsLess(4);
		}

		private static int GetSerializedVersion(Version version)
		{
			// unknown
			if (Config.IsExportTopmostSerializedVersion || version.IsGreaterEqual(5, 5))
			{
				return 5;
			}
			// active state inheritance
			if (version.IsGreaterEqual(4))
			{
				return 4;
			}
			// min is 3
			// tag is ushort for Release, otherwise string. For later versions for yaml only string left
			return 3;
			// tag is string
			//return 2;
			// tag is ushort
			//return 1;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if(IsReadComponents(reader.Version, reader.Flags))
			{
				Components = reader.ReadAssetArray<ComponentPair>();
			}

			if (IsReadIsActiveFirst(reader.Version))
			{
				IsActive = reader.ReadBoolean();
				Layer = reader.ReadUInt32();
				Tag = reader.ReadUInt16();
				Name = reader.ReadString();
			}
			else
			{
				Layer = reader.ReadUInt32();
				Name = reader.ReadString();

				if (IsReadTag(reader.Flags))
				{
					Tag = reader.ReadUInt16();
				}
#if UNIVERSAL
				else
				{
					TagString = reader.ReadString();
				}
				if (IsReadIcon(reader.Version, reader.Flags))
				{
					if (IsReadIconFirst(reader.Version))
					{
						Icon.Read(reader);
					}
				}
				if (IsReadNavMeshLayer(reader.Version, reader.Flags))
				{
					NavMeshLayer = reader.ReadUInt32();
					StaticEditorFlags = reader.ReadUInt32();
				}
#endif
				IsActive = reader.ReadBoolean();


#if UNIVERSAL
				if (IsReadIsStatic(reader.Version))
				{
					StaticEditorFlags = reader.ReadBoolean() ? uint.MaxValue : 0;
				}
				if (IsReadIcon(reader.Version, reader.Flags))
				{
					if (!IsReadIconFirst(reader.Version))
					{
						Icon.Read(reader);
					}
				}
#endif
			}
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

		public T GetComponent<T>()
			where T: Component
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
			foreach (ComponentPair pair in Components)
			{
				// component could has not impelemented asset type
				Component comp = pair.Component.FindAsset(File);
				if (comp is T t)
				{
					return t;
				}
			}
			return null;
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
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add(ComponentName, GetComponents(container.Version, container.Flags).ExportYAML(container));
			node.Add(LayerName, Layer);
			node.Add(NameName, Name);
			node.Add(TagStringName, GetTagString(container));
			node.Add(IconName, GetIcon().ExportYAML(container));
			node.Add(NavMeshLayerName, GetNavMeshLayer());
			node.Add(StaticEditorFlagsName, GetStaticEditorFlags());
			node.Add(IsActiveName, GetIsActive(container.Version));
			return node;
		}

		private IReadOnlyList<ComponentPair> GetComponents(Version version, TransferInstructionFlags flags)
		{
			return IsReadComponents(version, flags) ? Components : new ComponentPair[0];
		}
		private string GetTagString(IExportContainer container)
		{
#if UNIVERSAL
			if(!IsReadTag(container.Flags) && !IsReadIsActiveFirst(container.Version))
			{
				return TagString;
			}
#endif
			return container.TagIDToName(Tag);
		}
		private PPtr<Texture2D> GetIcon()
		{
#if UNIVERSAL
			return Icon;
#else
			return default;
#endif
		}
		private uint GetNavMeshLayer()
		{
#if UNIVERSAL
			return NavMeshLayer;
#else
			return 0;
#endif
		}
		private uint GetStaticEditorFlags()
		{
#if UNIVERSAL
			return StaticEditorFlags;
#else
			return 0;
#endif
		}
		/// <summary>
		/// There one is incompatible with old versions!
		/// </summary>
		private bool GetIsActive(Version version)
		{
			return IsActiveInherited(version) ? (File.Collection.IsScene(File) ? IsActive : true) : IsActive;
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
		
		public ComponentPair[] Components { get; private set; }
		public uint Layer { get; private set; }
		public string Name { get; private set; } = string.Empty;
		public ushort Tag { get; private set; }
#if UNIVERSAL
		public string TagString { get; private set; }
		public uint NavMeshLayer { get; private set; }
		public uint StaticEditorFlags { get; private set; }
#endif
		public bool IsActive { get; private set; }

		public const string ComponentName = "m_Component";
		public const string LayerName = "m_Layer";
		public const string NameName = "m_Name";
		public const string TagStringName = "m_TagString";
		public const string IconName = "m_Icon";
		public const string NavMeshLayerName = "m_NavMeshLayer";
		public const string StaticEditorFlagsName = "m_StaticEditorFlags";
		public const string IsActiveName = "m_IsActive";

#if UNIVERSAL
		public PPtr<Texture2D> Icon;
#endif
	}
}
