using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct StateConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadLeafInfo(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool IsReadID(Version version)
		{
			return version.IsLess(4, 3);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadFullPathID(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadSpeedParam(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadTimeParam(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool IsReadCycleOffset(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDefaultValues(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool IsReadMirror(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}

		public void Read(AssetStream stream)
		{
			m_transitionConstantArray = stream.ReadArray<OffsetPtr<TransitionConstant>>();
			m_blendTreeConstantIndexArray = stream.ReadInt32Array();
			if(IsReadLeafInfo(stream.Version))
			{
				m_leafInfoArray = stream.ReadArray<LeafInfoConstant>();
			}

			m_blendTreeConstantArray = stream.ReadArray<OffsetPtr<BlendTreeConstant>>();
			if (IsReadID(stream.Version))
			{
				ID = stream.ReadUInt32();
			}
			else
			{
				NameID = stream.ReadUInt32();
				PathID = stream.ReadUInt32();
			}
			if(IsReadFullPathID(stream.Version))
			{
				FullPathID = stream.ReadUInt32();
			}

			TagID = stream.ReadUInt32();
			if(IsReadSpeedParam(stream.Version))
			{
				SpeedParamID = stream.ReadUInt32();
				MirrorParamID = stream.ReadUInt32();
				CycleOffsetParamID = stream.ReadUInt32();
			}
			if (IsReadTimeParam(stream.Version))
			{
				TimeParamID = stream.ReadUInt32();
			}

			Speed = stream.ReadSingle();
			if (IsReadCycleOffset(stream.Version))
			{
				CycleOffset = stream.ReadSingle();
			}

			IKOnFeet = stream.ReadBoolean();
			if (IsReadDefaultValues(stream.Version))
			{
				WriteDefaultValues = stream.ReadBoolean();
			}

			Loop = stream.ReadBoolean();
			if (IsReadMirror(stream.Version))
			{
				Mirror = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<OffsetPtr<TransitionConstant>> TransitionConstantArray => m_transitionConstantArray;
		public IReadOnlyList<int> BlendTreeConstantIndexArray => m_blendTreeConstantIndexArray;
		public IReadOnlyList<LeafInfoConstant> LeafInfoArray => m_leafInfoArray;
		public IReadOnlyList<OffsetPtr<BlendTreeConstant>> BlendTreeConstantArray => m_blendTreeConstantArray;
		public uint ID { get; private set; }
		public uint NameID { get; private set; }
		public uint PathID { get; private set; }
		public uint FullPathID { get; private set; }
		public uint TagID { get; private set; }
		public uint SpeedParamID { get; private set; }
		public uint MirrorParamID { get; private set; }
		public uint CycleOffsetParamID { get; private set; }
		public uint TimeParamID { get; private set; }
		public float Speed { get; private set; }
		public float CycleOffset { get; private set; }
		public bool IKOnFeet { get; private set; }
		public bool WriteDefaultValues { get; private set; }
		public bool Loop { get; private set; }
		public bool Mirror { get; private set; }
				
		private OffsetPtr<TransitionConstant>[] m_transitionConstantArray;
		private int[] m_blendTreeConstantIndexArray;
		private LeafInfoConstant[] m_leafInfoArray;
		private OffsetPtr<BlendTreeConstant>[] m_blendTreeConstantArray;
		
	}
}
