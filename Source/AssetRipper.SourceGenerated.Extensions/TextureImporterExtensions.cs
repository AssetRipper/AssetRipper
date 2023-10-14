using AssetRipper.SourceGenerated.Classes.ClassID_1006;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TextureImporterExtensions
{
	public static void GetSwizzle(this ITextureImporter importer, out TextureImporterSwizzle channel0, out TextureImporterSwizzle channel1, out TextureImporterSwizzle channel2, out TextureImporterSwizzle channel3)
	{
		if (importer.Has_Swizzle())
		{
			uint value = importer.Swizzle;
			channel0 = (TextureImporterSwizzle)(value & 0x_00_00_00_FF);
			channel1 = (TextureImporterSwizzle)((value & 0x_00_00_FF_00) >> 8);
			channel2 = (TextureImporterSwizzle)((value & 0x_00_FF_00_00) >> 16);
			channel3 = (TextureImporterSwizzle)(value >> 24);
		}
		else
		{
			channel0 = TextureImporterSwizzle.R;
			channel1 = TextureImporterSwizzle.G;
			channel2 = TextureImporterSwizzle.B;
			channel3 = TextureImporterSwizzle.A;
		}
	}

	public static void SetSwizzle(this ITextureImporter importer, TextureImporterSwizzle channel0, TextureImporterSwizzle channel1, TextureImporterSwizzle channel2, TextureImporterSwizzle channel3)
	{
		if (importer.Has_Swizzle())
		{
			uint value = (byte)channel0 | ((uint)(byte)channel1 << 8) | ((uint)(byte)channel2 << 16) | ((uint)(byte)channel3 << 24);
			importer.Swizzle = value;
		}
	}
}
