using AssetRipper.AssemblyDumper.Types;
using AssetRipper.AssemblyDumper.Utils;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass009_CreateGroups
{
	public static void DoPass()
	{
		foreach ((int id, VersionedList<UniversalClass> list) in SharedState.Instance.ClassInformation)
		{
			ClassGroup group = CreateClasses(list, id);
			SharedState.Instance.ClassGroups.Add(id, group);
		}
		foreach ((string name, VersionedList<UniversalClass> list) in SharedState.Instance.SubclassInformation)
		{
			SubclassGroup group = CreateSubclasses(list, name);
			SharedState.Instance.SubclassGroups.Add(name, group);
		}
	}

	private static ClassGroup CreateClasses(VersionedList<UniversalClass> loadedClasses, int id)
	{
		TypeDefinition @interface = InterfaceCreator.CreateEmptyInterface(SharedState.Instance.Module, null, null);
		ClassGroup group = new ClassGroup(id, @interface);
		CreateTypeDefinitionsAndInitializeGroup(loadedClasses, group);
		return group;
	}

	private static SubclassGroup CreateSubclasses(VersionedList<UniversalClass> loadedClasses, string name)
	{
		TypeDefinition @interface = InterfaceCreator.CreateEmptyInterface(SharedState.Instance.Module, null, null);
		SubclassGroup group = new SubclassGroup(name, @interface);
		CreateTypeDefinitionsAndInitializeGroup(loadedClasses, group);
		return group;
	}

	private static void CreateTypeDefinitionsAndInitializeGroup(VersionedList<UniversalClass> loadedClasses, ClassGroupBase group)
	{
		int nonNullCount = loadedClasses.Values.Count(c => c != null);
		if (nonNullCount == 0)
		{
			throw new ArgumentException("Must have at least one class", nameof(loadedClasses));
		}
		else if (nonNullCount == 1)
		{
			KeyValuePair<UnityVersion, UniversalClass?> pair = loadedClasses.Single(c => c.Value != null);
			UniversalClass universalClass = pair.Value!;
			string typeName = universalClass.Name;
			UnityVersion endVersion = loadedClasses.Count == 2 && loadedClasses[1].Value == null ? loadedClasses[1].Key : UnityVersion.MaxVersion;
			CreateType(universalClass, pair.Key, endVersion, typeName, group);
		}
		else
		{
			for (int i = 0; i < loadedClasses.Count; i++)
			{
				KeyValuePair<UnityVersion, UniversalClass?> pair = loadedClasses[i];
				UnityVersion endVersion = i + 1 < loadedClasses.Count ? loadedClasses[i + 1].Key : UnityVersion.MaxVersion;
				UniversalClass? universalClass = pair.Value;
				if (universalClass is not null)
				{
					string typeName = $"{universalClass.Name}_{pair.Key.ToCleanString('_')}";
					CreateType(universalClass, pair.Key, endVersion, typeName, group);
				}
			}
		}
		group.InitializeHistory(SharedState.Instance.HistoryFile);
	}

	private static void CreateType(UniversalClass universalClass, UnityVersion startVersion, UnityVersion endVersion, string typeName, ClassGroupBase group)
	{
		TypeDefinition type = new TypeDefinition(group.Namespace, typeName, TypeAttributes.Public | TypeAttributes.BeforeFieldInit);
		SharedState.Instance.Module.TopLevelTypes.Add(type);

		GeneratedClassInstance instance = new GeneratedClassInstance(group, universalClass, type, startVersion, endVersion);
		group.Instances.Add(instance);
		SharedState.Instance.TypesToGroups.Add(type, group);
		SharedState.Instance.TypesToInstances.Add(type, instance);
	}
}
