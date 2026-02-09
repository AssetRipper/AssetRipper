using AssetRipper.SourceGenerated.Subclasses.BufferBinding;

namespace AssetRipper.SourceGenerated.Extensions;

public static class BufferBindingExtensions
{
	public static void SetValues(this IBufferBinding binding, string name, int index)
	{
		//binding.Name = name;//Name doesn't exist
		binding.NameIndex = -1;
		binding.Index = index;
		binding.ArraySize = 0;
	}
}
