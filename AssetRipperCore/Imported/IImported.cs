using System.Collections.Generic;

namespace AssetRipper.Imported
{
	public interface IImported
	{
		ImportedFrame RootFrame { get; }
		List<ImportedMesh> MeshList { get; }
		List<ImportedMaterial> MaterialList { get; }
		List<ImportedTexture> TextureList { get; }
		List<ImportedKeyframedAnimation> AnimationList { get; }
		List<ImportedMorph> MorphList { get; }
	}
}
