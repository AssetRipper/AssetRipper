﻿using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.Classes.Object;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;

namespace AssetRipper.Core.Classes.BlendTree
{
	public sealed class BlendTree : Motion
	{
		private BlendTree(LayoutInfo layout, AssetInfo assetInfo, AnimatorController.AnimatorController controller, StateConstant state, int nodeIndex) : base(layout)
		{
			AssetInfo = assetInfo;
			ObjectHideFlags = HideFlags.HideInHierarchy;

			VirtualSerializedFile virtualFile = (VirtualSerializedFile)assetInfo.File;
			BlendTreeNodeConstant node = state.GetBlendTree().NodeArray[nodeIndex].Instance;

			Name = nameof(BlendTree);

			Childs = new ChildMotion[node.ChildIndices.Length];
			for (int i = 0; i < node.ChildIndices.Length; i++)
			{
				Childs[i] = new ChildMotion(virtualFile, controller, state, nodeIndex, i);
			}

			BlendParameter = node.BlendEventID == uint.MaxValue ? string.Empty : controller.TOS[node.BlendEventID];
			BlendParameterY = node.BlendEventYID == uint.MaxValue ? string.Empty : controller.TOS[node.BlendEventYID];
			MinThreshold = node.GetMinThreshold(controller.SerializedFile.Version);
			MaxThreshold = node.GetMaxThreshold(controller.SerializedFile.Version);
			UseAutomaticThresholds = false;
			NormalizedBlendValues = node.BlendDirectData.Instance.m_NormalizedBlendValues;
			BlendType = node.BlendType;
		}

		public static BlendTree CreateVirtualInstance(VirtualSerializedFile virtualFile, AnimatorController.AnimatorController controller, StateConstant state, int nodeIndex)
		{
			return virtualFile.CreateAsset((assetInfo) => new BlendTree(virtualFile.Layout, assetInfo, controller, state, nodeIndex));
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

		public ChildMotion[] Childs { get; set; }
		public string BlendParameter { get; set; }
		public string BlendParameterY { get; set; }
		public float MinThreshold { get; set; }
		public float MaxThreshold { get; set; }
		public bool UseAutomaticThresholds { get; set; }
		public bool NormalizedBlendValues { get; set; }
		public BlendTreeType BlendType { get; set; }

		public const string ChildsName = "m_Childs";
		public const string BlendParameterName = "m_BlendParameter";
		public const string BlendParameterYName = "m_BlendParameterY";
		public const string MinThresholdName = "m_MinThreshold";
		public const string MaxThresholdName = "m_MaxThreshold";
		public const string UseAutomaticThresholdsName = "m_UseAutomaticThresholds";
		public const string NormalizedBlendValuesName = "m_NormalizedBlendValues";
		public const string BlendTypeName = "m_BlendType";
	}
}
