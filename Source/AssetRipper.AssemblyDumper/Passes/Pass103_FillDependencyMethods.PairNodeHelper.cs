using AssetRipper.AssemblyDumper.AST;

namespace AssetRipper.AssemblyDumper.Passes;

public static partial class Pass103_FillDependencyMethods
{
	private static class PairNodeHelper
	{
		public static void Apply(PairNode node, DependencyMethodContext context, ParentContext parentContext)
		{
			if (node.Key.AnyPPtrs)
			{
				if (node.Value.AnyPPtrs)
				{
					FieldDefinition stateField = context.Type.AddField(NodeHelper.GetStateFieldName(node), context.CorLibTypeFactory.Boolean, visibility: Visibility.Private);
					CilInstructionLabel valueLabel = new();
					CilInstructionLabel endLabel = new();

					context.Processor.Add(CilOpCodes.Ldarg_0);
					context.Processor.Add(CilOpCodes.Ldfld, stateField);
					context.Processor.Add(CilOpCodes.Brtrue, valueLabel);

					NodeHelper.Apply(node.Key, context, new ParentContext()
					{
						EmitLoad = c =>
						{
							parentContext.EmitLoad(c);
							c.Processor.Add(CilOpCodes.Callvirt, node.GetKey);
						},
						EmitIncrementStateAndGotoNextCase = c =>
						{
							c.Processor.Add(CilOpCodes.Ldarg_0);
							c.Processor.Add(CilOpCodes.Ldc_I4_1);
							c.Processor.Add(CilOpCodes.Stfld, stateField);
							c.Processor.Add(CilOpCodes.Br, valueLabel);
						},
						EmitIncrementStateAndReturnTrue = c =>
						{
							c.Processor.Add(CilOpCodes.Ldarg_0);
							c.Processor.Add(CilOpCodes.Ldc_I4_1);
							c.Processor.Add(CilOpCodes.Stfld, stateField);
							c.EmitReturnTrue();
						},
					});

					valueLabel.Instruction = context.Processor.Add(CilOpCodes.Nop);
					NodeHelper.Apply(node.Value, context, new ParentContext()
					{
						EmitLoad = c =>
						{
							parentContext.EmitLoad(c);
							c.Processor.Add(CilOpCodes.Callvirt, node.GetValue);
						},
						EmitIncrementStateAndGotoNextCase = parentContext.EmitIncrementStateAndGotoNextCase,
						EmitIncrementStateAndReturnTrue = parentContext.EmitIncrementStateAndReturnTrue,
					});

					endLabel.Instruction = context.Processor.Add(CilOpCodes.Nop);
				}
				else
				{
					NodeHelper.Apply(node.Key, context, new ParentContext()
					{
						EmitLoad = c =>
						{
							parentContext.EmitLoad(c);
							c.Processor.Add(CilOpCodes.Callvirt, node.GetKey);
						},
						EmitIncrementStateAndGotoNextCase = parentContext.EmitIncrementStateAndGotoNextCase,
						EmitIncrementStateAndReturnTrue = parentContext.EmitIncrementStateAndReturnTrue,
					});
				}
			}
			else if (node.Value.AnyPPtrs)
			{
				NodeHelper.Apply(node.Value, context, new ParentContext()
				{
					EmitLoad = c =>
					{
						parentContext.EmitLoad(c);
						c.Processor.Add(CilOpCodes.Callvirt, node.GetValue);
					},
					EmitIncrementStateAndGotoNextCase = parentContext.EmitIncrementStateAndGotoNextCase,
					EmitIncrementStateAndReturnTrue = parentContext.EmitIncrementStateAndReturnTrue,
				});
			}
			else
			{
				throw new InvalidOperationException("Neither Key nor Value have any PPtrs");
			}
		}
	}
}
