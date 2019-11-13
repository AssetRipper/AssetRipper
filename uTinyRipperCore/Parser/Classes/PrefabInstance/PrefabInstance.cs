using System.Collections.Generic;
using uTinyRipper.Classes.Prefabs;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;
using System;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// 2018.3 - Prefab has been renamed to PrefabInstance
	/// 3.5.0 - DataTemplate has been renamed to Prefab
	/// 1.5.0 - first introduction as DataTemplate
	/// </summary>
	public sealed class PrefabInstance : NamedObject
	{
		public PrefabInstance(Version version):
			base(version)
		{
			Objects = Array.Empty<PPtr<EditorExtension>>();
			Modification = new PrefabModification(version);
		}

		public PrefabInstance(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static PrefabInstance CreateVirtualInstance(VirtualSerializedFile virtualFile, GameObject root)
		{
			PrefabInstance instance = virtualFile.CreateAsset((assetInfo) => new PrefabInstance(assetInfo));
			instance.ObjectHideFlags = HideFlags.HideInHierarchy;
			instance.Objects = Array.Empty<PPtr<EditorExtension>>();
			instance.Modification = new PrefabModification(virtualFile.Version);
			instance.RootGameObject = root.File.CreatePPtr(root);
			instance.IsPrefabAsset = true;
			instance.Name = root.Name;
			return instance;
		}

		public static int ToSerializedVersion(Version version)
		{
			if (IsModificationsFormat(version))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsModificationsFormat(Version version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool HasLastTemplateIdentifier(Version version) => version.IsLess(2, 6);
		/// <summary>
		/// Not Editor scene
		/// </summary>
		public static bool IsRootGameObjectRelevant(TransferInstructionFlags flags) => !flags.IsEditorScene();
		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public static bool IsPrefabAssetRelevant(Version version) => version.IsLess(2018, 3);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsExplodedRelevant(Version version) => version.IsLess(5);

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		private static bool IsRootGameObjectFirst(Version version) => version.IsGreaterEqual(2018, 3);

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			string className = GetPrefabInstanceName(context.Version);
			context.AddNode(className, name, 0, ToSerializedVersion(context.Version));
			context.BeginChildren();
			if (IsModificationsFormat(context.Version))
			{
				Object.GenerateTypeTree(context);

				bool hasRootGameObject = IsRootGameObjectRelevant(context.Flags);
				bool isRootGameObjectFirst = IsRootGameObjectFirst(context.Version);
				if (hasRootGameObject && isRootGameObjectFirst)
				{
					context.AddPPtr(nameof(GameObject), RootGameObjectName);
				}

				PrefabModification.GenerateTypeTree(context, ModificationName);
				context.AddPPtr(className, GetSourcePrefabName(context.Version));
				if (hasRootGameObject && !isRootGameObjectFirst)
				{
					context.AddPPtr(nameof(GameObject), RootGameObjectName);
				}
				if (IsPrefabAssetRelevant(context.Version))
				{
					context.AddBool(GetIsPrefabAssetName(context.Version));
				}
				if (IsExplodedRelevant(context.Version))
				{
					context.AddBool(IsExplodedName);
				}
				context.Align();
			}
			else
			{
				GUID.GenerateTypeTree(context, LastMergeIdentifierName);
				if (HasLastTemplateIdentifier(context.Version))
				{
					GUID.GenerateTypeTree(context, LastTemplateIdentifierName);
				}
				context.AddArray(ObjectsName, PPtr<EditorExtension>.GenerateTypeTree);
				context.AddPPtr(className, FatherName);
				context.AddBool(IsDataTemplateName, TransferMetaFlags.AlignBytesFlag);
				NamedObject.GenerateTypeTree(context);
			}
			context.EndChildren();
		}

		public override void Read(AssetReader reader)
		{
			if (IsModificationsFormat(reader.Version))
			{
				ReadObject(reader);

				bool isRootGameObjectFirst = IsRootGameObjectFirst(reader.Version);
				if (isRootGameObjectFirst && IsRootGameObjectRelevant(reader.Flags))
				{
					RootGameObject.Read(reader);
				}

				Modification.Read(reader);
				SourcePrefab.Read(reader);
				if (!isRootGameObjectFirst)
				{
					RootGameObject.Read(reader);
				}
				if (IsPrefabAssetRelevant(reader.Version))
				{
					IsPrefabAsset = reader.ReadBoolean();
				}
				if (IsExplodedRelevant(reader.Version))
				{
					IsExploded = reader.ReadBoolean();
				}
				reader.AlignStream();
			}
			else
			{
				LastMergeIdentifier.Read(reader);
				if (HasLastTemplateIdentifier(reader.Version))
				{
					LastTemplateIdentifier.Read(reader);
				}
				Objects = reader.ReadAssetArray<PPtr<EditorExtension>>();
				Father = reader.ReadAsset<PPtr<PrefabInstance>>();
				IsDataTemplate = reader.ReadBoolean();
				reader.AlignStream();

				base.Read(reader);
			}
		}

		public override void Write(AssetWriter writer)
		{
			if (IsModificationsFormat(writer.Version))
			{
				WriteObject(writer);

				bool hasRootGameObject = IsRootGameObjectRelevant(writer.Flags);
				bool isRootGameObjectFirst = IsRootGameObjectFirst(writer.Version);
				if (hasRootGameObject && isRootGameObjectFirst)
				{
					RootGameObject.Write(writer);
				}

				Modification.Write(writer);
				SourcePrefab.Write(writer);
				if (hasRootGameObject && !isRootGameObjectFirst)
				{
					RootGameObject.Write(writer);
				}
				if (IsPrefabAssetRelevant(writer.Version))
				{
					writer.Write(IsPrefabAsset);
				}
				if (IsExplodedRelevant(writer.Version))
				{
					writer.Write(IsExploded);
				}
				writer.AlignStream();
			}
			else
			{
				LastMergeIdentifier.Write(writer);
				if (HasLastTemplateIdentifier(writer.Version))
				{
					LastTemplateIdentifier.Write(writer);
				}
				Objects.Write(writer);
				Father.Write(writer);
				writer.Write(IsDataTemplate);
				writer.AlignStream();

				base.Write(writer);
			}
		}

		public IEnumerable<EditorExtension> FetchObjects(IAssetContainer file)
		{
#warning TEMP HACK:
			//if (IsModificationsFormat(file.Version))
			{
				GameObject root = RootGameObject.GetAsset(file);
				foreach (EditorExtension asset in root.FetchHierarchy())
				{
					yield return asset;
				}
			}
			/*else
			{
				foreach (PPtr<EditorExtension> asset in Objects)
				{
					yield return asset.GetAsset(file);
				}
			}*/
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			if (IsModificationsFormat(context.Version))
			{
				foreach (PPtr<Object> asset in FetchDependenciesObject(context))
				{
					yield return asset;
				}

				if (IsRootGameObjectRelevant(context.Flags))
				{
					yield return context.FetchDependency(RootGameObject, RootGameObjectName);
				}
				foreach (PPtr<Object> asset in context.FetchDependencies(Modification, ModificationName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(SourcePrefab, SourcePrefabName);
			}
			else
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(Objects, ObjectsName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(Father, FatherName);

				foreach (PPtr<Object> asset in base.FetchDependencies(context))
				{
					yield return asset;
				}
			}
		}

		public string GetName(ISerializedFile file)
		{
			if (IsModificationsFormat(file.Version))
			{
				return RootGameObject.GetAsset(file).Name;
			}
			else
			{
				return Name;
			}
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
			if (IsModificationsFormat(container.Version))
			{
				YAMLMappingNode node = ExportYAMLRootObject(container);
				node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
				bool hasRootGameObject = IsRootGameObjectRelevant(container.ExportFlags);
				bool isRootGameObjectFirst = IsRootGameObjectFirst(container.ExportVersion);
				if (hasRootGameObject && isRootGameObjectFirst)
				{
					node.Add(RootGameObjectName, RootGameObject.ExportYAML(container));
				}

				node.Add(ModificationName, Modification.ExportYAML(container));
				node.Add(GetSourcePrefabName(container.ExportVersion), SourcePrefab.ExportYAML(container));
				if (hasRootGameObject && !isRootGameObjectFirst)
				{
					node.Add(RootGameObjectName, RootGameObject.ExportYAML(container));
				}
				if (IsPrefabAssetRelevant(container.ExportVersion))
				{
					node.Add(GetIsPrefabAssetName(container.ExportVersion), IsPrefabAsset);
				}
				if (IsExplodedRelevant(container.ExportVersion))
				{
					node.Add(IsExplodedName, IsExploded);
				}
				return node;
			}
			else
			{
				YAMLMappingNode node = new YAMLMappingNode();
				node.Add(LastMergeIdentifierName, LastMergeIdentifier.ExportYAML(container));
				if (HasLastTemplateIdentifier(container.ExportVersion))
				{
					node.Add(LastTemplateIdentifierName, LastTemplateIdentifier.ExportYAML(container));
				}
				node.Add(ObjectsName, Objects.ExportYAML(container));
				node.Add(FatherName, Father.ExportYAML(container));
				node.Add(IsDataTemplateName, IsDataTemplate);

				YAMLMappingNode baseNode = base.ExportYAMLRoot(container);
				node.Append(baseNode);
				return node;
			}
		}

		protected override string GetClassName(Version version)
		{
			return GetPrefabInstanceName(version);
		}

		public static string GetPrefabInstanceName(Version version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return nameof(ClassIDType.PrefabInstance);
			}
			else if (version.IsGreaterEqual(3, 5))
			{
				return nameof(ClassIDType.Prefab);
			}
			else
			{
				return nameof(ClassIDType.DataTemplate);
			}
		}

		private static string GetSourcePrefabName(Version version)
		{
			if (version.IsGreaterEqual(2018, 2))
			{
				return SourcePrefabName;
			}
			else if (version.IsGreaterEqual(3, 5))
			{
				return ParentPrefabName;
			}
			else
			{
				return FatherName;
			}
		}

		private static string GetIsPrefabAssetName(Version version)
		{
			if (version.IsGreaterEqual(2018, 2))
			{
				return IsPrefabAssetName;
			}
			else if (version.IsGreaterEqual(3, 5))
			{
				return IsPrefabParentName;
			}
			else
			{
				return IsDataTemplateName;
			}
		}

		public override string ExportExtension => PrefabKeyword;

		public PPtr<EditorExtension>[] Objects { get; set; }
		public PPtr<PrefabInstance> ParentPrefab
		{
			get => SourcePrefab;
			set => SourcePrefab = value;
		}
		// NOTE: unknown version
		public PPtr<PrefabInstance> Prefab
		{
			get => SourcePrefab;
			set => SourcePrefab = value;
		}
		public PPtr<PrefabInstance> Father
		{
			get => SourcePrefab;
			set => SourcePrefab = value;
		}
		public bool IsPrefabAsset { get; set; }
		public bool IsPrefabParent
		{
			get => IsPrefabAsset;
			set => IsPrefabAsset = value;
		}
		public bool IsDataTemplate
		{
			get => IsPrefabAsset;
			set => IsPrefabAsset = value;
		}
		public bool IsExploded { get; set; }

		public const string PrefabKeyword = "prefab";

		public const string LastMergeIdentifierName = "m_LastMergeIdentifier";
		public const string LastTemplateIdentifierName = "m_LastTemplateIdentifier";
		public const string ObjectsName = "m_Objects";
		public const string ModificationName = "m_Modification";
		public const string SourcePrefabName = "m_SourcePrefab";
		public const string ParentPrefabName = "m_ParentPrefab";
		public new const string PrefabName = "m_Prefab";
		public const string FatherName = "m_Father";
		public const string RootGameObjectName = "m_RootGameObject";
		public const string IsPrefabAssetName = "m_IsPrefabAsset";
		public const string IsPrefabParentName = "m_IsPrefabParent";
		public const string IsDataTemplateName = "m_IsDataTemplate";
		public const string IsExplodedName = "m_IsExploded";

		public GUID LastMergeIdentifier;
		public GUID LastTemplateIdentifier;
		public PrefabModification Modification;
		public PPtr<PrefabInstance> SourcePrefab;
		public PPtr<GameObject> RootGameObject;
	}
}
