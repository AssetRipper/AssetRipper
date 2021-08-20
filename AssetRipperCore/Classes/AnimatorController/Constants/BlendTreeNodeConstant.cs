using AssetRipper.Core.Project;
using AssetRipper.Core.Classes.BlendTree;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.YAML;
using System;
using System.Linq;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;
using AssetRipper.Core.IO;

namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public struct BlendTreeNodeConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool HasBlendType(UnityVersion version) => version.IsGreaterEqual(4, 1);
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool HasBlendEventYID(UnityVersion version) => version.IsGreaterEqual(4, 1);
		/// <summary>
		/// 4.0.x
		/// </summary>
		public static bool HasChildThresholdArray(UnityVersion version) => version.IsLess(4, 1);
		/// <summary>
		/// 4.1.0 and greater
		/// </summary>
		public static bool HasBlendData(UnityVersion version) => version.IsGreaterEqual(4, 1);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasBlendDirectData(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 4.5.2 to 5.0.0 exclusive
		/// </summary>
		public static bool HasClipIndex(UnityVersion version) => version.IsGreaterEqual(4, 5, 1, UnityVersionType.Patch, 3) && version.IsLess(5);
		/// <summary>
		/// 4.1.3 and greater
		/// </summary>
		public static bool HasCycleOffset(UnityVersion version) => version.IsGreaterEqual(4, 1, 3);

		public BlendTreeNodeConstant(ObjectReader reader)
		{
			var version = reader.version;

			if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
			{
				BlendType = (BlendTreeType)reader.ReadUInt32();
			}
			else BlendType = default;
			BlendEventID = reader.ReadUInt32();
			if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
			{
				BlendEventYID = reader.ReadUInt32();
			}
			else BlendEventYID = 0;
			ChildIndices = reader.ReadUInt32Array();
			if (version[0] < 4 || (version[0] == 4 && version[1] < 1)) //4.1 down
			{
				ChildThresholdArray = reader.ReadSingleArray();
			}
			else ChildThresholdArray = new float[0];

			if (version[0] > 4 || (version[0] == 4 && version[1] >= 1)) //4.1 and up
			{
				new Blend1dDataConstant(reader);
				new Blend2dDataConstant(reader);
			}
			Blend1dData = default;
			Blend2dData = default;

			if (version[0] >= 5) //5.0 and up
			{
				new BlendDirectDataConstant(reader);
			}
			BlendDirectData = default;

			ClipID = reader.ReadUInt32();
			if (version[0] == 4 && version[1] >= 5) //4.5 - 5.0
			{
				ClipIndex = reader.ReadUInt32();
			}
			else ClipIndex = 0;

			Duration = reader.ReadSingle();

			if (version[0] > 4
				|| (version[0] == 4 && version[1] > 1)
				|| (version[0] == 4 && version[1] == 1 && version[2] >= 3)) //4.1.3 and up
			{
				CycleOffset = reader.ReadSingle();
				Mirror = reader.ReadBoolean();
				reader.AlignStream();
			}
			else
			{
				CycleOffset = 0;
				Mirror = false;
			}
		}

		public void Read(AssetReader reader)
		{
			if (HasBlendType(reader.Version))
			{
				BlendType = (BlendTreeType)reader.ReadUInt32();
			}
			BlendEventID = reader.ReadUInt32();
			if (HasBlendEventYID(reader.Version))
			{
				BlendEventYID = reader.ReadUInt32();
			}
			ChildIndices = reader.ReadUInt32Array();
			if (HasChildThresholdArray(reader.Version))
			{
				ChildThresholdArray = reader.ReadSingleArray();
			}

			if (HasBlendData(reader.Version))
			{
				Blend1dData.Read(reader);
				Blend2dData.Read(reader);
			}
			if (HasBlendDirectData(reader.Version))
			{
				BlendDirectData.Read(reader);
			}

			ClipID = reader.ReadUInt32();
			if (HasClipIndex(reader.Version))
			{
				ClipIndex = reader.ReadUInt32();
			}

			Duration = reader.ReadSingle();
			if (HasCycleOffset(reader.Version))
			{
				CycleOffset = reader.ReadSingle();
				Mirror = reader.ReadBoolean();
				reader.AlignStream();
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

		public float GetThreshold(UnityVersion version, int index)
		{
			if (HasBlendData(version))
			{
				if (BlendType == BlendTreeType.Simple1D)
				{
					return Blend1dData.Instance.m_ChildThresholdArray[index];
				}
			}
			return 0.0f;
		}

		public float GetMinThreshold(UnityVersion version)
		{
			if (HasBlendData(version))
			{
				if (BlendType == BlendTreeType.Simple1D)
				{
					return Blend1dData.Instance.m_ChildThresholdArray.Min();
				}
			}
			return 0.0f;
		}

		public float GetMaxThreshold(UnityVersion version)
		{
			if (HasBlendData(version))
			{
				if (BlendType == BlendTreeType.Simple1D)
				{
					return Blend1dData.Instance.m_ChildThresholdArray.Max();
				}
			}
			return 1.0f;
		}

		public uint GetDirectBlendParameter(UnityVersion version, int index)
		{
			if (HasBlendDirectData(version))
			{
				if (BlendType == BlendTreeType.Direct)
				{
					return BlendDirectData.Instance.m_ChildBlendEventIDArray[index];
				}
			}
			return 0;
		}

		public bool IsBlendTree => ChildIndices.Length > 0;

		public BlendTreeType BlendType { get; set; }
		public uint BlendEventID { get; set; }
		public uint BlendEventYID { get; set; }
		public uint[] ChildIndices { get; set; }
		public float[] ChildThresholdArray { get; set; }
		public uint ClipID { get; set; }
		public uint ClipIndex { get; set; }
		public float Duration { get; set; }
		public float CycleOffset { get; set; }
		public bool Mirror { get; set; }

		public OffsetPtr<Blend1dDataConstant> Blend1dData;
		public OffsetPtr<Blend2dDataConstant> Blend2dData;
		public OffsetPtr<BlendDirectDataConstant> BlendDirectData;
	}
}
