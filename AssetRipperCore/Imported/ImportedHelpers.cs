using System.Collections.Generic;

namespace AssetRipper.Core.Imported
{
	public static class ImportedHelpers
	{
		public static ImportedMesh FindMesh(string path, List<ImportedMesh> importedMeshList)
		{
			foreach (var mesh in importedMeshList)
			{
				if (mesh.Path == path)
				{
					return mesh;
				}
			}

			return null;
		}

		public static ImportedMaterial FindMaterial(string name, List<ImportedMaterial> importedMats)
		{
			foreach (var mat in importedMats)
			{
				if (mat.Name == name)
				{
					return mat;
				}
			}

			return null;
		}

		public static ImportedTexture FindTexture(string name, List<ImportedTexture> importedTextureList)
		{
			if (string.IsNullOrEmpty(name))
			{
				return null;
			}

			foreach (var tex in importedTextureList)
			{
				if (tex.Name == name)
				{
					return tex;
				}
			}

			return null;
		}
	}
}
