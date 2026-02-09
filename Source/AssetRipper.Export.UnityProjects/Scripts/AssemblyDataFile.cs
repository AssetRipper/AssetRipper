using AssetRipper.SourceGenerated;
using System.Text.Json;

namespace AssetRipper.Export.UnityProjects.Scripts;

internal readonly record struct AssemblyDataFile(IReadOnlyList<UnityVersion> Versions, IReadOnlyList<KeyValuePair<UnityVersion, AssemblyData>> Assemblies)
{
	public static AssemblyDataFile Load()
	{
		return JsonSerializer.Deserialize(ReferenceAssembliesJson.GetStream(), AssemblyDataSerializerContext.Default.AssemblyDataFile);
	}

	public AssemblyData Get(UnityVersion version)
	{
		if (version < Assemblies[1].Key)
		{
			return Assemblies[0].Value;
		}
		if (version >= Assemblies[^1].Key)
		{
			return Assemblies[^1].Value;
		}

		// Binary search
		// We want to find the index of the last element that is less than or equal to the target.
		int low = 0;
		int high = Assemblies.Count - 1;
		while (low < high)
		{
			int mid = (low + high + 1) / 2;
			if (Assemblies[mid].Key <= version)
			{
				low = mid;
			}
			else
			{
				high = mid - 1;
			}
		}

		return Assemblies[low].Value;
	}
}
