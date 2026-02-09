using AsmResolver.DotNet;

namespace AssetRipper.Export.UnityProjects.Scripts.AssemblyDefinitions;

public readonly struct AssemblyDefinitionDetails : IEquatable<AssemblyDefinitionDetails>
{
	public readonly string AssemblyName;
	public readonly string OutputFolder;
	public readonly AssemblyDefinition? Assembly;

	public AssemblyDefinitionDetails(AssemblyDefinition assembly, string outputFolder)
	{
		AssemblyName = assembly.Name!;
		OutputFolder = outputFolder;
		Assembly = assembly;
	}

	public AssemblyDefinitionDetails(string assemblyName, string outputFolder)
	{
		AssemblyName = assemblyName;
		OutputFolder = outputFolder;
		Assembly = null;
	}

	public override bool Equals(object? obj)
	{
		return obj is AssemblyDefinitionDetails details && Equals(details);
	}

	public bool Equals(AssemblyDefinitionDetails other)
	{
		return AssemblyName == other.AssemblyName &&
			   OutputFolder == other.OutputFolder &&
			   EqualityComparer<AssemblyDefinition?>.Default.Equals(Assembly, other.Assembly);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(AssemblyName, OutputFolder, Assembly);
	}

	public static bool operator ==(AssemblyDefinitionDetails left, AssemblyDefinitionDetails right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(AssemblyDefinitionDetails left, AssemblyDefinitionDetails right)
	{
		return !(left == right);
	}
}
