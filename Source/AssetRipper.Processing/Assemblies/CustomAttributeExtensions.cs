using AsmResolver.DotNet;

namespace AssetRipper.Processing.Assemblies;

internal static class CustomAttributeExtensions
{
	public static bool IsType(this CustomAttribute attribute, string? @namespace, string? name)
	{
		ITypeDefOrRef? type = attribute.Constructor?.DeclaringType;
		return type is not null && type.Namespace == @namespace && type.Name == name;
	}

	/// <summary>
	/// System.Runtime.CompilerServices.CompilerGeneratedAttribute
	/// </summary>
	public static bool IsCompilerGeneratedAttribute(this CustomAttribute attribute)
	{
		return attribute.IsType("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
	}

	/// <summary>
	/// UnityEngine.SerializeField
	/// </summary>
	public static bool IsSerializeField(this CustomAttribute attribute)
	{
		return attribute.Constructor?.DeclaringType is { Namespace.Value: "UnityEngine", Name.Value: "SerializeField" };
	}

	/// <summary>
	/// System.NonSerializedAttribute
	/// </summary>
	public static bool IsNonSerializedAttribute(this CustomAttribute attribute)
	{
		return attribute.Constructor?.DeclaringType is { Namespace.Value: "System", Name.Value: "NonSerializedAttribute" };
	}
}
