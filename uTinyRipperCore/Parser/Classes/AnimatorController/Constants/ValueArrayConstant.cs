using System;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
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
