using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Generics;
using AssetRipper.Export.UnityProjects.Project.Collections;
using AssetRipper.Primitives;
using AssetRipper.SourceGenerated.Classes.ClassID_1007;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Texture;

namespace AssetRipper.Export.UnityProjects.Shaders;

public class ShaderExportCollection : AssetExportCollection<IShader>
{
	public ShaderExportCollection(ShaderExporterBase assetExporter, IShader asset) : base(assetExporter, asset)
	{
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		IShaderImporter importer = ShaderImporterFactory.CreateAsset(container.File, container.ExportVersion);
		if (importer.Has_NonModifiableTextures_C1007() && Asset.Has_NonModifiableTextures_C48())
		{
			PPtrConverter converter = new(Asset, importer);
			foreach ((Utf8String name, PPtr_Texture_5_0_0 pptr) in Asset.NonModifiableTextures_C48)
			{
				AssetPair<Utf8String, PPtr_Texture_5_0_0> pair = importer.NonModifiableTextures_C1007.AddNew();
				pair.Key = name;
				pair.Value.CopyValues(pptr, converter);
			}
		}
		if (importer.Has_AssetBundleName_C1007() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_C1007 = Asset.AssetBundleName;
		}
		return importer;
	}
}
