using System.IO;
using System.Reflection;
using System.Text;

namespace AssetRipper.Core.Utils
{
	public static class UnityPatchUtils
	{
		private const string PatchesDirPath = "Assets/Editor/AssetRipperPatches/";
		
		/// <summary>
		/// For some asset types, the complete recovery must be assisted by scripts that run in the Unity Editor.
		/// This method copies a script file from an embedded resource to the exported project.
		/// </summary>
		/// <param name="assembly">The assembly that the embedded resource belongs to</param>
		/// <param name="resourceName">The name of the embedded resource</param>
		/// <param name="exportDirPath">The path of the exported project</param>
		public static void ApplyPatchFromManifestResource(Assembly assembly, string resourceName, string exportDirPath)
		{
			// get filename from resourceName and append the '.cs' suffix
			string patchFileName = $"{resourceName.Split('.')[^2]}.cs";
			string patchFilePath = Path.Combine(exportDirPath, Path.Combine(PatchesDirPath, patchFileName));
			if (File.Exists(patchFilePath)) return;

			Stream stream = assembly.GetManifestResourceStream(resourceName) ?? throw new ArgumentException($"Load resource failed: {resourceName}");
			using StreamReader reader = new(stream);
			string patchCode = reader.ReadToEnd();
			Directory.CreateDirectory(Path.GetDirectoryName(patchFilePath)!);
			File.WriteAllBytes(patchFilePath, Encoding.UTF8.GetBytes(patchCode));
		}
	}
}
