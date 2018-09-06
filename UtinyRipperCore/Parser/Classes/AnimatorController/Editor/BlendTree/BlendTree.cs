using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers.Editor
{
	public sealed class BlendTree : Motion
	{
		public BlendTree(VirtualSerializedFile file, AnimatorController controller, StateConstant state, int nodeIndex) :
			base(file.CreateAssetInfo(ClassIDType.BlendTree))
		{
			BlendTreeNodeConstant node = state.GetBlendTree().NodeArray[nodeIndex].Instance;

			ObjectHideFlags = 1;
			Name = nameof(BlendTree);

			m_childs = new ChildMotion[node.ChildIndices.Count];
			for(int i = 0; i < node.ChildIndices.Count; i++)
			{
				m_childs[i] = new ChildMotion(file, controller, state, nodeIndex, i);
			}

			BlendParameter = node.BlendEventID == uint.MaxValue ? string.Empty : controller.TOS[node.BlendEventID];
			BlendParameterY = node.BlendEventYID == uint.MaxValue ? string.Empty : controller.TOS[node.BlendEventYID];
			MinThreshold = node.GetMinThreshold(controller.File.Version);
			MaxThreshold = node.GetMaxThreshold(controller.File.Version);
			UseAutomaticThresholds = false;
			NormalizedBlendValues = node.BlendDirectData.Instance.NormalizedBlendValues;
			BlendType = node.BlendType;

			file.AddAsset(this);
		}

		public override void Read(AssetReader reader)
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
			node.Add("m_BlendType", (int)BlendType);
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
		public BlendTreeType BlendType { get; private set; }

		private ChildMotion[] m_childs;
	}
}
