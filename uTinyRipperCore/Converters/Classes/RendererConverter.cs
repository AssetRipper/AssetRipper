using uTinyRipper.Classes;

namespace uTinyRipper.Converters
{
	public static class RendererConverter
	{
		public static void Convert(IExportContainer container, Renderer origin, Renderer instance)
		{
#warning TODO:
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
#if UNIVERSAL
			instance.ScaleInLightmap = origin.ScaleInLightmap;
			instance.ReceiveGI = origin.ReceiveGI;
			instance.PreserveUVs = origin.PreserveUVs;
			instance.IgnoreNormalsForChartDetection = origin.IgnoreNormalsForChartDetection;
			instance.ImportantGI = origin.ImportantGI;
			instance.SelectedWireframeHidden = origin.SelectedWireframeHidden;
			instance.StitchLightmapSeams = origin.StitchLightmapSeams;
			instance.SelectedEditorRenderState = origin.SelectedEditorRenderState;
			instance.MinimumChartSize = origin.MinimumChartSize;
			instance.AutoUVMaxDistance = origin.AutoUVMaxDistance;
			instance.AutoUVMaxAngle = origin.AutoUVMaxAngle;
			instance.GIBackfaceCull = origin.GIBackfaceCull;
#endif
			instance.SortingLayerID = origin.SortingLayerID;
			instance.SortingLayer = origin.SortingLayer;
			instance.SortingOrder = origin.SortingOrder;

			instance.LightmapTilingOffset = origin.LightmapTilingOffset;
			instance.LightmapTilingOffsetDynamic = origin.LightmapTilingOffsetDynamic;
			instance.StaticBatchInfo = origin.StaticBatchInfo;
			instance.StaticBatchRoot = origin.StaticBatchRoot;
			instance.ProbeAnchor = origin.ProbeAnchor;
			instance.LightProbeVolumeOverride = origin.LightProbeVolumeOverride;
#if UNIVERSAL
			instance.LightmapParameters = origin.LightmapParameters;
#endif
		}
	}
}
