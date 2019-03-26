using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimatorControllers.Editor;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
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

		public void Read(AssetReader reader)
		{
			m_conditionConstantArray = reader.ReadAssetArray<OffsetPtr<ConditionConstant>>();
			DestinationState = (int)reader.ReadUInt32();
			if(IsReadPathID(reader.Version))
			{
				FullPathID = reader.ReadUInt32();
			}
			ID = reader.ReadUInt32();
			UserID = reader.ReadUInt32();
			TransitionDuration = reader.ReadSingle();
			TransitionOffset = reader.ReadSingle();
			if(IsReadAtomic(reader.Version))
			{
				Atomic = reader.ReadBoolean();
			}
			else
			{
				ExitTime = reader.ReadSingle();
				HasExitTime = reader.ReadBoolean();
				HasFixedDuration = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);

				InterruptionSource = (TransitionInterruptionSource)reader.ReadInt32();
				OrderedInterruption = reader.ReadBoolean();
			}

			if (IsCanTransitionToSelf(reader.Version))
			{
				CanTransitionToSelf = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public bool GetHasFixedDuration(Version version)
		{
			return IsReadAtomic(version) ? true : HasFixedDuration;
		}

		public TransitionInterruptionSource GetInterruptionSource(Version version)
		{
			if (IsReadAtomic(version))
			{
				return Atomic ? TransitionInterruptionSource.None : TransitionInterruptionSource.Destination;
			}
			else
			{
				return InterruptionSource;
			}
		}

		public float GetExitTime(Version version)
		{
			if (IsReadAtomic(version))
			{
				foreach (OffsetPtr<ConditionConstant> conditionPtr in ConditionConstantArray)
				{
					if (conditionPtr.Instance.ConditionMode == AnimatorConditionMode.ExitTime)
					{
						return conditionPtr.Instance.ExitTime;
					}
				}
				return 1.0f;
			}
			else
			{
				return ExitTime;
			}
		}

		public bool GetHasExitTime(Version version)
		{
			if (IsReadAtomic(version))
			{
				foreach (OffsetPtr<ConditionConstant> conditionPtr in ConditionConstantArray)
				{
					if (conditionPtr.Instance.ConditionMode == AnimatorConditionMode.ExitTime)
					{
						return true;
					}
				}
				return false;
			}
			else
			{
				return HasExitTime;
			}
		}

		public bool IsExit => DestinationState >= 30000;

		public IReadOnlyList<OffsetPtr<ConditionConstant>> ConditionConstantArray => m_conditionConstantArray;
		public int DestinationState { get; private set; }
		public uint FullPathID { get; private set; }
		/// <summary>
		/// PathID
		/// </summary>
		public uint ID { get; private set; }
		/// <summary>
		/// Name
		/// </summary>
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
