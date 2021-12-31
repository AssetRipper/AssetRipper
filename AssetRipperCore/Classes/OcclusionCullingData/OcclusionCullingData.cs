using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AssetRipper.Core.Classes.OcclusionCullingData
{
	public sealed class OcclusionCullingData : NamedObject, IOcclusionCullingData
	{
		public OcclusionCullingData(AssetInfo assetInfo) : base(assetInfo) { }

		private OcclusionCullingData(LayoutInfo layout, AssetInfo assetInfo) : base(layout)
		{
			AssetInfo = assetInfo;
			Name = nameof(OcclusionCullingData);
		}

		public static OcclusionCullingData CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new OcclusionCullingData(virtualFile.Layout, assetInfo));
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasStaticRenderers(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			PVSData = reader.ReadByteArray();
			reader.AlignStream();

			Scenes = reader.ReadAssetArray<OcclusionScene>();
			if (HasStaticRenderers(reader.Flags))
			{
				StaticRenderers = reader.ReadAssetArray<SceneObjectIdentifier>();
				Portals = reader.ReadAssetArray<SceneObjectIdentifier>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(PVSDataName, PVSData.ExportYAML());
			node.Add(ScenesName, Scenes.ExportYAML(container));
			this.SetExportData(container);
			node.Add(StaticRenderersName, StaticRenderers.ExportYAML(container));
			node.Add(PortalsName, Portals.ExportYAML(container));
			return node;
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		public byte[] PVSData { get; set; }
		public IOcclusionScene[] Scenes { get; set; }
		public ISceneObjectIdentifier[] StaticRenderers { get; set; } = Array.Empty<ISceneObjectIdentifier>();
		public ISceneObjectIdentifier[] Portals { get; set; } = Array.Empty<ISceneObjectIdentifier>();

		public const string PVSDataName = "m_PVSData";
		public const string ScenesName = "m_Scenes";
		public const string StaticRenderersName = "m_StaticRenderers";
		public const string PortalsName = "m_Portals";
	}
}
