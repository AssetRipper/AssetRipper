using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class VariableHeader
	{
		public uint NameOffset { get; set; }
		public uint StartOffset { get; set; }
		public uint TypeOffset { get; set; }
		public Variable Variable { get; set; }
	}

	internal class VariableChunk
	{
		public VariableChunk(ref BufferBinding bufferBinding, int constantBufferIndex, uint variableOffset, ShaderGpuProgramType programType)
		{
			m_constantBufferIndex = constantBufferIndex;
			m_programType = programType;
			majorVersion = programType.GetMajorDXVersion();
			m_variables = new Variable[] { Variable.CreateResourceBindVariable(programType) };
			BuildVariableHeaders(variableOffset);

		}
		public VariableChunk(ref ConstantBuffer constantBuffer, int constantBufferIndex, uint variableOffset, ShaderGpuProgramType programType)
		{
			m_constantBufferIndex = constantBufferIndex;
			m_programType = programType;
			majorVersion = programType.GetMajorDXVersion();
			m_variables = BuildVariables(ref constantBuffer);
			BuildVariableHeaders(variableOffset);
		}
		private void BuildVariableHeaders(uint variableOffset)
		{
			const int memberSize = 12;
			uint variableSize = majorVersion >= 5 ? (uint)40 : (uint)24;
			uint variableCount = (uint)m_variables.Length;
			uint dataOffset = variableOffset + variableCount * variableSize;
			foreach (Variable variable in m_variables)
			{
				m_variableNameLookup[variable.Name] = dataOffset;
				VariableHeader header = new VariableHeader();
				header.NameOffset = dataOffset;
				dataOffset += (uint)variable.Name.Length + 1;
				header.StartOffset = (uint)variable.Index;
				header.Variable = variable;

				m_typeLookup[variable.ShaderType] = dataOffset;
				dataOffset += variable.ShaderType.Size();

				variable.ShaderType.MemberOffset = variable.ShaderType.Members.Length > 0 ? dataOffset : 0;
				dataOffset += (uint)variable.ShaderType.Members.Length * memberSize;

				foreach (ShaderTypeMember member in variable.ShaderType.Members)
				{
					m_variableNameLookup[member.Name] = dataOffset;
					dataOffset += (uint)member.Name.Length + 1;

					m_typeLookup[member.ShaderType] = dataOffset;
					dataOffset += member.ShaderType.Size();
				}

				m_variableHeaders.Add(header);
			}
			Size = dataOffset - variableOffset;
		}
		internal void Write(EndianWriter writer)
		{
			foreach (VariableHeader header in m_variableHeaders)
			{
				WriteVariableHeader(writer, header);
			}
			foreach (Variable variable in m_variables)
			{
				writer.WriteStringZeroTerm(variable.Name);
				WriteShaderType(writer, variable.ShaderType);

				foreach (ShaderTypeMember member in variable.ShaderType.Members)
				{
					uint nameOffset = m_variableNameLookup[member.Name];
					writer.Write(nameOffset);
					uint memberOffset = m_typeLookup[member.ShaderType];
					writer.Write(memberOffset);
					writer.Write(member.Index);
				}
				foreach (ShaderTypeMember member in variable.ShaderType.Members)
				{
					writer.WriteStringZeroTerm(member.Name);
					WriteShaderType(writer, member.ShaderType);
				}
			}
		}

		private Variable[] BuildVariables(ref ConstantBuffer constantBuffer)
		{
			List<Variable> variables = new List<Variable>();
			foreach (MatrixParameter param in constantBuffer.MatrixParams)
			{
				variables.Add(new Variable(param, m_programType));
			}
			foreach (VectorParameter param in constantBuffer.VectorParams)
			{
				variables.Add(new Variable(param, m_programType));
			}
			foreach (StructParameter param in constantBuffer.StructParams)
			{
				variables.Add(new Variable(param, m_programType));
			}
			variables = variables.OrderBy(v => v.Index).ToList();
			//Dummy variables prevents errors in rare edge cases but produces more verbose output
			bool useDummyVariables = true;
			if (useDummyVariables)
			{
				List<Variable> allVariables = new List<Variable>();
				uint currentSize = 0;
				for (int i = 0; i < variables.Count; i++)
				{
					Variable variable = variables[i];
					if (variable.Index - currentSize >= 16)
					{
						long sizeToAdd = variable.Index - currentSize;
						int id1 = m_constantBufferIndex;
						int id2 = allVariables.Count;
						allVariables.Add(Variable.CreateDummyVariable($"unused_{id1}_{id2}",
								(int)currentSize, (int)sizeToAdd, m_programType));
					}
					allVariables.Add(variable);
					currentSize = (uint)variable.Index + variable.Length;
				}
				if (constantBuffer.Size - currentSize >= 16)
				{
					long sizeToAdd = constantBuffer.Size - currentSize;
					int id1 = m_constantBufferIndex;
					int id2 = allVariables.Count;
					allVariables.Add(Variable.CreateDummyVariable($"unused_{id1}_{id2}", 
						(int)currentSize, (int)sizeToAdd, m_programType));
				}
				variables = allVariables;
			}
			else
			{
				for (int i = 0; i < variables.Count; i++)
				{
					if (i < variables.Count - 1)
					{
						variables[i].Length = (uint)variables[i + 1].Index - (uint)variables[i].Index;
					}
					else
					{
						variables[i].Length = (uint)constantBuffer.Size - (uint)variables[i].Index;
					}
				}
			}
			return variables.ToArray();
		}

		private void WriteVariableHeader(EndianWriter writer, VariableHeader header)
		{
			//name offset
			writer.Write(header.NameOffset);
			//startOffset
			writer.Write(header.StartOffset);
			//Size
			writer.Write(header.Variable.Length);
			//flags
			writer.Write((uint)ShaderVariableFlags.Used); //Unity only packs used variables as far as I can tell

			uint typeOffset = m_typeLookup[header.Variable.ShaderType];
			//type offset
			writer.Write(typeOffset);
			//default value offset
			writer.Write((uint)0); //Not used
			if (majorVersion >= 5)
			{
				//StartTexture
				writer.Write((uint)0);
				//TextureSize
				writer.Write((uint)0);
				//StartSampler
				writer.Write((uint)0);
				//SamplerSize
				writer.Write((uint)0);
			}
		}

		private void WriteShaderType(EndianWriter writer, ShaderType shaderType)
		{
			writer.Write((ushort)shaderType.ShaderVariableClass);
			writer.Write((ushort)shaderType.ShaderVariableType);
			writer.Write(shaderType.Rows);
			writer.Write(shaderType.Columns);
			writer.Write(shaderType.ElementCount);
			writer.Write(shaderType.MemberCount);
			writer.Write(shaderType.MemberOffset);
			if (majorVersion >= 5)
			{
				if (shaderType.ParentTypeOffset != 0 ||
					shaderType.Unknown2 != 0 ||
					shaderType.Unknown5 != 0 ||
					shaderType.ParentNameOffset != 0)
				{
					throw new Exception("Shader variable type has invalid value");
				}
				writer.Write(shaderType.ParentTypeOffset);
				writer.Write(shaderType.Unknown2);
				writer.Write(shaderType.Unknown4);
				writer.Write(shaderType.Unknown5);
				writer.Write(shaderType.ParentNameOffset);
			}
		}

		internal uint Count => (uint)m_variables.Length;
		internal uint Size { get; private set; }

		private readonly List<VariableHeader> m_variableHeaders = new List<VariableHeader>();
		private readonly Dictionary<string, uint> m_variableNameLookup = new Dictionary<string, uint>();
		private readonly Dictionary<ShaderType, uint> m_typeLookup = new Dictionary<ShaderType, uint>();
		private readonly Variable[] m_variables;

		private readonly int m_constantBufferIndex;
		private readonly ShaderGpuProgramType m_programType;
		private readonly int majorVersion;
	}
}
