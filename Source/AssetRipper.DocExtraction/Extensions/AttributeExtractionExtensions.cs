using AsmResolver;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;

namespace AssetRipper.DocExtraction.Extensions;

internal static class AttributeExtractionExtensions
{

	/// <summary>
	/// For <see cref="DocumentationBase.NativeName"/> on <see cref="TypeDocumentation"/>
	/// </summary>
	public static string? GetNativeClass(this ITypeDefOrRef type)
	{
		return type.TryGetCustomAttribute("UnityEngine", "NativeClassAttribute", out CustomAttribute? attribute)
			? GetFirstStringArgument(attribute)
			: null;
	}

	public static string? GetNativeName(this IHasCustomAttribute hasCustomAttributeObject)
	{
		return hasCustomAttributeObject.TryGetCustomAttribute("UnityEngine.Bindings", "NativeNameAttribute", out CustomAttribute? attribute)
			? GetFirstStringArgument(attribute)
			: null;
	}

	/// <summary>
	/// For <see cref="DocumentationBase.NativeName"/> on <see cref="DataMemberDocumentation"/> when <see cref="GetNativeName(IHasCustomAttribute)"/> fails.
	/// </summary>
	public static string? GetNativeProperty(this IHasCustomAttribute hasCustomAttributeObject)
	{
		return hasCustomAttributeObject.TryGetCustomAttribute("UnityEngine.Bindings", "NativePropertyAttribute", out CustomAttribute? attribute)
			? GetFirstStringArgument(attribute)
			: null;
	}

	public static string? GetObsoleteMessage(this IHasCustomAttribute hasCustomAttributeObject)
	{
		if (hasCustomAttributeObject.TryGetCustomAttribute("System", nameof(ObsoleteAttribute), out CustomAttribute? attribute))
		{
			string? stringArgument = GetFirstStringArgument(attribute);
			return string.IsNullOrEmpty(stringArgument) ? "Obsolete" : stringArgument;
		}
		return null;
	}

	private static string? GetFirstStringArgument(CustomAttribute attribute)
	{
		return attribute.Signature?.FixedArguments
			.FirstOrDefault(arg => (arg.ArgumentType as CorLibTypeSignature)?.ElementType == ElementType.String)?
			.Elements
			.Select(e => e as Utf8String)
			.Single()?.Value;
	}
}