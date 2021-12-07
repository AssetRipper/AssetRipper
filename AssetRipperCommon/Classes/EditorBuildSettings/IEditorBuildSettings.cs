using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.EditorBuildSettings
{
	public interface IEditorBuildSettings : IUnityObjectBase
	{
		void InitializeScenesArray(int length);
		IScene[] Scenes { get; }
	}
}