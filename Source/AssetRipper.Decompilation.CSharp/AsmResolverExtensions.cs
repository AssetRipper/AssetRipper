using AsmResolver.DotNet;

namespace AssetRipper.Decompilation.CSharp;

public static class AsmResolverExtensions
{
	public static TResult AcceptVisitor<TState, TResult>(this TypeDefinition type, IDefinitionVisitor<TState, TResult> visitor, TState state)
	{
		return visitor.VisitType(type, state);
	}

	public static TResult AcceptVisitor<TState, TResult>(this FieldDefinition field, IDefinitionVisitor<TState, TResult> visitor, TState state)
	{
		return visitor.VisitField(field, state);
	}

	public static TResult AcceptVisitor<TState, TResult>(this MethodDefinition method, IDefinitionVisitor<TState, TResult> visitor, TState state)
	{
		return visitor.VisitMethod(method, state);
	}

	public static TResult AcceptVisitor<TState, TResult>(this EventDefinition @event, IDefinitionVisitor<TState, TResult> visitor, TState state)
	{
		return visitor.VisitEvent(@event, state);
	}

	public static TResult AcceptVisitor<TState, TResult>(this PropertyDefinition property, IDefinitionVisitor<TState, TResult> visitor, TState state)
	{
		return visitor.VisitProperty(property, state);
	}
}
