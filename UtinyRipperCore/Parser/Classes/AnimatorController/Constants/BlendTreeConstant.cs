using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct BlendTreeConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 4.5.0
		/// </summary>
		public static bool IsReadBlendEventArrayConstant(Version version)
		{
			return version.IsLess(4, 5);
		}

		public void Read(AssetStream stream)
		{
			m_nodeArray = stream.ReadArray<OffsetPtr<BlendTreeNodeConstant>>();
			if (IsReadBlendEventArrayConstant(stream.Version))
			{
				BlendEventArrayConstant.Read(stream);
			}
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<OffsetPtr<BlendTreeNodeConstant>> NodeArray => m_nodeArray;

		public OffsetPtr<ValueArrayConstant> BlendEventArrayConstant;

		private OffsetPtr<BlendTreeNodeConstant>[] m_nodeArray;
	}
}
