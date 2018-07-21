using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct TransitionConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadPathID(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadAtomic(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool IsCanTransitionToSelf(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		
		public void Read(AssetStream stream)
		{
			m_conditionConstantArray = stream.ReadArray<OffsetPtr<ConditionConstant>>();
			DestinationState = stream.ReadUInt32();
			if(IsReadPathID(stream.Version))
			{
				FullPathID = stream.ReadUInt32();
			}
			ID = stream.ReadUInt32();
			UserID = stream.ReadUInt32();
			TransitionDuration = stream.ReadSingle();
			TransitionOffset = stream.ReadSingle();
			if(IsReadAtomic(stream.Version))
			{
				Atomic = stream.ReadBoolean();
			}
			else
			{
				ExitTime = stream.ReadSingle();
				HasExitTime = stream.ReadBoolean();
				HasFixedDuration = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);

				InterruptionSource = (TransitionInterruptionSource)stream.ReadInt32();
				OrderedInterruption = stream.ReadBoolean();
			}

			if (IsCanTransitionToSelf(stream.Version))
			{
				CanTransitionToSelf = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstantArray => m_conditionConstantArray;
		public uint DestinationState { get; private set; }
		public uint FullPathID { get; private set; }
		public uint ID { get; private set; }
		public uint UserID { get; private set; }
		public float TransitionDuration { get; private set; }
		public float TransitionOffset { get; private set; }
		public bool Atomic { get; private set; }
		public float ExitTime { get; private set; }
		public bool HasExitTime { get; private set; }
		public bool HasFixedDuration { get; private set; }
		public TransitionInterruptionSource InterruptionSource { get; private set; }
		public bool OrderedInterruption { get; private set; }
		public bool CanTransitionToSelf { get; private set; }

		private OffsetPtr<ConditionConstant>[] m_conditionConstantArray;
	}
}
