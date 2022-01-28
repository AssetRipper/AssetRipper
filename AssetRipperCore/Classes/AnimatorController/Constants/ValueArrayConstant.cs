using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class ValueArrayConstant : IAssetReadable, IYAMLExportable
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
