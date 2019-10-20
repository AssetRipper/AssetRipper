using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uTinyRipper.Classes.Shaders;

namespace DXShaderExporter
{
	internal class ShaderType
	{
		public ShaderVariableClass ShaderVariableClass;
		public ShaderVariableType ShaderVariableType;
		public ushort Rows;
		public ushort Columns;
		public ushort ElementCount;
		public ushort MemberCount;
		public uint MemberOffset;
		//SM 5.0 Variables
		public uint parentTypeOffset = 0;
		public uint unknown2 = 0;
		public uint unknown4 = 0;
		public uint unknown5 = 0;
		public uint parentNameOffset;
		public List<ShaderTypeMember> members = new List<ShaderTypeMember>();
		ShaderGpuProgramType programType;
		public ShaderType(StructParameter structParameter, ShaderGpuProgramType programType)
		{
			members.AddRange(structParameter.VectorMembers.Select(p => new ShaderTypeMember(p, programType)));
			members.AddRange(structParameter.MatrixMembers.Select(p => new ShaderTypeMember(p, programType)));
			members = members
				.OrderBy(v => v.Index)
				.ToList();
			ShaderVariableClass = ShaderVariableClass.Struct; //TODO: matrix colums or rows?
			ShaderVariableType = ShaderVariableType.Void;
			Rows = 0;
			Columns = 0;
			ElementCount = 0;
			MemberCount = (ushort)members.Count();
			MemberOffset = 0;
			this.programType = programType;
		}
		public ShaderType(MatrixParameter matrixParam, ShaderGpuProgramType programType)
		{
			ShaderVariableClass = ShaderVariableClass.MatrixColumns;
			ShaderVariableType = GetVariableType(matrixParam.Type);
			Rows = matrixParam.RowCount;
			Columns = matrixParam.ColumnCount;
			ElementCount = (ushort)matrixParam.ArraySize;
			MemberCount = 0;
			MemberOffset = 0;
			this.programType = programType;
		}
		public ShaderType(VectorParameter vectorParam, ShaderGpuProgramType programType)
		{
			ShaderVariableClass = vectorParam.Dim > 1 ?
				ShaderVariableClass.Vector :
				ShaderVariableClass.Scalar;
			ShaderVariableType = GetVariableType(vectorParam.Type);
			Rows = 1;
			Columns = vectorParam.Dim;
			ElementCount = (ushort)vectorParam.ArraySize;
			MemberCount = 0;
			MemberOffset = 0;
			this.programType = programType;
		}
		static ShaderVariableType GetVariableType(ShaderParamType paramType)
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
			var majorVersion = DXShaderObjectExporter.GetMajorVersion(programType);
			return majorVersion >= 5 ? (uint)36 : (uint)16;
		}
	}
}
