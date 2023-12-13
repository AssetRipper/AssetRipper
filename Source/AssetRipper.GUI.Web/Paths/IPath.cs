namespace AssetRipper.GUI.Web.Paths;

public interface IPath<TSelf>
{
	string ToJson();
	static abstract TSelf FromJson(string json);
}
