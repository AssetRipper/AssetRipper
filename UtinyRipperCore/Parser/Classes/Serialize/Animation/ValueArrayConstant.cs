using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct ValueArrayConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetStream stream)
		{
			m_valueArray = stream.ReadArray<ValueConstant>();
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}
		
		public IReadOnlyList<ValueConstant> ValueArray => m_valueArray;
		
		private ValueConstant[] m_valueArray;
	}
}
