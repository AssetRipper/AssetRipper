using AssetRipper.AssemblyDumper.Methods;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;

namespace AssetRipper.AssemblyDumper.Attributes;

public static class AssemblyAttributeCreator
{
	/// <summary>
	/// <see cref="AssemblyCompanyAttribute"/>
	/// </summary>
	public static CustomAttribute AddAssemblyCompanyAttribute(this AssemblyBuilder builder, string parameter)
	{
		return builder.AddSingleStringAttribute<AssemblyCompanyAttribute>(parameter);
	}

	/// <summary>
	/// <see cref="AssemblyConfigurationAttribute"/>
	/// </summary>
	public static CustomAttribute AddAssemblyConfigurationAttribute(this AssemblyBuilder builder, string parameter)
	{
		return builder.AddSingleStringAttribute<AssemblyConfigurationAttribute>(parameter);
	}

	/// <summary>
	/// <see cref="AssemblyFileVersionAttribute"/>
	/// </summary>
	public static CustomAttribute AddAssemblyFileVersionAttribute(this AssemblyBuilder builder, string parameter)
	{
		return builder.AddSingleStringAttribute<AssemblyFileVersionAttribute>(parameter);
	}

	/// <summary>
	/// <see cref="AssemblyInformationalVersionAttribute"/>
	/// </summary>
	public static CustomAttribute AddAssemblyInformationalVersionAttribute(this AssemblyBuilder builder, string parameter)
	{
		return builder.AddSingleStringAttribute<AssemblyInformationalVersionAttribute>(parameter);
	}

	/// <summary>
	/// <see cref="AssemblyProductAttribute"/>
	/// </summary>
	public static CustomAttribute AddAssemblyProductAttribute(this AssemblyBuilder builder, string parameter)
	{
		return builder.AddSingleStringAttribute<AssemblyProductAttribute>(parameter);
	}

	/// <summary>
	/// <see cref="AssemblyTitleAttribute"/>
	/// </summary>
	public static CustomAttribute AddAssemblyTitleAttribute(this AssemblyBuilder builder, string parameter)
	{
		return builder.AddSingleStringAttribute<AssemblyTitleAttribute>(parameter);
	}

	/// <summary>
	/// <see cref="InternalsVisibleToAttribute"/>
	/// </summary>
	public static CustomAttribute AddInternalsVisibleToAttribute(this AssemblyBuilder builder, AssemblyDescriptor targetAssembly)
	{
		return builder.AddSingleStringAttribute<InternalsVisibleToAttribute>(targetAssembly.Name ?? throw new ArgumentException(nameof(targetAssembly)));
	}

	/// <summary>
	/// <see cref="RuntimeCompatibilityAttribute"/>
	/// </summary>
	public static CustomAttribute AddRuntimeCompatibilityAttribute(this AssemblyBuilder builder, bool wrapNonExceptionThrows = true)
	{
		IMethodDefOrRef constructor = builder.Importer.ImportDefaultConstructor<RuntimeCompatibilityAttribute>();
		CustomAttribute attribute = builder.Assembly.AddCustomAttribute(constructor);
		attribute.AddNamedArgument(
			builder.Importer.Boolean,
			nameof(RuntimeCompatibilityAttribute.WrapNonExceptionThrows),
			builder.Importer.Boolean,
			wrapNonExceptionThrows,
			CustomAttributeArgumentMemberType.Property);
		return attribute;
	}

	/// <summary>
	/// <see cref="TargetFrameworkAttribute"/>
	/// </summary>
	public static CustomAttribute AddTargetFrameworkAttribute(this AssemblyBuilder builder)
	{
		return builder.AddSingleStringAttribute<TargetFrameworkAttribute>(builder.Module.OriginalTargetRuntime.ToString());
	}

	/// <summary>
	/// <see cref="TargetFrameworkAttribute"/>
	/// </summary>
	public static CustomAttribute AddTargetFrameworkAttribute(this AssemblyBuilder builder, string displayName)
	{
		CustomAttribute attribute = builder.AddTargetFrameworkAttribute();
		attribute.AddNamedArgument(builder.Importer.String, nameof(TargetFrameworkAttribute.FrameworkDisplayName), builder.Importer.String, displayName, CustomAttributeArgumentMemberType.Property);
		return attribute;
	}

	private static CustomAttribute AddSingleStringAttribute<T>(this AssemblyBuilder builder, string parameterValue) where T : Attribute
	{
		IMethodDefOrRef constructor = builder.Importer.ImportConstructor<T>(1);
		return builder.Assembly.AddCustomAttribute(constructor, (builder.Importer.String, parameterValue));
	}
}
