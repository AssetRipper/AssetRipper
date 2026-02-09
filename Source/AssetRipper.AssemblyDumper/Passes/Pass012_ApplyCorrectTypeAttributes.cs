namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass012_ApplyCorrectTypeAttributes
{
	public static void DoPass()
	{
		foreach (ClassGroup classGroup in SharedState.Instance.ClassGroups.Values)
		{
			classGroup.FixTypeAttributes();
		}
		foreach (SubclassGroup subclassGroup in SharedState.Instance.SubclassGroups.Values)
		{
			subclassGroup.FixTypeAttributes();
		}
	}

	private static void FixTypeAttributes(this ClassGroupBase group)
	{
		foreach (GeneratedClassInstance instance in group.Instances)
		{
			instance.Type.Attributes = GetTypeAttributes(instance.Class);
		}
	}

	private static TypeAttributes GetTypeAttributes(UniversalClass universalClass)
	{
		TypeAttributes attributes = TypeAttributes.Public | TypeAttributes.BeforeFieldInit;
		if (universalClass.IsAbstract)
		{
			attributes |= TypeAttributes.Abstract;
		}
		else if (universalClass.IsSealed)
		{
			attributes |= TypeAttributes.Sealed;
		}
		return attributes;
	}
}
