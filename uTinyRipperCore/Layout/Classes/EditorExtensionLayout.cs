using uTinyRipper.Classes;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class EditorExtensionLayout
	{
		public EditorExtensionLayout(LayoutInfo info)
		{
			if (info.Version.IsLess(3, 5) && !info.Flags.IsRelease())
			{
				HasExtensionPtr = true;
			}
			if (info.Version.IsGreaterEqual(3, 5) && !info.Flags.IsRelease())
			{
				HasCorrespondingSourceObjectInvariant = true;
				if (info.Version.IsGreaterEqual(2018, 2))
				{
					HasCorrespondingSourceObject = true;
				}
				else
				{
					HasPrefabParentObject = true;
				}

				HasPrefabInstanceInvariant = true;
				if (info.Version.IsGreaterEqual(2018, 3))
				{
					HasPrefabInstance = true;
				}
				else
				{
					HasPrefabInternal = true;
				}
			}
			if (info.Version.IsGreaterEqual(2018, 3) && !info.Flags.IsRelease())
			{
				HasPrefabAsset = true;
			}

			CorrespondingSourceObjectInvariantName = HasCorrespondingSourceObject ?	CorrespondingSourceObjectName : PrefabParentObjectName;
			PrefabInstanceInvariantName = HasPrefabInstance ? PrefabInstanceName : PrefabInternalName;
		}

		public static void GenerateTypeTree(TypeTreeContext context)
		{
			ObjectLayout.GenerateTypeTree(context);
			EditorExtensionLayout layout = context.Layout.EditorExtension;
			if (layout.HasExtensionPtr)
			{
				context.AddPPtr(context.Layout.Object.Name, layout.ExtensionPtrName);
			}
			if (layout.HasCorrespondingSourceObject)
			{
				context.AddPPtr(layout.Name, layout.CorrespondingSourceObjectInvariantName);
				context.AddPPtr(context.Layout.PrefabInstance.Name, layout.PrefabInstanceInvariantName);
			}
			if (layout.HasPrefabAsset)
			{
				context.AddPPtr(context.Layout.Prefab.Name, layout.PrefabAssetName);
			}
		}

		public int Version => 1;

		/// <summary>
		/// Less than 3.5.0 and Not Release
		/// </summary>
		public bool HasExtensionPtr { get; }
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public bool HasCorrespondingSourceObjectInvariant { get; }
		/// <summary>
		/// 2018.2 and greater and Not Release
		/// </summary>
		public bool HasCorrespondingSourceObject { get; }
		/// <summary>
		/// 3.5.0 to 2018.2 exclusive and Not Release
		/// </summary>
		public bool HasPrefabParentObject { get; }
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public bool HasPrefabInstanceInvariant { get; }
		/// <summary>
		/// 2018.3 and greater and Not Release
		/// </summary>
		public bool HasPrefabInstance { get; }
		/// <summary>
		/// 3.5.0 to 2018.3 exclusive and Not Release
		/// </summary>
		public bool HasPrefabInternal { get; }
		/// <summary>
		/// 2018.3 and greater and Not Release
		/// </summary>
		public bool HasPrefabAsset { get; }

		public string Name => nameof(EditorExtension);
		public string ExtensionPtrName => "m_ExtensionPtr";
		public string CorrespondingSourceObjectInvariantName { get; }
		public string CorrespondingSourceObjectName => "m_CorrespondingSourceObject";
		public string PrefabParentObjectName => "m_PrefabParentObject";
		public string PrefabInstanceInvariantName { get; }
		public string PrefabInstanceName => "m_PrefabInstance";
		public string PrefabInternalName => "m_PrefabInternal";
		public string PrefabAssetName => "m_PrefabAsset";
	}
}
