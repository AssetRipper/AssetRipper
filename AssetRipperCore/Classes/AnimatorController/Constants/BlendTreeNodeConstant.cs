using AssetRipper.Core.Classes.BlendTree;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Linq;


namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	public sealed class BlendTreeNodeConstant : IAssetReadable, IYAMLExportable
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
				return new();
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
					return Blend1dData.Instance.ChildThresholdArray[index];
				}
			}
			return 0.0f;
		}

		public Vector2f GetPosition(UnityVersion version, int index)
		{
			if (HasBlendData(version))
			{
				if (BlendType != BlendTreeType.Simple1D && BlendType != BlendTreeType.Direct)
				{
					return Blend2dData.Instance.m_ChildPositionArray[index];
				}
			}
			return new();
		}

		public float GetMinThreshold(UnityVersion version)
		{
			if (HasBlendData(version))
			{
				if (BlendType == BlendTreeType.Simple1D)
				{
					return Blend1dData.Instance.ChildThresholdArray.Min();
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
					return Blend1dData.Instance.ChildThresholdArray.Max();
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

		public OffsetPtr<Blend1dDataConstant> Blend1dData = new();
		public OffsetPtr<Blend2dDataConstant> Blend2dData = new();
		public OffsetPtr<BlendDirectDataConstant> BlendDirectData = new();
	}
}
