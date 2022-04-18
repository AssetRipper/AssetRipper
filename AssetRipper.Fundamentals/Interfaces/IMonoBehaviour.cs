using AssetRipper.Core.Structure.Assembly.Serializable;

namespace AssetRipper.Core.Interfaces
{
	public interface IMonoBehaviour
	{
		SerializableStructure Structure { get; set; }
	}
}
