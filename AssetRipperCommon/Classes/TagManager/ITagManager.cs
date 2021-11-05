using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes.TagManager
{
	public interface ITagManager : IUnityObjectBase
	{
		string[] Tags { get; set; }
	}
}