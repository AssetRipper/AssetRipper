using AssetRipper.Assets.Metadata;
using AssetRipper.Assets.Utils;
using AssetRipper.Import.Structure.Assembly;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.Processing.Utils;

/// <summary>
/// Attempts to recover field paths from <see cref="uint"/> hash values.
/// </summary>
/// <remarks>
/// Replicates Unity CRC32 checksum usage for field names and paths.
/// </remarks>
public readonly struct PathProcessor
{
	public PathProcessor(IAssemblyManager assemblyManager)
	{
		this.assemblyManager = assemblyManager;
	}

	private readonly Dictionary<string, uint> cachedPropertyNames = new();
	private readonly Dictionary<uint, string> cachedChecksums = new();

	private readonly HashSet<AssetInfo> processedAssets = new();

	private readonly IAssemblyManager assemblyManager;

	public uint Add(string path)
	{
		if (cachedPropertyNames.TryGetValue(path, out uint value))
		{
			return value;
		}

		uint output = CrcUtils.CalculateDigestUTF8(path);

		AddKeys(output, path);
		return output;
	}

	public void Add(IMonoScript script)
	{
		if (!processedAssets.Add(script.AssetInfo))
		{
			return;
		}

		SerializableType? behaviour = script.GetBehaviourType(assemblyManager);

		if (behaviour is null)
		{
			return;
		}

		for (int f = 0; f < behaviour.Fields.Count; f++)
		{
			SerializableType.Field field = behaviour.Fields[f];

			Add(field.Name);
		}
	}

	public bool TryGetPath(uint identifier, [NotNullWhen(true)] out string? path)
	{
		return cachedChecksums.TryGetValue(identifier, out path);
	}

	public void Reset()
	{
		cachedPropertyNames.Clear();
		cachedChecksums.Clear();
		processedAssets.Clear();
	}

	private void AddKeys(uint checksum, string propertyName)
	{
		cachedPropertyNames[propertyName] = checksum;
		cachedChecksums[checksum] = propertyName;
	}
}
