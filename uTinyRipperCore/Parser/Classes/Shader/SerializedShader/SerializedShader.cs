using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Classes.Shaders.Exporters;

namespace uTinyRipper.Classes.Shaders
{
	public struct SerializedShader : IAssetReadable
	{
		public void Read(AssetReader reader)
		{
			PropInfo.Read(reader);
			m_subShaders = reader.ReadArray<SerializedSubShader>();
			Name = reader.ReadStringAligned();
			CustomEditorName = reader.ReadStringAligned();
			FallbackName = reader.ReadStringAligned();
			m_dependencies = reader.ReadArray<SerializedShaderDependency>();
			DisableNoSubshadersMessage = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
		}

		public void Export(TextWriter writer, Shader shader, Func<ShaderGpuProgramType, ShaderTextExporter> exporterInstantiator)
		{
			writer.Write("Shader \"{0}\" {{\n", Name);

			PropInfo.Export(writer);

			foreach(SerializedSubShader subShader in SubShaders)
			{
				subShader.Export(writer, shader, exporterInstantiator);
			}

			if(FallbackName != string.Empty)
			{
				writer.WriteIntent(1);
				writer.Write("Fallback \"{0}\"\n", FallbackName);
			}

			if (CustomEditorName != string.Empty)
			{
				writer.WriteIntent(1);
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
