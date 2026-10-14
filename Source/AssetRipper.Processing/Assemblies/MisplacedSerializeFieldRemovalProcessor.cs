using AsmResolver.DotNet;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// This processor removes <c>[SerializeField]</c> from everything except fields.
/// </summary>
/// <remarks>
/// Before Unity 6.3, the attribute had no usage restriction, so it compiled on types, properties, and methods.
/// Unity only reads it on fields, so elsewhere it has no effect, but newer versions of Unity reject it with error CS0592.
/// </remarks>
public sealed class MisplacedSerializeFieldRemovalProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		manager.ClearStreamCache();
		foreach (TypeDefinition type in manager.GetAllTypes())
		{
			RemoveSerializeFieldAttributes(type);
			RemoveSerializeFieldAttributes(type.Properties);
			RemoveSerializeFieldAttributes(type.Events);
			RemoveSerializeFieldAttributes(type.Methods);
		}
	}

	private static void RemoveSerializeFieldAttributes<T>(IEnumerable<T> attributeProviders) where T : IHasCustomAttribute
	{
		foreach (T attributeProvider in attributeProviders)
		{
			RemoveSerializeFieldAttributes(attributeProvider);
		}
	}

	private static void RemoveSerializeFieldAttributes(IHasCustomAttribute attributeProvider)
	{
		for (int i = attributeProvider.CustomAttributes.Count - 1; i >= 0; i--)
		{
			CustomAttribute attribute = attributeProvider.CustomAttributes[i];
			if (attribute.Constructor?.DeclaringType is { Namespace.Value: "UnityEngine", Name.Value: "SerializeField" })
			{
				attributeProvider.CustomAttributes.RemoveAt(i);
			}
		}
	}
}
