namespace AssetRipper.Export.UnityProjects.Scripts.UnityPackages;

/// <summary>
/// A shader file in a Unity package.
/// </summary>
/// <param name="Name">The name the shader declares, such as "Universal Render Pipeline/Lit".</param>
/// <param name="Guid">The guid of the shader file.</param>
public sealed record class PackageShader(string Name, UnityGuid Guid, string PackageName, string PackageVersion);
