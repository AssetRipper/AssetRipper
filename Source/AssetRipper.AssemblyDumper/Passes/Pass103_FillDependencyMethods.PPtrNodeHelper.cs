using AssetRipper.AssemblyDumper.AST;

namespace AssetRipper.AssemblyDumper.Passes;

public static partial class Pass103_FillDependencyMethods
{
	private static class PPtrNodeHelper
	{
		public static void Apply(PPtrNode node, DependencyMethodContext context, ParentContext parentContext)
		{
			IMethodDescriptor conversionMethod = node.ClassInstance.Type.Methods.Single(m => m.Name == "op_Implicit" && m.Signature?.ReturnType?.Name == "PPtr");
			context.Processor.Add(CilOpCodes.Ldarg_0);
			context.Processor.Add(CilOpCodes.Ldstr, NodeHelper.GetFullPath(node));
			parentContext.EmitLoad(context);
			context.Processor.Add(CilOpCodes.Call, conversionMethod);
			context.Processor.Add(CilOpCodes.Newobj, context.TupleConstructor);
			context.Processor.Add(CilOpCodes.Stfld, context.CurrentField);
			parentContext.EmitIncrementStateAndReturnTrue(context);
		}
	}
}
