using AssetRipper.SourceGenerated.Classes.ClassID_20;
using AssetRipper.SourceGenerated.Enums;

namespace AssetRipper.SourceGenerated.Extensions;

public static class CameraExtensions
{
	public static FieldOfViewAxis GetFOVAxisMode(this ICamera camera) => (FieldOfViewAxis)camera.FOVAxisMode_C20;

	public static GateFitMode GetGateFitMode(this ICamera camera) => (GateFitMode)camera.GateFitMode_C20;

	public static StereoTargetEyeMask GetStereoTargetEyeMask(this ICamera camera) => (StereoTargetEyeMask)camera.TargetEye_C20;
}
