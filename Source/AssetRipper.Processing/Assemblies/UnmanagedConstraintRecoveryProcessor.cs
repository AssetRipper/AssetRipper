using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Recover unmanaged constraints for generic parameters in Il2Cpp games. This prevents compile errors when using pointers to generic parameters.
/// </summary>
public sealed class UnmanagedConstraintRecoveryProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		if (manager.ScriptingBackend != ScriptingBackend.IL2Cpp)
		{
			return;
		}

		ModuleDefinition? mscorlib = manager.Mscorlib?.ManifestModule;
		if (mscorlib is null)
		{
			return;
		}

		if (!mscorlib.TryGetTopLevelType("System.Runtime.CompilerServices", "IsUnmanagedAttribute", out TypeDefinition? unmanagedAttributeType))
		{
			return;
		}

		MethodDefinition? unmanagedAttributeConstructor = unmanagedAttributeType.Methods.FirstOrDefault(m => m.IsConstructor && m.Parameters.Count == 0);
		if (unmanagedAttributeConstructor is null)
		{
			return;
		}

		manager.ClearStreamCache();

		foreach (MethodDefinition method in manager.GetAllMethods())
		{
			foreach (TypeSignature? parameterSignature in method.Parameters.Select(p => p.ParameterType).Append(method.Signature?.ReturnType))
			{
				if (!IsPointerToGenericParameter(parameterSignature, out GenericParameterSignature? genericParameterSignature))
				{
				}
				else if (genericParameterSignature.ParameterType is GenericParameterType.Method)
				{
					GenericParameter genericParameter = method.GenericParameters[genericParameterSignature.Index];
					AddIsUnmanagedAttribute(genericParameter, unmanagedAttributeConstructor);
				}
				else
				{
					TypeDefinition? declaringType = method.DeclaringType;
					while (declaringType is not null && declaringType.GenericParameters.Count > genericParameterSignature.Index)
					{
						GenericParameter genericParameter = declaringType.GenericParameters[genericParameterSignature.Index];
						AddIsUnmanagedAttribute(genericParameter, unmanagedAttributeConstructor);

						declaringType = declaringType.DeclaringType;
					}
				}
			}
		}
	}

	static bool IsPointerToGenericParameter(TypeSignature? type, [NotNullWhen(true)] out GenericParameterSignature? genericParameter)
	{
		genericParameter = (type as PointerTypeSignature)?.BaseType as GenericParameterSignature;
		return genericParameter != null;
	}

	static void AddIsUnmanagedAttribute(GenericParameter genericParameter, MethodDefinition constructor)
	{
		if (!genericParameter.HasCustomAttribute("System.Runtime.CompilerServices", "IsUnmanagedAttribute"))
		{
			genericParameter.AddCustomAttribute(genericParameter.DeclaringModule!.DefaultImporter.ImportMethod(constructor));
		}
	}
}
