using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// This processor removes nullable annotations from all locations.
/// </summary>
public sealed class NullableRemovalProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		manager.ClearStreamCache();
		foreach (TypeDefinition type in manager.GetAllTypes())
		{
			RemoveNullableAttributes(type);
			RemoveNullableAttributes(type.GenericParameters);
			RemoveNullableAttributes(type.Fields);
			RemoveNullableAttributes(type.Properties);
			RemoveNullableAttributes(type.Events);
			RemoveNullableAttributes(type.Methods);

			foreach (MethodDefinition method in type.Methods)
			{
				RemoveNullableAttributes(method.ParameterDefinitions);
				RemoveNullableAttributes(method.GenericParameters);
			}
		}
	}

	private static void RemoveNullableAttributes<T>(IEnumerable<T> attributeProviders) where T : IHasCustomAttribute
	{
		foreach (T attributeProvider in attributeProviders)
		{
			RemoveNullableAttributes(attributeProvider);
		}
	}

	private static void RemoveNullableAttributes(IHasCustomAttribute attributeProvider)
	{
		for (int i = attributeProvider.CustomAttributes.Count - 1; i >= 0; i--)
		{
			CustomAttribute attribute = attributeProvider.CustomAttributes[i];
			if (attribute.Constructor?.DeclaringType is { Namespace.Value: "System.Runtime.CompilerServices", Name.Value: "NullableAttribute" or "NullableContextAttribute" })
			{
				attributeProvider.CustomAttributes.RemoveAt(i);
			}
		}
	}
}
