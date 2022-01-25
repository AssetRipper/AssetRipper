using AssetRipper.Core.Classes.Texture2D;

namespace AssetRipper.Core.Interfaces
{
	public interface IHasImageData
	{
		public byte[] ImageDataByteArray { get; }

		public TextureFormat TextureFormat { get; }

		public int Width { get; }

		public int Height { get; }
	}
}