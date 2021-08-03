using AssetRipper.Core.Converters.Game;
using AssetRipper.Core.Classes;

namespace AssetRipper.Core.Layout.Classes
{
	public sealed class ComponentLayout
	{
		public ComponentLayout(LayoutInfo info) { }

		public static void GenerateTypeTree(TypeTreeContext context)
		{
			EditorExtensionLayout.GenerateTypeTree(context);
			ComponentLayout layout = context.Layout.Component;
			context.AddPPtr(context.Layout.GameObject.Name, layout.GameObjectName);
		}

		/// <summary>
		/// All versions
		/// </summary>
		public bool HasGameObject => true;

		public string Name => nameof(Component);
		public string GameObjectName => "m_GameObject";
	}
}
