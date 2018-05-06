using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class BlendTree : Motion
	{
		public BlendTree(AssetInfo assetsInfo) :
			base(assetsInfo)
		{
		}

		private static AssetInfo CreateAssetsInfo(ISerializedFile file)
		{
			return new AssetInfo(file, 0, ClassIDType.BlendTree);
		}

		public override void Read(AssetStream stream)
		{
			throw new NotSupportedException();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Childs", Childs.ExportYAML(container));
			node.Add("m_BlendParameter", BlendParameter);
			node.Add("m_BlendParameterY", BlendParameterY);
			node.Add("m_MinThreshold", MinThreshold);
			node.Add("m_MaxThreshold", MaxThreshold);
			node.Add("m_UseAutomaticThresholds", UseAutomaticThresholds);
			node.Add("m_NormalizedBlendValues", NormalizedBlendValues);
			node.Add("m_BlendType", BlendType);
			return node;
		}

		public override string ExportExtension => throw new NotSupportedException();

		public IReadOnlyList<ChildMotion> Childs => m_childs;
		public string BlendParameter { get; private set; }
		public string BlendParameterY { get; private set; }
		public float MinThreshold { get; private set; }
		public float MaxThreshold { get; private set; }
		public bool UseAutomaticThresholds { get; private set; }
		public bool NormalizedBlendValues { get; private set; }
		public int BlendType { get; private set; }

		private ChildMotion[] m_childs;
	}
}
