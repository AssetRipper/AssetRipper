using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes
{
	public class DefaultAsset : NamedObject
	{
		public DefaultAsset(LayoutInfo layout) : base(layout)
		{
			Message = string.Empty;
		}

		public DefaultAsset(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.0.0f1 and greater (NOTE: unknown version)
		/// </summary>
		public static bool HasMessage(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasMessage(reader.Version))
			{
				Message = reader.ReadString();
				IsWarning = reader.ReadBoolean();
			}
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasMessage(writer.Version))
			{
				writer.Write(Message);
				writer.Write(IsWarning);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasMessage(container.ExportVersion))
			{
				node.Add(MessageName, Message);
				node.Add(IsWarningName, IsWarning);
			}
			return node;
		}

		public string Message { get; set; }
		public bool IsWarning { get; set; }

		public const string MessageName = "m_Message";
		public const string IsWarningName = "m_IsWarning";
	}
}
