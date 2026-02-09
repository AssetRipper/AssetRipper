using AssetRipper.AssemblyDumper.Methods;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class DebuggerExtensions
{
	private static IMethodDefOrRef? debuggerBrowsableConstructor;

	private static IMethodDefOrRef DebuggerBrowsableConstructor
	{
		get
		{
			debuggerBrowsableConstructor ??= SharedState.Instance.Importer.ImportConstructor<DebuggerBrowsableAttribute>(1);
			return debuggerBrowsableConstructor;
		}
	}

	public static CustomAttribute AddDebuggerBrowsableNeverAttribute(this FieldDefinition field)
	{
		return field.AddDebuggerBrowsableNeverAttributeInternal();
	}

	public static CustomAttribute AddDebuggerBrowsableNeverAttribute(this PropertyDefinition field)
	{
		return field.AddDebuggerBrowsableNeverAttributeInternal();
	}

	private static CustomAttribute AddDebuggerBrowsableNeverAttributeInternal(this IHasCustomAttribute hasCustomAttribute)
	{
		return hasCustomAttribute.AddCustomAttribute(DebuggerBrowsableConstructor,
			(SharedState.Instance.Importer.Int32, (int)DebuggerBrowsableState.Never));
	}

	public static CustomAttribute AddDebuggerDisplayAttribute(this TypeDefinition type, string value)
	{
		return type.AddCustomAttribute(SharedState.Instance.Importer.ImportConstructor<DebuggerDisplayAttribute>(1),
			(SharedState.Instance.Importer.String, value));
	}
}
