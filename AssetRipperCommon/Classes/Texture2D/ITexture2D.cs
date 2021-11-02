using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.Texture2D
{
	public interface ITexture2D : IHasImageData, INamedObject
	{
		/// <summary>
		/// Actually int before 2020 and uint afterwards
		/// </summary>
		long CompleteImageSize { get; }
	}
}
