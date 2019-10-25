using System;
using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct ValueArrayConstant : IAssetReadable, IYAMLExportable
	{
		public void Read(AssetReader reader)
		{
			m_valueArray = reader.ReadAssetArray<ValueConstant>();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<ValueConstant> ValueArray => m_valueArray;

		private ValueConstant[] m_valueArray;
	}
}
