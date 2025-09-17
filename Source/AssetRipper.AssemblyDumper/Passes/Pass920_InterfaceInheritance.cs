using AssetRipper.AssemblyDumper.Types;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass920_InterfaceInheritance
{
	public static void DoPass()
	{
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values
			.OrderBy(g => g.Types.Select(t => t.GetInheritanceDepth()).Max())
			.Order(GroupComparer.Instance))
		{
			DoPassOnGroup(group);
		}
	}

	private static void DoPassOnGroup(ClassGroup group)
	{
		List<HashSet<ITypeDefOrRef>> instances = group.Types.Select(t => t.GetAllInterfaces()).ToList();
		HashSet<ITypeDefOrRef> existingInterfaces = group.Interface.GetAllInterfaces();
		existingInterfaces.Add(group.Interface);

		foreach (ITypeDefOrRef potentialInterface in instances.First())
		{
			if (!existingInterfaces.Contains(potentialInterface) && instances.All(i => i.Contains(potentialInterface)))
			{
				ITypeDefOrRef interfaceReference = potentialInterface.ToTypeDefOrRef();
				group.Interface.AddInterfaceImplementation(interfaceReference);

				if (interfaceReference is TypeDefinition interfaceType)
				{
					existingInterfaces.UnionWith(interfaceType.GetAllInterfaces());
				}
			}
		}
	}

	private static HashSet<ITypeDefOrRef> GetAllInterfaces(this TypeDefinition type)
	{
		HashSet<ITypeDefOrRef> result = new(SignatureComparer.Default);
		HashSet<TypeDefinition> alreadyQueued = new()
		{
			type
		};
		Queue<TypeDefinition> queue = new()
		{
			type
		};

		while (queue.TryDequeue(out TypeDefinition? current))
		{
			foreach (InterfaceImplementation interfaceImplementation in current.Interfaces)
			{
				if (interfaceImplementation.Interface is not null)
				{
					result.Add(interfaceImplementation.Interface);
					if (interfaceImplementation.Interface is TypeDefinition interfaceType && alreadyQueued.Add(interfaceType))
					{
						queue.Enqueue(interfaceType);
					}
				}
			}
			if (current.BaseType is TypeDefinition baseType && alreadyQueued.Add(baseType))
			{
				queue.Enqueue(baseType);
			}
		}

		return result;
	}

	private static void Add<T>(this Queue<T> queue, T item) => queue.Enqueue(item);

	private static int GetInheritanceDepth(this TypeDefinition type)
	{
		int depth = 0;
		while (type.BaseType is TypeDefinition baseType)
		{
			depth++;
			type = baseType;
		}
		return depth;
	}

	private sealed class GroupComparer : IComparer<ClassGroup>
	{
		public static GroupComparer Instance { get; } = new();

		int IComparer<ClassGroup>.Compare(ClassGroup? x, ClassGroup? y)
		{
			return x is null
				? y is null ? 0 : -1
				: y is null ? 1 : Compare(x, y);
		}

		public static int Compare(ClassGroup x, ClassGroup y)
		{
			HashSet<TypeDefinition> xTypes = x.Types.ToHashSet();
			HashSet<TypeDefinition> yTypes = y.Types.ToHashSet();

			bool xInheritsFromY = xTypes.Any(t => ContainsBaseType(yTypes, t));
			bool yInheritsFromX = yTypes.Any(t => ContainsBaseType(xTypes, t));

			return yInheritsFromX
				? xInheritsFromY ? 0 : -1
				: xInheritsFromY ? 1 : 0;

			static bool ContainsBaseType(HashSet<TypeDefinition> set, TypeDefinition type)
			{
				while (type.BaseType is TypeDefinition baseType)
				{
					if (set.Contains(baseType))
					{
						return true;
					}
					type = baseType;
				}
				return false;
			}
		}
	}
}
