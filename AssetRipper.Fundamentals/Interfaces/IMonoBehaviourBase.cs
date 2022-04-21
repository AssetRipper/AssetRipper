using AssetRipper.Core.Structure.Assembly.Serializable;

namespace AssetRipper.Core.Interfaces
{
	public interface IMonoBehaviourBase
	{
		SerializableStructure Structure { get; set; }
	}
}
