using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO;
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

		public void Export(ShaderWriter writer)
		{
			writer.Write("Shader \"{0}\" {{\n", Name);

			PropInfo.Export(writer);

			for (int i = 0; i < SubShaders.Length; i++)
			{
				SubShaders[i].Export(writer);
			}

			if (FallbackName.Length != 0)
			{
				writer.WriteIndent(1);
				writer.Write("Fallback \"{0}\"\n", FallbackName);
			}

			if (CustomEditorName.Length != 0)
			{
				writer.WriteIndent(1);
				writer.Write("CustomEditor \"{0}\"\n", CustomEditorName);
			}

			writer.Write('}');
		}
	}
}
