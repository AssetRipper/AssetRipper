using AssetRipper.Assets;
using AssetRipper.Assets.Cloning;
using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Classes.ClassID_1007;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Subclasses.PPtr_Texture;

namespace AssetRipper.Export.UnityProjects.Shaders;

public class ShaderExportCollection : AssetExportCollection<IShader>
{
	public ShaderExportCollection(ShaderExporterBase assetExporter, IShader asset) : base(assetExporter, asset)
	{
	}

	public override bool Export(IExportContainer container, string projectDirectory, FileSystem fileSystem)
	{
		string subPath = fileSystem.Path.Join(projectDirectory, FileSystem.FixInvalidPathCharacters(Asset.GetBestDirectory()));
		if (subPath.EndsWith("/Shader", StringComparison.OrdinalIgnoreCase))
		{
			subPath = subPath.Substring(0, subPath.Length - 6) + "Shaders";
		}
		else if (subPath.EndsWith("\\Shader", StringComparison.OrdinalIgnoreCase))
		{
			subPath = subPath.Substring(0, subPath.Length - 6) + "Shaders";
		}
		else if (subPath.Contains("/Shader/", StringComparison.OrdinalIgnoreCase))
		{
			subPath = subPath.Replace("/Shader/", "/Shaders/", StringComparison.OrdinalIgnoreCase);
		}
		else if (subPath.Contains("\\Shader\\", StringComparison.OrdinalIgnoreCase))
		{
			subPath = subPath.Replace("\\Shader\\", "\\Shaders\\", StringComparison.OrdinalIgnoreCase);
		}

		string fileName = GetUniqueFileName(Asset, subPath, fileSystem);

		fileSystem.Directory.Create(subPath);

		string filePath = fileSystem.Path.Join(subPath, fileName);
		bool result = ExportInner(container, filePath, projectDirectory, fileSystem);
		if (result)
		{
			Meta meta = new Meta(GUID, CreateImporter(container));
			ExportMeta(container, meta, filePath, fileSystem);
			return true;
		}
		return false;
	}

	protected override IUnityObjectBase CreateImporter(IExportContainer container)
	{
		IShaderImporter importer = ShaderImporter.Create(container.File, container.ExportVersion);
		if (importer.Has_NonModifiableTextures() && Asset.Has_NonModifiableTextures())
		{
			PPtrConverter converter = new(Asset, importer);
			foreach ((Utf8String name, PPtr_Texture_5 pptr) in Asset.NonModifiableTextures)
			{
				AssetPair<Utf8String, PPtr_Texture_5> pair = importer.NonModifiableTextures.AddNew();
				pair.Key = name;
				pair.Value.CopyValues(pptr, converter);
			}
		}
		if (importer.Has_AssetBundleName_R() && Asset.AssetBundleName is not null)
		{
			importer.AssetBundleName_R = Asset.AssetBundleName;
		}
		return importer;
	}
}
