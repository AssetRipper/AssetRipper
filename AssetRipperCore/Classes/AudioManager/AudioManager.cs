﻿using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AudioManager
{
	public sealed class AudioManager : GlobalGameManager
	{
		public AudioManager(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// RequestedDSPBufferSize has been added
			if (HasRequestedDSPBufferSize(version))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasRolloffScale(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 5.0.0b1 and less
		/// </summary>
		public static bool HasSpeedOfSound(UnityVersion version) => version.IsLessEqual(5, 0, 0, UnityVersionType.Beta, 1);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasDefaultSpeakerMode(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool HasSampleRate(UnityVersion version) => version.IsGreater(5, 0, 0, UnityVersionType.Beta, 1);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasDSPBufferSize(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool HasVirtualVoiceCount(UnityVersion version) => version.IsGreater(5, 0, 0, UnityVersionType.Beta, 1);
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool HasSpatializerPlugin(UnityVersion version) => version.IsGreaterEqual(5, 2);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasAmbisonicDecoderPlugin(UnityVersion version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasDisableAudio(UnityVersion version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 5.3.6 and greater
		/// </summary>
		public static bool HasVirtualizeEffects(UnityVersion version) => version.IsGreaterEqual(5, 3, 6);
		/// <summary>
		/// 2019.1.1 and greater
		/// </summary>
		public static bool HasRequestedDSPBufferSize(UnityVersion version)
		{
			if (version.IsGreaterEqual(2019))
			{
				return version.IsGreaterEqual(2019, 1, 1);
			}
			return version.IsGreaterEqual(2018, 3, 14);
		}

		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		private static bool IsAlign(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);

		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		private static bool IsRolloffScaleFirst(UnityVersion version) => version.IsGreaterEqual(3, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Volume = reader.ReadSingle();
			if (HasRolloffScale(reader.Version))
			{
				if (IsRolloffScaleFirst(reader.Version))
				{
					RolloffScale = reader.ReadSingle();
				}
			}
			if (HasSpeedOfSound(reader.Version))
			{
				SpeedOfSound = reader.ReadSingle();
			}
			DopplerFactor = reader.ReadSingle();
			if (HasDefaultSpeakerMode(reader.Version))
			{
				DefaultSpeakerMode = (AudioSpeakerMode)reader.ReadInt32();
			}
			if (HasSampleRate(reader.Version))
			{
				SampleRate = reader.ReadInt32();
			}
			if (HasRolloffScale(reader.Version))
			{
				if (!IsRolloffScaleFirst(reader.Version))
				{
					RolloffScale = reader.ReadSingle();
				}
			}
			if (HasDSPBufferSize(reader.Version))
			{
				DSPBufferSize = reader.ReadInt32();
			}
			if (HasVirtualVoiceCount(reader.Version))
			{
				VirtualVoiceCount = reader.ReadInt32();
				RealVoiceCount = reader.ReadInt32();
			}
			if (HasSpatializerPlugin(reader.Version))
			{
				SpatializerPlugin = reader.ReadString();
			}
			if (HasAmbisonicDecoderPlugin(reader.Version))
			{
				AmbisonicDecoderPlugin = reader.ReadString();
			}
			if (HasDisableAudio(reader.Version))
			{
				DisableAudio = reader.ReadBoolean();
			}
			if (HasVirtualizeEffects(reader.Version))
			{
				VirtualizeEffects = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasRequestedDSPBufferSize(reader.Version))
			{
				RequestedDSPBufferSize = reader.ReadInt32();
			}
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(VolumeName, Volume);
			node.Add(RolloffScaleName, RolloffScale);
			node.Add(DopplerFactorName, DopplerFactor);
			node.Add(DefaultSpeakerModeName, (int)DefaultSpeakerMode);
			node.Add(SampleRateName, SampleRate);
			node.Add(DSPBufferSizeName, DSPBufferSize);
			node.Add(VirtualVoiceCountName, GetVirtualVoiceCount(container.Version));
			node.Add(RealVoiceCountName, GetRealVoiceCount(container.Version));
			node.Add(SpatializerPluginName, GetSpatializerPlugin(container.Version));
			node.Add(AmbisonicDecoderPluginName, GetAmbisonicDecoderPlugin(container.Version));
			node.Add(DisableAudioName, DisableAudio);
			node.Add(VirtualizeEffectsName, GetVirtualizeEffects(container.Version));
			if (HasRequestedDSPBufferSize(container.ExportVersion))
			{
				node.Add(RequestedDSPBufferSizeName, GetRequestedDSPBufferSize(container.Version));
			}
			return node;
		}

		private int GetVirtualVoiceCount(UnityVersion version)
		{
			return HasVirtualVoiceCount(version) ? VirtualVoiceCount : 512;
		}
		private int GetRealVoiceCount(UnityVersion version)
		{
			return HasVirtualVoiceCount(version) ? RealVoiceCount : 32;
		}
		private string GetSpatializerPlugin(UnityVersion version)
		{
			return HasSpatializerPlugin(version) ? SpatializerPlugin : string.Empty;
		}
		private string GetAmbisonicDecoderPlugin(UnityVersion version)
		{
			return HasAmbisonicDecoderPlugin(version) ? AmbisonicDecoderPlugin : string.Empty;
		}
		private bool GetVirtualizeEffects(UnityVersion version)
		{
			return HasVirtualizeEffects(version) ? VirtualizeEffects : true;
		}
		private int GetRequestedDSPBufferSize(UnityVersion version)
		{
			return HasRequestedDSPBufferSize(version) ? RequestedDSPBufferSize : DSPBufferSize;
		}

		public float Volume { get; set; }
		public float RolloffScale { get; set; }
		/// <summary>
		/// DopplerVelocity previously
		/// </summary>
		public float SpeedOfSound { get; set; }
		public float DopplerFactor { get; set; }
		public AudioSpeakerMode DefaultSpeakerMode { get; set; }
		public int SampleRate { get; set; }
		/// <summary>
		/// iOSDSPBufferSize previously
		/// </summary>
		public int DSPBufferSize { get; set; }
		public int VirtualVoiceCount { get; set; }
		public int RealVoiceCount { get; set; }
		public string SpatializerPlugin { get; set; }
		public string AmbisonicDecoderPlugin { get; set; }
		public bool DisableAudio { get; set; }
		public bool VirtualizeEffects { get; set; }
		public int RequestedDSPBufferSize { get; set; }

		public const string VolumeName = "m_Volume";
		public const string RolloffScaleName = "Rolloff Scale";
		public const string DopplerFactorName = "Doppler Factor";
		public const string DefaultSpeakerModeName = "Default Speaker Mode";
		public const string SampleRateName = "m_SampleRate";
		public const string DSPBufferSizeName = "m_DSPBufferSize";
		public const string VirtualVoiceCountName = "m_VirtualVoiceCount";
		public const string RealVoiceCountName = "m_RealVoiceCount";
		public const string SpatializerPluginName = "m_SpatializerPlugin";
		public const string AmbisonicDecoderPluginName = "m_AmbisonicDecoderPlugin";
		public const string DisableAudioName = "m_DisableAudio";
		public const string VirtualizeEffectsName = "m_VirtualizeEffects";
		public const string RequestedDSPBufferSizeName = "m_RequestedDSPBufferSize";
	}
}
