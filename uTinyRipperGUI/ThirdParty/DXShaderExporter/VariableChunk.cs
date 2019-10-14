using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uTinyRipper;
using uTinyRipper.Classes.Shaders;

namespace DXShaderExporter
{
	internal class VariableHeader
	{
		internal uint nameOffset;
		internal uint startOffset;
		internal uint typeOffset;
		internal Variable variable;
	}
	internal class VariableChunk
	{
		private ConstantBuffer constantBuffer;
		private int constantBufferIndex;
		private ShaderGpuProgramType programType;
		List<Variable> variables;
		List<VariableHeader> variableHeaders = new List<VariableHeader>();
		Dictionary<string, uint> variableNameLookup = new Dictionary<string, uint>();
		Dictionary<ShaderType, uint> typeLookup = new Dictionary<ShaderType, uint>();
		int majorVersion;
		uint size;
		internal uint Size => size;
		internal uint Count => (uint)variables.Count;

		public VariableChunk(ConstantBuffer constantBuffer, int constantBufferIndex, uint variableOffset, ShaderGpuProgramType programType)
		{
			const int memberSize = 12;
			this.constantBuffer = constantBuffer;
			this.constantBufferIndex = constantBufferIndex;
			this.programType = programType;
			this.variables = BuildVariables(constantBuffer);

			majorVersion = DXShaderObjectExporter.GetMajorVersion(programType);
			uint variableSize = majorVersion >= 5 ? (uint)40 : (uint)24;
			uint variableCount = (uint)variables.Count;
			uint dataOffset = variableOffset + variableCount * variableSize;
			foreach (var variable in variables)
			{
				variableNameLookup[variable.Name] = dataOffset;
				var header = new VariableHeader();
				header.nameOffset = dataOffset;
				dataOffset += (uint)variable.Name.Length + 1;
				header.startOffset = (uint)variable.Index;
				header.variable = variable;

				typeLookup[variable.ShaderType] = dataOffset;
				dataOffset += variable.ShaderType.Size();

				variable.ShaderType.MemberOffset = variable.ShaderType.members.Count > 0 ? dataOffset : 0;
				dataOffset += (uint)variable.ShaderType.members.Count * memberSize;

				foreach (var member in variable.ShaderType.members)
				{
					variableNameLookup[member.Name] = dataOffset;
					dataOffset += (uint)member.Name.Length + 1;

					typeLookup[member.ShaderType] = dataOffset;
					dataOffset += member.ShaderType.Size();
				}

				variableHeaders.Add(header);
			}
			size = dataOffset - variableOffset;
		}
		List<Variable> BuildVariables(ConstantBuffer constantBuffer)
		{
			List<Variable> variables = new List<Variable>();
			foreach (var param in constantBuffer.MatrixParams) variables.Add(new Variable(param, programType));
			foreach (var param in constantBuffer.VectorParams) variables.Add(new Variable(param, programType));
			foreach (var param in constantBuffer.StructParams) variables.Add(new Variable(param, programType));
			variables = variables.OrderBy(v => v.Index).ToList();
			//Dummy variables prevents errors in rare edge cases but produces more verbose output
			bool useDummyVariables = true;
			if(useDummyVariables) {
				var allVariables = new List<Variable>();
				uint currentSize = 0;
				for (int i = 0; i < variables.Count; i++)
				{
					var variable = variables[i];
					if (variable.Index > currentSize)
					{
						var sizeToAdd = variable.Index - currentSize;
						var id1 = constantBufferIndex;
						var id2 = allVariables.Count;
						allVariables.Add(new Variable($"unused_{id1}_{id2}", (int)currentSize, (int)sizeToAdd, programType));
					}
					allVariables.Add(variable);
					currentSize = (uint)variable.Index + variable.ShaderType.Size();
				}
				if (currentSize < constantBuffer.Size)
				{
					var sizeToAdd = constantBuffer.Size - currentSize;
					var id1 = constantBufferIndex;
					var id2 = allVariables.Count;
					allVariables.Add(new Variable($"unused_{id1}_{id2}", (int)currentSize, (int)sizeToAdd, programType));
				}
				variables = allVariables;
			} else {
				for (int i = 0; i < variables.Count; i++)
				{
					if (i < variables.Count - 1)
					{
						variables[i].Length = (uint)variables[i + 1].Index - (uint)variables[i].Index;
					} else
					{
						variables[i].Length = (uint)constantBuffer.Size - (uint)variables[i].Index;
					}
				}
			}
			return variables;
		}
		internal void Write(EndianWriter writer)
		{
			foreach(var header in variableHeaders)
			{
				WriteVariableHeader(writer, header);
			}
			foreach (var variable in variables)
			{
				writer.WriteStringZeroTerm(variable.Name);
				WriteShaderType(writer, variable.ShaderType);

				foreach (var member in variable.ShaderType.members)
				{
					var nameOffset = variableNameLookup[member.Name];
					writer.Write(nameOffset);
					var memberOffset = typeLookup[member.ShaderType];
					writer.Write(memberOffset);
					writer.Write(member.Index);
				}
				foreach (var member in variable.ShaderType.members)
				{
					writer.WriteStringZeroTerm(member.Name);
					WriteShaderType(writer, member.ShaderType);
				}
			}
		}
		private void WriteVariableHeader(EndianWriter writer, VariableHeader header)
		{
			//name offset
			writer.Write(header.nameOffset);
			//startOffset
			writer.Write(header.startOffset);
			//Size
			writer.Write(header.variable.Length);
			//flags
			writer.Write((uint)ShaderVariableFlags.Used); //Unity only packs used variables as far as I can tell

			var typeOffset = typeLookup[header.variable.ShaderType];
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
				if (shaderType.parentTypeOffset != 0 ||
					shaderType.unknown2 != 0 ||
					shaderType.unknown5 != 0 ||
					shaderType.parentNameOffset != 0)
				{
					throw new Exception("Shader variable type has invalid value");
				}
				writer.Write(shaderType.parentTypeOffset);
				writer.Write(shaderType.unknown2);
				writer.Write(shaderType.unknown4);
				writer.Write(shaderType.unknown5);
				writer.Write(shaderType.parentNameOffset);
			}
		}
	}
}
