using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AudioClips;
using UtinyRipper.Classes.Textures;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public sealed class AudioClip : NamedObject
	{
		public AudioClip(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadLoadType(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadIsTrackerFormat(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadAmbisonic(Version version)
		{
			return version.IsGreater(2017);
		}
		
		/// <summary>
		/// 5.0.0b1
		/// </summary>
		public static bool IsReadAudioClipFlags(Version version)
		{
#warning unknown
			return version.IsEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadCompressionFormat(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		/// <summary>
		/// 2.0.0 to 3.0.0 exclusive
		/// </summary>
		public static bool IsReadDecompressOnLoad(Version version)
		{
			return version.IsGreaterEqual(2) && version.IsLess(3);
		}
		/// <summary>
		/// 2.6.0 to 5.0.0b1
		/// </summary>
		public static bool IsReadType(Version version)
		{
#warning unknown top version
			return version.IsGreaterEqual(2, 6) && version.IsLessEqual(5, 0, 0, VersionType.Beta, 1);
		}
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool IsReadLength(Version version)
		{
			return version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsRead3D(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadUseHardware(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool IsReadStreamingInfo(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}
		/// <summary>
		/// 3.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadStream(Version version)
		{
			return version.IsGreaterEqual(3);
		}

		/// <summary>
		/// 5.0.0b1
		/// </summary>
		private static bool IsReadFSBResourceFirst(Version version)
		{
#warning unknown
			return version.IsEqual(5, 0, 0, VersionType.Beta, 1);
		}

		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		private static bool IsAlignTrackerFormat(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}

		/// <summary>
		/// 2.0.x
		/// </summary>
		private static bool IsReadDecompressOnLoadFirst(Version version)
		{
			return version.IsEqual(2, 0);
		}
		/// <summary>
		/// 2.1.0 to 2.6.0 exclusive
		/// </summary>
		private static bool IsReadDecompressOnLoadSecond(Version version)
		{
			return version.IsGreaterEqual(2, 1) && version.IsLess(2, 6);
		}
		/// <summary>
		/// 2.6.0
		/// </summary>
		private static bool IsReadDecompressOnLoadThird(Version version)
		{
			return version.IsEqual(2, 6);
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsStreamInt32(Version version)
		{
			return version.IsGreaterEqual(3, 2);
		}
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlignBools(Version version)
		{
			return version.IsGreaterEqual(2, 1);
		}
		/// <summary>
		/// 2.1.0 to 3.0.0 exclusive or 3.2.0 and greater
		/// </summary>
		private static bool IsAlignAudioData(Version version)
		{
			return version.IsGreaterEqual(2, 1) && version.IsLess(3) || version.IsGreaterEqual(3, 2);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				// topmost engine version doesn't conatain any serialized versions
				return 1;
			}
			
			if (version.IsGreaterEqual(5))
			{
				return 1;
			}
			if (version.IsGreaterEqual(3, 5))
			{
				return 4;
			}
			if (version.IsGreaterEqual(2, 6))
			{
				return 3;
			}
			// min version is 2nd
			return 2;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);
			
			if (IsReadLoadType(stream.Version))
			{
				LoadType = (AudioClipLoadType)stream.ReadInt32();
				Channels = stream.ReadInt32();
				Frequency = stream.ReadInt32();
				BitsPerSample = stream.ReadInt32();
				Length = stream.ReadSingle();
				
				if (IsReadIsTrackerFormat(stream.Version))
				{
					IsTrackerFormat = stream.ReadBoolean();
				}
				if (IsReadAmbisonic(stream.Version))
				{
					Ambisonic = stream.ReadBoolean();
				}
				if (IsAlignTrackerFormat(stream.Version))
				{
					stream.AlignStream(AlignType.Align4);
				}

				if (IsReadAudioClipFlags(stream.Version))
				{
					AudioClipFlags = stream.ReadInt32();
				}
				if (IsReadFSBResourceFirst(stream.Version))
				{
					FSBResource.Read(stream);
				}

				SubsoundIndex = stream.ReadInt32();
				PreloadAudioData = stream.ReadBoolean();
				LoadInBackground = stream.ReadBoolean();
				Legacy3D = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);

				if (!IsReadFSBResourceFirst(stream.Version))
				{
					FSBResource.Read(stream);
				}

				if (IsReadType(stream.Version))
				{
					Type = (FMODSoundType)stream.ReadInt32();
				}
				if (IsReadCompressionFormat(stream.Version))
				{
					CompressionFormat = (AudioCompressionFormat)stream.ReadInt32();
				}
				stream.AlignStream(AlignType.Align4);
			}
			else
			{
				if (IsReadDecompressOnLoadFirst(stream.Version))
				{
					DecompressOnLoad = stream.ReadBoolean();
				}

				Format = (FMODSoundFormat)stream.ReadInt32();
				if (IsReadType(stream.Version))
				{
					Type = (FMODSoundType)stream.ReadInt32();
				}
				if (IsReadLength(stream.Version))
				{
					Length = stream.ReadSingle();
					Frequency = stream.ReadInt32();
					Size = stream.ReadInt32();
				}
			
				if (IsReadDecompressOnLoadSecond(stream.Version))
				{
					DecompressOnLoad = stream.ReadBoolean();
				}
				if (IsRead3D(stream.Version))
				{
					Legacy3D = stream.ReadBoolean();
				}
				if (IsReadUseHardware(stream.Version))
				{
					UseHardware = stream.ReadBoolean();
				}
				if (IsAlignBools(stream.Version))
				{
					stream.AlignStream(AlignType.Align4);
				}

				if (IsStreamInt32(stream.Version))
				{
					Stream = stream.ReadInt32();
				}

				if (IsReadStreamingInfo(stream.Version))
				{
					if (Stream == 2)
					{
						string resImageName = $"{File.Name}.resS";
						StreamingInfo.Read(stream, resImageName);
					}
					else
					{
						m_audioData = stream.ReadByteArray();
						stream.AlignStream(AlignType.Align4);
					}
				}
				else
				{
					m_audioData = stream.ReadByteArray();
					if (IsAlignAudioData(stream.Version))
					{
						stream.AlignStream(AlignType.Align4);
					}
				}

				if (IsReadDecompressOnLoadThird(stream.Version))
				{
					DecompressOnLoad = stream.ReadBoolean();
				}

				if (IsReadStream(stream.Version))
				{
					if (!IsStreamInt32(stream.Version))
					{
						Stream = stream.ReadBoolean() ? 1 : 0;
					}
				}
			}
		}

		public override void ExportBinary(IAssetsExporter exporter, Stream stream)
		{
			if (IsReadLoadType(exporter.Version))
			{
				ResourcesFile res = File.Collection.FindResourcesFile(File, FSBResource.Source);
				if (res == null)
				{
					Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{Name}' because resources file '{FSBResource.Source}' wasn't found");
					return;
				}

				res.Stream.Position = FSBResource.Offset;
				if (StreamedResource.IsReadSize(exporter.Version))
				{
					res.Stream.CopyStream(stream, FSBResource.Size);
				}
				else
				{
					// I think they read data by it's type for this verison, so I can't even export raw data :/
				}
			}
			else
			{
				if (IsReadStreamingInfo(exporter.Version))
				{
					if (Stream == 2)
					{
						ResourcesFile res = File.Collection.FindResourcesFile(File, StreamingInfo.Path);
						if (res == null)
						{
							Logger.Log(LogType.Warning, LogCategory.Export, $"Can't export '{Name}' because resources file '{StreamingInfo.Path}' wasn't found");
							return;
						}

						res.Stream.Position = FSBResource.Offset;
						res.Stream.CopyStream(stream, StreamingInfo.Size);
					}
					else
					{
						stream.Write(m_audioData, 0, m_audioData.Length);
					}
				}
				else
				{
					stream.Write(m_audioData, 0, m_audioData.Length);
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public override string ExportExtension
		{
			get
			{
				if (IsReadType(File.Version))
				{
					switch (Type)
					{
						case FMODSoundType.ACC:
							return "m4a";
						case FMODSoundType.AIFF:
							return "aif";
						case FMODSoundType.IT:
							return "it";
						case FMODSoundType.MOD:
							return "mod";
						case FMODSoundType.MPEG:
							return "mp3";
						case FMODSoundType.OGGVORBIS:
							return "ogg";
						case FMODSoundType.S3M:
							return "s3m";
						case FMODSoundType.WAV:
							return "wav";
						case FMODSoundType.XM:
							return "xm";
						case FMODSoundType.XMA:
							return "wav";
						case FMODSoundType.VAG:
							return "vag";
						case FMODSoundType.AUDIOQUEUE:
							return "fsb";
						default:
							return "audioClip";
					}
				}
				else
				{
					switch (CompressionFormat)
					{
						case AudioCompressionFormat.PCM:
						case AudioCompressionFormat.ADPCM:
						case AudioCompressionFormat.Vorbis:
						case AudioCompressionFormat.MP3:
						case AudioCompressionFormat.GCADPCM:
							return "fsb";
						case AudioCompressionFormat.VAG:
						case AudioCompressionFormat.HEVAG:
							return "vag";
						case AudioCompressionFormat.XMA:
							return "wav";
						case AudioCompressionFormat.AAC:
							return "m4a";
						case AudioCompressionFormat.ATRAC9:
							return "at9";
						default:
							return "audioClip";
					}
				}
			}
		}

		public AudioClipLoadType LoadType { get; private set; }
		public int Channels { get; private set; }
		public int BitsPerSample { get; private set; }
		public bool IsTrackerFormat { get; private set; }
		public bool Ambisonic { get; private set; }
		public int AudioClipFlags { get; private set; }
		public int SubsoundIndex { get; private set; }
		public bool PreloadAudioData { get; private set; }
		public bool LoadInBackground { get; private set; }
		public AudioCompressionFormat CompressionFormat { get; private set; }

		public bool DecompressOnLoad { get; private set; }
		public FMODSoundFormat Format { get; private set; }
		/// <summary>
		/// SoundType in some versions
		/// </summary>
		public FMODSoundType Type {get; private set; }
		public float Length { get; private set; }
		public int Frequency { get; private set; }
		public int Size { get; private set; }
		/// <summary>
		/// 3D previously
		/// </summary>
		public bool Legacy3D { get; private set; }
		public bool UseHardware { get; private set; }
		public IReadOnlyList<byte> AudioData => m_audioData;
		public int Stream { get; private set; }

		public StreamedResource FSBResource;
		public StreamingInfo StreamingInfo;

		private byte[] m_audioData;
	}
}
