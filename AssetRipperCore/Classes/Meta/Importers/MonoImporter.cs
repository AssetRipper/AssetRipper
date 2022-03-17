using AssetRipper.Core.Classes.Meta.Importers.Asset;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Meta.Importers
{
	public class MonoImporter : AssetImporter, IMonoImporter
	{
		public MonoImporter(LayoutInfo layout) : base(layout)
		{
			if (HasDefaultReferences(layout.Version))
			{
				DefaultReferences = new Dictionary<string, PPtr<Object.Object>>();
			}
		}

		public MonoImporter(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// NOTE: unknown conversion (default values has been changed?)
			if (version.IsGreaterEqual(3, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		private static bool HasDefaultReferences(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 3.4.0 and greater
		/// </summary>
		private static bool HasExecutionOrder(UnityVersion version) => version.IsGreaterEqual(3, 4);

		public override bool IncludesImporter(UnityVersion version)
		{
			return true;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasDefaultReferences(reader.Version))
			{
				DefaultReferences = new Dictionary<string, PPtr<Object.Object>>();
				DefaultReferences.Read(reader);
			}
			if (HasExecutionOrder(reader.Version))
			{
				ExecutionOrder = reader.ReadInt16();
				reader.AlignStream();

				Icon = reader.ReadAsset<PPtr<Texture2D.Texture2D>>();
			}

			PostRead(reader);
		}

		public override void Write(AssetWriter writer)
		{
			base.Write(writer);

			if (HasDefaultReferences(writer.Version))
			{
				DefaultReferences.Write(writer);
			}
			if (HasExecutionOrder(writer.Version))
			{
				writer.Write(ExecutionOrder);
				writer.AlignStream();

				Icon.Write(writer);
			}

			PostWrite(writer);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			if (HasDefaultReferences(container.ExportVersion))
			{
				node.Add(DefaultReferencesName, DefaultReferences.ExportYAML(container));
			}
			if (HasExecutionOrder(container.ExportVersion))
			{
				node.Add(ExecutionOrderName, ExecutionOrder);
				node.Add(IconName, Icon.ExportYAML(container));
			}
			PostExportYAML(container, node);
			return node;
		}

		public override ClassIDType ClassID => ClassIDType.MonoImporter;

		public Dictionary<string, PPtr<Object.Object>> DefaultReferences { get; set; }
		public short ExecutionOrder { get; set; }
		// map to Preview field just to reduce structure size. also they has same meaning
		public PPtr<Texture2D.Texture2D> Icon
		{
			get => Preview;
			set => Preview = value;
		}

		protected override bool IncludesIDToName => false;

		public const string DefaultReferencesName = "m_DefaultReferences";
		public const string ExecutionOrderName = "executionOrder";
		public const string IconName = "icon";
	}
}
