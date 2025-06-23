using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Subclasses.LineParameters;

namespace AssetRipper.SourceGenerated.Extensions;

public static class LineParametersExtensions
{
	public static void Initialize(this ILineParameters lineParameters, UnityVersion version)
	{
		lineParameters.WidthMultiplier = 1.0f;
		if (lineParameters.Has_WidthCurve())
		{
			lineParameters.WidthCurve.SetDefaultRotationOrderAndCurveLoopType();
			lineParameters.WidthCurve.Curve.AddNew().SetValues(version, 0.0f, 1.0f, KeyframeExtensions.DefaultFloatWeight);
		}
		else
		{
			lineParameters.StartWidth = 1f;
			lineParameters.EndWidth = 1f;
			//Just guessing. I did not verify these values anywhere.
		}
		if (lineParameters.Has_ColorGradient())
		{
			lineParameters.ColorGradient.Ctime0 = 0;
			lineParameters.ColorGradient.Atime0 = 0;
			lineParameters.ColorGradient.Ctime1 = ushort.MaxValue;
			lineParameters.ColorGradient.Atime1 = ushort.MaxValue;
			lineParameters.ColorGradient.NumColorKeys = 2;
			lineParameters.ColorGradient.NumAlphaKeys = 2;
			if (lineParameters.ColorGradient.Has_Key0_ColorRGBA32())
			{
				lineParameters.ColorGradient.Key0_ColorRGBA32.SetAsWhite();
				lineParameters.ColorGradient.Key1_ColorRGBA32.SetAsWhite();
			}
			else
			{
				lineParameters.ColorGradient.Key0_ColorRGBAf.SetAsWhite();
				lineParameters.ColorGradient.Key1_ColorRGBAf.SetAsWhite();
			}
		}
		else if (lineParameters.Has_EndColor() && lineParameters.Has_StartColor())
		{
			lineParameters.StartColor.SetAsWhite();
			lineParameters.EndColor.SetAsWhite();
		}
		lineParameters.NumCornerVertices = 0;
		lineParameters.NumCapVertices = 0;
		lineParameters.Alignment = (int)LineAlignment.View;
		lineParameters.TextureMode = (int)LineTextureMode.Stretch;
		lineParameters.ShadowBias = 0.5f;
		lineParameters.GenerateLightingData = false;
	}

	public static LineAlignment GetAlignment(this ILineParameters parameters)
	{
		return (LineAlignment)parameters.Alignment;
	}

	public static LineTextureMode GetTextureMode(this ILineParameters parameters)
	{
		return (LineTextureMode)parameters.TextureMode;
	}
}
