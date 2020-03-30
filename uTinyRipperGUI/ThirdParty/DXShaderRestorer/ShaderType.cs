using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes.Shaders;

namespace DXShaderRestorer
{
	internal class ShaderType
	{
		public ShaderType(StructParameter structParameter, ShaderGpuProgramType programType)
		{
			List<ShaderTypeMember> members = new List<ShaderTypeMember>();
			members.AddRange(structParameter.VectorMembers.Select(p => new ShaderTypeMember(p, programType)));
			members.AddRange(structParameter.MatrixMembers.Select(p => new ShaderTypeMember(p, programType)));
			Members = members.OrderBy(v => v.Index).ToArray();
			ShaderVariableClass = ShaderVariableClass.Struct; //TODO: matrix colums or rows?
			ShaderVariableType = ShaderVariableType.Void;
			Rows = 0;
			Columns = 0;
			ElementCount = 0;
			MemberCount = (ushort)Members.Count();
			MemberOffset = 0;
			m_programType = programType;
		}

		public ShaderType(MatrixParameter matrixParam, ShaderGpuProgramType programType)
		{
			Members = Array.Empty<ShaderTypeMember>();
			ShaderVariableClass = ShaderVariableClass.MatrixColumns;
			ShaderVariableType = GetVariableType(matrixParam.Type);
			Rows = matrixParam.RowCount;
			Columns = matrixParam.ColumnCount;
			ElementCount = (ushort)matrixParam.ArraySize;
			MemberCount = 0;
			MemberOffset = 0;
			m_programType = programType;
		}

		public ShaderType(VectorParameter vectorParam, ShaderGpuProgramType programType)
		{
			Members = Array.Empty<ShaderTypeMember>();
			ShaderVariableClass = vectorParam.Dim > 1 ? ShaderVariableClass.Vector : ShaderVariableClass.Scalar;
			ShaderVariableType = GetVariableType(vectorParam.Type);
			Rows = 1;
			Columns = vectorParam.Dim;
			ElementCount = (ushort)vectorParam.ArraySize;
			MemberCount = 0;
			MemberOffset = 0;
			m_programType = programType;
		}

		private static ShaderVariableType GetVariableType(ShaderParamType paramType)
		{
			switch (paramType)
			{
				case ShaderParamType.Bool:
					return ShaderVariableType.Bool;
				case ShaderParamType.Float:
					return ShaderVariableType.Float;
				case ShaderParamType.Half:
					return ShaderVariableType.Float;
				case ShaderParamType.Int:
					return ShaderVariableType.Int;
				case ShaderParamType.Short:
					return ShaderVariableType.Int;
				case ShaderParamType.TypeCount:
					return ShaderVariableType.Int; //TODO
				case ShaderParamType.UInt:
					return ShaderVariableType.UInt; //TODO
				default:
					throw new Exception($"Unexpected param type {paramType}");
			}
		}

		/*public override bool Equals(object obj)
		{
			var shaderType = obj as ShaderType;
			if (shaderType == null) return false;
			return (ShaderVariableClass == shaderType.ShaderVariableClass &&
					ShaderVariableType == shaderType.ShaderVariableType &&
					Rows == shaderType.Rows &&
					Columns == shaderType.Columns &&
					ElementCount == shaderType.ElementCount &&
					MemberCount == shaderType.MemberCount &&
					MemberOffset == shaderType.MemberOffset);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = -1667745916;
				hashCode = hashCode * -1521134295 + ShaderVariableClass.GetHashCode();
				hashCode = hashCode * -1521134295 + ShaderVariableType.GetHashCode();
				hashCode = hashCode * -1521134295 + Rows.GetHashCode();
				hashCode = hashCode * -1521134295 + Columns.GetHashCode();
				hashCode = hashCode * -1521134295 + ElementCount.GetHashCode();
				hashCode = hashCode * -1521134295 + MemberCount.GetHashCode();
				hashCode = hashCode * -1521134295 + MemberOffset.GetHashCode();
				return hashCode;
			}
		}*/

		public uint Length()
		{
			uint variableSize = 4; //TODO: does this vary with ShaderVariableType? 
			return variableSize * Rows * Columns * ElementCount;
		}

		public uint Size()
		{
			int majorVersion = m_programType.GetMajorDXVersion();
			return majorVersion >= 5 ? (uint)36 : (uint)16;
		}

		public ShaderVariableClass ShaderVariableClass { get; }
		public ShaderVariableType ShaderVariableType { get; }
		public ushort Rows { get; }
		public ushort Columns { get; }
		public ushort ElementCount { get; }
		public ushort MemberCount { get; }
		public uint MemberOffset { get; set; }
		//SM 5.0 Variables
		public uint ParentTypeOffset { get; }
		public uint Unknown2 { get; }
		public uint Unknown4 { get; }
		public uint Unknown5 { get; }
		public uint ParentNameOffset { get; }
		public ShaderTypeMember[] Members { get; }

		private readonly ShaderGpuProgramType m_programType;
	}
}
