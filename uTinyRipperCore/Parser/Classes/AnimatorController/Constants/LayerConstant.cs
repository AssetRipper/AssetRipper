using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.AnimatorControllers
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

		public void Read(AssetReader reader)
		{
			StateMachineIndex = (int)reader.ReadUInt32();
			StateMachineMotionSetIndex = (int)reader.ReadUInt32();
			BodyMask.Read(reader);
			SkeletonMask.Read(reader);
			Binding = reader.ReadUInt32();
			LayerBlendingMode = (AnimatorLayerBlendingMode)reader.ReadInt32();
			if (IsReadDefaultWeight(reader.Version))
			{
				DefaultWeight = reader.ReadSingle();
			}

			IKPass = reader.ReadBoolean();
			if (IsReadSyncedLayerAffectsTiming(reader.Version))
			{
				SyncedLayerAffectsTiming = reader.ReadBoolean();
			}
			reader.AlignStream(AlignType.Align4);
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
