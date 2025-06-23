using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser;

public sealed class SerializedType : SerializedTypeBase
{
	public int[] TypeDependencies { get; set; } = Array.Empty<int>();

	protected override bool IgnoreScriptTypeForHash(FormatVersion formatVersion, UnityVersion unityVersion)
	{
		//This code is most likely correct, but not guaranteed.
		//Reverse engineering it was painful, and it's possible that mistakes were made.
		return !unityVersion.Equals(0, 0, 0) && unityVersion < WriteIDHashForScriptTypeVersion;
	}

	protected override void ReadTypeDependencies(SerializedReader reader)
	{
		TypeDependencies = reader.ReadInt32Array();
	}

	protected override void WriteTypeDependencies(SerializedWriter writer)
	{
		writer.WriteArray(TypeDependencies);
	}

	private static UnityVersion WriteIDHashForScriptTypeVersion { get; } = new UnityVersion(2018, 3, 0, UnityVersionType.Alpha, 1);
}
