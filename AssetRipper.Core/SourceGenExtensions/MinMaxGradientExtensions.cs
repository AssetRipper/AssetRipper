using AssetRipper.Core.Classes.ParticleSystem.MinMaxGradient;
using AssetRipper.SourceGenerated.Subclasses.MinMaxGradient;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MinMaxGradientExtensions
	{
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
			else if (gradient.Has_MinColor_ColorRGBAf_3_5_0_f5())
			{
				gradient.MinColor_ColorRGBAf_3_5_0_f5.SetAsWhite();
			}

			if (gradient.Has_MaxColor_ColorRGBA32())
			{
				gradient.MaxColor_ColorRGBA32.SetAsWhite();
			}
			else if (gradient.Has_MaxColor_ColorRGBAf_3_5_0_f5())
			{
				gradient.MaxColor_ColorRGBAf_3_5_0_f5.SetAsWhite();
			}

			//gradient.MaxGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
			//gradient.MinGradient = new Gradient(ColorRGBAf.White, ColorRGBAf.White);
		}
	}
}
