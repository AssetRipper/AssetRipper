using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedCustomEditorForRenderPipeline : IAssetReadable
	{
		public string customEditorName { get; set; }
		public string renderPipelineType { get; set; }

		public void Read(AssetReader reader)
		{
			customEditorName = reader.ReadAlignedString();
			renderPipelineType = reader.ReadAlignedString();
		}
	}
}
