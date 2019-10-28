using System.Collections.Generic;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.OcclusionCullingSettingses;
using uTinyRipper.Converters;
using uTinyRipper.Project;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes
{
	/// <summary>
	/// SceneSettings previously
	/// </summary>
	public sealed class OcclusionCullingSettings : LevelGameManager
	{
		public OcclusionCullingSettings(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static bool IsSceneCompatible(Object asset)
		{
			if (asset.ClassID == ClassIDType.GameObject)
			{
				return true;
			}
			if (asset.ClassID.IsSceneSettings())
			{
				return true;
			}
			if (asset.ClassID == ClassIDType.MonoBehaviour)
			{
				MonoBehaviour monoBeh = (MonoBehaviour)asset;
				if (!monoBeh.IsSceneObject)
				{
					return false;
				}
			}

			return asset is Component;
		}

		/// <summary>
		/// 3.0.0 to 5.5.0 exclusive
		/// </summary>
		public static bool IsReadPVSData(Version version)
		{
			return version.IsGreaterEqual(3) && version.IsLess(5, 5);
		}
		/// <summary>
		/// 3.5.0 to 4.3.0 exclusive
		/// </summary>
		public static bool IsReadQueryMode(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(4, 3);
		}
		/// <summary>
		/// (3.5.0 to 5.5.0 exclusive) or (5.0.0 and greater and Release)
		/// </summary>
		public static bool IsReadPortals(Version version, TransferInstructionFlags flags)
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
		/// 3.0.0 and greater and Not Release
		/// </summary>
		public static bool IsReadOcclusionBakeSettings(Version version, TransferInstructionFlags flags)
		{
			return version.IsGreaterEqual(3) && !flags.IsRelease();
		}
		/// <summary>
		/// 5.5.0 and greater
		/// </summary>
		public static bool IsReadSceneGUID(Version version)
		{
			return version.IsGreaterEqual(5, 5);
		}
		/// <summary>
		/// (3.0.0 to 5.5.0 exclusive) or (5.0.0 and greater and Release)
		/// </summary>
		public static bool IsReadStaticRenderers(Version version, TransferInstructionFlags flags)
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
		/// Less than 5.5.0
		/// </summary>
		private static bool IsReadOcclusionBakeSettingsFirst(Version version)
		{
			return version.IsLess(5, 5);
		}

		private static int GetSerializedVersion(Version version)
		{
			// min version is 2nd
			return 2;
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadPVSData(reader.Version))
			{
				m_PVSData = reader.ReadByteArray();
				reader.AlignStream(AlignType.Align4);
			}
			if (IsReadQueryMode(reader.Version))
			{
				QueryMode = reader.ReadInt32();
			}

			if (IsReadOcclusionBakeSettings(reader.Version, reader.Flags))
			{
				if (IsReadOcclusionBakeSettingsFirst(reader.Version))
				{
					OcclusionBakeSettings.Read(reader);
				}
			}

			if (IsReadSceneGUID(reader.Version))
			{
				SceneGUID.Read(reader);
				OcclusionCullingData.Read(reader);
			}
			if (IsReadStaticRenderers(reader.Version, reader.Flags))
			{
				m_staticRenderers = reader.ReadAssetArray<PPtr<Renderer>>();
			}
			if (IsReadPortals(reader.Version, reader.Flags))
			{
				m_portals = reader.ReadAssetArray<PPtr<OcclusionPortal>>();
			}

			if (IsReadOcclusionBakeSettings(reader.Version, reader.Flags))
			{
				if (!IsReadOcclusionBakeSettingsFirst(reader.Version))
				{
					OcclusionBakeSettings.Read(reader);
				}
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(OcclusionCullingData, OcclusionCullingDataName);
			foreach (PPtr<Object> asset in context.FetchDependencies(StaticRenderers, StaticRenderersName))
			{
				yield return asset;
			}
			foreach (PPtr<Object> asset in context.FetchDependencies(Portals, PortalsName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(OcclusionBakeSettingsName, GetExportOcclusionBakeSettings(container).ExportYAML(container));
			node.Add(SceneGUIDName, GetExportSceneGUID(container).ExportYAML(container));
			node.Add(OcclusionCullingDataName, GetExportOcclusionCullingData(container).ExportYAML(container));
			return node;
		}

		private OcclusionBakeSettings GetExportOcclusionBakeSettings(IExportContainer container)
		{
			if (IsReadOcclusionBakeSettings(container.Version, container.Flags))
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
		private GUID GetExportSceneGUID(IExportContainer container)
		{
			if (IsReadPVSData(container.Version))
			{
				SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
				return scene.GUID;
			}
			else
			{
				return SceneGUID;
			}
		}
		private PPtr<OcclusionCullingData> GetExportOcclusionCullingData(IExportContainer container)
		{
			if (IsReadPVSData(container.Version))
			{
				SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
				if (scene.OcclusionCullingData == null)
				{
					return default;
				}
				return scene.OcclusionCullingData.File.CreatePPtr(scene.OcclusionCullingData);
			}
			if (IsReadSceneGUID(container.Version))
			{
				return OcclusionCullingData;
			}
			return default;
		}

		public IReadOnlyList<byte> PVSData => m_PVSData;
		public int QueryMode { get; private set; }
		/// <summary>
		/// PVSObjectsArray/m_PVSObjectsArray previously
		/// </summary>
		public IReadOnlyList<PPtr<Renderer>> StaticRenderers => m_staticRenderers;
		/// <summary>
		/// PVSPortalsArray previously
		/// </summary>
		public IReadOnlyList<PPtr<OcclusionPortal>> Portals => m_portals;

		public OcclusionBakeSettings OcclusionBakeSettings;
		public GUID SceneGUID;
		public PPtr<OcclusionCullingData> OcclusionCullingData;

		public const string SceneKeyword = nameof(ClassIDType.Scene);

		public const string OcclusionBakeSettingsName = "m_OcclusionBakeSettings";
		public const string SceneGUIDName = "m_SceneGUID";
		public const string OcclusionCullingDataName = "m_OcclusionCullingData";
		public const string StaticRenderersName = "m_StaticRenderers";
		public const string PortalsName = "m_Portals";

		private byte[] m_PVSData;
		private PPtr<Renderer>[] m_staticRenderers;
		private PPtr<OcclusionPortal>[] m_portals;
	}
}
