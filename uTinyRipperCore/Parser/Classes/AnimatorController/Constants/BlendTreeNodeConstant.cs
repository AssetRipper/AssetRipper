using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AnimatorControllers.Editor;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct BlendTreeNodeConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool IsReadBlendType(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool IsReadBlendEventYID(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}
		/// <summary>
		/// 4.0.x
		/// </summary>
		public static bool IsReadChildThresholdArray(Version version)
		{
			return version.IsLess(4, 1);
		}
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool IsReadBlendData(Version version)
		{
			return version.IsGreaterEqual(4, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadBlendDirectData(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		/// <summary>
		/// 4.5.2 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadClipIndex(Version version)
		{
			return version.IsGreaterEqual(4, 5, 1, VersionType.Patch, 3) && version.IsLess(5);
		}
		/// <summary>
		/// 4.1.3 and greater
		/// </summary>
		public static bool IsReadCycleOffset(Version version)
		{
			return version.IsGreaterEqual(4, 1, 3);
		}
		
		public void Read(AssetReader reader)
		{
			if (IsReadBlendType(reader.Version))
			{
				BlendType = (BlendTreeType)reader.ReadUInt32();
			}
			BlendEventID = reader.ReadUInt32();
			if (IsReadBlendEventYID(reader.Version))
			{
				BlendEventYID = reader.ReadUInt32();
			}
			m_childIndices = reader.ReadUInt32Array();
			if (IsReadChildThresholdArray(reader.Version))
			{
				m_childThresholdArray = reader.ReadSingleArray();
			}

			if (IsReadBlendData(reader.Version))
			{
				Blend1dData.Read(reader);
				Blend2dData.Read(reader);
			}
			if(IsReadBlendDirectData(reader.Version))
			{
				BlendDirectData.Read(reader);
			}

			ClipID = reader.ReadUInt32();
			if (IsReadClipIndex(reader.Version))
			{
				ClipIndex = reader.ReadUInt32();
			}

			Duration = reader.ReadSingle();
			if (IsReadCycleOffset(reader.Version))
			{
				CycleOffset = reader.ReadSingle();
				Mirror = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}
		
		public PPtr<Motion> CreateMotion(AnimatorController controller, int clipIndex)
		{
			if (clipIndex == -1)
			{
				return default;
			}
			else
			{
				return controller.AnimationClips[clipIndex].CastTo<Motion>();
			}
		}

		public float GetThreshold(Version version, int index)
		{
			if (IsReadBlendData(version))
			{
				if(BlendType == BlendTreeType.Simple1D)
				{
					return Blend1dData.Instance.ChildThresholdArray[index];
				}
			}
			return 0.0f;
		}

		public float GetMinThreshold(Version version)
		{
			if (IsReadBlendData(version))
			{
				if (BlendType == BlendTreeType.Simple1D)
				{
					return Blend1dData.Instance.ChildThresholdArray.Min();
				}
			}
			return 0.0f;
		}

		public float GetMaxThreshold(Version version)
		{
			if (IsReadBlendData(version))
			{
				if(BlendType == BlendTreeType.Simple1D)
				{
					return Blend1dData.Instance.ChildThresholdArray.Max();
				}
			}
			return 1.0f;
		}
		
		public uint GetDirectBlendParameter(Version version, int index)
		{
			if(IsReadBlendDirectData(version))
			{
				if(BlendType == BlendTreeType.Direct)
				{
					return BlendDirectData.Instance.ChildBlendEventIDArray[index];
				}
			}
			return 0;
		}
		
		public bool IsBlendTree => ChildIndices.Count > 0;

		public BlendTreeType BlendType { get; private set; }
		public uint BlendEventID { get; private set; }
		public uint BlendEventYID { get; private set; }
		public IReadOnlyList<uint> ChildIndices => m_childIndices;
		public IReadOnlyList<float> ChildThresholdArray => m_childThresholdArray;
		public uint ClipID { get; private set; }
		public uint ClipIndex { get; private set; }
		public float Duration { get; private set; }
		public float CycleOffset { get; private set; }
		public bool Mirror { get; private set; }

		public OffsetPtr<Blend1dDataConstant> Blend1dData;
		public OffsetPtr<Blend2dDataConstant> Blend2dData;
		public OffsetPtr<BlendDirectDataConstant> BlendDirectData;

		private uint[] m_childIndices;
		private float[] m_childThresholdArray;
	}
}
