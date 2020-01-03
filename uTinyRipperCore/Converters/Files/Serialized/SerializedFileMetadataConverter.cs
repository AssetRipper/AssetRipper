using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public static class SerializedFileMetadataConverter
	{
		public static void CombineFormats(FileGeneration generation, SerializedFileMetadata origin)
		{
			RTTIClassHierarchyDescriptorConverter.CombineFormats(generation, ref origin.Hierarchy);
			if (AssetEntry.HasTypeIndex(generation))
			{
				for (int i = 0; i < origin.Entries.Length; i++)
				{
					ref AssetEntry entry = ref origin.Entries[i];
					ref RTTIBaseClassDescriptor type = ref origin.Hierarchy.Types[entry.TypeIndex];
					entry.TypeID = type.ClassID == ClassIDType.MonoBehaviour ? (-type.ScriptID - 1) : (int)type.ClassID;
					entry.ClassID = type.ClassID;
					entry.ScriptID = type.ScriptID;
				}
			}
		}
	}
}
