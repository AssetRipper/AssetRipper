using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Interfaces
{
	public interface IUnityObjectBase : IUnityAssetBase
	{
		AssetInfo AssetInfo { get; set; }
		string AssetClassName { get; }
		ClassIDType ClassID { get; }
		string? OriginalPath { get; set; }
		string? OriginalDirectory { get; set; }
		string? OriginalName { get; set; }
		string? OriginalExtension { get; set; }
		string? AssetBundleName { get; set; }
		ISerializedFile SerializedFile { get; }
		UnityGUID GUID { get; set; }
		long PathID { get; }
		YamlDocument ExportYamlDocument(IExportContainer container);
	}

	public static class UnityObjectBaseExtensions
	{
		public static string GetOriginalName(this IUnityObjectBase _this)
		{
			if (_this is IHasNameString named)
			{
				return named.NameString;
			}
			else
			{
				throw new Exception($"Unable to get name for {_this.ClassID}");
			}
		}

		public static string? TryGetName(this IUnityObjectBase _this)
		{
			if (_this is IHasNameString named)
			{
				return named.NameString;
			}
			else
			{
				return null;
			}
		}
	}
}
