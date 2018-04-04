using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimationClips;
using UtinyRipper.Classes.AudioSources;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class AudioSource : AudioBehaviour
	{
		public AudioSource(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadOutputAudioMixerGroup(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadMinVolume(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 1.6.0 and greater
		/// </summary>
		public static bool IsReadPitch(Version version)
		{
			return version.IsGreaterEqual(1, 6);
		}
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool IsReadRolloffFactor(Version version)
		{
			return version.IsLess(3);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadMute(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool IsReadSpatialize(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadSpatializePostEffects(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadPriority(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		public static bool IsReadBypassListenerEffects(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadRolloffCustomCurve(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 5.0.0b1 and greater
		/// </summary>
		public static bool IsReadReverbZoneMixCustomCurve(Version version)
		{
#warning: unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlignAwake(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlignMute(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlignBypass(Version version)
		{
			return version.IsGreaterEqual(3);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 4;
			}

			if (version.IsGreaterEqual(5))
			{
				return 4;
			}
			if (version.IsGreaterEqual(3, 5))
			{
				return 3;
			}
			if (version.IsGreaterEqual(3))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadOutputAudioMixerGroup(stream.Version))
			{
				OutputAudioMixerGroup.Read(stream);
			}
			AudioClip.Read(stream);
			PlayOnAwake = stream.ReadBoolean();
			if (IsAlignAwake(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}
			
			Volume = stream.ReadSingle();
			if (IsReadMinVolume(stream.Version))
			{
				MinVolume = stream.ReadSingle();
				MaxVolume = stream.ReadSingle();
			}
			if (IsReadPitch(stream.Version))
			{
				Pitch = stream.ReadSingle();
			}
			Loop = stream.ReadBoolean();
			if (IsReadRolloffFactor(stream.Version))
			{
				RolloffFactor = stream.ReadSingle();
			}
			if (IsReadMute(stream.Version))
			{
				Mute = stream.ReadBoolean();
			}
			if (IsReadSpatialize(stream.Version))
			{
				Spatialize = stream.ReadBoolean();
			}
			if (IsReadSpatializePostEffects(stream.Version))
			{
				SpatializePostEffects = stream.ReadBoolean();
			}
			if (IsAlignMute(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadPriority(stream.Version))
			{
				Priority = stream.ReadInt32();
				DopplerLevel = stream.ReadSingle();
				MinDistance = stream.ReadSingle();
				MaxDistance = stream.ReadSingle();
				Pan2D = stream.ReadSingle();
				RolloffMode = (AudioRolloffMode)stream.ReadInt32();
				BypassEffects = stream.ReadBoolean();
			}
			if (IsReadBypassListenerEffects(stream.Version))
			{
				BypassListenerEffects = stream.ReadBoolean();
				BypassReverbZones = stream.ReadBoolean();
			}
			if (IsAlignBypass(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if (IsReadRolloffCustomCurve(stream.Version))
			{
				RolloffCustomCurve.Read(stream);
				PanLevelCustomCurve.Read(stream);
				SpreadCustomCurve.Read(stream);
			}
			if (IsReadReverbZoneMixCustomCurve(stream.Version))
			{
				ReverbZoneMixCustomCurve.Read(stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return OutputAudioMixerGroup.FetchDependency(file, isLog, ToLogString, "OutputAudioMixerGroup");
			yield return AudioClip.FetchDependency(file, isLog, ToLogString, "m_audioClip");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: serialized version acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("OutputAudioMixerGroup", OutputAudioMixerGroup.ExportYAML(exporter));
			node.Add("m_audioClip", AudioClip.ExportYAML(exporter));
			node.Add("m_PlayOnAwake", PlayOnAwake);
			node.Add("m_Volume", Volume);
			node.Add("m_Pitch", Pitch);
			node.Add("Loop", Loop);
			node.Add("Mute", Mute);
			node.Add("Spatialize", Spatialize);
			node.Add("SpatializePostEffects", SpatializePostEffects);
			node.Add("Priority", Priority);
			node.Add("DopplerLevel", DopplerLevel);
			node.Add("MinDistance", MinDistance);
			node.Add("MaxDistance", MaxDistance);
			node.Add("Pan2D", Pan2D);
			node.Add("rolloffMode", (int)RolloffMode);
			node.Add("BypassEffects", BypassEffects);
			node.Add("BypassListenerEffects", BypassListenerEffects);
			node.Add("BypassReverbZones", BypassReverbZones);
			node.Add("rolloffCustomCurve", RolloffCustomCurve.ExportYAML(exporter));
			node.Add("panLevelCustomCurve", PanLevelCustomCurve.ExportYAML(exporter));
			node.Add("spreadCustomCurve", SpreadCustomCurve.ExportYAML(exporter));
			node.Add("reverbZoneMixCustomCurve", ReverbZoneMixCustomCurve.ExportYAML(exporter));
			return node;
		}

		public bool PlayOnAwake { get; private set; }
		public float Volume { get; private set; }
		public float MinVolume { get; private set; }
		public float MaxVolume { get; private set; }
		public float Pitch { get; private set; }
		public bool Loop { get; private set; }
		public float RolloffFactor { get; private set; }
		public bool Mute { get; private set; }
		public bool Spatialize { get; private set; }
		public bool SpatializePostEffects { get; private set; }
		public int Priority { get; private set; }
		public float DopplerLevel { get; private set; }
		public float MinDistance { get; private set; }
		public float MaxDistance { get; private set; }
		public float Pan2D { get; private set; }
		public AudioRolloffMode RolloffMode { get; private set; }
		public bool BypassEffects { get; private set; }
		public bool BypassListenerEffects { get; private set; }
		public bool BypassReverbZones { get; private set; }

		public PPtr<AudioMixerGroup> OutputAudioMixerGroup;
		public PPtr<AudioClip> AudioClip;
		public AnimationCurveTpl<Float> RolloffCustomCurve;
		public AnimationCurveTpl<Float> PanLevelCustomCurve;
		public AnimationCurveTpl<Float> SpreadCustomCurve;
		public AnimationCurveTpl<Float> ReverbZoneMixCustomCurve;
	}
}
