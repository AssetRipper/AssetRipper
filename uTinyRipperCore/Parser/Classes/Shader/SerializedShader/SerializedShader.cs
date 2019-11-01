using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedShader : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			PropInfo.Read(reader);
			SubShaders = reader.ReadAssetArray<SerializedSubShader>();
			Name = reader.ReadString();
			CustomEditorName = reader.ReadString();
			FallbackName = reader.ReadString();
			m_dependencies = reader.ReadAssetArray<SerializedShaderDependency>();
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

			if(FallbackName != string.Empty)
			{
				writer.WriteIndent(1);
				writer.Write("Fallback \"{0}\"\n", FallbackName);
			}

			if (CustomEditorName != string.Empty)
			{
				writer.WriteIndent(1);
				writer.Write("CustomEditor \"{0}\"\n", CustomEditorName);
			}

			writer.Write('}');
		}

		public SerializedSubShader[] SubShaders { get; set; }
		public string Name { get; private set; }
		public string CustomEditorName { get; private set; }
		public string FallbackName { get; private set; }
		public IReadOnlyList<SerializedShaderDependency> Dependencies => m_dependencies;
		public bool DisableNoSubshadersMessage { get; private set; }

		public SerializedProperties PropInfo;
		
		private SerializedShaderDependency[] m_dependencies;
	}
}
