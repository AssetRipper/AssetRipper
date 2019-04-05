using System.Collections.Generic;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedShader : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			PropInfo.Read(reader);
			m_subShaders = reader.ReadAssetArray<SerializedSubShader>();
			Name = reader.ReadString();
			CustomEditorName = reader.ReadString();
			FallbackName = reader.ReadString();
			m_dependencies = reader.ReadAssetArray<SerializedShaderDependency>();
			DisableNoSubshadersMessage = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public void Export(ShaderWriter writer)
		{
			writer.Write("Shader \"{0}\" {{\n", Name);

			PropInfo.Export(writer);

			foreach(SerializedSubShader subShader in SubShaders)
			{
				subShader.Export(writer);
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

		public IReadOnlyList<SerializedSubShader> SubShaders => m_subShaders;
		public string Name { get; private set; }
		public string CustomEditorName { get; private set; }
		public string FallbackName { get; private set; }
		public IReadOnlyList<SerializedShaderDependency> Dependencies => m_dependencies;
		public bool DisableNoSubshadersMessage { get; private set; }

		public SerializedProperties PropInfo;
		
		private SerializedSubShader[] m_subShaders;
		private SerializedShaderDependency[] m_dependencies;
	}
}
