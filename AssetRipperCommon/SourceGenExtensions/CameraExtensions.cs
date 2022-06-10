using AssetRipper.Core.Classes.Camera;
using AssetRipper.SourceGenerated.Classes.ClassID_20;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class CameraExtensions
	{
		public static FieldOfViewAxis GetFOVAxisMode(this ICamera camera) => (FieldOfViewAxis)camera.FOVAxisMode_C20;

		public static ProjectionMatrixMode GetProjectionMatrixMode(this ICamera camera) => (ProjectionMatrixMode)camera.ProjectionMatrixMode_C20;

		public static GateFitMode GetGateFitMode(this ICamera camera) => (GateFitMode)camera.GateFitMode_C20;

		public static RenderingPath GetRenderingPath(this ICamera camera) => (RenderingPath)camera.RenderingPath_C20;

		public static StereoTargetEyeMask GetStereoTargetEyeMask(this ICamera camera) => (StereoTargetEyeMask)camera.TargetEye_C20;
	}
}
