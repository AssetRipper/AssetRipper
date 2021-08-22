using System;

namespace ShaderTextRestorer
{
	[Flags]
	internal enum ShaderVariableFlags
	{
		None				= 0,
		UserPacked			= 1,
		Used				= 2,
		InterfacePointer	= 4,
		InterfaceParameter	= 8,
	}
}
