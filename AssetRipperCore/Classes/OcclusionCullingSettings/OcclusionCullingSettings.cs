using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.OcclusionCullingSettings
{
	/// <summary>
	/// 5.5.0 - SceneSettings has been renamed to OcclusionCullingSettings
	/// 4.0.0 - Scene has been renamed to SceneSettings
	/// </summary>
	public sealed class OcclusionCullingSettings : LevelGameManager
	{
		public OcclusionCullingSettings(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// min version is 2nd
			return 2;
		}

		/// <summary>
		/// 3.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasReadPVSData(UnityVersion version) => version.IsGreaterEqual(3) && version.IsLess(5, 5);
		/// <summary>
		/// 3.5.0 to 4.3.0 exclusive
		/// </summary>
		public static bool HasQueryMode(UnityVersion version) => version.IsGreaterEqual(3, 5) && version.IsLess(4, 3);
		/// <summary>
		/// (3.5.0 to 5.5.0 exclusive) or (5.0.0 and greater and Release)
		/// </summary>
		public static bool HasPortals(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(3, 5, 0))
			{
				if (version.IsLess(5, 5))
				{
					return true;
				}
				return flags.IsRelease();
			}
			return false;
		}
		/// <summary>
		/// 3.5.0 and greater and Not Release
		/// </summary>
		public static bool HasOcclusionBakeSettings(UnityVersion version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(3, 5);
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool HasSceneGUID(UnityVersion version) => version.IsGreaterEqual(5, 5);
		/// <summary>
		/// (3.0.0 to 5.5.0 exclusive) or (5.5.0 and greater and Release)
		/// </summary>
		public static bool HasStaticRenderers(UnityVersion version, TransferInstructionFlags flags)
		{
			if (version.IsGreaterEqual(3, 0, 0))
			{
				if (version.IsLess(5, 5))
				{
					return true;
				}
				return flags.IsRelease();
			}
			return false;
		}
		/// <summary>
		/// 3.0.0 to 3.5.0 exclusive and Not Release
		/// </summary>
		public static bool HasViewCellSize(UnityVersion version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsLess(3, 5) && version.IsGreaterEqual(3);
		}

		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		private static bool IsOcclusionBakeSettingsFirst(UnityVersion version) => version.IsGreaterEqual(5, 5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasReadPVSData(reader.Version))
			{
				PVSData = reader.ReadByteArray();
				reader.AlignStream();
			}
			if (HasQueryMode(reader.Version))
			{
				QueryMode = reader.ReadInt32();
			}

			if (HasOcclusionBakeSettings(reader.Version, reader.Flags))
			{
				if (IsOcclusionBakeSettingsFirst(reader.Version))
				{
					OcclusionBakeSettings.Read(reader);
				}
			}

			if (HasSceneGUID(reader.Version))
			{
				SceneGUID.Read(reader);
				OcclusionCullingData.Read(reader);
			}
			if (HasStaticRenderers(reader.Version, reader.Flags))
			{
				StaticRenderers = reader.ReadAssetArray<PPtr<Renderer.Renderer>>();
			}
			if (HasPortals(reader.Version, reader.Flags))
			{
				Portals = reader.ReadAssetArray<PPtr<OcclusionPortal>>();
			}
			if (HasViewCellSize(reader.Version, reader.Flags))
			{
				ViewCellSize = reader.ReadSingle();
			}
			if (HasOcclusionBakeSettings(reader.Version, reader.Flags))
			{
				if (!IsOcclusionBakeSettingsFirst(reader.Version))
				{
					OcclusionBakeSettings.Read(reader);
				}
			}
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(OcclusionCullingData, OcclusionCullingDataName);
			foreach (PPtr<Object.Object> asset in context.FetchDependencies(StaticRenderers, StaticRenderersName))
			{
				yield return asset;
			}
			foreach (PPtr<Object.Object> asset in context.FetchDependencies(Portals, PortalsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(OcclusionBakeSettingsName, GetExportOcclusionBakeSettings(container).ExportYAML(container));
			node.Add(SceneGUIDName, GetExportSceneGUID(container).ExportYAML(container));
			node.Add(OcclusionCullingDataName, GetExportOcclusionCullingData(container).ExportYAML(container));
			return node;
		}

		private OcclusionBakeSettings GetExportOcclusionBakeSettings(IExportContainer container)
		{
			if (HasOcclusionBakeSettings(container.Version, container.Flags))
			{
				return OcclusionBakeSettings;
			}
			else
			{
				OcclusionBakeSettings settings = new OcclusionBakeSettings();
				settings.SmallestOccluder = 5.0f;
				settings.SmallestHole = 0.25f;
				settings.BackfaceThreshold = 100.0f;
				return settings;
			}
		}
		private UnityGUID GetExportSceneGUID(IExportContainer container)
		{
			if (HasReadPVSData(container.Version))
			{
				SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
				return scene.GUID;
			}
			else
			{
				return SceneGUID;
			}
		}
		private PPtr<OcclusionCullingData.OcclusionCullingData> GetExportOcclusionCullingData(IExportContainer container)
		{
			if (HasReadPVSData(container.Version))
			{
				SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
				if (scene.OcclusionCullingData == null)
				{
					return default;
				}
				return scene.OcclusionCullingData.File.CreatePPtr(scene.OcclusionCullingData);
			}
			if (HasSceneGUID(container.Version))
			{
				return OcclusionCullingData;
			}
			return default;
		}

		public byte[] PVSData { get; set; }
		public int QueryMode { get; set; }
		/// <summary>
		/// PVSObjectsArray/m_PVSObjectsArray previously
		/// </summary>
		public PPtr<Renderer.Renderer>[] StaticRenderers { get; set; }
		public float ViewCellSize
		{
			get => OcclusionBakeSettings.ViewCellSize;
			set => OcclusionBakeSettings.ViewCellSize = value;
		}
		/// <summary>
		/// PVSPortalsArray previously
		/// </summary>
		public PPtr<OcclusionPortal>[] Portals { get; set; }

		public OcclusionBakeSettings OcclusionBakeSettings;
		public UnityGUID SceneGUID;
		public PPtr<OcclusionCullingData.OcclusionCullingData> OcclusionCullingData;

		public const string SceneKeyword = nameof(ClassIDType.Scene);

		public const string ViewCellSizeName = "m_ViewCellSize";
		public const string OcclusionBakeSettingsName = "m_OcclusionBakeSettings";
		public const string SceneGUIDName = "m_SceneGUID";
		public const string OcclusionCullingDataName = "m_OcclusionCullingData";
		public const string StaticRenderersName = "m_StaticRenderers";
		public const string PortalsName = "m_Portals";
	}
}
