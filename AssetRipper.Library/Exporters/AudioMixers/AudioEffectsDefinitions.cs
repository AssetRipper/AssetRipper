namespace AssetRipper.Library.Exporters.AudioMixers
{
	public static class AudioEffectDefinitions
	{
		private enum UnityEffectType
		{
			Attenuation = -2,
			Send = -3,
			Receive = -4,
			DuckVolume = -5
		}

		/// <summary>
		/// <see href="https://documentation.help/FMOD-Ex/FMOD_DSP_TYPE.html" />
		/// </summary>
		private enum FMOD_DSP_TYPE
		{
			FMOD_DSP_TYPE_UNKNOWN,
			FMOD_DSP_TYPE_MIXER,
			FMOD_DSP_TYPE_OSCILLATOR,
			FMOD_DSP_TYPE_LOWPASS,
			FMOD_DSP_TYPE_ITLOWPASS,
			FMOD_DSP_TYPE_HIGHPASS,
			FMOD_DSP_TYPE_ECHO,
			FMOD_DSP_TYPE_FLANGE,
			FMOD_DSP_TYPE_DISTORTION,
			FMOD_DSP_TYPE_NORMALIZE,
			FMOD_DSP_TYPE_PARAMEQ,
			FMOD_DSP_TYPE_PITCHSHIFT,
			FMOD_DSP_TYPE_CHORUS,
			FMOD_DSP_TYPE_VSTPLUGIN,
			FMOD_DSP_TYPE_WINAMPPLUGIN,
			FMOD_DSP_TYPE_ITECHO,
			FMOD_DSP_TYPE_COMPRESSOR,
			FMOD_DSP_TYPE_SFXREVERB,
			FMOD_DSP_TYPE_LOWPASS_SIMPLE,
			FMOD_DSP_TYPE_DELAY,
			FMOD_DSP_TYPE_TREMOLO,
			FMOD_DSP_TYPE_LADSPAPLUGIN,
			FMOD_DSP_TYPE_HIGHPASS_SIMPLE
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

			return (FMOD_DSP_TYPE)effectType switch
			{
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_LOWPASS => "Lowpass",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_HIGHPASS => "Highpass",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_ECHO => "Echo",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_FLANGE => "Flange",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_DISTORTION => "Distortion",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_NORMALIZE => "Normalize",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_PARAMEQ => "ParamEQ",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_PITCHSHIFT => "Pitch Shifter",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_CHORUS => "Chorus",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_COMPRESSOR => "Compressor",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_SFXREVERB => "SFX Reverb",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_LOWPASS_SIMPLE => "Lowpass Simple",
				FMOD_DSP_TYPE.FMOD_DSP_TYPE_HIGHPASS_SIMPLE => "Highpass Simple",
				_ => null,
			};
		}
	}
}
