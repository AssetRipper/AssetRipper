using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface ITextAsset : IUnityObjectBase
	{
		string Script { get; }
	}
}
