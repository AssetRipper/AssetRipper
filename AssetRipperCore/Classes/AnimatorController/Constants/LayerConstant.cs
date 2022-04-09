using AssetRipper.Core.Classes.AnimatorController.Editor.AnimatorControllerLayer;
using AssetRipper.Core.Classes.AnimatorController.Mask;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;


namespace AssetRipper.Core.Classes.AnimatorController.Constants
{
	/// <summary>
	/// HumanLayerConstant in previous versions
	/// </summary>
	public sealed class LayerConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasDefaultWeight(UnityVersion version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasSyncedLayerAffectsTiming(UnityVersion version) => version.IsGreaterEqual(4, 2);

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

		public HumanPoseMask BodyMask = new();
		public OffsetPtr<SkeletonMask> SkeletonMask = new();
	}
}
