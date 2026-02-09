using AssetRipper.SourceGenerated.Subclasses.MinMaxGradient;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MinMaxGradientExtensions
{
	public enum MinMaxGradientState : ushort
	{
		Color = 0,
		Gradient = 1,
		RandomBetweenTwoColors = 2,
		RandomBetweenTwoGradients = 3,
		RandomColor = 4,
	}

	public static MinMaxGradientState GetMinMaxState(this IMinMaxGradient gradient)
	{
		return gradient.Has_MinMaxState_Int16()
			? unchecked((MinMaxGradientState)gradient.MinMaxState_Int16)
			: (MinMaxGradientState)gradient.MinMaxState_UInt16;
	}

	public static void SetMinMaxState(this IMinMaxGradient gradient, MinMaxGradientState state)
	{
		gradient.MinMaxState_Int16 = unchecked((short)state);
		gradient.MinMaxState_UInt16 = (ushort)state;
	}

	public static void SetToDefault(this IMinMaxGradient gradient)
	{
		gradient.SetMinMaxState(MinMaxGradientState.Color);

		if (gradient.Has_MinColor_ColorRGBA32())
		{
			gradient.MinColor_ColorRGBA32.SetAsWhite();
		}
		else
		{
			gradient.MinColor_ColorRGBAf.SetAsWhite();
		}

		if (gradient.Has_MaxColor_ColorRGBA32())
		{
			gradient.MaxColor_ColorRGBA32.SetAsWhite();
		}
		else
		{
			gradient.MaxColor_ColorRGBAf.SetAsWhite();
		}

		//gradient.MaxGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
		//gradient.MinGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
	}
}
