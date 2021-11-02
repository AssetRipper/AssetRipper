using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface IComponent : IUnityObjectBase
	{
		PPtr<IGameObject> GameObjectPtr { get; }
	}
}
