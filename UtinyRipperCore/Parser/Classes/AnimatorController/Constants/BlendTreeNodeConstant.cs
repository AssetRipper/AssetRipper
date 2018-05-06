using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
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
		/// Less than 5.0.0
		/// </summary>
		public static bool IsReadClipIndex(Version version)
		{
			return version.IsLess(5);
		}
		/// <summary>
		/// 4.1.3 and greater
		/// </summary>
		public static bool IsReadCycleOffset(Version version)
		{
			return version.IsGreaterEqual(4, 1, 3);
		}

		public void Read(AssetStream stream)
		{
			if (IsReadBlendType(stream.Version))
			{
				BlendType = stream.ReadUInt32();
			}
			BlendEventID = stream.ReadUInt32();
			if (IsReadBlendEventYID(stream.Version))
			{
				BlendEventYID = stream.ReadUInt32();
			}
			m_childIndices = stream.ReadUInt32Array();
			if (IsReadChildThresholdArray(stream.Version))
			{
				m_childThresholdArray = stream.ReadSingleArray();
			}

			if (IsReadBlendData(stream.Version))
			{
				Blend1dData.Read(stream);
				Blend2dData.Read(stream);
			}
			if(IsReadBlendDirectData(stream.Version))
			{
				BlendDirectData.Read(stream);
			}

			ClipID = stream.ReadUInt32();
			if(IsReadClipIndex(stream.Version))
			{
				ClipIndex = stream.ReadUInt32();
			}

			Duration = stream.ReadSingle();
			if (IsReadCycleOffset(stream.Version))
			{
				CycleOffset = stream.ReadSingle();
				Mirror = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public uint BlendType { get; private set; }
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
