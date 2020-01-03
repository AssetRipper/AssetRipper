using System;
using uTinyRipper.Classes.AnimatorTransitions;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct TransitionConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathID(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		public static bool HasAtomic(Version version) => version.IsLess(5);
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool IsCanTransitionToSelf(Version version) => version.IsGreaterEqual(4, 5);

		public void Read(AssetReader reader)
		{
			ConditionConstantArray = reader.ReadAssetArray<OffsetPtr<ConditionConstant>>();
			DestinationState = (int)reader.ReadUInt32();
			if (HasPathID(reader.Version))
			{
				FullPathID = reader.ReadUInt32();
			}
			ID = reader.ReadUInt32();
			UserID = reader.ReadUInt32();
			TransitionDuration = reader.ReadSingle();
			TransitionOffset = reader.ReadSingle();
			if (HasAtomic(reader.Version))
			{
				Atomic = reader.ReadBoolean();
			}
			else
			{
				ExitTime = reader.ReadSingle();
				HasExitTime = reader.ReadBoolean();
				HasFixedDuration = reader.ReadBoolean();
				reader.AlignStream();

				InterruptionSource = (TransitionInterruptionSource)reader.ReadInt32();
				OrderedInterruption = reader.ReadBoolean();
			}

			if (IsCanTransitionToSelf(reader.Version))
			{
				CanTransitionToSelf = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public bool GetHasFixedDuration(Version version)
		{
			return HasAtomic(version) ? true : HasFixedDuration;
		}

		public TransitionInterruptionSource GetInterruptionSource(Version version)
		{
			if (HasAtomic(version))
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
			if (HasAtomic(version))
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
			if (HasAtomic(version))
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

		public OffsetPtr<ConditionConstant>[] ConditionConstantArray { get; set; }
		public int DestinationState { get; set; }
		public uint FullPathID { get; set; }
		/// <summary>
		/// PathID
		/// </summary>
		public uint ID { get; set; }
		/// <summary>
		/// Name
		/// </summary>
		public uint UserID { get; set; }
		public float TransitionDuration { get; set; }
		public float TransitionOffset { get; set; }
		public bool Atomic { get; set; }
		public float ExitTime { get; set; }
		public bool HasExitTime { get; set; }
		public bool HasFixedDuration { get; set; }
		public TransitionInterruptionSource InterruptionSource { get; set; }
		public bool OrderedInterruption { get; set; }
		public bool CanTransitionToSelf { get; set; }
	}
}
