using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.PrefabInstance
{
	public sealed class PrefabInstance : NamedObject, IPrefabInstance
	{
		public PrefabInstance(LayoutInfo layout) : base(layout)
		{
			Objects = Array.Empty<PPtr<EditorExtension>>();
		}

		public PrefabInstance(AssetInfo assetInfo) : base(assetInfo)
		{
			Objects = Array.Empty<PPtr<EditorExtension>>();
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(3, 5))
			{
				// NOTE: unknown conversion
				return 2;
			}
			else
			{
				return 1;
			}
		}

		public override void Read(AssetReader reader)
		{
			if (IsModificationFormat(reader.Version))
			{
				ReadObject(reader);

				if (HasRootGameObject(reader.Version, reader.Flags) && IsRootGameObjectFirst(reader.Version))
				{
					RootGameObject.Read(reader);
				}

				Modification.Read(reader);
				SourcePrefab.Read(reader);
				if (!IsRootGameObjectFirst(reader.Version))
				{
					RootGameObject.Read(reader);
				}
				if (HasIsPrefabAsset(reader.Version))
				{
					IsPrefabAsset = reader.ReadBoolean();
				}
				if (HasIsExploded(reader.Version))
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
			if (IsModificationFormat(writer.Version))
			{
				WriteObject(writer);

				if (HasRootGameObject(writer.Version, writer.Flags) && IsRootGameObjectFirst(writer.Version))
				{
					RootGameObject.Write(writer);
				}

				Modification.Write(writer);
				SourcePrefab.Write(writer);
				if (!IsRootGameObjectFirst(writer.Version))
				{
					RootGameObject.Write(writer);
				}
				if (HasIsPrefabAsset(writer.Version))
				{
					writer.Write(IsPrefabAsset);
				}
				if (HasIsExploded(writer.Version))
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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			if (IsModificationFormat(context.Version))
			{
				foreach (PPtr<IUnityObjectBase> asset in FetchDependenciesObject(context))
				{
					yield return asset;
				}

				if (HasRootGameObject(context.Version, context.Flags))
				{
					yield return context.FetchDependency(RootGameObject, RootGameObjectName);
				}
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(Modification, ModificationName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(SourcePrefab, SourcePrefabName);
			}
			else
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Objects, ObjectsName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(Father, FatherName);

				foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
				{
					yield return asset;
				}
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
			if (IsModificationFormat(container.ExportVersion))
			{
				YAMLMappingNode node = ExportYAMLRootObject(container);
				node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
				if (HasRootGameObject(container.ExportVersion, container.ExportFlags) && IsRootGameObjectFirst(container.ExportVersion))
				{
					node.Add(RootGameObjectName, RootGameObject.ExportYAML(container));
				}

				node.Add(ModificationName, Modification.ExportYAML(container));
				node.Add(SourcePrefabInvariantName(container.ExportVersion), SourcePrefab.ExportYAML(container));
				if (!IsRootGameObjectFirst(container.ExportVersion))
				{
					node.Add(RootGameObjectName, RootGameObject.ExportYAML(container));
				}
				if (HasIsPrefabAssetInvariant(container.ExportVersion))
				{
					node.Add(IsPrefabAssetInvariantName(container.ExportVersion), IsPrefabAsset);
				}
				if (HasIsExploded(container.ExportVersion))
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

		public static string SourcePrefabInvariantName(UnityVersion version)
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

		public static string IsPrefabAssetInvariantName(UnityVersion version)
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

		#region VersionMethods
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasLastMergeIdentifier(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool HasLastTemplateIdentifier(UnityVersion version) => version.IsLess(2, 6);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasObjects(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool HasModification(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasSourcePrefab(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 3.5.0 to 2018.2 exclusive
		/// </summary>
		public static bool HasParentPrefab(UnityVersion version) => version.IsGreaterEqual(3, 5) && version.IsLess(2018, 2);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasFather(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// (3.5.0 to 2018.3 exclusive) or (2018.3 and greater and Editor Scene)
		/// </summary>
		public static bool HasRootGameObject(UnityVersion version, TransferInstructionFlags flags)
		{
			return (version.IsGreaterEqual(3, 5) && version.IsLess(2018, 3)) ||
				(flags.IsEditorScene() && version.IsGreaterEqual(2018, 3));
		}
		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public static bool HasIsPrefabAssetInvariant(UnityVersion version) => version.IsLess(2018, 3);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasIsPrefabAsset(UnityVersion version) => version.IsGreaterEqual(2018, 2);
		/// <summary>
		/// 3.5.0 to 2018.2 exclusive
		/// </summary>
		public static bool HasIsPrefabParent(UnityVersion version) => version.IsGreaterEqual(3, 5) && version.IsLess(2018, 2);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasIsDataTemplate(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool HasIsExploded(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsModificationFormat(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public static bool IsInheritedFromObject(UnityVersion version) => version.IsGreaterEqual(3, 5);
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public static bool IsInheritedFromNamedObject(UnityVersion version) => version.IsLess(3, 5);
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public static bool IsRootGameObjectFirst(UnityVersion version) => version.IsGreaterEqual(2018, 3);
		#endregion


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

		public UnityGUID LastMergeIdentifier = new();
		public UnityGUID LastTemplateIdentifier = new();
		public PrefabModification Modification = new();
		public PPtr<PrefabInstance> SourcePrefab = new();
		public PPtr<GameObject.GameObject> RootGameObject = new();

		public PPtr<IGameObject> RootGameObjectPtr
		{
			get => RootGameObject.CastTo<IGameObject>();
			set => RootGameObject = value.CastTo<GameObject.GameObject>();
		}
		public PPtr<IPrefabInstance> SourcePrefabPtr
		{
			get => SourcePrefab.CastTo<IPrefabInstance>();
			set => SourcePrefab = value.CastTo<PrefabInstance>();
		}

		public const string LastMergeIdentifierName = "m_LastMergeIdentifier";
		public const string LastTemplateIdentifierName = "m_LastTemplateIdentifier";
		public const string ObjectsName = "m_Objects";
		public const string ModificationName = "m_Modification";
		public const string SourcePrefabName = "m_SourcePrefab";
		public const string ParentPrefabName = "m_ParentPrefab";
		public const string FatherName = "m_Father";
		public const string RootGameObjectName = "m_RootGameObject";
		public const string IsPrefabAssetName = "m_IsPrefabAsset";
		public const string IsPrefabParentName = "m_IsPrefabParent";
		public const string IsDataTemplateName = "m_IsDataTemplate";
		public const string IsExplodedName = "m_IsExploded";
	}
}
