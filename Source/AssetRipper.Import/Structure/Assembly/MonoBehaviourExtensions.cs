using AssetRipper.Import.Structure.Assembly.Serializable;
using AssetRipper.SourceGenerated.Classes.ClassID_114;

namespace AssetRipper.Import.Structure.Assembly;

public static class MonoBehaviourExtensions
{
	public static SerializableStructure? LoadStructure(this IMonoBehaviour monoBehaviour)
	{
		if (monoBehaviour.Structure is SerializableStructure structure)
		{
			return structure;
		}
		return (monoBehaviour.Structure as UnloadedStructure)?.LoadStructure();
	}
}
