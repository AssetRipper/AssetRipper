using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class ValueArrayConstant : IAssetReadable, IYamlExportable
	{
		public void Read(AssetReader reader)
		{
			ValueArray = reader.ReadAssetArray<ValueConstant>();
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public ValueConstant[] ValueArray { get; set; }
	}
}
