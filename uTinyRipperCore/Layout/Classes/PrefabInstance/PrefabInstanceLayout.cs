using uTinyRipper.Converters;
using uTinyRipper.Layout.Misc;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Layout
{
	/// <summary>
	/// 1.5.0 - first introduction
	/// </summary>
	public sealed class PrefabInstanceLayout
	{
		public PrefabInstanceLayout(LayoutInfo info)
		{
			PrefabModification = new PrefabModificationLayout(info);
			PropertyModification = new PropertyModificationLayout(info);

			if (info.Version.IsGreaterEqual(3, 5))
			{
				// NOTE: unknown conversion
				Version = 2;
			}
			else
			{
				Version = 1;
			}

			// Fields
			if (info.Version.IsGreaterEqual(3, 5))
			{
				if (info.Version.IsLess(2018, 3) || !info.Flags.IsEditorScene())
				{
					HasRootGameObject = true;
				}
				HasModification = true;
				if (info.Version.IsGreaterEqual(2018, 2))
				{
					HasSourcePrefab = true;
				}
				else
				{
					HasParentPrefab = true;
				}
				if (info.Version.IsLess(2018, 3))
				{
					HasIsPrefabAssetInvariant = true;
					if (info.Version.IsGreaterEqual(2018, 2))
					{
						HasIsPrefabAsset = true;
					}
					else
					{
						HasIsPrefabParent = true;
					}
				}
				if (info.Version.IsLess(5))
				{
					HasIsExploded = true;
				}
			}
			else
			{
				HasLastMergeIdentifier = true;
				if (info.Version.IsLess(2, 6))
				{
					HasLastTemplateIdentifier = true;
				}
				HasObjects = true;
				HasFather = true;
				HasIsPrefabAssetInvariant = true;
				HasIsDataTemplate = true;
			}

			// Flags
			if (info.Version.IsGreaterEqual(3, 5))
			{
				IsModificationFormat = true;
			}
			if (info.Version.IsGreaterEqual(2018, 3))
			{
				IsRootGameObjectFirst = true;
			}

			// Names
			if (info.Version.IsGreaterEqual(2018, 3))
			{
				Name = nameof(ClassIDType.PrefabInstance);
			}
			else if (info.Version.IsGreaterEqual(3, 5))
			{
				Name = nameof(ClassIDType.Prefab);
			}
			else
			{
				Name = nameof(ClassIDType.DataTemplate);
			}
			if (info.Version.IsGreaterEqual(2018, 2))
			{
				SourcePrefabInvariantName = SourcePrefabName;
			}
			else if (info.Version.IsGreaterEqual(3, 5))
			{
				SourcePrefabInvariantName = ParentPrefabName;
			}
			else
			{
				SourcePrefabInvariantName = FatherName;
			}
			if (info.Version.IsGreaterEqual(2018, 2))
			{
				IsPrefabAssetInvariantName = IsPrefabAssetName;
			}
			else if (info.Version.IsGreaterEqual(3, 5))
			{
				IsPrefabAssetInvariantName = IsPrefabParentName;
			}
			else
			{
				IsPrefabAssetInvariantName = IsDataTemplateName;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			PrefabInstanceLayout layout = context.Layout.PrefabInstance;
			context.AddNode(layout.Name, name, layout.Version);
			context.BeginChildren();
			if (layout.IsModificationFormat)
			{
				ObjectLayout.GenerateTypeTree(context);
				if (layout.HasRootGameObject && layout.IsRootGameObjectFirst)
				{
					context.AddPPtr(context.Layout.GameObject.Name, layout.RootGameObjectName);
				}

				PrefabModificationLayout.GenerateTypeTree(context, layout.ModificationName);
				if (layout.HasSourcePrefab)
				{
					context.AddPPtr(layout.Name, layout.SourcePrefabName);
				}
				else
				{
					context.AddPPtr(layout.Name, layout.ParentPrefabName);
				}
				if (!layout.IsRootGameObjectFirst)
				{
					context.AddPPtr(context.Layout.GameObject.Name, layout.RootGameObjectName);
				}
				if (layout.HasIsPrefabAsset)
				{
					context.AddBool(layout.IsPrefabAssetName);
				}
				else
				{
					context.AddBool(layout.IsPrefabParentName);
				}
				if (layout.HasIsExploded)
				{
					context.AddBool(layout.IsExplodedName);
				}
				context.Align();
			}
			else
			{
				GUIDLayout.GenerateTypeTree(context, layout.LastMergeIdentifierName);
				if (layout.HasLastTemplateIdentifier)
				{
					GUIDLayout.GenerateTypeTree(context, layout.LastTemplateIdentifierName);
				}
				context.AddArray(layout.ObjectsName, (c, n) => c.AddPPtr(c.Layout.EditorExtension.Name, n));
				context.AddPPtr(layout.Name, layout.FatherName);
				context.AddBool(layout.IsDataTemplateName, TransferMetaFlags.AlignBytesFlag);
				NamedObjectLayout.GenerateTypeTree(context);
			}
			context.EndChildren();
		}

		public PrefabModificationLayout PrefabModification { get; }
		public PropertyModificationLayout PropertyModification { get; }

		public int Version { get; }

		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public bool HasLastMergeIdentifier { get; }
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public bool HasLastTemplateIdentifier { get; }
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public bool HasObjects { get; }
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public bool HasModification { get; }
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasSourcePrefabInvariant => true;
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public bool HasSourcePrefab { get; }
		/// <summary>
		/// 3.5.0 to 2018.2 exclusive
		/// </summary>
		public bool HasParentPrefab { get; }
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public bool HasFather { get; }
		/// <summary>
		/// (3.5.0 to 2018.3 exclusive) or (2018.3 and greater and Editor Scene)
		/// </summary>
		public bool HasRootGameObject { get; }
		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public bool HasIsPrefabAssetInvariant { get; }
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public bool HasIsPrefabAsset { get; }
		/// <summary>
		/// 3.5.0 to 2018.2 exclusive
		/// </summary>
		public bool HasIsPrefabParent { get; }
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public bool HasIsDataTemplate { get; }
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public bool HasIsExploded { get; }

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public bool IsModificationFormat { get; }
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		public bool IsInheritedFromObject => IsModificationFormat;
		/// <summary>
		/// Less than 3.5.0
		/// </summary>
		public bool IsInheritedFromNamedObject => !IsModificationFormat;
		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public bool IsRootGameObjectFirst { get; }

		public string Name { get; }
		public string LastMergeIdentifierName => "m_LastMergeIdentifier";
		public string LastTemplateIdentifierName => "m_LastTemplateIdentifier";
		public string ObjectsName => "m_Objects";
		public string ModificationName => "m_Modification";
		public string SourcePrefabInvariantName { get; }
		public string SourcePrefabName => "m_SourcePrefab";
		public string ParentPrefabName => "m_ParentPrefab";
		public string FatherName => "m_Father";
		public string RootGameObjectName => "m_RootGameObject";
		public string IsPrefabAssetInvariantName { get; }
		public string IsPrefabAssetName => "m_IsPrefabAsset";
		public string IsPrefabParentName => "m_IsPrefabParent";
		public string IsDataTemplateName => "m_IsDataTemplate";
		public string IsExplodedName => "m_IsExploded";
	}
}
