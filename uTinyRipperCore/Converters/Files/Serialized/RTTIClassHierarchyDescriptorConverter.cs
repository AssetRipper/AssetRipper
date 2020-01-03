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
	}
}
