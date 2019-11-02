using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters
{
	public static class RTTIBaseClassDescriptorConverter
	{
		public static void CombineFormats(FileGeneration generation, ref RTTIBaseClassDescriptor origin)
		{
			if (origin.Tree != null)
			{
				TypeTreeConverter.CombineFormats(generation, origin.Tree);
			}
		}
	}
}
