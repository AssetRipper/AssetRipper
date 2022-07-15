using AssetRipper.Core.Classes.RenderTexture;
using AssetRipper.SourceGenerated.Classes.ClassID_84;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class RenderTextureExtensions
	{
		public static RenderTextureFormat GetColorFormat(this IRenderTexture texture)
		{
			return (RenderTextureFormat)texture.ColorFormat_C84;
		}
	}
}
