using AssetRipper.IO.Files.SerializedFiles;
using AssetRipper.IO.Files.SerializedFiles.Parser;

namespace AssetRipper.IO.Files.Converters
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
				origin.Types[i].OldType.MaybeSetNamesFromBuffer();
			}
			if (generation >= FormatVersion.RefactorTypeData)
			{
				for (int i = 0; i < origin.Object.Length; i++)
				{
					ObjectInfo entry = origin.Object[i];
					SerializedType type = origin.Types[entry.TypeID];
					entry.ClassID = (short)type.TypeID;
					entry.ScriptTypeIndex = type.ScriptTypeIndex;
					entry.Stripped = type.IsStrippedType;
				}
			}
		}
	}
}
