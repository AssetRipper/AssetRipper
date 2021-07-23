using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.Parser.Files.SerializedFiles.Parser;

namespace AssetRipper.Converters.Files
{
	public static class SerializedFileMetadataConverter
	{
		public static void CombineFormats(FormatVersion generation, SerializedFileMetadata origin)
		{
			if (!SerializedFileMetadata.HasEnableTypeTree(generation))
			{
				origin.EnableTypeTree = true;
			}
			for (int i = 0; i < origin.Types.Length; i++)
			{
				SerializedTypeConverter.CombineFormats(generation, ref origin.Types[i]);
			}
			if (generation >= FormatVersion.RefactorTypeData)
			{
				for (int i = 0; i < origin.Object.Length; i++)
				{
					ref ObjectInfo entry = ref origin.Object[i];
					ref SerializedType type = ref origin.Types[entry.TypeID];
					entry.ClassID = type.TypeID;
					entry.ScriptTypeIndex = type.ScriptTypeIndex;
					entry.Stripped = type.IsStrippedType;
				}
			}
		}
	}
}
