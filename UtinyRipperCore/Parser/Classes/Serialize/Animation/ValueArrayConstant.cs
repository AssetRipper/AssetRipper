using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes
{
	public struct ValueArrayConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_valueArray = reader.ReadArray<ValueConstant>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}
		
		public IReadOnlyList<ValueConstant> ValueArray => m_valueArray;
		
		private ValueConstant[] m_valueArray;
	}
}
