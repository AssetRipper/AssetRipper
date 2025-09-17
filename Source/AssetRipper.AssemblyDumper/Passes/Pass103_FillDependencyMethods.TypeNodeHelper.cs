using AssetRipper.AssemblyDumper.AST;

namespace AssetRipper.AssemblyDumper.Passes;

public static partial class Pass103_FillDependencyMethods
{
	private static class TypeNodeHelper
	{
		public static void ApplyAsRoot(TypeNode node, DependencyMethodContext context)
		{
			if (node.Parent is not null)
			{
				throw new InvalidOperationException("This node is not a root node");
			}
			Apply(node, context, new ParentContext()
			{
				EmitLoad = c =>
				{
					c.Processor.Add(CilOpCodes.Ldarg_0);
					c.Processor.Add(CilOpCodes.Ldfld, c.ThisField);
				},
				EmitIncrementStateAndGotoNextCase = c =>
				{
					//This is the root node, so it has no parent to increment the state of.
					c.Processor.Add(CilOpCodes.Ldc_I4_0);
					c.Processor.Add(CilOpCodes.Ret);
				},
				EmitIncrementStateAndReturnTrue = c =>
				{
					c.EmitReturnTrue();
				},
			});
		}

		public static void Apply(TypeNode node, DependencyMethodContext context, ParentContext parentContext)
		{
			List<FieldNode> children = node.Children.Where(c => c.AnyPPtrs).ToList();
			if (children.Count == 1)
			{
				FieldNode child = children[0];
				NodeHelper.Apply(child, context, new ParentContext()
				{
					EmitLoad = c =>
					{
						parentContext.EmitLoad(c);
						c.Processor.Add(CilOpCodes.Ldfld, child.Field);
					},
					EmitIncrementStateAndGotoNextCase = parentContext.EmitIncrementStateAndGotoNextCase,
					EmitIncrementStateAndReturnTrue = parentContext.EmitIncrementStateAndReturnTrue,
				});
				return;
			}

			FieldDefinition stateField = context.Type.AddField(NodeHelper.GetStateFieldName(node), context.CorLibTypeFactory.Int32, visibility: Visibility.Private);
			CilInstructionLabel[] cases = new CilInstructionLabel[children.Count];
			for (int i = 0; i < cases.Length; i++)
			{
				cases[i] = new();
			}
			CilInstructionLabel defaultCase = new();
			CilInstructionLabel endLabel = new();

			context.Processor.Add(CilOpCodes.Ldarg_0);
			context.Processor.Add(CilOpCodes.Ldfld, stateField);
			context.Processor.Add(CilOpCodes.Switch, cases);
			context.Processor.Add(CilOpCodes.Br, defaultCase);
			for (int i = 0; i < cases.Length; i++)
			{
				FieldNode child = children[i];
				cases[i].Instruction = context.Processor.Add(CilOpCodes.Nop);
				NodeHelper.Apply(children[i], context, new ParentContext()
				{
					EmitLoad = c =>
					{
						parentContext.EmitLoad(c);
						c.Processor.Add(CilOpCodes.Ldfld, child.Field);
					},
					EmitIncrementStateAndGotoNextCase = c =>
					{
						c.Processor.Add(CilOpCodes.Ldarg_0);
						c.Processor.Add(CilOpCodes.Ldc_I4, i + 1);
						c.Processor.Add(CilOpCodes.Stfld, stateField);
						if (i + 1 < cases.Length)
						{
							c.Processor.Add(CilOpCodes.Br, cases[i + 1]);
						}
						else
						{
							c.Processor.Add(CilOpCodes.Br, defaultCase);
						}
					},
					EmitIncrementStateAndReturnTrue = c =>
					{
						c.Processor.Add(CilOpCodes.Ldarg_0);
						c.Processor.Add(CilOpCodes.Ldc_I4, i + 1);
						c.Processor.Add(CilOpCodes.Stfld, stateField);
						if (i + 1 < cases.Length)
						{
							c.EmitReturnTrue();
						}
						else
						{
							parentContext.EmitIncrementStateAndReturnTrue(c);
						}
					},
				});
				context.Processor.Add(CilOpCodes.Br, endLabel);
			}
			defaultCase.Instruction = context.Processor.Add(CilOpCodes.Nop);
			parentContext.EmitIncrementStateAndGotoNextCase(context);
			endLabel.Instruction = context.Processor.Add(CilOpCodes.Nop);
		}
	}
}
