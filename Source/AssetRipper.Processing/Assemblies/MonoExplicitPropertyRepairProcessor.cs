using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Fixes explicit property implementations in Mono assemblies.
/// </summary>
/// <remarks>
/// Mono doesn't always include properties for explicit interface implementations, so we have to create them ourselves.
/// <see href="https://github.com/AssetRipper/AssetRipper/issues/1682"/>
/// </remarks>
public sealed class MonoExplicitPropertyRepairProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		if (manager.ScriptingBackend == ScriptingBackend.IL2Cpp)
		{
			return; // For Il2Cpp, this issue is handled by Cpp2IL
		}

		manager.ClearStreamCache();

		List<(PropertyDefinition InterfaceProperty, TypeSignature InterfaceType, MethodDefinition Method)> getMethodsToCreate = new();
		List<(PropertyDefinition InterfaceProperty, TypeSignature InterfaceType, MethodDefinition Method)> setMethodsToCreate = new();

		foreach (ModuleDefinition module in manager.GetAssemblies().SelectMany(a => a.Modules))
		{
			foreach (TypeDefinition type in module.GetAllTypes().Where(t => t.MethodImplementations.Count > 0))
			{
				foreach (MethodImplementation methodImpl in type.MethodImplementations)
				{
					if (methodImpl.Body is not MethodDefinition method || method.DeclaringType != type || method.IsGetMethod || method.IsSetMethod)
					{
						continue;
					}

					IMethodDefOrRef? interfaceMethod = methodImpl.Declaration;
					if (interfaceMethod is null)
					{
						continue;
					}

					MethodDefinition? interfaceMethodResolved = interfaceMethod.Resolve();
					if (interfaceMethodResolved != null)
					{
						if (interfaceMethodResolved.IsGetMethod)
						{
							PropertyDefinition interfacePropertyResolved = interfaceMethodResolved.DeclaringType!.Properties.First(p => p.Semantics.Contains(interfaceMethodResolved.Semantics));
							getMethodsToCreate.Add((interfacePropertyResolved, interfaceMethod.DeclaringType!.ToTypeSignature(), method));
						}
						else if (interfaceMethodResolved.IsSetMethod)
						{
							PropertyDefinition interfacePropertyResolved = interfaceMethodResolved.DeclaringType!.Properties.First(p => p.Semantics.Contains(interfaceMethodResolved.Semantics));
							setMethodsToCreate.Add((interfacePropertyResolved, interfaceMethod.DeclaringType!.ToTypeSignature(), method));
						}
					}
				}

				if (getMethodsToCreate.Count > 0)
				{
					foreach ((PropertyDefinition InterfaceProperty, TypeSignature InterfaceType, MethodDefinition Method) entry in getMethodsToCreate)
					{
						(PropertyDefinition interfaceProperty, TypeSignature interfaceType, MethodDefinition getMethod) = entry;
						MethodDefinition? setMethod = setMethodsToCreate
							.FirstOrDefault(e => e.InterfaceProperty == interfaceProperty && SignatureComparer.Default.Equals(e.InterfaceType, interfaceType))
							.Method;

						string name = $"{interfaceType.FullName}.{interfaceProperty.Name}";
						PropertySignature propertySignature = getMethod.IsStatic
							? PropertySignature.CreateStatic(getMethod.Signature!.ReturnType, getMethod.Signature.ParameterTypes)
							: PropertySignature.CreateInstance(getMethod.Signature!.ReturnType, getMethod.Signature.ParameterTypes);
						PropertyDefinition property = new PropertyDefinition(name, interfaceProperty.Attributes, propertySignature);
						type.Properties.Add(property);
						property.SetSemanticMethods(getMethod, setMethod);
					}
				}
				if (setMethodsToCreate.Count > 0)
				{
					foreach ((PropertyDefinition InterfaceProperty, TypeSignature InterfaceType, MethodDefinition Method) entry in setMethodsToCreate)
					{
						(PropertyDefinition interfaceProperty, TypeSignature interfaceType, MethodDefinition setMethod) = entry;
						if (getMethodsToCreate.Any(e => e.InterfaceProperty == interfaceProperty && SignatureComparer.Default.Equals(e.InterfaceType, interfaceType)) == true)
						{
							continue;
						}

						string name = $"{interfaceType.FullName}.{interfaceProperty.Name}";
						PropertySignature propertySignature = setMethod.IsStatic
							? PropertySignature.CreateStatic(setMethod.Signature!.ParameterTypes[^1], setMethod.Signature.ParameterTypes.Take(setMethod.Signature.ParameterTypes.Count - 1))
							: PropertySignature.CreateInstance(setMethod.Signature!.ParameterTypes[^1], setMethod.Signature.ParameterTypes.Take(setMethod.Signature.ParameterTypes.Count - 1));
						PropertyDefinition property = new PropertyDefinition(name, interfaceProperty.Attributes, propertySignature);
						type.Properties.Add(property);
						property.SetSemanticMethods(null, setMethod);
					}
				}

				getMethodsToCreate.Clear();
				setMethodsToCreate.Clear();
			}
		}
	}
}
