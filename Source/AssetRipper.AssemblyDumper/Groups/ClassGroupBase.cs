using AssetRipper.AssemblyDumper.Types;
using AssetRipper.DocExtraction.DataStructures;
using System.Diagnostics;

namespace AssetRipper.AssemblyDumper.Groups;

internal abstract class ClassGroupBase
{
	private TypeDefinition? mainClass;
	public List<GeneratedClassInstance> Instances { get; } = new();
	public TypeDefinition Interface { get; }
	public List<InterfaceProperty> InterfaceProperties { get; } = new();
	public ComplexTypeHistory? History { get; set; }

	public abstract string Name { get; }
	public abstract string Namespace { get; }
	public abstract int ID { get; }
	public abstract bool IsSealed { get; }
	public abstract bool UniformlyNamed { get; }
	public virtual bool IsPPtr => false;
	public virtual bool IsString => false;
	public UnityVersion MinimumVersion => Instances[0].VersionRange.Start;
	public IEnumerable<UniversalClass> Classes => Instances.Select(x => x.Class);
	public IEnumerable<TypeDefinition> Types => Instances.Select(x => x.Type);

	protected ClassGroupBase(TypeDefinition @interface)
	{
		Interface = @interface ?? throw new ArgumentNullException(nameof(@interface));
	}

	public TypeDefinition GetOrCreateMainClass()
	{
		mainClass ??= Instances.Count == 1 ? Instances[0].Type : StaticClassCreator.CreateEmptyStaticClass(Interface.DeclaringModule!, Namespace, Name);
		return mainClass;
	}

	public UniversalClass GetClassForVersion(UnityVersion version)
	{
		return GetInstanceForVersion(version).Class;
	}

	public TypeDefinition GetTypeForVersion(UnityVersion version)
	{
		return GetInstanceForVersion(version).Type;
	}

	public GeneratedClassInstance GetInstanceForVersion(UnityVersion version)
	{
		Debug.Assert(Instances.Count != 0, "No classes available");
		foreach (GeneratedClassInstance instance in Instances)
		{
			if (instance.VersionRange.Contains(version))
			{
				return instance;
			}
		}
		throw new Exception($"No instance found for {version}");
	}

	public TypeDefinition GetSingularTypeOrInterface()
	{
		return Instances.Count == 1
			? Instances[0].Type
			: Interface;
	}

	public override string ToString() => Name;

	public void GetSerializedVersions(out int minimum, out int maximum)
	{
		minimum = 1;
		maximum = 1;
		foreach (GeneratedClassInstance instance in Instances)
		{
			int instanceVersion = instance.GetSerializedVersion();
			if (instanceVersion < minimum)
			{
				minimum = instanceVersion;
			}
			else if (instanceVersion > maximum)
			{
				maximum = instanceVersion;
			}
		}
	}

	public void InitializeHistory(HistoryFile historyFile)
	{
		History = null;

		foreach (GeneratedClassInstance instance in Instances)
		{
			instance.InitializeHistory(historyFile);
		}

		ComplexTypeHistory? firstHistory = Instances[0].History;
		if (firstHistory is not null)
		{
			for (int i = 1; i < Instances.Count; i++)
			{
				ComplexTypeHistory? subsequentHistory = Instances[i].History;
				if (firstHistory != subsequentHistory)
				{
					return;
				}
			}
			History = firstHistory;
		}
	}
}
