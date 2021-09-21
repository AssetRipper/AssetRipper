using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;

namespace AssetRipper.Core.Classes.Shader.SerializedShader
{
	public struct SerializedShader : IAssetReadable
	{
		public SerializedProperties PropInfo;
		public SerializedSubShader[] SubShaders { get; set; }
		public string Name { get; set; }
		public string CustomEditorName { get; set; }
		public string FallbackName { get; set; }
		public SerializedShaderDependency[] Dependencies { get; set; }
		public SerializedCustomEditorForRenderPipeline[] CustomEditorForRenderPipelines { get; set; }
		public bool DisableNoSubshadersMessage { get; set; }

		/// <summary>
		/// 2021 and greater
		/// </summary>
		public static bool HasCustomEditor(UnityVersion version) => version.IsGreaterEqual(2021);

		public void Read(AssetReader reader)
		{
			PropInfo.Read(reader);
			SubShaders = reader.ReadAssetArray<SerializedSubShader>();
			Name = reader.ReadString();
			CustomEditorName = reader.ReadString();
			FallbackName = reader.ReadString();
			Dependencies = reader.ReadAssetArray<SerializedShaderDependency>();
			if (HasCustomEditor(reader.Version))
			{
				CustomEditorForRenderPipelines = reader.ReadAssetArray<SerializedCustomEditorForRenderPipeline>();
			}
			DisableNoSubshadersMessage = reader.ReadBoolean();
			reader.AlignStream();
		}
	}
}
