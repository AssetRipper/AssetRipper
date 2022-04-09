using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes.AudioClip
{
	public sealed class AudioClip : NamedObject, IAudioClip
	{
		public AudioClip(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			if (version.IsGreaterEqual(5))
			{
				// old AudioClip asset format isn't compatible with new Engine version
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

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasLoadType(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasIsTrackerFormat(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasAmbisonic(UnityVersion version) => version.IsGreater(2017);

		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasAudioClipFlags(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// (Less or equal to 5.6.0b4) or (5.6.011 and greater)
		/// </summary>
		public static bool HasLoadInBackground(UnityVersion version) => version.IsLessEqual(5, 6, 0, UnityVersionType.Beta, 4) || version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 11);
		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasCompressionFormat(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasEditorResource(TransferInstructionFlags flags) => !flags.IsRelease();

		/// <summary>
		/// 2.0.0 to 3.0.0 exclusive
		/// </summary>
		public static bool HasDecompressOnLoad(UnityVersion version) => version.IsGreaterEqual(2) && version.IsLess(3);
		/// <summary>
		/// 2.6.0 to 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasTypeField(UnityVersion version) => version.IsGreaterEqual(2, 6) && version.IsLessEqual(5, 0, 0, UnityVersionType.Beta);
		/// <summary>
		/// Less than 2.6.0
		/// </summary>
		public static bool HasLength(UnityVersion version) => version.IsLess(2, 6);
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool Has3D(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 3.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasUseHardware(UnityVersion version) => version.IsGreaterEqual(3);
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		public static bool HasStreamingInfo(UnityVersion version) => version.IsGreaterEqual(3, 2);
		/// <summary>
		/// 3.0.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasStream(UnityVersion version) => version.IsGreaterEqual(3);

		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		private static bool IsAlignTrackerFormat(UnityVersion version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		}

		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		private static bool IsFSBResourceFirst(UnityVersion version) => version.IsEqual(5, 0, 0, UnityVersionType.Beta);
		private static int GetDecompressOnLoadOrder(UnityVersion version)
		{
			if (version.IsGreaterEqual(3))
			{
				return 0;
			}

			if (version.IsEqual(2, 6))
			{
				return 3;
			}
			if (version.IsGreaterEqual(2, 1) && version.IsLess(2, 6))
			{
				return 2;
			}
			return 1;
		}
		/// <summary>
		/// 3.2.0 and greater
		/// </summary>
		private static bool IsStreamInt32(UnityVersion version) => version.IsGreaterEqual(3, 2);
		/// <summary>
		/// 2.1.0 and greater
		/// </summary>
		private static bool IsAlignBools(UnityVersion version) => version.IsGreaterEqual(2, 1);
		/// <summary>
		/// 2.1.0 to 3.0.0 exclusive or 3.2.0 and greater
		/// </summary>
		private static bool IsAlignAudioData(UnityVersion version) => version.IsGreaterEqual(2, 1) && version.IsLess(3) || version.IsGreaterEqual(3, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasLoadType(reader.Version))
			{
				LoadType = (AudioClipLoadType)reader.ReadInt32();
				Channels = reader.ReadInt32();
				Frequency = reader.ReadInt32();
				BitsPerSample = reader.ReadInt32();
				Length = reader.ReadSingle();

				if (HasIsTrackerFormat(reader.Version))
				{
					IsTrackerFormat = reader.ReadBoolean();
				}
				if (HasAmbisonic(reader.Version))
				{
					Ambisonic = reader.ReadBoolean();
				}
				if (IsAlignTrackerFormat(reader.Version))
				{
					reader.AlignStream();
				}

				if (HasAudioClipFlags(reader.Version))
				{
					AudioClipFlags = reader.ReadInt32();
				}
				if (IsFSBResourceFirst(reader.Version))
				{
					FSBResource.Read(reader);
				}

				SubsoundIndex = reader.ReadInt32();
				PreloadAudioData = reader.ReadBoolean();
				if (HasLoadInBackground(reader.Version))
				{
					LoadInBackground = reader.ReadBoolean();
				}
				Legacy3D = reader.ReadBoolean();
				reader.AlignStream();

				if (!IsFSBResourceFirst(reader.Version))
				{
					FSBResource.Read(reader);
				}

				if (HasTypeField(reader.Version))
				{
					Type = (FMODSoundType)reader.ReadInt32();
				}
				if (HasCompressionFormat(reader.Version))
				{
					CompressionFormat = (AudioCompressionFormat)reader.ReadInt32();
				}
				reader.AlignStream();
			}
			else
			{
				int decompressionOrder = GetDecompressOnLoadOrder(reader.Version);
				if (decompressionOrder == 1)
				{
					DecompressOnLoad = reader.ReadBoolean();
				}

				Format = (FMODSoundFormat)reader.ReadInt32();
				if (HasTypeField(reader.Version))
				{
					Type = (FMODSoundType)reader.ReadInt32();
				}
				if (HasLength(reader.Version))
				{
					Length = reader.ReadSingle();
					Frequency = reader.ReadInt32();
					Size = reader.ReadInt32();
				}

				if (decompressionOrder == 2)
				{
					DecompressOnLoad = reader.ReadBoolean();
				}
				if (Has3D(reader.Version))
				{
					Legacy3D = reader.ReadBoolean();
				}
				if (HasUseHardware(reader.Version))
				{
					UseHardware = reader.ReadBoolean();
				}
				if (IsAlignBools(reader.Version))
				{
					reader.AlignStream();
				}

				if (IsStreamInt32(reader.Version))
				{
					LoadType = (AudioClipLoadType)reader.ReadInt32();
				}

				if (HasStreamingInfo(reader.Version))
				{
					bool isInnerData = LoadType == AudioClipLoadType.Streaming ? SerializedFile.Collection.FindResourceFile(StreamingFileName) == null : true;
					if (isInnerData)
					{
						AudioData = reader.ReadByteArray();
						reader.AlignStream();
					}
					else
					{
						streamingInfo.Read(reader, StreamingFileName);
					}
				}
				else
				{
					AudioData = reader.ReadByteArray();
					if (IsAlignAudioData(reader.Version))
					{
						reader.AlignStream();
					}
				}

				if (decompressionOrder == 3)
				{
					DecompressOnLoad = reader.ReadBoolean();
				}

				if (HasStream(reader.Version))
				{
					if (!IsStreamInt32(reader.Version))
					{
						LoadType = reader.ReadBoolean() ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
					}
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
			/*YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(container.ExportVersion);
			node.Add(LoadTypeName, (int)LoadType);
			node.Add(ChannelsName, Channels);
			node.Add(FrequencyName, Frequency);
			node.Add(BitsPerSampleName, BitsPerSample);
			node.Add(LengthName, Length);
			node.Add(IsTrackerFormatName, IsTrackerFormat);
			node.Add(AmbisonicName, Ambisonic);
			node.Add(SubsoundIndexName, SubsoundIndex);
			node.Add(PreloadAudioDataName, PreloadAudioData);
			node.Add(LoadInBackgroundName, LoadInBackground);
			node.Add(Legacy3DName, Legacy3D);
			node.Add(ResourceName, FSBResource.ExportYAML(container));
			node.Add(CompressionFormatName, (int)CompressionFormat);
			node.Add(EditorResourceName, EditorResource.ExportYAML(container));
			node.Add(EditorCompressionFormatName, (int)EditorCompressionFormat);
			return node;*/
		}

		/// <summary>
		/// Stream previously
		/// </summary>
		public AudioClipLoadType LoadType { get; set; }
		public int Channels { get; set; }
		public int BitsPerSample { get; set; }
		public bool IsTrackerFormat { get; set; }
		public bool Ambisonic { get; set; }
		public int AudioClipFlags { get; set; }
		public int SubsoundIndex { get; set; }
		public bool PreloadAudioData { get; set; }
		public bool LoadInBackground { get; set; }
		public AudioCompressionFormat CompressionFormat { get; set; }

		public bool DecompressOnLoad { get; set; }
		public FMODSoundFormat Format { get; set; }
		/// <summary>
		/// SoundType in some versions
		/// </summary>
		public FMODSoundType Type { get; set; }
		public float Length { get; set; }
		public int Frequency { get; set; }
		public int Size { get; set; }
		/// <summary>
		/// 3D previously
		/// </summary>
		public bool Legacy3D { get; set; }
		public bool UseHardware { get; set; }
		public byte[] AudioData { get; set; }

		/// <summary>
		/// SerializedFile.Name + "." + StreamingFileExtension;
		/// </summary>
		private string StreamingFileName => SerializedFile.Name + "." + StreamingFileExtension;

		public IStreamedResource Resource
		{
			get => HasLoadType(SerializedFile.Version) ? FSBResource : null;
		}

		public IStreamingInfo StreamingInfo
		{
			get => HasStreamingInfo(SerializedFile.Version) ? streamingInfo : null;
		}

		public bool HasType => HasTypeField(SerializedFile.Version);

		public const string StreamingFileExtension = "resS";

		public const string LoadTypeName = "m_LoadType";
		public const string ChannelsName = "m_Channels";
		public const string FrequencyName = "m_Frequency";
		public const string BitsPerSampleName = "m_BitsPerSample";
		public const string LengthName = "m_Length";
		public const string IsTrackerFormatName = "m_IsTrackerFormat";
		public const string AmbisonicName = "m_Ambisonic";
		public const string SubsoundIndexName = "m_SubsoundIndex";
		public const string PreloadAudioDataName = "m_PreloadAudioData";
		public const string LoadInBackgroundName = "m_LoadInBackground";
		public const string Legacy3DName = "m_Legacy3D";
		public const string ResourceName = "m_Resource";
		public const string CompressionFormatName = "m_CompressionFormat";
		public const string EditorResourceName = "m_EditorResource";
		public const string EditorCompressionFormatName = "m_EditorCompressionFormat";

		public StreamedResource FSBResource = new();
		public StreamingInfo streamingInfo = new();
	}
}
