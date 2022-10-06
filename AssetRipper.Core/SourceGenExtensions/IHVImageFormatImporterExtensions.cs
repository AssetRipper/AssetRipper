using AssetRipper.SourceGenerated.Classes.ClassID_1055;
using FilterMode = AssetRipper.SourceGenerated.Enums.FilterMode_0;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class IHVImageFormatImporterExtensions
	{
		public static void SetToDefault(this IIHVImageFormatImporter importer)
		{
			importer.TextureSettings_C1055.FilterMode = (int)FilterMode.Bilinear;
			importer.TextureSettings_C1055.Aniso = 1;
			importer.SRGBTexture_C1055 = true;
		}
	}
}
