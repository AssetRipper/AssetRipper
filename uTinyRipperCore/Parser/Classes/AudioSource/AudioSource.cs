using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Classes.AudioSources;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
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
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadReverbZoneMixCustomCurve(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadOutputAudioMixerGroup(reader.Version))
			{
				OutputAudioMixerGroup.Read(reader);
			}
			AudioClip.Read(reader);
			PlayOnAwake = reader.ReadBoolean();
			if (IsAlignAwake(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
			
			Volume = reader.ReadSingle();
			if (IsReadMinVolume(reader.Version))
			{
				MinVolume = reader.ReadSingle();
				MaxVolume = reader.ReadSingle();
			}
			if (IsReadPitch(reader.Version))
			{
				Pitch = reader.ReadSingle();
			}
			Loop = reader.ReadBoolean();
			if (IsReadRolloffFactor(reader.Version))
			{
				RolloffFactor = reader.ReadSingle();
			}
			if (IsReadMute(reader.Version))
			{
				Mute = reader.ReadBoolean();
			}
			if (IsReadSpatialize(reader.Version))
			{
				Spatialize = reader.ReadBoolean();
			}
			if (IsReadSpatializePostEffects(reader.Version))
			{
				SpatializePostEffects = reader.ReadBoolean();
			}
			if (IsAlignMute(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadPriority(reader.Version))
			{
				Priority = reader.ReadInt32();
				DopplerLevel = reader.ReadSingle();
				MinDistance = reader.ReadSingle();
				MaxDistance = reader.ReadSingle();
				Pan2D = reader.ReadSingle();
				RolloffMode = (AudioRolloffMode)reader.ReadInt32();
				BypassEffects = reader.ReadBoolean();
			}
			if (IsReadBypassListenerEffects(reader.Version))
			{
				BypassListenerEffects = reader.ReadBoolean();
				BypassReverbZones = reader.ReadBoolean();
			}
			if (IsAlignBypass(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if (IsReadRolloffCustomCurve(reader.Version))
			{
				RolloffCustomCurve.Read(reader);
				PanLevelCustomCurve.Read(reader);
				SpreadCustomCurve.Read(reader);
			}
			if (IsReadReverbZoneMixCustomCurve(reader.Version))
			{
				ReverbZoneMixCustomCurve.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			yield return OutputAudioMixerGroup.FetchDependency(file, isLog, ToLogString, "OutputAudioMixerGroup");
			yield return AudioClip.FetchDependency(file, isLog, ToLogString, "m_audioClip");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add("OutputAudioMixerGroup", OutputAudioMixerGroup.ExportYAML(container));
			node.Add("m_audioClip", AudioClip.ExportYAML(container));
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
			node.Add("rolloffCustomCurve", GetRolloffCustomCurve(container.Version).ExportYAML(container));
			node.Add("panLevelCustomCurve", GetPanLevelCustomCurve(container.Version).ExportYAML(container));
			node.Add("spreadCustomCurve", GetSpreadCustomCurve(container.Version).ExportYAML(container));
			node.Add("reverbZoneMixCustomCurve", GetReverbZoneMixCustomCurve(container.Version).ExportYAML(container));
			return node;
		}

		private AnimationCurveTpl<Float> GetRolloffCustomCurve(Version version)
		{
			return IsReadRolloffCustomCurve(version) ? RolloffCustomCurve : new AnimationCurveTpl<Float>(1.0f, 0.0f, 1.0f / 3.0f);
		}
		private AnimationCurveTpl<Float> GetPanLevelCustomCurve(Version version)
		{
			if(IsReadRolloffCustomCurve(version))
			{
				return PanLevelCustomCurve;
			}

			KeyframeTpl<Float> frame = new KeyframeTpl<Float>(0.0f, 0.0f, 1.0f / 3.0f);
			return new AnimationCurveTpl<Float>(frame);
		}
		private AnimationCurveTpl<Float> GetSpreadCustomCurve(Version version)
		{
			if (IsReadRolloffCustomCurve(version))
			{
				return SpreadCustomCurve;
			}

			KeyframeTpl<Float> frame = new KeyframeTpl<Float>(0.0f, 0.0f, 1.0f / 3.0f);
			return new AnimationCurveTpl<Float>(frame);
		}
		private AnimationCurveTpl<Float> GetReverbZoneMixCustomCurve(Version version)
		{
			if (IsReadReverbZoneMixCustomCurve(version))
			{
				return ReverbZoneMixCustomCurve;
			}

			KeyframeTpl<Float> frame = new KeyframeTpl<Float>(0.0f, 1.0f, 1.0f / 3.0f);
			return new AnimationCurveTpl<Float>(frame);
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
