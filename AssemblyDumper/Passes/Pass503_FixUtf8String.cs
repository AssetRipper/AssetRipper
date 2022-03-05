using AssemblyDumper.Utils;
using AssetRipper.Core.Classes;

namespace AssemblyDumper.Passes
{
	public static class Pass503_FixUtf8String
	{
		const MethodAttributes PropertyOverrideAttributes =
			MethodAttributes.Public |
			MethodAttributes.HideBySig |
			MethodAttributes.SpecialName |
			MethodAttributes.ReuseSlot |
			MethodAttributes.Virtual;

		public static void DoPass()
		{
			Console.WriteLine("Pass 503: Fix Utf8String");

			TypeDefinition type = SharedState.TypeDictionary[Pass002_RenameSubnodes.Utf8StringName];
			type.ImplementFullProperty(nameof(Utf8StringBase.Data), PropertyOverrideAttributes, null, type.GetFieldByName("data"));
			type.FixYamlMethod(nameof(Utf8StringBase.ExportYAMLRelease));
			type.FixYamlMethod(nameof(Utf8StringBase.ExportYAMLEditor));
		}

		private static void FixYamlMethod(this TypeDefinition type, string methodName)
		{
			MethodDefinition method = type.Methods.Single(m => m.Name == methodName);
			IMethodDefOrRef baseMethod = SharedState.Importer.ImportCommonMethod<Utf8StringBase>(m => m.Name == methodName);
			CilInstructionCollection processor = method.CilMethodBody!.Instructions;
			processor.Clear();
			processor.Add(CilOpCodes.Ldarg_0);
			processor.Add(CilOpCodes.Ldarg_1);
			processor.Add(CilOpCodes.Call, baseMethod);
			processor.Add(CilOpCodes.Ret);
		}
	}
}
