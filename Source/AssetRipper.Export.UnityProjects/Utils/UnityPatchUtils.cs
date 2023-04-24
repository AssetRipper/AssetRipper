using System.Text;

namespace AssetRipper.Export.UnityProjects.Utils;

public static class UnityPatchUtils
{
	private const string RelativePathToPatchesDirectory = "Assets/Editor/AssetRipperPatches/";

	/// <summary>
	/// For some asset types, the complete recovery must be assisted by scripts that run in the Unity Editor.
	/// This method copies a script file from a <see cref="string"/> to the exported project.
	/// </summary>
	/// <param name="text">The text of the patch</param>
	/// <param name="name">The name of the patch</param>
	/// <param name="exportDirectoryPath">The path of the exported project</param>
	public static void ApplyPatchFromText(string text, string name, string exportDirectoryPath)
	{
		string patchFileName = $"{name}.cs";
		string patchDirectoryPath = Path.Combine(exportDirectoryPath, RelativePathToPatchesDirectory);
		string patchFilePath = Path.Combine(patchDirectoryPath, patchFileName);
		if (File.Exists(patchFilePath))
		{
			return;
		}

		Directory.CreateDirectory(patchDirectoryPath);
		File.WriteAllBytes(patchFilePath, Encoding.UTF8.GetBytes(text));
	}
}
