using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.Converters;

public static class SerializedFileMetadataConverter
{
	public static void CombineFormats(FormatVersion generation, SerializedFileMetadata origin)
	{
		if (!SerializedFileMetadata.HasEnableTypeTree(generation))
		{
			origin.EnableTypeTree = true;
		}
		if (generation >= FormatVersion.RefactorTypeData)
		{
			for (int i = 0; i < origin.Object.Length; i++)
			{
				origin.Object[i].Initialize(origin.Types);
			}
		}
	}
}
