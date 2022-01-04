using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface IMovieTexture : IUnityObjectBase
	{
		byte[] MovieData { get; set; }
	}
}
