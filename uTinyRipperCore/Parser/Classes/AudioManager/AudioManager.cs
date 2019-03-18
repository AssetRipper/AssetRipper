using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AudioManagers;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	public sealed class AudioManager : GlobalGameManager
	{
		public AudioManager(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadRolloffScale(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 5.0.0b1 and less
		/// </summary>
		public static bool IsReadSpeedOfSound(Version version)
		{
			return version.IsLessEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadDefaultSpeakerMode(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadSampleRate(Version version)
		{
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadDSPBufferSize(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadVirtualVoiceCount(Version version)
		{
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 5.2.0 and greater
		/// </summary>
		public static bool IsReadSpatializerPlugin(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadAmbisonicDecoderPlugin(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		/// <summary>
		/// 4.1.2 and greater
		/// </summary>
		public static bool IsReadDisableAudio(Version version)
		{
			return version.IsGreaterEqual(4, 1, 2);
		}
		/// <summary>
		/// 5.3.6 and greater
		/// </summary>
		public static bool IsReadVirtualizeEffects(Version version)
		{
			return version.IsGreaterEqual(5, 3, 6);
		}
		
		/// <summary>
		/// 3.5.0 and greater
		/// </summary>
		private static bool IsReadRolloffScaleFirst(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}
		/// <summary>
		/// 5.0.0b2 and greater
		/// </summary>
		private static bool IsAlign(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Volume = reader.ReadSingle();
			if(IsReadRolloffScale(reader.Version))
			{
				if(IsReadRolloffScaleFirst(reader.Version))
				{
					RolloffScale = reader.ReadSingle();
				}
			}
			if(IsReadSpeedOfSound(reader.Version))
			{
				SpeedOfSound = reader.ReadSingle();
			}
			DopplerFactor = reader.ReadSingle();
			if(IsReadDefaultSpeakerMode(reader.Version))
			{
				DefaultSpeakerMode = (AudioSpeakerMode)reader.ReadInt32();
			}
			if (IsReadSampleRate(reader.Version))
			{
				SampleRate = reader.ReadInt32();
			}
			if (IsReadRolloffScale(reader.Version))
			{
				if (!IsReadRolloffScaleFirst(reader.Version))
				{
					RolloffScale = reader.ReadSingle();
				}
			}
			if (IsReadDSPBufferSize(reader.Version))
			{
				DSPBufferSize = reader.ReadInt32();
			}
			if (IsReadVirtualVoiceCount(reader.Version))
			{
				VirtualVoiceCount = reader.ReadInt32();
				RealVoiceCount = reader.ReadInt32();
			}
			if (IsReadSpatializerPlugin(reader.Version))
			{
				SpatializerPlugin = reader.ReadString();
			}
			if (IsReadAmbisonicDecoderPlugin(reader.Version))
			{
				AmbisonicDecoderPlugin = reader.ReadString();
			}
			if (IsReadDisableAudio(reader.Version))
			{
				DisableAudio = reader.ReadBoolean();
			}
			if (IsReadVirtualizeEffects(reader.Version))
			{
				VirtualizeEffects = reader.ReadBoolean();
			}
			if (IsAlign(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			//node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Volume", Volume);
			node.Add("Rolloff Scale", RolloffScale);
			node.Add("Doppler Factor", DopplerFactor);
			node.Add("Default Speaker Mode", (int)DefaultSpeakerMode);
			node.Add("m_SampleRate", SampleRate);
			node.Add("m_DSPBufferSize", DSPBufferSize);
			node.Add("m_VirtualVoiceCount", GetVirtualVoiceCount(container.Version));
			node.Add("m_RealVoiceCount", GetRealVoiceCount(container.Version));
			node.Add("m_SpatializerPlugin", GetSpatializerPlugin(container.Version));
			node.Add("m_AmbisonicDecoderPlugin", GetAmbisonicDecoderPlugin(container.Version));
			node.Add("m_DisableAudio", DisableAudio);
			node.Add("m_VirtualizeEffects", GetVirtualizeEffects(container.Version));
			return node;
		}

		private int GetVirtualVoiceCount(Version version)
		{
			return IsReadVirtualVoiceCount(version) ? VirtualVoiceCount : 512;
		}
		private int GetRealVoiceCount(Version version)
		{
			return IsReadVirtualVoiceCount(version) ? RealVoiceCount : 32;
		}
		private string GetSpatializerPlugin(Version version)
		{
			return IsReadSpatializerPlugin(version) ? SpatializerPlugin : string.Empty;
		}
		private string GetAmbisonicDecoderPlugin(Version version)
		{
			return IsReadAmbisonicDecoderPlugin(version) ? AmbisonicDecoderPlugin : string.Empty;
		}
		private bool GetVirtualizeEffects(Version version)
		{
			return IsReadVirtualizeEffects(version) ? VirtualizeEffects : true;
		}

		public float Volume { get; private set; }
		public float RolloffScale { get; private set; }
		/// <summary>
		/// DopplerVelocity previously
		/// </summary>
		public float SpeedOfSound { get; private set; }
		public float DopplerFactor { get; private set; }
		public AudioSpeakerMode DefaultSpeakerMode { get; private set; }
		public int SampleRate { get; private set; }
		/// <summary>
		/// iOSDSPBufferSize previously
		/// </summary>
		public int DSPBufferSize { get; private set; }
		public int VirtualVoiceCount { get; private set; }
		public int RealVoiceCount { get; private set; }
		public string SpatializerPlugin { get; private set; }
		public string AmbisonicDecoderPlugin { get; private set; }
		public bool DisableAudio { get; private set; }
		public bool VirtualizeEffects { get; private set; }
	}
}
