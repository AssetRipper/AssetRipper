using uTinyRipper.Classes;

namespace uTinyRipper.Game.Assembly
{
	public interface ISerializableStructure : IAsset, IDependent
	{
		ISerializableStructure CreateDuplicate();
		//int CalculateSize(int depth);
	}
}
