using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.DotNet.Signatures;
using AsmResolver.PE.DotNet.Cil;
using AsmResolver.PE.DotNet.Metadata.Tables;
using AssetRipper.Import.Logging;
using AssetRipper.Import.Structure.Assembly.Managers;

namespace AssetRipper.Processing.Assemblies;

/// <summary>
/// This processor reverses the code generation done by the Photon Fusion weaver, so that the decompiled scripts compile.
/// </summary>
/// <remarks>
/// When a game using Photon Fusion is built, its weaver rewrites networked properties to read and write unmanaged network
/// buffers, and it generates serialization types in the <c>Fusion.CodeGen</c> namespace. That code accesses internal and
/// private types, so it can't be compiled from source. This processor:
/// <list type="bullet">
/// <item>removes the <c>Fusion.CodeGen</c> types and every member marked with <c>[WeaverGenerated]</c>,</item>
/// <item>restores woven <c>[Networked]</c> properties to their source form: auto-properties, or <c>=> default</c> for read-only ones,</item>
/// <item>removes the code inserted at the start of RPC methods to send them,</item>
/// <item>removes the attributes marking types and properties as woven, so that the Fusion weaver processes them again if it's installed.</item>
/// </list>
/// Only assemblies that will be decompiled are processed. Assemblies exported as compiled files keep their woven code, which works as is.
/// </remarks>
/// <param name="shouldProcess">Whether an assembly, identified by name, will be decompiled.</param>
public sealed class FusionWeavingReversalProcessor(Func<string, bool> shouldProcess) : IAssetProcessor
{
	private const string FusionNamespace = "Fusion";
	private const string CodeGenNamespace = "Fusion.CodeGen";

	public void Process(GameData gameData) => Process(gameData.AssemblyManager);

	private void Process(IAssemblyManager manager)
	{
		bool clearedCache = false;
		foreach (ModuleDefinition module in manager.GetAssemblies().Where(a => a.Name?.Value is { } name && shouldProcess(name)).SelectMany(a => a.Modules))
		{
			if (!IsWoven(module))
			{
				continue;
			}
			if (!clearedCache)
			{
				manager.ClearStreamCache();
				clearedCache = true;
			}
			int propertyCount = RestoreNetworkedProperties(module);
			int rpcCount = RemoveRpcSendCode(module);
			int memberCount = RemoveGeneratedMembers(module);
			Logger.Info(LogCategory.Processing, $"Reversed Photon Fusion weaving in {module.Name}: restored {propertyCount} networked properties and {rpcCount} RPCs, and removed {memberCount} generated types and members.");
		}
	}

	private static bool IsWoven(ModuleDefinition module)
	{
		return module.GetAllTypes().Any(t => t.CustomAttributes.Any(a => IsFusionAttribute(a, "NetworkBehaviourWeavedAttribute") || IsFusionAttribute(a, "NetworkStructWeavedAttribute")));
	}

	private static int RestoreNetworkedProperties(ModuleDefinition module)
	{
		int count = 0;
		foreach (TypeDefinition type in module.GetAllTypes().Where(t => !IsCodeGenType(t)).ToArray())
		{
			foreach (PropertyDefinition property in type.Properties)
			{
				if (!property.CustomAttributes.Any(IsWovenPropertyAttribute) || property.GetMethod is null)
				{
					continue;
				}

				RemoveAttributes(property, IsWovenPropertyAttribute);
				if (property.SetMethod is not null && !property.GetMethod.IsStatic && type.GenericParameters.Count == 0)
				{
					MakeAutoProperty(module, type, property);
				}
				else
				{
					MakeDefaultGetter(property.GetMethod);
				}
				count++;
			}

			// The weaver marks types as processed, which would prevent it from processing the restored source.
			RemoveAttributes(type, a => IsFusionAttribute(a, "NetworkBehaviourWeavedAttribute") || IsFusionAttribute(a, "NetworkStructWeavedAttribute"));
		}
		return count;
	}

	private static void MakeAutoProperty(ModuleDefinition module, TypeDefinition type, PropertyDefinition property)
	{
		TypeSignature propertyType = property.Signature!.ReturnType;
		FieldDefinition backingField = new($"<{property.Name}>k__BackingField", FieldAttributes.Private, new FieldSignature(propertyType));
		CustomAttribute compilerGenerated = CreateCompilerGeneratedAttribute(module);
		backingField.CustomAttributes.Add(compilerGenerated);
		type.Fields.Add(backingField);

		MethodDefinition getter = property.GetMethod!;
		getter.CilMethodBody = new CilMethodBody();
		getter.CilMethodBody.Instructions.Add(CilOpCodes.Ldarg_0);
		getter.CilMethodBody.Instructions.Add(CilOpCodes.Ldfld, backingField);
		getter.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
		getter.CustomAttributes.Add(CreateCompilerGeneratedAttribute(module));

		MethodDefinition setter = property.SetMethod!;
		setter.CilMethodBody = new CilMethodBody();
		setter.CilMethodBody.Instructions.Add(CilOpCodes.Ldarg_0);
		setter.CilMethodBody.Instructions.Add(CilOpCodes.Ldarg_1);
		setter.CilMethodBody.Instructions.Add(CilOpCodes.Stfld, backingField);
		setter.CilMethodBody.Instructions.Add(CilOpCodes.Ret);
		setter.CustomAttributes.Add(CreateCompilerGeneratedAttribute(module));

		// Woven types are often unsafe, explicitly laid out structs. A field without an offset isn't allowed in those.
		if (type.IsExplicitLayout)
		{
			MakeSequentialLayout(type);
		}
	}

	/// <summary>
	/// Replace a getter with <c>return default;</c>, which is how read-only networked properties are written in source.
	/// </summary>
	private static void MakeDefaultGetter(MethodDefinition getter)
	{
		TypeSignature returnType = getter.Signature!.ReturnType;
		CilMethodBody body = new();
		getter.CilMethodBody = body;
		if (returnType is ByReferenceTypeSignature)
		{
			// There is no default for a reference, so the getter throws, as it does when Fusion hasn't spawned the object.
			body.Instructions.Add(CilOpCodes.Ldnull);
			body.Instructions.Add(CilOpCodes.Throw);
		}
		else if (returnType.IsValueType)
		{
			CilLocalVariable local = new(returnType);
			body.LocalVariables.Add(local);
			body.Instructions.Add(CilOpCodes.Ldloca_S, local);
			body.Instructions.Add(CilOpCodes.Initobj, returnType.ToTypeDefOrRef());
			body.Instructions.Add(CilOpCodes.Ldloc_0);
			body.Instructions.Add(CilOpCodes.Ret);
		}
		else
		{
			body.Instructions.Add(CilOpCodes.Ldnull);
			body.Instructions.Add(CilOpCodes.Ret);
		}
	}

	/// <summary>
	/// Remove the code that the weaver inserts at the start of RPC methods to send them over the network.
	/// </summary>
	/// <remarks>
	/// A woven RPC starts with <c>if (InvokeRpc) { InvokeRpc = false; } else { /* send the RPC */ }</c>, followed by the original body.
	/// For static RPCs, the original body is inside the first branch. The check and the reset are replaced with no-ops,
	/// so the first branch always runs and the sending branch becomes unreachable, which the decompiler leaves out.
	/// The <c>InvokeRpc</c> fields are internal to Fusion, so this code can't be compiled from source.
	/// </remarks>
	private static int RemoveRpcSendCode(ModuleDefinition module)
	{
		int count = 0;
		foreach (MethodDefinition method in module.GetAllTypes().Where(t => !IsCodeGenType(t)).SelectMany(t => t.Methods))
		{
			if (method.CilMethodBody is not { } body || !method.CustomAttributes.Any(a => IsFusionAttribute(a, "RpcAttribute")))
			{
				continue;
			}

			CilInstructionCollection instructions = body.Instructions;
			int check = FindInvokeRpcAccess(instructions, CilCode.Ldfld, CilCode.Ldsfld, 0, 3);
			if (check < 0 || instructions[check + 1].OpCode.FlowControl != CilFlowControl.ConditionalBranch)
			{
				continue;
			}
			int reset = FindInvokeRpcAccess(instructions, CilCode.Stfld, CilCode.Stsfld, check + 2, 4);
			if (reset < 0)
			{
				continue;
			}

			for (int i = 0; i <= reset; i++)
			{
				instructions[i].OpCode = CilOpCodes.Nop;
				instructions[i].Operand = null;
			}
			count++;
		}
		return count;

		static int FindInvokeRpcAccess(CilInstructionCollection instructions, CilCode instanceCode, CilCode staticCode, int start, int maxCount)
		{
			for (int i = start; i < instructions.Count && i < start + maxCount; i++)
			{
				CilInstruction instruction = instructions[i];
				if ((instruction.OpCode.Code == instanceCode || instruction.OpCode.Code == staticCode)
					&& instruction.Operand is IFieldDescriptor { Name.Value: "InvokeRpc" })
				{
					return i;
				}
			}
			return -1;
		}
	}

	private static void MakeSequentialLayout(TypeDefinition type)
	{
		type.Attributes = (type.Attributes & ~TypeAttributes.LayoutMask) | TypeAttributes.SequentialLayout;
		type.ClassLayout = null;
		foreach (FieldDefinition field in type.Fields)
		{
			field.FieldOffset = null;
		}
	}

	private static int RemoveGeneratedMembers(ModuleDefinition module)
	{
		int count = 0;

		foreach (TypeDefinition type in module.TopLevelTypes.Where(IsCodeGenType).ToArray())
		{
			module.TopLevelTypes.Remove(type);
			count++;
		}

		foreach (TypeDefinition type in module.GetAllTypes().ToArray())
		{
			count += RemoveWhere(type.NestedTypes, IsWeaverGenerated);
			count += RemoveWhere(type.Properties, IsWeaverGenerated);
			count += RemoveWhere(type.Methods, m => IsWeaverGenerated(m) || m.CustomAttributes.Any(a => IsFusionAttribute(a, "NetworkRpcWeavedInvokerAttribute")));
			int removedFields = RemoveWhere(type.Fields, IsWeaverGenerated);
			if (removedFields > 0 && type.IsExplicitLayout)
			{
				// The weaver gives woven structs an explicit layout around the fields it generates.
				MakeSequentialLayout(type);
			}
			count += removedFields;
		}

		return count;
	}

	private static int RemoveWhere<T>(IList<T> list, Func<T, bool> predicate)
	{
		int count = 0;
		for (int i = list.Count - 1; i >= 0; i--)
		{
			if (predicate(list[i]))
			{
				list.RemoveAt(i);
				count++;
			}
		}
		return count;
	}

	private static bool IsCodeGenType(TypeDefinition type)
	{
		TypeDefinition outermost = type;
		while (outermost.DeclaringType is not null)
		{
			outermost = outermost.DeclaringType;
		}
		return outermost.Namespace?.Value is { } ns && (ns == CodeGenNamespace || ns.StartsWith(CodeGenNamespace + ".", StringComparison.Ordinal));
	}

	private static bool IsWeaverGenerated(IHasCustomAttribute member)
	{
		return member.CustomAttributes.Any(a => IsFusionAttribute(a, "WeaverGeneratedAttribute"));
	}

	private static bool IsWovenPropertyAttribute(CustomAttribute attribute)
	{
		return attribute.Constructor?.DeclaringType is { Namespace.Value: FusionNamespace } type
			&& type.Name?.Value is { } name
			&& name.StartsWith("NetworkedWeaved", StringComparison.Ordinal);
	}

	private static bool IsFusionAttribute(CustomAttribute attribute, string name)
	{
		return attribute.IsType(FusionNamespace, name);
	}

	private static void RemoveAttributes(IHasCustomAttribute owner, Func<CustomAttribute, bool> predicate)
	{
		RemoveWhere(owner.CustomAttributes, predicate);
	}

	private static CustomAttribute CreateCompilerGeneratedAttribute(ModuleDefinition module)
	{
		TypeReference attributeType = new(module, module.CorLibTypeFactory.CorLibScope, "System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
		MemberReference constructor = attributeType.CreateMemberReference(".ctor", MethodSignature.CreateInstance(module.CorLibTypeFactory.Void));
		return new CustomAttribute(constructor);
	}
}
