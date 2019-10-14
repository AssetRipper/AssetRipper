using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderExporter
{
	internal class ConstantBufferChunk
	{

		private ShaderSubProgram shaderSubprogram;
		private uint contantBufferOffset;
		private Dictionary<string, uint> nameLookup;
		List<VariableChunk> variables;
		uint size;
		internal ConstantBufferChunk(ShaderSubProgram shaderSubprogram, uint contantBufferOffset, Dictionary<string, uint> nameLookup)
		{
			this.shaderSubprogram = shaderSubprogram;
			this.contantBufferOffset = contantBufferOffset;
			this.nameLookup = nameLookup;

			uint headerSize = (uint)shaderSubprogram.ConstantBuffers.Count * 24;
			uint variableOffset = contantBufferOffset + headerSize;
			variables = new List<VariableChunk>();
			int constantBufferIndex = 0;
			foreach (var constantBuffer in shaderSubprogram.ConstantBuffers)
			{
				var variableChunk = new VariableChunk(constantBuffer, constantBufferIndex++, variableOffset, shaderSubprogram.ProgramType);
				variables.Add(variableChunk);
				variableOffset += variableChunk.Size;
			}
			size = variableOffset - contantBufferOffset;
		}
		internal uint Size => size;

		internal uint Count => (uint)shaderSubprogram.ConstantBuffers.Count;

		internal void Write(EndianWriter writer)
		{
			uint headerSize = (uint)shaderSubprogram.ConstantBuffers.Count * 24;
			uint variableOffset = contantBufferOffset + headerSize;
			for(int i = 0; i < shaderSubprogram.ConstantBuffers.Count; i++)
			{
				var constantBuffer = shaderSubprogram.ConstantBuffers[i];
				var variableChunk = variables[i];
				uint nameOffset = nameLookup[constantBuffer.Name];
				writer.Write(nameOffset);
				writer.Write(variableChunk.Count);
				writer.Write(variableOffset);
				writer.Write((uint)constantBuffer.Size);
				//Flags
				writer.Write((uint)ConstantBufferFlags.None);
				//ContantBufferType
				writer.Write((uint)ConstantBufferType.ConstantBuffer);
				variableOffset += variableChunk.Size;
			}
			foreach (var variableChunk in variables)
			{
				variableChunk.Write(writer);
			}
		}
	}
}
