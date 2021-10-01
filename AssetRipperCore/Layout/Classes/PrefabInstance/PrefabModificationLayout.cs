using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.PrefabInstance;

namespace AssetRipper.Core.Layout.Classes.PrefabInstance
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
