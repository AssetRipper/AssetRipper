using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Metadata;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_1042;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Font;
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Miscellaneous
{
	public sealed class FontAssetExportCollection : AssetsExportCollection
	{
		public FontAssetExportCollection(FontAssetExporter assetExporter, IFont font) : base(assetExporter, font)
		{
			if (font.TryGetFontMaterial(out IMaterial? fontMaterial))
			{
				Debug.Assert(fontMaterial.MainAsset == font);
				AddAsset(fontMaterial);
			}
		}

		protected override IUnityObjectBase CreateImporter(IExportContainer container)
		{
			IFont origin = (IFont)Asset;
			ITrueTypeFontImporter instance = TrueTypeFontImporterFactory.CreateAsset(container.ExportVersion, container.File);
			instance.FontSize_C1042 = (int)origin.FontSize_C128;
			instance.IncludeFontData_C1042 = true;
			instance.Style_C1042 = (int)origin.GetDefaultStyle();
			if (origin.FontNames_C128.Count > 0)
			{
				instance.FontName_C1042?.CopyValues(origin.FontNames_C128[0]);
				foreach (Utf8String name in origin.FontNames_C128)
				{
					instance.FontNames_C1042.AddNew().CopyValues(name);
				}
			}
			if (origin.Has_FallbackFonts_C128() && instance.Has_FallbackFontReferences_C1042())
			{
				PPtrConverter ptrConverter = new(origin, instance);
				foreach (IPPtr_Font ptrFont in origin.FallbackFonts_C128)
				{
					instance.FallbackFontReferences_C1042.AddNew().CopyValues(ptrFont, ptrConverter);
				}
			}
			instance.CharacterSpacing_C1042 = origin.CharacterSpacing_C128;
			instance.CharacterPadding_C1042 = origin.CharacterPadding_C128;
			instance.FontRenderingMode_C1042 = origin.FontRenderingMode_C128;
			instance.AscentCalculationMode_C1042E = AscentCalculationMode.FaceAscender;
			instance.ForceTextureCase_C1042E = FontTextureCase.Dynamic;
			instance.UseLegacyBoundsCalculation_C1042 = origin.UseLegacyBoundsCalculation_C128;
			instance.ShouldRoundAdvanceValue_C1042 = origin.ShouldRoundAdvanceValue_C128;
			if (instance.Has_AssetBundleName_C1042() && origin.AssetBundleName is not null)
			{
				instance.AssetBundleName_C1042.String = origin.AssetBundleName;
			}
			return instance;
		}

		protected override string GetExportExtension(IUnityObjectBase asset)
		{
			byte[] fontData = ((IFont)asset).FontData_C128;
			uint type = BitConverter.ToUInt32(fontData, 0);
			return type == OttoAsciiFourCC ? "otf" : "ttf";
		}

		protected override long GenerateExportID(IUnityObjectBase asset)
		{
			Debug.Assert(asset is IMaterial);
			return ExportIdHandler.GetMainExportID(asset);//The font material always has the same id: 2100000
		}

		/// <summary>
		/// OTTO ascii
		/// </summary>
		private const int OttoAsciiFourCC = 0x4F54544F;
	}
}
