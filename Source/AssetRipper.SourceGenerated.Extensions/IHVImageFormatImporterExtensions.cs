using AssetRipper.SourceGenerated.Classes.ClassID_1055;
using FilterMode = AssetRipper.SourceGenerated.Enums.FilterMode_0;

namespace AssetRipper.SourceGenerated.Extensions;

public static class IHVImageFormatImporterExtensions
{
	public static void SetToDefault(this IIHVImageFormatImporter importer)
	{
		importer.TextureSettings.FilterMode = (int)FilterMode.Bilinear;
		importer.TextureSettings.Aniso = 1;
		importer.SRGBTexture = true;
	}
}
