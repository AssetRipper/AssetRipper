using System;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	/// <summary>
	/// HumanLayerConstant in previous versions
	/// </summary>
	public struct LayerConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadDefaultWeight(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadSyncedLayerAffectsTiming(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}

		public void Read(AssetStream stream)
		{
			StateMachineIndex = (int)stream.ReadUInt32();
			StateMachineMotionSetIndex = (int)stream.ReadUInt32();
			BodyMask.Read(stream);
			SkeletonMask.Read(stream);
			Binding = stream.ReadUInt32();
			LayerBlendingMode = (AnimatorLayerBlendingMode)stream.ReadInt32();
			if (IsReadDefaultWeight(stream.Version))
			{
				DefaultWeight = stream.ReadSingle();
			}

			IKPass = stream.ReadBoolean();
			if (IsReadSyncedLayerAffectsTiming(stream.Version))
			{
				SyncedLayerAffectsTiming = stream.ReadBoolean();
			}
			stream.AlignStream(AlignType.Align4);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}
		
		public int StateMachineIndex { get; private set; }
		public int StateMachineMotionSetIndex { get; private set; }
		public uint Binding { get; private set; }
		public AnimatorLayerBlendingMode LayerBlendingMode { get; private set; }
		public float DefaultWeight { get; private set; }
		public bool IKPass { get; private set; }
		public bool SyncedLayerAffectsTiming { get; private set; }
		
		public HumanPoseMask BodyMask;
		public OffsetPtr<SkeletonMask> SkeletonMask;
	}
}
