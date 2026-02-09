using AssetRipper.SourceGenerated.NativeEnums.Fmod;

namespace AssetRipper.Processing.AudioMixers;

public static class AudioEffectDefinitions
{
	/// <summary>
	/// Similar to <see cref="SourceGenerated.NativeEnums.Global.UnityEffectType"/>
	/// </summary>
	private enum UnityEffectType
	{
		Attenuation = -2,
		Send = -3,
		Receive = -4,
		DuckVolume = -5
	}

	public static bool IsPluginEffect(int effectType)
	{
		return effectType >= 1000;
	}

	public static string? EffectTypeToName(int effectType)
	{
		if (IsPluginEffect(effectType))
		{
			throw new ArgumentException("This method should not be used on custom plugin effects");
		}

		if (effectType < 0)
		{
			return (UnityEffectType)effectType switch
			{
				UnityEffectType.Attenuation => "Attenuation",
				UnityEffectType.Send => "Send",
				UnityEffectType.Receive => "Receive",
				UnityEffectType.DuckVolume => "Duck Volume",
				_ => null,
			};
		}

		return (FmodDspType)effectType switch
		{
			FmodDspType.Lowpass => "Lowpass",
			FmodDspType.Highpass => "Highpass",
			FmodDspType.Echo => "Echo",
			FmodDspType.Flange => "Flange",
			FmodDspType.Distortion => "Distortion",
			FmodDspType.Normalize => "Normalize",
			FmodDspType.Parameq => "ParamEQ",
			FmodDspType.Pitchshift => "Pitch Shifter",
			FmodDspType.Chorus => "Chorus",
			FmodDspType.Compressor => "Compressor",
			FmodDspType.Sfxreverb => "SFX Reverb",
			FmodDspType.LowpassSimple => "Lowpass Simple",
			FmodDspType.HighpassSimple => "Highpass Simple",
			_ => null,
		};
	}
}
