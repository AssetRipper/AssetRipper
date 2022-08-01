using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// Fixes event declarations that look like:
	/// <code>
	/// event MyEvent
	/// {
	///     add;
	///     remove;
	/// }
	/// </code>
	/// And instead replaces them with:
	/// <code>
	/// event MyEvent;
	/// </code>
	/// </summary>
	internal class FixEventDeclarationsTransform : DepthFirstAstVisitor, IAstTransform
	{
		private static bool AccessorHasImplementation(Accessor accessor)
		{
			return !accessor.IsNull && !accessor.Body.IsNull;
		}

		public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
		{
			base.VisitCustomEventDeclaration(eventDeclaration);

			bool hasAccessorImplementation = AccessorHasImplementation(eventDeclaration.AddAccessor) || AccessorHasImplementation(eventDeclaration.RemoveAccessor);

			if (hasAccessorImplementation)
			{
				return;
			}

			EventDeclaration declaration = new EventDeclaration();
			declaration.Modifiers = eventDeclaration.Modifiers;
			declaration.ReturnType = eventDeclaration.ReturnType.Clone();
			declaration.Variables.Add(new VariableInitializer(eventDeclaration.Name));
			((TypeDeclaration)eventDeclaration.Parent!).Members.Add(declaration);
			eventDeclaration.Remove();
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
