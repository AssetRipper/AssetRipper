using uTinyRipper.Classes.Prefabs;
using uTinyRipper.Converters;

namespace uTinyRipper.Layout
{
	public sealed class PrefabModificationLayout
	{
		public PrefabModificationLayout(LayoutInfo info)
		{
			if (info.Version.IsGreaterEqual(2018, 3))
			{
				IsComponentPointer = true;
			}
		}

		public static void GenerateTypeTree(TypeTreeContext context, string name)
		{
			PrefabModificationLayout layout = context.Layout.PrefabInstance.PrefabModification;
			context.AddNode(layout.Name, name);
			context.BeginChildren();
			context.AddPPtr(context.Layout.Transform.Name, layout.TransformParentName);
			context.AddArray(layout.ModificationsName, PropertyModificationLayout.GenerateTypeTree);
			if (layout.IsComponentPointer)
			{
				context.AddArray(layout.RemovedComponentsName, (c, n) => c.AddPPtr(c.Layout.Component.Name, n));
			}
			else
			{
				context.AddArray(layout.RemovedComponentsName, (c, n) => c.AddPPtr(c.Layout.Object.Name, n));
			}
			context.EndChildren();
		}

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasTransformParent => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasModifications => true;
		/// <summary>
		/// All versions
		/// </summary>
		public bool HasRemovedComponents => true;

		/// <summary>
		/// 2018.3 and greater
		/// </summary>
		public bool IsComponentPointer { get; }

		public string Name => nameof(PrefabModification);
		public string TransformParentName => "m_TransformParent";
		public string ModificationsName => "m_Modifications";
		public string RemovedComponentsName => "m_RemovedComponents";
	}
}
