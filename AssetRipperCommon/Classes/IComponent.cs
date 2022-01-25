using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;

namespace AssetRipper.Core.Classes
{
	public interface IComponent : IEditorExtension
	{
		PPtr<IGameObject> GameObjectPtr { get; }
	}

	public static class ComponentExtensions
	{
		public static IGameObject GetGameObject(this IComponent component)
		{
			return component.GameObjectPtr.GetAsset(component.SerializedFile);
		}

		public static IGameObject TryGetGameObject(this IComponent component)
		{
			return component.GameObjectPtr.TryGetAsset(component.SerializedFile);
		}

		public static IGameObject GetRoot(this IComponent component)
		{
			return component.GetGameObject().GetRoot();
		}

		public static int GetRootDepth(this IComponent component)
		{
			return component.GetGameObject().GetRootDepth();
		}
	}
}
