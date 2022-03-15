using AssetRipper.Core.Classes.Renderer;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters
{
	public static class RendererConverter
	{
		public static void Convert(IExportContainer container, Renderer origin, Renderer instance)
		{
			ComponentConverter.Convert(container, origin, instance);
			instance.Enabled = origin.Enabled;
			instance.CastShadows = origin.CastShadows;
			instance.ReceiveShadows = origin.ReceiveShadows;
			instance.DynamicOccludee = origin.DynamicOccludee;
			instance.MotionVectors = origin.MotionVectors;
			instance.LightProbeUsage = origin.LightProbeUsage;
			instance.ReflectionProbeUsage = origin.ReflectionProbeUsage;
			instance.RenderingLayerMask = origin.RenderingLayerMask;
			instance.RendererPriority = origin.RendererPriority;
			instance.LightmapIndex = origin.LightmapIndex;
			instance.LightmapIndexDynamic = origin.LightmapIndexDynamic;
			instance.Materials = origin.Materials;
			instance.SubsetIndices = origin.SubsetIndices;

			instance.SortingLayerID = origin.SortingLayerID;
			instance.SortingLayer = origin.SortingLayer;
			instance.SortingOrder = origin.SortingOrder;

			instance.LightmapTilingOffset = origin.LightmapTilingOffset;
			instance.LightmapTilingOffsetDynamic = origin.LightmapTilingOffsetDynamic;
			instance.StaticBatchInfo.CopyValues(origin.StaticBatchInfo);
			instance.StaticBatchRoot = origin.StaticBatchRoot;
			instance.ProbeAnchor = origin.ProbeAnchor;
			instance.LightProbeVolumeOverride = origin.LightProbeVolumeOverride;
		}
	}
}
