using AssetRipper.SourceGenerated.Classes.ClassID_1055;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class IHVImageFormatImporterExtensions
	{
		public static void SetToDefault(this IIHVImageFormatImporter importer)
		{
			importer.TextureSettings_C1055.FilterMode = (int)Classes.Texture2D.FilterMode.Bilinear;
			importer.TextureSettings_C1055.Aniso = 1;
			importer.SRGBTexture_C1055 = true;
		}
	}
}
