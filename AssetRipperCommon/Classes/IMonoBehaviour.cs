using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface IMonoBehaviour : IComponent, IHasName
	{
	}

	public static class IMonoBehaviourExtensions
	{
		/// <summary>
		/// Whether this MonoBeh belongs to scene/prefab hierarchy or not
		/// </summary>
#warning TODO: find out why GameObject may have a value like PPtr(0, 894) even though such game object doesn't exist
		public static bool IsSceneObject(this IMonoBehaviour monoBehaviour) => !monoBehaviour.GameObjectPtr.IsNull;
		public static bool IsScriptableObject(this IMonoBehaviour monoBehaviour) => monoBehaviour.Name.Length > 0;
	}
}
