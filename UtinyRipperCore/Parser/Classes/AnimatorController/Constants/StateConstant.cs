using System;
using System.Collections.Generic;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AnimatorControllers.Editor;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimatorControllers
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

		public void Read(AssetStream stream)
		{
			m_transitionConstantArray = stream.ReadArray<OffsetPtr<TransitionConstant>>();
			m_blendTreeConstantIndexArray = stream.ReadInt32Array();
			if(IsReadLeafInfo(stream.Version))
			{
				m_leafInfoArray = stream.ReadArray<LeafInfoConstant>();
			}

			m_blendTreeConstantArray = stream.ReadArray<OffsetPtr<BlendTreeConstant>>();
			NameID = stream.ReadUInt32();
			if (IsReadPathID(stream.Version))
			{
				PathID = stream.ReadUInt32();
			}
			if(IsReadFullPathID(stream.Version))
			{
				FullPathID = stream.ReadUInt32();
			}

			TagID = stream.ReadUInt32();
			if(IsReadSpeedParam(stream.Version))
			{
				SpeedParamID = stream.ReadUInt32();
				MirrorParamID = stream.ReadUInt32();
				CycleOffsetParamID = stream.ReadUInt32();
			}
			if (IsReadTimeParam(stream.Version))
			{
				TimeParamID = stream.ReadUInt32();
			}

			Speed = stream.ReadSingle();
			if (IsReadCycleOffset(stream.Version))
			{
				CycleOffset = stream.ReadSingle();
			}

			IKOnFeet = stream.ReadBoolean();
			if (IsReadDefaultValues(stream.Version))
			{
				WriteDefaultValues = stream.ReadBoolean();
			}

			Loop = stream.ReadBoolean();
			if (IsReadMirror(stream.Version))
			{
				Mirror = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
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
					BlendTree blendTree = new BlendTree(file, controller, this, nodeIndex);
					return PPtr<Motion>.CreateVirtualPointer(blendTree);
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
