using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Enums;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Processing.Editor;

internal static class EditorFormatConverter
{
	public static void Convert(IRenderer renderer)
	{
		renderer.ScaleInLightmap_C25 = 1.0f;
		renderer.ReceiveGI_C25 = (int)ReceiveGI.Lightmaps;
		renderer.PreserveUVs_C25 = false;
		renderer.IgnoreNormalsForChartDetection_C25 = false;
		renderer.ImportantGI_C25 = false;
		renderer.StitchLightmapSeams_C25 = false;
		renderer.SelectedEditorRenderState_C25 = (int)(EditorSelectedRenderState)3;
		renderer.MinimumChartSize_C25 = 4;
		renderer.AutoUVMaxDistance_C25 = 0.5f;
		renderer.AutoUVMaxAngle_C25 = 89.0f;
		renderer.LightmapParameters_C25P = null;
		if (renderer.Has_StaticBatchInfo_C25())
		{
			if (!renderer.StaticBatchInfo_C25.IsDefault())
			{
				renderer.MarkGameObjectAsStatic();
			}
		}
		else if (renderer.Has_SubsetIndices_C25())
		{
			if (renderer.SubsetIndices_C25.Count != 0)
			{
				renderer.MarkGameObjectAsStatic();
			}
		}
	}
}
