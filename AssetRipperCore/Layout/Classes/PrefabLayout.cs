using AssetRipper.Core.Classes;

namespace AssetRipper.Core.Layout.Classes
{
	/// <summary>
	/// 2018.3 - first introduction
	/// </summary>
	public sealed class PrefabLayout
	{
		public PrefabLayout(LayoutInfo info) { }

		public string Name => nameof(Prefab);
		public string RootGameObjectName => "m_RootGameObject";
	}
}
