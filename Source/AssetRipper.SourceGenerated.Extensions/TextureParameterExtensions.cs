using AssetRipper.SourceGenerated.Subclasses.TextureParameter;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TextureParameterExtensions
{
	public static void SetValues(this ITextureParameter parameter, string name, int index, sbyte dimension, int sampler)
	{
		//parameter.Name = name;//Name doesn't exist
		parameter.NameIndex = -1;
		parameter.Index = index;
		parameter.Dim = dimension;
		parameter.SamplerIndex = sampler;
		parameter.MultiSampled = false;
	}

	public static void SetValues(this ITextureParameter parameter, string name, int index, sbyte dimension, int sampler, bool multiSampled)
	{
		//parameter.Name = name;//Name doesn't exist
		parameter.NameIndex = -1;
		parameter.Index = index;
		parameter.Dim = dimension;
		parameter.SamplerIndex = sampler;
		parameter.MultiSampled = multiSampled;
	}
}
