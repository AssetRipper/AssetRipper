using System;
using System.Collections.Generic;
using System.IO;
using UtinyRipper.Classes.Shaders.Exporters;

namespace UtinyRipper.Classes.Shaders
{
	public struct SerializedShader : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			PropInfo.Read(stream);
			m_subShaders = stream.ReadArray<SerializedSubShader>();
			Name = stream.ReadStringAligned();
			CustomEditorName = stream.ReadStringAligned();
			FallbackName = stream.ReadStringAligned();
			m_dependencies = stream.ReadArray<SerializedShaderDependency>();
			DisableNoSubshadersMessage = stream.ReadBoolean();
			stream.AlignStream(AlignType.Align4);
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
