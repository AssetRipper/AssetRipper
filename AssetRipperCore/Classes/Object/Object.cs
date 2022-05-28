using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;

namespace AssetRipper.Core.Classes.Object
{
	public abstract class Object : UnityObjectBase
	{
		protected Object(LayoutInfo layout) : base(layout) { }

		protected Object(AssetInfo assetInfo) : base(assetInfo) { }

		public override HideFlags ObjectHideFlagsOld
		{
			get => (HideFlags)m_ObjectHideFlags;
			set => m_ObjectHideFlags = (uint)value;
		}

		public override void Read(AssetReader reader)
		{
			if (HasHideFlag(reader.Version, reader.Flags))
			{
				m_ObjectHideFlags = reader.ReadUInt32();
			}
		}

		public override void Write(AssetWriter writer)
		{
			if (HasHideFlag(writer.Version, writer.Flags))
			{
				writer.Write(m_ObjectHideFlags);
			}
		}

		public sealed override YamlNode ExportYaml(IExportContainer container)
		{
			return ExportYamlRoot(container);
		}

		protected virtual YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			if (HasHideFlag(container.ExportVersion, container.ExportFlags))
			{
				node.Add(ObjectHideFlagsName, m_ObjectHideFlags);
			}
			return node;
		}

		/// <summary>
		/// greater than 2.0.0 and Not Release
		/// </summary>
		public static bool HasHideFlag(UnityVersion version, TransferInstructionFlags flags) => version.IsGreaterEqual(2) && !flags.IsRelease();

		private uint m_ObjectHideFlags;

		public const string ObjectHideFlagsName = "m_ObjectHideFlags";
		public const string InstanceIDName = "m_InstanceID";
		public const string LocalIdentfierInFileName = "m_LocalIdentfierInFile";
	}
}
