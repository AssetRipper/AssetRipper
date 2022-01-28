using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.KeyframeTpl;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AudioSource
{
	public sealed class AudioSource : AudioBehaviour
	{
		public AudioSource(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
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

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasOutputAudioMixerGroup(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasMinVolume(UnityVersion version) => version.IsLess(3);
		/// <summary>
		/// 1.6.0 and greater
		/// </summary>
		public static bool HasPitch(UnityVersion version) => version.IsGreaterEqual(1, 6);
		/// <summary>
		/// Less than 3.0.0
		/// </summary>
		public static bool HasRolloffFactor(UnityVersion version) => version.IsLess(3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasMute(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasSpatialize(UnityVersion version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasSpatializePostEffects(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasPriority(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasBypassListenerEffects(UnityVersion version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasRolloffCustomCurve(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasReverbZoneMixCustomCurve(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);

		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlignAwake(UnityVersion version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlignMute(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		private static bool IsAlignBypass(UnityVersion version) => version.IsGreaterEqual(3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasOutputAudioMixerGroup(reader.Version))
			{
				OutputAudioMixerGroup.Read(reader);
			}
			AudioClip.Read(reader);
			PlayOnAwake = reader.ReadBoolean();
			if (IsAlignAwake(reader.Version))
			{
				reader.AlignStream();
			}

			Volume = reader.ReadSingle();
			if (HasMinVolume(reader.Version))
			{
				MinVolume = reader.ReadSingle();
				MaxVolume = reader.ReadSingle();
			}
			if (HasPitch(reader.Version))
			{
				Pitch = reader.ReadSingle();
			}
			Loop = reader.ReadBoolean();
			if (HasRolloffFactor(reader.Version))
			{
				RolloffFactor = reader.ReadSingle();
			}
			if (HasMute(reader.Version))
			{
				Mute = reader.ReadBoolean();
			}
			if (HasSpatialize(reader.Version))
			{
				Spatialize = reader.ReadBoolean();
			}
			if (HasSpatializePostEffects(reader.Version))
			{
				SpatializePostEffects = reader.ReadBoolean();
			}
			if (IsAlignMute(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasPriority(reader.Version))
			{
				Priority = reader.ReadInt32();
				DopplerLevel = reader.ReadSingle();
				MinDistance = reader.ReadSingle();
				MaxDistance = reader.ReadSingle();
				Pan2D = reader.ReadSingle();
				RolloffMode = (AudioRolloffMode)reader.ReadInt32();
				BypassEffects = reader.ReadBoolean();
			}
			if (HasBypassListenerEffects(reader.Version))
			{
				BypassListenerEffects = reader.ReadBoolean();
				BypassReverbZones = reader.ReadBoolean();
			}
			if (IsAlignBypass(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasRolloffCustomCurve(reader.Version))
			{
				RolloffCustomCurve.Read(reader);
				PanLevelCustomCurve.Read(reader);
				SpreadCustomCurve.Read(reader);
			}
			if (HasReverbZoneMixCustomCurve(reader.Version))
			{
				ReverbZoneMixCustomCurve.Read(reader);
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(OutputAudioMixerGroup, OutputAudioMixerGroupName);
			yield return context.FetchDependency(AudioClip, AudioClipName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(OutputAudioMixerGroupName, OutputAudioMixerGroup.ExportYAML(container));
			node.Add(AudioClipName, AudioClip.ExportYAML(container));
			node.Add(PlayOnAwakeName, PlayOnAwake);
			node.Add(VolumeName, Volume);
			node.Add(PitchName, Pitch);
			node.Add(LoopName, Loop);
			node.Add(MuteName, Mute);
			node.Add(SpatializeName, Spatialize);
			node.Add(SpatializePostEffectsName, SpatializePostEffects);
			node.Add(PriorityName, Priority);
			node.Add(DopplerLevelName, DopplerLevel);
			node.Add(MinDistanceName, MinDistance);
			node.Add(MaxDistanceName, MaxDistance);
			node.Add(Pan2DName, Pan2D);
			node.Add(RolloffModeName, (int)RolloffMode);
			node.Add(BypassEffectsName, BypassEffects);
			node.Add(BypassListenerEffectsName, BypassListenerEffects);
			node.Add(BypassReverbZonesName, BypassReverbZones);
			node.Add(RolloffCustomCurveName, GetRolloffCustomCurve(container.Version).ExportYAML(container));
			node.Add(PanLevelCustomCurveName, GetPanLevelCustomCurve(container.Version).ExportYAML(container));
			node.Add(SpreadCustomCurveName, GetSpreadCustomCurve(container.Version).ExportYAML(container));
			node.Add(ReverbZoneMixCustomCurveName, GetReverbZoneMixCustomCurve(container.Version).ExportYAML(container));
			return node;
		}

		private AnimationCurveTpl<Float> GetRolloffCustomCurve(UnityVersion version)
		{
			return HasRolloffCustomCurve(version) ? RolloffCustomCurve : new AnimationCurveTpl<Float>(1.0f, 0.0f, 1.0f / 3.0f);
		}
		private AnimationCurveTpl<Float> GetPanLevelCustomCurve(UnityVersion version)
		{
			if (HasRolloffCustomCurve(version))
			{
				return PanLevelCustomCurve;
			}

			KeyframeTpl<Float> frame = new KeyframeTpl<Float>(0.0f, 0.0f, 1.0f / 3.0f);
			return new AnimationCurveTpl<Float>(frame);
		}
		private AnimationCurveTpl<Float> GetSpreadCustomCurve(UnityVersion version)
		{
			if (HasRolloffCustomCurve(version))
			{
				return SpreadCustomCurve;
			}

			KeyframeTpl<Float> frame = new KeyframeTpl<Float>(0.0f, 0.0f, 1.0f / 3.0f);
			return new AnimationCurveTpl<Float>(frame);
		}
		private AnimationCurveTpl<Float> GetReverbZoneMixCustomCurve(UnityVersion version)
		{
			if (HasReverbZoneMixCustomCurve(version))
			{
				return ReverbZoneMixCustomCurve;
			}

			KeyframeTpl<Float> frame = new KeyframeTpl<Float>(0.0f, 1.0f, 1.0f / 3.0f);
			return new AnimationCurveTpl<Float>(frame);
		}

		public bool PlayOnAwake { get; set; }
		public float Volume { get; set; }
		public float MinVolume { get; set; }
		public float MaxVolume { get; set; }
		public float Pitch { get; set; }
		public bool Loop { get; set; }
		public float RolloffFactor { get; set; }
		public bool Mute { get; set; }
		public bool Spatialize { get; set; }
		public bool SpatializePostEffects { get; set; }
		public int Priority { get; set; }
		public float DopplerLevel { get; set; }
		public float MinDistance { get; set; }
		public float MaxDistance { get; set; }
		public float Pan2D { get; set; }
		public AudioRolloffMode RolloffMode { get; set; }
		public bool BypassEffects { get; set; }
		public bool BypassListenerEffects { get; set; }
		public bool BypassReverbZones { get; set; }

		public const string OutputAudioMixerGroupName = "OutputAudioMixerGroup";
		public const string AudioClipName = "m_audioClip";
		public const string PlayOnAwakeName = "m_PlayOnAwake";
		public const string VolumeName = "m_Volume";
		public const string PitchName = "m_Pitch";
		public const string LoopName = "Loop";
		public const string MuteName = "Mute";
		public const string SpatializeName = "Spatialize";
		public const string SpatializePostEffectsName = "SpatializePostEffects";
		public const string PriorityName = "Priority";
		public const string DopplerLevelName = "DopplerLevel";
		public const string MinDistanceName = "MinDistance";
		public const string MaxDistanceName = "MaxDistance";
		public const string Pan2DName = "Pan2D";
		public const string RolloffModeName = "rolloffMode";
		public const string BypassEffectsName = "BypassEffects";
		public const string BypassListenerEffectsName = "BypassListenerEffects";
		public const string BypassReverbZonesName = "BypassReverbZones";
		public const string RolloffCustomCurveName = "rolloffCustomCurve";
		public const string PanLevelCustomCurveName = "panLevelCustomCurve";
		public const string SpreadCustomCurveName = "spreadCustomCurve";
		public const string ReverbZoneMixCustomCurveName = "reverbZoneMixCustomCurve";

		public PPtr<AudioMixerGroup> OutputAudioMixerGroup = new();
		public PPtr<AudioClip.AudioClip> AudioClip = new();
		public AnimationCurveTpl<Float> RolloffCustomCurve = new();
		public AnimationCurveTpl<Float> PanLevelCustomCurve = new();
		public AnimationCurveTpl<Float> SpreadCustomCurve = new();
		public AnimationCurveTpl<Float> ReverbZoneMixCustomCurve = new();
	}
}
