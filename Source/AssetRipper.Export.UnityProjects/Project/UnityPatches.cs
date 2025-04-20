using System.Text;

namespace AssetRipper.Export.UnityProjects.Project;

public static class UnityPatches
{
	private const string RelativePathToPatchesDirectory = "Assets/Editor/AssetRipperPatches/";

	/// <summary>
	/// For some asset types, the complete recovery must be assisted by scripts that run in the Unity Editor.
	/// This method copies a script file from a <see cref="string"/> to the exported project.
	/// </summary>
	/// <param name="text">The text of the patch</param>
	/// <param name="name">The name of the patch</param>
	/// <param name="exportDirectoryPath">The path of the exported project</param>
	public static void ApplyPatchFromText(string text, string name, string exportDirectoryPath, FileSystem fileSystem)
	{
		string patchFileName = $"{name}.cs";
		string patchDirectoryPath = fileSystem.Path.Join(exportDirectoryPath, RelativePathToPatchesDirectory);
		string patchFilePath = fileSystem.Path.Join(patchDirectoryPath, patchFileName);
		if (fileSystem.File.Exists(patchFilePath))
		{
			return;
		}

		fileSystem.Directory.Create(patchDirectoryPath);
		fileSystem.File.WriteAllBytes(patchFilePath, Encoding.UTF8.GetBytes(text));
	}
}
