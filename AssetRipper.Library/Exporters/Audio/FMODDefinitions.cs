
using AssetRipper.SourceGenerated.Subclasses.Utf8String;
using System.Diagnostics;

namespace AssetRipper.Library.Exporters.Audio
{
	public enum UnityEffectType
	{
		kUnknown    = -1,
		kFader      = -2,
		kSend       = -3,
		kReceive    = -4,
		kDuckVolume = -5
	};
	
	public enum FModDspType
	{
		FMOD_DSP_TYPE_UNKNOWN, /* This unit was created via a non FMOD plugin so has an unknown purpose. */
		FMOD_DSP_TYPE_MIXER, /* This unit does nothing but take inputs and mix them together then feed the result to the soundcard unit. */
		FMOD_DSP_TYPE_OSCILLATOR, /* This unit generates sine/square/saw/triangle or noise tones. */
		FMOD_DSP_TYPE_LOWPASS, /* This unit filters sound using a high quality, resonant lowpass filter algorithm but consumes more CPU time. */
		FMOD_DSP_TYPE_ITLOWPASS, /* This unit filters sound using a resonant lowpass filter algorithm that is used in Impulse Tracker, but with limited cutoff range (0 to 8060hz). */
		FMOD_DSP_TYPE_HIGHPASS, /* This unit filters sound using a resonant highpass filter algorithm. */
		FMOD_DSP_TYPE_ECHO, /* This unit produces an echo on the sound and fades out at the desired rate. */
		FMOD_DSP_TYPE_FLANGE, /* This unit produces a flange effect on the sound. */
		FMOD_DSP_TYPE_DISTORTION, /* This unit distorts the sound. */
		FMOD_DSP_TYPE_NORMALIZE, /* This unit normalizes or amplifies the sound to a certain level. */
		FMOD_DSP_TYPE_PARAMEQ, /* This unit attenuates or amplifies a selected frequency range. */
		FMOD_DSP_TYPE_PITCHSHIFT, /* This unit bends the pitch of a sound without changing the speed of playback. */
		FMOD_DSP_TYPE_CHORUS, /* This unit produces a chorus effect on the sound. */
		FMOD_DSP_TYPE_VSTPLUGIN, /* This unit allows the use of Steinberg VST plugins */
		FMOD_DSP_TYPE_WINAMPPLUGIN, /* This unit allows the use of Nullsoft Winamp plugins */
		FMOD_DSP_TYPE_ITECHO, /* This unit produces an echo on the sound and fades out at the desired rate as is used in Impulse Tracker. */
		FMOD_DSP_TYPE_COMPRESSOR, /* This unit implements dynamic compression (linked multichannel, wideband) */
		FMOD_DSP_TYPE_SFXREVERB, /* This unit implements SFX reverb */
		FMOD_DSP_TYPE_LOWPASS_SIMPLE, /* This unit filters sound using a simple lowpass with no resonance, but has flexible cutoff and is fast. */
		FMOD_DSP_TYPE_DELAY, /* This unit produces different delays on individual channels of the sound. */
		FMOD_DSP_TYPE_TREMOLO, /* This unit produces a tremolo / chopper effect on the sound. */
		FMOD_DSP_TYPE_LADSPAPLUGIN, /* This unit allows the use of LADSPA standard plugins. */
		FMOD_DSP_TYPE_HIGHPASS_SIMPLE, /* This unit filters sound using a simple highpass with no resonance, but has flexible cutoff and is fast. */
		FMOD_DSP_TYPE_HARDWARE = 1000, /* Offset that platform specific FMOD_HARDWARE DSPs will start at. */
		FMOD_DSP_TYPE_FORCEINT = 65536 /* Makes sure this enum is signed 32bit. */
	}

	public static class FMODDefinitions
	{
		public static bool IsPluginEffect(int effectType)
		{
			return effectType >= 1000;
		}

		public static bool IsFMODEffect(int effectType)
		{
			return effectType >= 0 && effectType < 1000;
		}

		public static string? EffectTypeToName(int effectType)
		{
			Debug.Assert(!IsPluginEffect(effectType));

			if (!IsFMODEffect(effectType))
			{
				return (UnityEffectType)effectType switch
				{
					UnityEffectType.kFader => "Attenuation",
					UnityEffectType.kSend => "Send",
					UnityEffectType.kReceive => "Receive",
					UnityEffectType.kDuckVolume => "Duck Volume",
					_ => null,
				};
			}

			return (FModDspType)effectType switch
			{
				FModDspType.FMOD_DSP_TYPE_LOWPASS => "Lowpass",
				FModDspType.FMOD_DSP_TYPE_HIGHPASS => "Highpass",
				FModDspType.FMOD_DSP_TYPE_ECHO => "Echo",
				FModDspType.FMOD_DSP_TYPE_FLANGE => "Flange",
				FModDspType.FMOD_DSP_TYPE_DISTORTION => "Distortion",
				FModDspType.FMOD_DSP_TYPE_NORMALIZE => "Normalize",
				FModDspType.FMOD_DSP_TYPE_PARAMEQ => "ParamEQ",
				FModDspType.FMOD_DSP_TYPE_PITCHSHIFT => "Pitch Shifter",
				FModDspType.FMOD_DSP_TYPE_CHORUS => "Chorus",
				FModDspType.FMOD_DSP_TYPE_COMPRESSOR => "Compressor",
				FModDspType.FMOD_DSP_TYPE_SFXREVERB => "SFX Reverb",
				FModDspType.FMOD_DSP_TYPE_LOWPASS_SIMPLE => "Lowpass Simple",
				FModDspType.FMOD_DSP_TYPE_HIGHPASS_SIMPLE => "Highpass Simple",
				_ => null,
			};
		}
	}
}
