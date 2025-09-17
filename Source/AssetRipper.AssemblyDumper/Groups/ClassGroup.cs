namespace AssetRipper.AssemblyDumper.Groups;

internal sealed class ClassGroup : ClassGroupBase
{
	public override int ID { get; }

	public override bool IsSealed => Instances.All(instance => instance.Type.IsSealed);

	public override string Name => Instances[Instances.Count - 1].Name;

	public override string Namespace => SharedState.GetClassNamespace(ID);

	public IEnumerable<string> Names => Instances.Select(instance => instance.Name).Distinct();

	public ClassGroup(int id, TypeDefinition @interface) : base(@interface)
	{
		ID = id;
	}

	public override bool UniformlyNamed => Instances.All(instance => instance.Name == Instances[0].Name);
}
