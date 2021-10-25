namespace AssetRipper.Core.Classes
{
	public interface INamedObject
	{
		string Name { get; set; }
		string ValidName { get; }
	}
}