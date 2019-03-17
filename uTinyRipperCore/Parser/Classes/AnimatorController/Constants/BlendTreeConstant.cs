using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
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

		public void Read(AssetReader reader)
		{
			m_nodeArray = reader.ReadAssetArray<OffsetPtr<BlendTreeNodeConstant>>();
			if (IsReadBlendEventArrayConstant(reader.Version))
			{
				BlendEventArrayConstant.Read(reader);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<OffsetPtr<BlendTreeNodeConstant>> NodeArray => m_nodeArray;

		public OffsetPtr<ValueArrayConstant> BlendEventArrayConstant;

		private OffsetPtr<BlendTreeNodeConstant>[] m_nodeArray;
	}
}
