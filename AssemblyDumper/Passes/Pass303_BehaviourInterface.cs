using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;

namespace AssemblyDumper.Passes
{
	public static class Pass303_BehaviourInterface
	{
		const MethodAttributes InterfacePropertyImplementationAttributes =
			MethodAttributes.Public |
			MethodAttributes.Final |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.NewSlot |
			MethodAttributes.Virtual;
		const string PropertyName = "Enabled";
		const string FieldName = "m_Enabled";
		public static void DoPass()
		{
			Console.WriteLine("Pass 303: Behaviour Interface");
			if (!SharedState.TypeDictionary.TryGetValue("Behaviour", out TypeDefinition? type))
			{
				throw new Exception("Behaviour not found");
			}
			else
			{
				ITypeDefOrRef behaviourInterface = SharedState.Importer.ImportCommonType<IBehaviour>();
				type.Interfaces.Add(new InterfaceImplementation(behaviourInterface));
				FieldDefinition field = type.GetFieldByName(FieldName);
				
				PropertyDefinition property = type.AddFullProperty(PropertyName, InterfacePropertyImplementationAttributes, SystemTypeGetter.Boolean);
				CilInstructionCollection getProcessor = property.GetMethod!.CilMethodBody!.Instructions;
				getProcessor.Add(CilOpCodes.Ldarg_0);
				getProcessor.Add(CilOpCodes.Ldfld, field);
				getProcessor.Add(CilOpCodes.Ldc_I4_0);
				getProcessor.Add(CilOpCodes.Cgt_Un);
				getProcessor.Add(CilOpCodes.Ret);
				getProcessor.OptimizeMacros();

				CilInstructionCollection setProcessor = property.SetMethod!.CilMethodBody!.Instructions;
				CilInstructionLabel jumpTrue = new CilInstructionLabel();
				CilInstructionLabel jumpFalse = new CilInstructionLabel();
				setProcessor.Add(CilOpCodes.Ldarg_0);
				setProcessor.Add(CilOpCodes.Ldarg_1);
				setProcessor.Add(CilOpCodes.Brtrue, jumpTrue);
				setProcessor.Add(CilOpCodes.Ldc_I4_0);
				setProcessor.Add(CilOpCodes.Br, jumpFalse);
				jumpTrue.Instruction = setProcessor.Add(CilOpCodes.Ldc_I4_1);
				jumpFalse.Instruction = setProcessor.Add(CilOpCodes.Stfld, field);
				setProcessor.Add(CilOpCodes.Ret);
				setProcessor.OptimizeMacros();
			}
		}
	}
}
