using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimatorControllers.Editor;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct StateConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// Less than 5.2.0
		/// </summary>
		public static bool IsReadLeafInfo(Version version)
		{
			return version.IsLess(5, 2);
		}
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool IsReadPathID(Version version)
		{
			return version.IsGreaterEqual(4, 3);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadFullPathID(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 5.1.0 and greater
		/// </summary>
		public static bool IsReadSpeedParam(Version version)
		{
			return version.IsGreaterEqual(5, 1);
		}
		/// <summary>
		/// 2017.2 and greater
		/// </summary>
		public static bool IsReadTimeParam(Version version)
		{
			return version.IsGreaterEqual(2017, 2);
		}
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool IsReadCycleOffset(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadDefaultValues(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool IsReadMirror(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}

		public bool GetWriteDefaultValues(Version version)
		{
			return IsReadDefaultValues(version) ? WriteDefaultValues : true;
		}
		public uint GetID(Version version)
		{
			return IsReadFullPathID(version) ? FullPathID : NameID;
		}

		public void Read(AssetReader reader)
		{
			m_transitionConstantArray = reader.ReadAssetArray<OffsetPtr<TransitionConstant>>();
			m_blendTreeConstantIndexArray = reader.ReadInt32Array();
			if(IsReadLeafInfo(reader.Version))
			{
				m_leafInfoArray = reader.ReadAssetArray<LeafInfoConstant>();
			}

			m_blendTreeConstantArray = reader.ReadAssetArray<OffsetPtr<BlendTreeConstant>>();
			NameID = reader.ReadUInt32();
			if (IsReadPathID(reader.Version))
			{
				PathID = reader.ReadUInt32();
			}
			if(IsReadFullPathID(reader.Version))
			{
				FullPathID = reader.ReadUInt32();
			}

			TagID = reader.ReadUInt32();
			if(IsReadSpeedParam(reader.Version))
			{
				SpeedParamID = reader.ReadUInt32();
				MirrorParamID = reader.ReadUInt32();
				CycleOffsetParamID = reader.ReadUInt32();
			}
			if (IsReadTimeParam(reader.Version))
			{
				TimeParamID = reader.ReadUInt32();
			}

			Speed = reader.ReadSingle();
			if (IsReadCycleOffset(reader.Version))
			{
				CycleOffset = reader.ReadSingle();
			}

			IKOnFeet = reader.ReadBoolean();
			if (IsReadDefaultValues(reader.Version))
			{
				WriteDefaultValues = reader.ReadBoolean();
			}

			Loop = reader.ReadBoolean();
			if (IsReadMirror(reader.Version))
			{
				Mirror = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public bool IsBlendTree(Version version)
		{
			if (BlendTreeConstantArray.Count == 0)
			{
				return false;
			}
			return GetBlendTree().NodeArray.Count > 1;
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
			if (BlendTreeConstantArray.Count == 0)
			{
				return default;
			}
			else
			{
				BlendTreeNodeConstant node = GetBlendTree().NodeArray[nodeIndex].Instance;
				if (node.IsBlendTree)
				{
					BlendTree blendTree = BlendTree.CreateVirtualInstance(file, controller, this, nodeIndex);
					return blendTree.File.CreatePPtr(blendTree).CastTo<Motion>();
				}
				else
				{
					int clipIndex = -1;
					if (IsReadLeafInfo(controller.File.Version))
					{
						for(int i = 0; i < LeafInfoArray.Count; i++)
						{
							LeafInfoConstant leafInfo = LeafInfoArray[i];
							int index = leafInfo.IDArray.IndexOf(node.ClipID);
							if (index >= 0)
							{
								clipIndex = leafInfo.IndexOffset + index;
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

		public IReadOnlyList<OffsetPtr<TransitionConstant>> TransitionConstantArray => m_transitionConstantArray;
		public IReadOnlyList<int> BlendTreeConstantIndexArray => m_blendTreeConstantIndexArray;
		public IReadOnlyList<LeafInfoConstant> LeafInfoArray => m_leafInfoArray;
		public IReadOnlyList<OffsetPtr<BlendTreeConstant>> BlendTreeConstantArray => m_blendTreeConstantArray;
		/// <summary>
		/// ID previously
		/// </summary>
		public uint NameID { get; private set; }
		public uint PathID { get; private set; }
		public uint FullPathID { get; private set; }
		public uint TagID { get; private set; }
		public uint SpeedParamID { get; private set; }
		public uint MirrorParamID { get; private set; }
		public uint CycleOffsetParamID { get; private set; }
		public uint TimeParamID { get; private set; }
		public float Speed { get; private set; }
		public float CycleOffset { get; private set; }
		public bool IKOnFeet { get; private set; }
		public bool WriteDefaultValues { get; private set; }
		public bool Loop { get; private set; }
		public bool Mirror { get; private set; }
		
		private OffsetPtr<TransitionConstant>[] m_transitionConstantArray;
		private int[] m_blendTreeConstantIndexArray;
		private LeafInfoConstant[] m_leafInfoArray;
		private OffsetPtr<BlendTreeConstant>[] m_blendTreeConstantArray;
		
	}
}
