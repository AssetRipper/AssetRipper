using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.Import.Structure.Assembly.Managers;
using System.Text.RegularExpressions;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Removes the public key from the InternalsVisibleToAttribute.
/// </summary>
/// <remarks>
/// <see href="https://github.com/AssetRipper/AssetRipper/issues/1736"/>
/// </remarks>
public sealed partial class InternalsVisibileToPublicKeyRemover : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		foreach (AssemblyDefinition assembly in manager.GetAssemblies())
		{
			foreach (CustomAttribute customAttribute in assembly.CustomAttributes)
			{
				if (!customAttribute.IsType("System.Runtime.CompilerServices", "InternalsVisibleToAttribute"))
				{
					// Not the attribute we are looking for
				}
				else if (!TryGetSingleFixedArgument(customAttribute, out CustomAttributeArgument? argument))
				{
					// Invalid attribute signature
				}
				else if (!IsStringArgument(argument, out string? value))
				{
					// Invalid argument type
				}
				else if (!TryMatchRegex(value, out string? targetAssembly))
				{
					// No public key present, so we don't need to do anything
				}
				else
				{
					argument.Elements[0] = targetAssembly;
				}
			}
		}
	}

	private static bool IsStringArgument(CustomAttributeArgument argument, [NotNullWhen(true)] out string? value)
	{
		if (argument.ArgumentType is CorLibTypeSignature { ElementType: ElementType.String } && argument.Elements.Count == 1)
		{
			if (argument.Element is string str)
			{
				value = str;
				return true;
			}
			else if (argument.Element is AsmResolver.Utf8String str2)
			{
				value = str2;
				return true;
			}
		}
		value = null;
		return false;
	}

	private static bool TryGetSingleFixedArgument(CustomAttribute customAttribute, [NotNullWhen(true)] out CustomAttributeArgument? argument)
	{
		if (customAttribute.Signature is null or { FixedArguments.Count: not 1 } or { NamedArguments.Count: not 0 })
		{
			argument = null;
			return false;
		}
		argument = customAttribute.Signature.FixedArguments[0];
		return true;
	}

	private static bool TryMatchRegex(string value, [NotNullWhen(true)] out string? targetAssembly)
	{
		Match match = PublicKeyRegex.Match(value);
		if (match.Success)
		{
			targetAssembly = match.Groups[1].Value;
			return true;
		}
		else
		{
			targetAssembly = null;
			return false;
		}
	}

	[GeneratedRegex(@"^([\w\.]+), PublicKey=[a-fA-F0-9]+$")]
	private static partial Regex PublicKeyRegex { get; }
}
