using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DXShaderExporter
{
	internal enum ShaderVariableFlags
	{
		None = 0,
		UserPacked = 1,
		Used = 2,
		InterfacePointer = 4,
		InterfaceParameter = 8
	}
}
