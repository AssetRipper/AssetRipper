using AssetRipper.AssemblyDumper.Methods;
using System.Security;

namespace AssetRipper.AssemblyDumper.Attributes;

public static class ModuleAttributeCreator
{
	/// <summary>
	/// <see cref="UnverifiableCodeAttribute"/>
	/// </summary>
	public static CustomAttribute AddUnverifiableCodeAttribute(this AssemblyBuilder builder)
	{
		IMethodDefOrRef constructor = builder.Importer.ImportDefaultConstructor<UnverifiableCodeAttribute>();
		return builder.Module.AddCustomAttribute(constructor);
	}
}
