using AssetRipper.AssemblyDumper.Types;
using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass045_AddMarkerInterfaces
{
	public static void DoPass()
	{
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			if (group.UniformlyNamed)
			{
				group.Interface.AddInterfaceImplementation(GetOrAddMarkerInterface(group.Name));
			}
			else
			{
				foreach (GeneratedClassInstance instance in group.Instances)
				{
					instance.Type.AddInterfaceImplementation(GetOrAddMarkerInterface(instance.Name));
				}
			}
		}
	}

	private static string GetInterfaceName(string className) => $"I{className}Marker";

	private static TypeDefinition GetOrAddMarkerInterface(string className)
	{
		string interfaceName = GetInterfaceName(className);
		if (!SharedState.Instance.MarkerInterfaces.TryGetValue(className, out TypeDefinition? result))
		{
			result = InterfaceCreator.CreateEmptyInterface(SharedState.Instance.Module, SharedState.MarkerInterfacesNamespace, interfaceName);
			result.AddInterfaceImplementation<IUnityObjectBase>(SharedState.Instance.Importer);
			SharedState.Instance.MarkerInterfaces.Add(className, result);
		}
		return result;
	}
}
