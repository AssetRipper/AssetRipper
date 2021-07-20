using AssetRipper.Converters.Project;
using AssetRipper.Parser.IO.Asset;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.YAML;
using System;

namespace AssetRipper.Parser.Classes.AnimatorController.Constants
{
	public struct ValueArrayConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			ValueArray = reader.ReadAssetArray<ValueConstant>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public ValueConstant[] ValueArray { get; set; }
	}
}
