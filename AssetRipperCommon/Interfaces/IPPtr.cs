using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Interfaces
{
	//Might remove
	public interface IPPtr<T> where T : UnityObjectBase
	{
		bool IsNull { get; }
		bool IsVirtual { get; }

		IPPtr<T1> CastTo<T1>() where T1 : UnityObjectBase;
		YAMLNode ExportYAML();
		//T FindAsset(IAssetContainer file);
		//T GetAsset(IAssetContainer file);
		//bool IsAsset(IAssetContainer file, UnityObjectBase asset);
		bool IsAsset(UnityObjectBase asset);
		//bool IsValid(IExportContainer container);
		void Read(AssetReader reader);
		//string ToLogString(IAssetContainer container);
		string ToString();
		//T TryGetAsset(IAssetContainer file);
		void Write(AssetWriter writer);
	}
}