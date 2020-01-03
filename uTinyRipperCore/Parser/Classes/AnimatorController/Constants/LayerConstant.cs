using System;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
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
		public static bool HasDefaultWeight(Version version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasSyncedLayerAffectsTiming(Version version) => version.IsGreaterEqual(4, 2);

		public void Read(AssetReader reader)
		{
			StateMachineIndex = (int)reader.ReadUInt32();
			StateMachineMotionSetIndex = (int)reader.ReadUInt32();
			BodyMask.Read(reader);
			SkeletonMask.Read(reader);
			Binding = reader.ReadUInt32();
			LayerBlendingMode = (AnimatorLayerBlendingMode)reader.ReadInt32();
			if (HasDefaultWeight(reader.Version))
			{
				DefaultWeight = reader.ReadSingle();
			}

			IKPass = reader.ReadBoolean();
			if (HasSyncedLayerAffectsTiming(reader.Version))
			{
				SyncedLayerAffectsTiming = reader.ReadBoolean();
			}
			reader.AlignStream();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}
		
		public int StateMachineIndex { get; set; }
		public int StateMachineMotionSetIndex { get; set; }
		public uint Binding { get; set; }
		public AnimatorLayerBlendingMode LayerBlendingMode { get; set; }
		public float DefaultWeight { get; set; }
		public bool IKPass { get; set; }
		public bool SyncedLayerAffectsTiming { get; set; }
		
		public HumanPoseMask BodyMask;
		public OffsetPtr<SkeletonMask> SkeletonMask;
	}
}
