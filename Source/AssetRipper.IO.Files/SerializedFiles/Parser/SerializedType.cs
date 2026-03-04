using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

public sealed class SerializedType : SerializedTypeBase, IEquatable<SerializedType?>
{
	private static UnityVersion WriteIDHashForScriptTypeVersion => new UnityVersion(2018, 3, 0, UnityVersionType.Alpha, 1);

	public int[] TypeDependencies { get; set; } = [];

	protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, UnityVersion unityVersion)
	{
		//This code is most likely correct, but not guaranteed.
		//Reverse engineering it was painful, and it's possible that mistakes were made.
		return !unityVersion.Equals(0, 0, 0) && unityVersion < WriteIDHashForScriptTypeVersion;
	}

	private protected override void ReadTypeDependencies(SerializedReader reader)
	{
		TypeDependencies = reader.ReadInt32Array();
	}

	private protected override void WriteTypeDependencies(SerializedWriter writer)
	{
		writer.WriteArray(TypeDependencies);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as SerializedType);
	}

	public bool Equals(SerializedType? other)
	{
		return other is not null
			&& RawTypeID == other.RawTypeID
			&& IsStrippedType == other.IsStrippedType
			&& ScriptTypeIndex == other.ScriptTypeIndex
			&& OldType.Equals(other.OldType)
			&& ScriptID == other.ScriptID
			&& OldTypeHash == other.OldTypeHash
			&& TypeDependencies.AsSpan().SequenceEqual(other.TypeDependencies);
	}

	public override int GetHashCode()
	{
		HashCode hash = new();
		hash.Add(RawTypeID);
		hash.Add(IsStrippedType);
		hash.Add(ScriptTypeIndex);
		hash.Add(OldType);
		hash.Add(ScriptID);
		hash.Add(OldTypeHash);
		foreach (int i in TypeDependencies)
		{
			hash.Add(i);
		}
		return hash.ToHashCode();
	}

	public static bool operator ==(SerializedType? left, SerializedType? right)
	{
		return EqualityComparer<SerializedType>.Default.Equals(left, right);
	}

	public static bool operator !=(SerializedType? left, SerializedType? right)
	{
		return !(left == right);
	}
}
