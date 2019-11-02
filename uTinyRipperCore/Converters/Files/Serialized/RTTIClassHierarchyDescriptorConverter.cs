using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public static class RTTIClassHierarchyDescriptorConverter
	{
		public static void CombineFormats(FileGeneration generation, ref RTTIClassHierarchyDescriptor origin)
		{
			if (!RTTIClassHierarchyDescriptor.HasSerializeTypeTrees(generation))
			{
				origin.SerializeTypeTrees = true;
			}
			for (int i = 0; i < origin.Types.Length; i++)
			{
				RTTIBaseClassDescriptorConverter.CombineFormats(generation, ref origin.Types[i]);
			}
		}

#warning HACK: TEMP:
		public static void FixResourceVersion(string name, ref RTTIClassHierarchyDescriptor origin)
		{
			if (origin.Version == new Version(5, 6, 4, VersionType.Patch, 1))
			{
				if (FilenameUtils.IsDefaultResource(name))
				{
					origin.Version = new Version(5, 6, 5, VersionType.Final);
				}
			}
		}
	}
}
