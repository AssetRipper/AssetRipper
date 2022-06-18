using AssetRipper.Core.Classes.ReflectionProbe;
using AssetRipper.SourceGenerated.Classes.ClassID_215;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ReflectionProbeExtensions
	{
		public static ReflectionProbeType GetType_(this IReflectionProbe probe)
		{
			return (ReflectionProbeType)probe.Type_C215;
		}

		public static ReflectionProbeMode GetMode(this IReflectionProbe probe)
		{
			return (ReflectionProbeMode)probe.Mode_C215;
		}

		public static ReflectionProbeRefreshMode GetRefreshMode(this IReflectionProbe probe)
		{
			return (ReflectionProbeRefreshMode)probe.RefreshMode_C215;
		}

		public static ReflectionProbeTimeSlicingMode GetTimeSlicingMode(this IReflectionProbe probe)
		{
			return (ReflectionProbeTimeSlicingMode)probe.TimeSlicingMode_C215;
		}

		public static ReflectionProbeClearFlags GetClearFlags(this IReflectionProbe probe)
		{
			return (ReflectionProbeClearFlags)probe.ClearFlags_C215;
		}
	}
}
