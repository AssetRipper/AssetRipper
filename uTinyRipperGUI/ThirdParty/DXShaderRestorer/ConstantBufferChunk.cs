using System.Collections.Generic;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class ConstantBufferChunk
	{
		public ConstantBufferChunk(Version version, ref ShaderSubProgram shaderSubprogram, uint contantBufferOffset, Dictionary<string, uint> nameLookup)
		{
			m_shaderSubprogram = shaderSubprogram;
			m_contantBufferOffset = contantBufferOffset;
			m_nameLookup = nameLookup;

			ShaderGpuProgramType programType = shaderSubprogram.GetProgramType(version);
			uint headerSize = (uint)Count * 24;
			uint variableOffset = contantBufferOffset + headerSize;
			int constantBufferIndex = 0;
			List<VariableChunk> variables = new List<VariableChunk>();
			for (int i = 0; i < shaderSubprogram.ConstantBuffers.Length; i++)
			{
				ref ConstantBuffer constantBuffer = ref shaderSubprogram.ConstantBuffers[i];
				VariableChunk variableChunk = new VariableChunk(ref constantBuffer, constantBufferIndex++, variableOffset, programType);
				variables.Add(variableChunk);
				variableOffset += variableChunk.Size;
			}
			for (int i = 0; i < shaderSubprogram.BufferParameters.Length; i++)
			{
				ref BufferBinding bufferBindings = ref shaderSubprogram.BufferParameters[i];
				VariableChunk variableChunk = new VariableChunk(ref bufferBindings, constantBufferIndex++, variableOffset, programType);
				variables.Add(variableChunk);
				variableOffset += variableChunk.Size;
			}
			m_variables = variables;
			Size = variableOffset - contantBufferOffset;
		}

		public void Write(EndianWriter writer)
		{
			uint headerSize = (uint)Count * 24;
			uint variableOffset = m_contantBufferOffset + headerSize;
			int variableIndex = 0;
			for (int i = 0; i < m_shaderSubprogram.ConstantBuffers.Length; i++, variableIndex++)
			{
				ConstantBuffer constantBuffer = m_shaderSubprogram.ConstantBuffers[i];
				VariableChunk variableChunk = m_variables[variableIndex];
				uint nameOffset = m_nameLookup[constantBuffer.Name];
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
			for (int i = 0; i < m_shaderSubprogram.BufferParameters.Length; i++, variableIndex++)
			{
				BufferBinding bufferParamater = m_shaderSubprogram.BufferParameters[i];
				VariableChunk variableChunk = m_variables[variableIndex];
				uint nameOffset = m_nameLookup[bufferParamater.Name];
				writer.Write(nameOffset);
				writer.Write(variableChunk.Count);
				writer.Write(variableOffset);
				//Size
				writer.Write((uint)4); 
				//Flags
				writer.Write((uint)ConstantBufferFlags.None);
				//ContantBufferType
				writer.Write((uint)ConstantBufferType.ResourceBindInformation);
				variableOffset += variableChunk.Size;
			}
			foreach (VariableChunk variableChunk in m_variables)
			{
				variableChunk.Write(writer);
			}
		}

		internal uint Count => (uint)(m_shaderSubprogram.ConstantBuffers.Length + m_shaderSubprogram.BufferParameters.Length);

		internal uint Size { get; }

		private readonly IReadOnlyList<VariableChunk> m_variables;
		private readonly Dictionary<string, uint> m_nameLookup;
		private readonly uint m_contantBufferOffset;

		private ShaderSubProgram m_shaderSubprogram;
	}
}
