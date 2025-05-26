using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Collections;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AssetRipper.CIL;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

public sealed class InitializeLocalsAndOutParametersProcessor : IAssetProcessor
{
	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private static void Process(IAssemblyManager manager)
	{
		manager.ClearStreamCache();
		foreach (MethodDefinition method in manager.GetAllMethods())
		{
			if (method.CilMethodBody is null)
			{
				continue;
			}

			//if (method.IsInstanceConstructor() || method.Semantics is not null)
			{
				//continue;
			}

			method.CilMethodBody.InitializeLocals = false;
			continue;

			CilInstructionCollection instructions = method.CilMethodBody.Instructions;

			instructions.ExpandMacros(); // Adding instructions can make _S opcodes invalid, so expand them first.
			CilInstruction[] priorInstructions = new CilInstruction[instructions.Count];
			instructions.CopyTo(priorInstructions, 0);
			instructions.Clear();

			foreach (Parameter parameter in method.Parameters)
			{
				if (parameter.Definition?.IsOut is true && parameter.ParameterType is ByReferenceTypeSignature byReferenceTypeSignature)
				{
					instructions.Add(CilOpCodes.Ldarg, parameter);
					instructions.AddDefaultValue(byReferenceTypeSignature.BaseType);
					instructions.AddStoreIndirect(byReferenceTypeSignature.BaseType);
				}
			}
			foreach (CilLocalVariable localVariable in instructions.Owner.LocalVariables)
			{
				instructions.InitializeDefaultValue(localVariable);
			}
			instructions.AddRange(priorInstructions);

			instructions.OptimizeMacros(); // We undo the earlier expansion.
		}
	}
}
