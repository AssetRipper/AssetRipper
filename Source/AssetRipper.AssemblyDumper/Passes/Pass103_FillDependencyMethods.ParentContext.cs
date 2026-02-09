namespace AssetRipper.AssemblyDumper.Passes;

public static partial class Pass103_FillDependencyMethods
{
	private sealed class ParentContext
	{
		public required Action<DependencyMethodContext> EmitLoad { get; init; }
		public required Action<DependencyMethodContext> EmitIncrementStateAndGotoNextCase { get; init; }
		public required Action<DependencyMethodContext> EmitIncrementStateAndReturnTrue { get; init; }
	}
}
