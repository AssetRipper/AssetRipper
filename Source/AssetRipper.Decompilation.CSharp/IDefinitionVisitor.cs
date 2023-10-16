using AsmResolver.DotNet;

namespace AssetRipper.Decompilation.CSharp;

public interface IDefinitionVisitor<in TState, out TResult>
{
	TResult VisitType(TypeDefinition type, TState state);
	TResult VisitField(FieldDefinition field, TState state);
	TResult VisitMethod(MethodDefinition method, TState state);
	TResult VisitEvent(EventDefinition @event, TState state);
	TResult VisitProperty(PropertyDefinition property, TState state);
}
