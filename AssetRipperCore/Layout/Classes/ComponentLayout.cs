using AssetRipper.Core.Classes;

namespace AssetRipper.Core.Layout.Classes
{
	public sealed class ComponentLayout
	{
		public ComponentLayout(LayoutInfo info) { }

		public string Name => nameof(Component);
		public string GameObjectName => "m_GameObject";
	}
}
