using System;
using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Classes.Objects;
using uTinyRipper.Converters;
using uTinyRipper.Classes.AnimatorControllers;
using uTinyRipper.Classes.BlendTrees;

namespace uTinyRipper.Classes
{
	public sealed class BlendTree : Motion
	{
		private BlendTree(AssetInfo assetInfo, AnimatorController controller, StateConstant state, int nodeIndex) :
			base(assetInfo, HideFlags.HideInHierarchy)
		{
			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;
			BlendTreeNodeConstant node = state.GetBlendTree().NodeArray[nodeIndex].Instance;

			Name = nameof(BlendTree);

			m_childs = new ChildMotion[node.ChildIndices.Count];
			for (int i = 0; i < node.ChildIndices.Count; i++)
			{
				m_childs[i] = new ChildMotion(virtualFile, controller, state, nodeIndex, i);
			}

			BlendParameter = node.BlendEventID == uint.MaxValue ? string.Empty : controller.TOS[node.BlendEventID];
			BlendParameterY = node.BlendEventYID == uint.MaxValue ? string.Empty : controller.TOS[node.BlendEventYID];
			MinThreshold = node.GetMinThreshold(controller.File.Version);
			MaxThreshold = node.GetMaxThreshold(controller.File.Version);
			UseAutomaticThresholds = false;
			NormalizedBlendValues = node.BlendDirectData.Instance.NormalizedBlendValues;
			BlendType = node.BlendType;
		}

		public static BlendTree CreateVirtualInstance(VirtualSerializedFile virtualFile, AnimatorController controller, StateConstant state, int nodeIndex)
		{
			return virtualFile.CreateAsset((assetInfo) => new BlendTree(assetInfo, controller, state, nodeIndex));
		}

		public override void Read(AssetReader reader)
		{
			throw new NotSupportedException();
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(ChildsName, Childs.ExportYAML(container));
			node.Add(BlendParameterName, BlendParameter);
			node.Add(BlendParameterYName, BlendParameterY);
			node.Add(MinThresholdName, MinThreshold);
			node.Add(MaxThresholdName, MaxThreshold);
			node.Add(UseAutomaticThresholdsName, UseAutomaticThresholds);
			node.Add(NormalizedBlendValuesName, NormalizedBlendValues);
			node.Add(BlendTypeName, (int)BlendType);
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

		public const string ChildsName = "m_Childs";
		public const string BlendParameterName = "m_BlendParameter";
		public const string BlendParameterYName = "m_BlendParameterY";
		public const string MinThresholdName = "m_MinThreshold";
		public const string MaxThresholdName = "m_MaxThreshold";
		public const string UseAutomaticThresholdsName = "m_UseAutomaticThresholds";
		public const string NormalizedBlendValuesName = "m_NormalizedBlendValues";
		public const string BlendTypeName = "m_BlendType";

		private ChildMotion[] m_childs;
	}
}
