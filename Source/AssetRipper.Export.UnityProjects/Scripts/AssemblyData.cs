namespace AssetRipper.Export.UnityProjects.Scripts;

internal readonly record struct AssemblyData(IReadOnlyList<string> Mono2, IReadOnlyList<string> Mono4, IReadOnlyList<string> Unity, IReadOnlyList<KeyValuePair<string, UnityGuid>> UnityExtensions)
{
	public bool Equals(AssemblyData other)
	{
		return Mono2.SequenceEqual(other.Mono2) && Mono4.SequenceEqual(other.Mono4) && Unity.SequenceEqual(other.Unity) && UnityExtensions.SequenceEqual(other.UnityExtensions);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		foreach (string assembly in Mono2)
		{
			hash.Add(assembly);
		}
		foreach (string assembly in Mono4)
		{
			hash.Add(assembly);
		}
		foreach (string assembly in Unity)
		{
			hash.Add(assembly);
		}
		foreach (KeyValuePair<string, UnityGuid> pair in UnityExtensions)
		{
			hash.Add(pair);
		}
		return hash.ToHashCode();
	}
}
