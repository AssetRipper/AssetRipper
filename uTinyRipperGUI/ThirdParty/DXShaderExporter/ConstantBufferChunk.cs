using System.Collections.Generic;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class ConstantBufferChunk
	{
		public ConstantBufferChunk(ShaderSubProgram shaderSubprogram, uint contantBufferOffset, Dictionary<string, uint> nameLookup)
		{
			m_shaderSubprogram = shaderSubprogram;
			m_contantBufferOffset = contantBufferOffset;
			m_nameLookup = nameLookup;

			uint headerSize = (uint)shaderSubprogram.ConstantBuffers.Length * 24;
			uint variableOffset = contantBufferOffset + headerSize;
			int constantBufferIndex = 0;
			List<VariableChunk> variables = new List<VariableChunk>();
			foreach (ConstantBuffer constantBuffer in shaderSubprogram.ConstantBuffers)
			{
				VariableChunk variableChunk = new VariableChunk(constantBuffer, constantBufferIndex++, variableOffset, shaderSubprogram.ProgramType);
				variables.Add(variableChunk);
				variableOffset += variableChunk.Size;
			}
			m_variables = variables;
			Size = variableOffset - contantBufferOffset;
		}

		public void Write(EndianWriter writer)
		{
			uint headerSize = (uint)m_shaderSubprogram.ConstantBuffers.Length * 24;
			uint variableOffset = m_contantBufferOffset + headerSize;
			for (int i = 0; i < m_shaderSubprogram.ConstantBuffers.Length; i++)
			{
				ConstantBuffer constantBuffer = m_shaderSubprogram.ConstantBuffers[i];
				VariableChunk variableChunk = m_variables[i];
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
			foreach (VariableChunk variableChunk in m_variables)
			{
				variableChunk.Write(writer);
			}
		}

		internal uint Count => (uint)m_shaderSubprogram.ConstantBuffers.Length;

		internal uint Size { get; }

		private readonly IReadOnlyList<VariableChunk> m_variables;
		private readonly Dictionary<string, uint> m_nameLookup;
		private readonly uint m_contantBufferOffset;

		private ShaderSubProgram m_shaderSubprogram;
	}
}
