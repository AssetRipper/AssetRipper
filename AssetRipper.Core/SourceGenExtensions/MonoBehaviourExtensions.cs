using AssetRipper.Core.Classes.Misc;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MonoBehaviourExtensions
	{
		/// <summary>
		/// Whether this MonoBehaviour belongs to scene/prefab hierarchy or not<br/>
		/// TODO: find out why GameObject may have a value like PPtr(0, 894) even though such game object doesn't exist
		/// </summary>
		public static bool IsSceneObject(this IMonoBehaviour monoBehaviour) => !monoBehaviour.GameObject_C114.IsNull();
		public static bool IsScriptableObject(this IMonoBehaviour monoBehaviour) => monoBehaviour.NameString.Length > 0;
	}
}
