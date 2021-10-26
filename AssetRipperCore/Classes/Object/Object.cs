using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Object
{
	public abstract class Object : UnityObjectBase
	{
		protected Object(AssetLayout layout) : base(layout) { }

		protected Object(AssetInfo assetInfo) : base(assetInfo) { }

		public override void Read(AssetReader reader)
		{
			AssetUnityVersion = reader.Version;
			EndianType = reader.EndianType;
			if (HasHideFlag(reader.Version, reader.Flags))
			{
				ObjectHideFlags = (HideFlags)reader.ReadUInt32();
			}
		}

		public override void Write(AssetWriter writer)
		{
			if (HasHideFlag(writer.Version, writer.Flags))
			{
				writer.Write((uint)ObjectHideFlags);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			if (HasHideFlag(container.Version,container.Flags))
			{
				node.Add(ObjectHideFlagsName, (uint)ObjectHideFlags);
			}
			return node;
		}

		/// <summary>
		/// greater than 2.0.0 and Not Release
		/// </summary>
		public static bool HasHideFlag(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2) && !flags.IsRelease();

		public const string ObjectHideFlagsName = "m_ObjectHideFlags";
		public const string InstanceIDName = "m_InstanceID";
		public const string LocalIdentfierInFileName = "m_LocalIdentfierInFile";
	}
}
