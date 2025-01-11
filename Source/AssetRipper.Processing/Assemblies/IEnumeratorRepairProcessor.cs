using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using System.Collections;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// Fixes IEnumerator implementations in IL2CPP assemblies.
/// </summary>
/// <remarks>
/// For no apparent reason, compiler-generated IEnumerator.MoveNext implementations seem to be invalid, which causes problems for ILSpy.
/// The methods are compiled as `private bool MoveNext()`, but they should be `public bool MoveNext()`.
/// Alternatively, they could be `bool System.Collections.IEnumerator.MoveNext()`, but it's better not to change the method signature.
/// </remarks>
public sealed class IEnumeratorRepairProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);
	private static void Process(IAssemblyManager manager)
	{
		if (manager.ScriptingBackend != ScriptingBackend.IL2Cpp)
		{
			return; // Not sure if Mono needs this
		}
		manager.ClearStreamCache();
		foreach (ModuleDefinition module in manager.GetAssemblies().SelectMany(a => a.Modules))
		{
			foreach (TypeDefinition type in module.GetAllTypes().Where(t => t.IsNestedPrivate && t.IsCompilerGenerated() && t.Implements("System.Collections", "IEnumerator")))
			{
				const string MoveNext = nameof(IEnumerator.MoveNext);
				const string FullName = "System.Collections.IEnumerator.MoveNext";

				if (type.MethodImplementations.Any(m => m.Body?.Name?.Value is MoveNext or FullName))
				{
					continue;
				}

				MethodDefinition? method = type.Methods.FirstOrDefault(m =>
				{
					return m.Name == MoveNext
						&& m.Parameters.Count == 0
						&& m.Signature?.ReturnType is CorLibTypeSignature { ElementType: ElementType.Boolean };
				});

				if (method is null)
				{
					continue;
				}

				method.IsPublic = true;
			}
		}
	}
}
