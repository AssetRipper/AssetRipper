using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class MonoBehaviourExtensions
	{
		/// <summary>
		/// Does this MonoBehaviour belongs to scene/prefab hierarchy? In other words, is <see cref="IMonoBehaviour.GameObject"/> a non-null pptr?
		/// </summary>
		public static bool IsSceneObject(this IMonoBehaviour monoBehaviour) => !monoBehaviour.GameObject.IsNull();
		/// <summary>
		/// Does this MonoBehaviour have a name?
		/// </summary>
		public static bool IsScriptableObject(this IMonoBehaviour monoBehaviour) => !monoBehaviour.Name.IsEmpty;
	}
}
