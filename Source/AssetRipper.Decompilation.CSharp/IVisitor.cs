namespace AssetRipper.Decompilation.CSharp;

public interface IVisitor<in TNode, in TState, out TResult>
{
	TResult Visit(TNode node, TState state);
}
