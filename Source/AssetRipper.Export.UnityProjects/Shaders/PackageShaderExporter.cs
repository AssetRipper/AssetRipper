using AssetRipper.Assets;
using AssetRipper.Export.UnityProjects.Scripts.UnityPackages;
using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_48;

namespace AssetRipper.Export.UnityProjects.Shaders;

/// <summary>
/// Replaces shaders that were compiled from a Unity package with references to the package's shader files.
/// </summary>
/// <remarks>
/// Compiled shaders can't be turned back into source, so other exporters write placeholders. A shader that comes
/// from a package the project references, such as URP's Lit shader, can instead point at the package's own file.
/// Only packages that the game's assemblies came from are used, so a shader is never matched to a package the game
/// didn't include.
/// </remarks>
public sealed class PackageShaderExporter : IAssetExporter
{
	private static long ShaderFileID { get; } = ExportIdHandler.GetMainExportID((int)ClassIDType.Shader);

	private readonly UnityPackageIndex index;
	private readonly IReadOnlySet<string> referencedPackages;

	public PackageShaderExporter(UnityPackageIndex index, IReadOnlySet<string> referencedPackages)
	{
		this.index = index;
		this.referencedPackages = referencedPackages;
	}

	public bool TryCreateCollection(IUnityObjectBase asset, [NotNullWhen(true)] out IExportCollection? exportCollection)
	{
		if (asset is IShader shader && TryGetPackageShader(shader, out PackageShader? packageShader))
		{
			exportCollection = new SingleRedirectExportCollection(asset, ShaderFileID, packageShader.Guid, AssetType.Meta);
			return true;
		}
		exportCollection = null;
		return false;
	}

	public bool TryGetPackageShader(IShader shader, [NotNullWhen(true)] out PackageShader? packageShader)
	{
		if (shader.Has_ParsedForm()
			&& index.TryGetShader(shader.ParsedForm.Name, out packageShader)
			&& referencedPackages.Contains(packageShader.PackageName))
		{
			return true;
		}
		packageShader = null;
		return false;
	}

	public AssetType ToExportType(IUnityObjectBase asset) => AssetType.Meta;

	public bool ToUnknownExportType(Type type, out AssetType assetType)
	{
		assetType = AssetType.Meta;
		return false;
	}
}
