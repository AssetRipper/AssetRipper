using System.Collections.Generic;
using uTinyRipper.Classes.Prefabs;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;
using System;
using uTinyRipper.Layout;

namespace uTinyRipper.Classes
{
	public sealed class PrefabInstance : NamedObject
	{
		public PrefabInstance(AssetLayout layout):
			base(layout)
		{
			Objects = Array.Empty<PPtr<EditorExtension>>();
			Modification = new PrefabModification(layout);
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
			instance.Modification = new PrefabModification(virtualFile.Layout);
			instance.RootGameObject = root.File.CreatePPtr(root);
			instance.IsPrefabAsset = true;
			instance.Name = root.Name;
			return instance;
		}

		public override void Read(AssetReader reader)
		{
			PrefabInstanceLayout layout = reader.Layout.PrefabInstance;
			if (layout.IsModificationFormat)
			{
				ReadObject(reader);

				if (layout.HasRootGameObject && layout.IsRootGameObjectFirst)
				{
					RootGameObject.Read(reader);
				}

				Modification.Read(reader);
				SourcePrefab.Read(reader);
				if (!layout.IsRootGameObjectFirst)
				{
					RootGameObject.Read(reader);
				}
				if (layout.HasIsPrefabAsset)
				{
					IsPrefabAsset = reader.ReadBoolean();
				}
				if (layout.HasIsExploded)
				{
					IsExploded = reader.ReadBoolean();
				}
				reader.AlignStream();
			}
			else
			{
				LastMergeIdentifier.Read(reader);
				if (layout.HasLastTemplateIdentifier)
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
			PrefabInstanceLayout layout = writer.Layout.PrefabInstance;
			if (layout.IsModificationFormat)
			{
				WriteObject(writer);

				if (layout.HasRootGameObject && layout.IsRootGameObjectFirst)
				{
					RootGameObject.Write(writer);
				}

				Modification.Write(writer);
				SourcePrefab.Write(writer);
				if (!layout.IsRootGameObjectFirst)
				{
					RootGameObject.Write(writer);
				}
				if (layout.HasIsPrefabAsset)
				{
					writer.Write(IsPrefabAsset);
				}
				if (layout.HasIsExploded)
				{
					writer.Write(IsExploded);
				}
				writer.AlignStream();
			}
			else
			{
				LastMergeIdentifier.Write(writer);
				if (layout.HasLastTemplateIdentifier)
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
			//if (file.Layout.PrefabInstance.IsModificationFormat)
			{
				foreach (EditorExtension asset in RootGameObject.GetAsset(file).FetchHierarchy())
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
			PrefabInstanceLayout layout = context.Layout.PrefabInstance;
			if (layout.IsModificationFormat)
			{
				foreach (PPtr<Object> asset in FetchDependenciesObject(context))
				{
					yield return asset;
				}

				if (layout.HasRootGameObject)
				{
					yield return context.FetchDependency(RootGameObject, layout.RootGameObjectName);
				}
				foreach (PPtr<Object> asset in context.FetchDependencies(Modification, layout.ModificationName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(SourcePrefab, layout.SourcePrefabName);
			}
			else
			{
				foreach (PPtr<Object> asset in context.FetchDependencies(Objects, layout.ObjectsName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(Father, layout.FatherName);

				foreach (PPtr<Object> asset in base.FetchDependencies(context))
				{
					yield return asset;
				}
			}
		}

		public string GetName(IAssetContainer file)
		{
			if (file.Layout.PrefabInstance.IsModificationFormat)
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
			PrefabInstanceLayout layout = container.ExportLayout.PrefabInstance;
			if (layout.IsModificationFormat)
			{
				YAMLMappingNode node = ExportYAMLRootObject(container);
				node.AddSerializedVersion(layout.Version);
				if (layout.HasRootGameObject && layout.IsRootGameObjectFirst)
				{
					node.Add(layout.RootGameObjectName, RootGameObject.ExportYAML(container));
				}

				node.Add(layout.ModificationName, Modification.ExportYAML(container));
				node.Add(layout.SourcePrefabInvariantName, SourcePrefab.ExportYAML(container));
				if (!layout.IsRootGameObjectFirst)
				{
					node.Add(layout.RootGameObjectName, RootGameObject.ExportYAML(container));
				}
				if (layout.HasIsPrefabAssetInvariant)
				{
					node.Add(layout.IsPrefabAssetInvariantName, IsPrefabAsset);
				}
				if (layout.HasIsExploded)
				{
					node.Add(layout.IsExplodedName, IsExploded);
				}
				return node;
			}
			else
			{
				YAMLMappingNode node = new YAMLMappingNode();
				node.Add(layout.LastMergeIdentifierName, LastMergeIdentifier.ExportYAML(container));
				if (layout.HasLastTemplateIdentifier)
				{
					node.Add(layout.LastTemplateIdentifierName, LastTemplateIdentifier.ExportYAML(container));
				}
				node.Add(layout.ObjectsName, Objects.ExportYAML(container));
				node.Add(layout.FatherName, Father.ExportYAML(container));
				node.Add(layout.IsDataTemplateName, IsDataTemplate);

				YAMLMappingNode baseNode = base.ExportYAMLRoot(container);
				node.Append(baseNode);
				return node;
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

		public UnityGUID LastMergeIdentifier;
		public UnityGUID LastTemplateIdentifier;
		public PrefabModification Modification;
		public PPtr<PrefabInstance> SourcePrefab;
		public PPtr<GameObject> RootGameObject;
	}
}
