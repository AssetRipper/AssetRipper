using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Export.UnityProjects.Project;
using AssetRipper.SourceGenerated.Classes.ClassID_1042;
using AssetRipper.SourceGenerated.Classes.ClassID_128;
using AssetRipper.SourceGenerated.Classes.ClassID_21;
using AssetRipper.SourceGenerated.Classes.ClassID_27;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Font;
using System.Diagnostics;

namespace AssetRipper.Export.UnityProjects.Miscellaneous;

public sealed class FontAssetExportCollection : AssetsExportCollection<IFont>
{
	public FontAssetExportCollection(FontAssetExporter assetExporter, IFont font) : base(assetExporter, font)
	{
		if (font.TryGetFontMaterial(out IMaterial? fontMaterial))
		{
			Debug.Assert(fontMaterial.MainAsset == font);
			AddAsset(fontMaterial);
		}
		if (font.TryGetFontTexture(out ITexture? fontTexture))
		{
			Debug.Assert(fontTexture.MainAsset == font);
			AddAsset(fontTexture);
		}
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		IFont origin = Asset;
		ITrueTypeFontImporter instance = TrueTypeFontImporter.Create(container.File, container.ExportVersion);
		instance.FontSize = (int)origin.FontSize;
		instance.IncludeFontData = true;
		instance.Style = (int)origin.GetDefaultStyle();
		if (origin.FontNames.Count > 0)
		{
			instance.FontName = origin.FontNames[0];
			foreach (Utf8String name in origin.FontNames)
			{
				instance.FontNames.Add(name);
			}
		}
		if (origin.Has_FallbackFonts() && instance.Has_FallbackFontReferences())
		{
			PPtrConverter ptrConverter = new(origin, instance);
			foreach (IPPtr_Font ptrFont in origin.FallbackFonts)
			{
				instance.FallbackFontReferences.AddNew().CopyValues(ptrFont, ptrConverter);
			}
		}
		instance.CharacterSpacing = origin.CharacterSpacing;
		instance.CharacterPadding = origin.CharacterPadding;
		instance.FontRenderingMode = origin.FontRenderingMode;
		instance.AscentCalculationModeE = AscentCalculationMode.FaceAscender;
		instance.ForceTextureCaseE = FontTextureCase.Dynamic;
		instance.UseLegacyBoundsCalculation = origin.UseLegacyBoundsCalculation;
		instance.ShouldRoundAdvanceValue = origin.ShouldRoundAdvanceValue;
		if (instance.Has_AssetBundleName_R() && origin.AssetBundleName is not null)
		{
			instance.AssetBundleName_R = origin.AssetBundleName;
		}
		return instance;
	}

	protected override string GetExportExtension(IUnityObjectBase asset)
	{
		return ((IFont)asset).GetFontExtension();
	}

	protected override long GenerateExportID(IUnityObjectBase asset)
	{
		Debug.Assert(asset is IMaterial or ITexture);
		return ExportIdHandler.GetMainExportID(asset);
		//The font material always has the same id: 2100000
		//Source: https://github.com/AssetRipper/TestProjects/blob/b19ddf4550504790d9da266d4fb4ec457859076e/2018/4/FontExportTest/Assets/Scenes/SampleScene.unity#LL172C14-L172C21

		//The font texture is assumed to be 2800000, but has not been verified.
	}
}
