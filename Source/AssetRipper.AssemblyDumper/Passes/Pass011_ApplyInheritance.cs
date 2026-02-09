using AssetRipper.Assets;

namespace AssetRipper.AssemblyDumper.Passes;

public static class Pass011_ApplyInheritance
{
	public static void DoPass()
	{
		//Not necessary but left just because
		/*foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				instance.Class.DerivedClasses.Clear();
				instance.Class.DescendantCount = 0;
				instance.Class.BaseClass = null;
			}
		}*/
		Dictionary<UniversalClass, GeneratedClassInstance> classInstanceDictionary = new();
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			foreach (GeneratedClassInstance instance in group.Instances)
			{
				classInstanceDictionary.Add(instance.Class, instance);
			}
		}
		foreach (ClassGroup group in SharedState.Instance.ClassGroups.Values)
		{
			group.ApplyBaseTypes(classInstanceDictionary);
		}
		foreach (SubclassGroup group in SharedState.Instance.SubclassGroups.Values)
		{
			group.ApplyBaseTypes(classInstanceDictionary);
		}
		foreach (UniversalClass universalClass in SharedState.Instance.ClassGroups.Values.SelectMany(g => g.Classes))
		{
			universalClass.SetDescendantCount();
		}
	}

	private static void ApplyBaseTypes(this ClassGroupBase group, Dictionary<UniversalClass, GeneratedClassInstance> classInstanceDictionary)
	{
		ITypeDefOrRef unityObjectBaseDefinition = SharedState.Instance.Importer.ImportType<UnityObjectBase>();
		ITypeDefOrRef unityAssetBaseDefinition = SharedState.Instance.Importer.ImportType<UnityAssetBase>();

		foreach (GeneratedClassInstance instance in group.Instances)
		{
			string name = instance.Name;
			string? baseTypeName = instance.Class.BaseString;
			if (string.IsNullOrEmpty(baseTypeName))
			{
				instance.Type.BaseType = name switch
				{
					"Object" => unityObjectBaseDefinition,
					_ => unityAssetBaseDefinition,
				};
			}
			else
			{
				GeneratedClassInstance baseInstance = SharedState.Instance.GetGeneratedInstanceForObjectType(baseTypeName, instance.VersionRange.Start);
				instance.Type.BaseType = baseInstance.Type;
				instance.Class.BaseClass = baseInstance.Class;
				baseInstance.Class.DerivedClasses.Add(instance.Class);
				baseInstance.Derived.Add(instance);
				instance.Base = baseInstance;
			}
		}
	}

	private static void SetDescendantCount(this UniversalClass universalClass)
	{
		if (universalClass.DescendantCount <= 1 && universalClass.DerivedClasses.Count > 0)
		{
			foreach (UniversalClass derivedClass in universalClass.DerivedClasses)
			{
				derivedClass.SetDescendantCount();
				universalClass.DescendantCount += derivedClass.DescendantCount;
			}
		}
		else if (universalClass.DescendantCount < 1 && universalClass.DerivedClasses.Count == 0)
		{
			universalClass.DescendantCount = 1;
		}
	}
}