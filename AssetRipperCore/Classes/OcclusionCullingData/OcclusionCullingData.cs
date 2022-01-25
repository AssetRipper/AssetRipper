using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.IO;

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

			m_Scenes = reader.ReadAssetArray<OcclusionScene>();
			if (HasStaticRenderers(reader.Flags))
			{
				m_StaticRenderers = reader.ReadAssetArray<SceneObjectIdentifier>();
				m_Portals = reader.ReadAssetArray<SceneObjectIdentifier>();
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(PVSDataName, PVSData.ExportYAML());
			node.Add(ScenesName, m_Scenes.ExportYAML(container));
			this.SetExportData(container);
			node.Add(StaticRenderersName, m_StaticRenderers.ExportYAML(container));
			node.Add(PortalsName, m_Portals.ExportYAML(container));
			return node;
		}

		public void InitializeScenes(int count)
		{
			m_Scenes = new OcclusionScene[count];
			for (int i = 0; i < count; i++)
			{
				m_Scenes[i] = new OcclusionScene();
			}
		}

		public void InitializeStaticRenderers(int count)
		{
			m_StaticRenderers = new SceneObjectIdentifier[count];
			for (int i = 0; i < count; ++i)
			{
				m_StaticRenderers[i] = new SceneObjectIdentifier();
			}
		}

		public void InitializePortals(int count)
		{
			m_Portals = new SceneObjectIdentifier[count];
			for (int i = 0; i < count; i++)
			{
				m_Portals[i] = new SceneObjectIdentifier();
			}
		}

		public override string ExportPath => Path.Combine(AssetsKeyword, OcclusionCullingSettings.OcclusionCullingSettings.SceneKeyword, ClassID.ToString());

		private OcclusionScene[] m_Scenes = Array.Empty<OcclusionScene>();
		private SceneObjectIdentifier[] m_StaticRenderers = Array.Empty<SceneObjectIdentifier>();
		private SceneObjectIdentifier[] m_Portals = Array.Empty<SceneObjectIdentifier>();

		public byte[] PVSData { get; set; }
		public IOcclusionScene[] Scenes => m_Scenes;
		public ISceneObjectIdentifier[] StaticRenderers => m_StaticRenderers;
		public ISceneObjectIdentifier[] Portals => m_Portals;

		public const string PVSDataName = "m_PVSData";
		public const string ScenesName = "m_Scenes";
		public const string StaticRenderersName = "m_StaticRenderers";
		public const string PortalsName = "m_Portals";
	}
}
