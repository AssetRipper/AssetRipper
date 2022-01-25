using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public struct StateConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		public static bool HasLeafInfo(UnityVersion version) => version.IsLess(5, 2);
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasPathID(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasFullPathID(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool HasSpeedParam(UnityVersion version) => version.IsGreaterEqual(5, 1);
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool HasTimeParam(UnityVersion version) => version.IsGreaterEqual(2017, 2);
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool HasCycleOffset(UnityVersion version) => version.IsGreaterEqual(4, 1);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasDefaultValues(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool HasMirror(UnityVersion version) => version.IsGreaterEqual(4, 1);

		public bool GetWriteDefaultValues(UnityVersion version)
		{
			return HasDefaultValues(version) ? WriteDefaultValues : true;
		}
		public uint GetID(UnityVersion version)
		{
			return HasFullPathID(version) ? FullPathID : NameID;
		}

		public void Read(AssetReader reader)
		{
			TransitionConstantArray = reader.ReadAssetArray<OffsetPtr<TransitionConstant>>();
			BlendTreeConstantIndexArray = reader.ReadInt32Array();
			if (HasLeafInfo(reader.Version))
			{
				LeafInfoArray = reader.ReadAssetArray<LeafInfoConstant>();
			}

			BlendTreeConstantArray = reader.ReadAssetArray<OffsetPtr<BlendTreeConstant>>();
			NameID = reader.ReadUInt32();
			if (HasPathID(reader.Version))
			{
				PathID = reader.ReadUInt32();
			}
			if (HasFullPathID(reader.Version))
			{
				FullPathID = reader.ReadUInt32();
			}

			TagID = reader.ReadUInt32();
			if (HasSpeedParam(reader.Version))
			{
				SpeedParamID = reader.ReadUInt32();
				MirrorParamID = reader.ReadUInt32();
				CycleOffsetParamID = reader.ReadUInt32();
			}
			if (HasTimeParam(reader.Version))
			{
				TimeParamID = reader.ReadUInt32();
			}

			Speed = reader.ReadSingle();
			if (HasCycleOffset(reader.Version))
			{
				CycleOffset = reader.ReadSingle();
			}

			IKOnFeet = reader.ReadBoolean();
			if (HasDefaultValues(reader.Version))
			{
				WriteDefaultValues = reader.ReadBoolean();
			}

			Loop = reader.ReadBoolean();
			if (HasMirror(reader.Version))
			{
				Mirror = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public bool IsBlendTree(UnityVersion version)
		{
			if (BlendTreeConstantArray.Length == 0)
			{
				return false;
			}
			return GetBlendTree().NodeArray.Length > 1;
		}

		public BlendTreeConstant GetBlendTree()
		{
			return BlendTreeConstantArray[0].Instance;
		}

		/*public PPtr<Motion> CreateMotion(VirtualSerializedFile file, AnimatorController controller)
		{
			if (IsBlendTree(controller.File.Version))
			{
				BlendTree blendTree = new BlendTree(file, controller, GetBlendTree(), 0);
				return PPtr<Motion>.CreateVirtualPointer(blendTree);
			}
			else
			{
				return CreateMotion(file, controller, 0);
			}
		}*/

		public PPtr<Motion> CreateMotion(VirtualSerializedFile file, AnimatorController controller, int nodeIndex)
		{
			if (BlendTreeConstantArray.Length == 0)
			{
				return default;
			}
			else
			{
				BlendTreeNodeConstant node = GetBlendTree().NodeArray[nodeIndex].Instance;
				if (node.IsBlendTree)
				{
					BlendTree.BlendTree blendTree = BlendTree.BlendTree.CreateVirtualInstance(file, controller, this, nodeIndex);
					return blendTree.SerializedFile.CreatePPtr(blendTree).CastTo<Motion>();
				}
				else
				{
					int clipIndex = -1;
					if (HasLeafInfo(controller.SerializedFile.Version))
					{
						for (int i = 0; i < LeafInfoArray.Length; i++)
						{
							LeafInfoConstant leafInfo = LeafInfoArray[i];
							int index = leafInfo.m_IDArray.IndexOf(node.ClipID);
							if (index >= 0)
							{
								clipIndex = (int)leafInfo.m_IndexOffset + index;
								break;
							}
						}
					}
					else
					{
						clipIndex = unchecked((int)node.ClipID);
					}
					return node.CreateMotion(controller, clipIndex);
				}
			}
		}

		public OffsetPtr<TransitionConstant>[] TransitionConstantArray { get; set; }
		public int[] BlendTreeConstantIndexArray { get; set; }
		public LeafInfoConstant[] LeafInfoArray { get; set; }
		public OffsetPtr<BlendTreeConstant>[] BlendTreeConstantArray { get; set; }
		/// <summary>
		/// ID previously
		/// </summary>
		public uint NameID { get; set; }
		public uint PathID { get; set; }
		public uint FullPathID { get; set; }
		public uint TagID { get; set; }
		public uint SpeedParamID { get; set; }
		public uint MirrorParamID { get; set; }
		public uint CycleOffsetParamID { get; set; }
		public uint TimeParamID { get; set; }
		public float Speed { get; set; }
		public float CycleOffset { get; set; }
		public bool IKOnFeet { get; set; }
		public bool WriteDefaultValues { get; set; }
		public bool Loop { get; set; }
		public bool Mirror { get; set; }

	}
}
