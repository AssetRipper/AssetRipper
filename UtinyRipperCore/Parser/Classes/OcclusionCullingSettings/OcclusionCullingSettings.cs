using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.AssetExporters.Classes;
using UtinyRipper.Classes.OcclusionCullingSettingses;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
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
		
		public static bool IsCompatible(Object asset)
		{
			if (asset.ClassID == ClassIDType.GameObject)
			{
				return true;
			}
			if (asset.ClassID.IsSceneSettings())
			{
				return true;
			}
			return asset is Component;
		}

		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadAnything(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadPVSData(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// 3.5.0 to 4.3.0 exclusive
		/// </summary>
		public static bool IsReadQueryMode(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(4, 3);
		}
		/// <summary>
		/// Greater than 3.5.0
		/// </summary>
		public static bool IsReadPortals(Version version)
		{
			return version.IsGreaterEqual(3, 5);
		}

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadOcclusionBakeSettings(TransferInstructionFlags flags)
		{
			return !flags.IsSerializeGameRelease();
		}
		/// <summary>
		/// Less than 5.5.0 or Release
		/// </summary>
		public static bool IsReadStaticRenderers(Version version, TransferInstructionFlags flags)
		{
			if (version.IsLess(5, 5))
			{
				return true;
			}
			else
			{
				return flags.IsSerializeGameRelease();
			}
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			// min version is 2nd
			return 2;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if(IsReadAnything(stream.Version))
			{
				if (IsReadPVSData(stream.Version))
				{
					m_PVSData = stream.ReadByteArray();
					stream.AlignStream(AlignType.Align4);
					if (IsReadQueryMode(stream.Version))
					{
						QueryMode = stream.ReadInt32();
					}

					m_staticRenderers = stream.ReadArray<PPtr<Renderer>>();
					if (IsReadPortals(stream.Version))
					{
						m_portals = stream.ReadArray<PPtr<OcclusionPortal>>();
					}

					if (IsReadOcclusionBakeSettings(stream.Flags))
					{
						OcclusionBakeSettings.Read(stream);
					}
				}
				else
				{
					if (IsReadOcclusionBakeSettings(stream.Flags))
					{
						OcclusionBakeSettings.Read(stream);
					}
					SceneGUID.Read(stream);
					OcclusionCullingData.Read(stream);

					if (IsReadStaticRenderers(stream.Version, stream.Flags))
					{
						m_staticRenderers = stream.ReadArray<PPtr<Renderer>>();
						m_portals = stream.ReadArray<PPtr<OcclusionPortal>>();
					}
				}
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}

			yield return OcclusionCullingData.FetchDependency(file, isLog, ToLogString, "m_OcclusionCullingData");
			foreach (PPtr<Renderer> staticRenderer in StaticRenderers)
			{
				yield return staticRenderer.FetchDependency(file, isLog, ToLogString, "m_StaticRenderers");
			}
			foreach (PPtr<OcclusionPortal> portal in Portals)
			{
				yield return portal.FetchDependency(file, isLog, ToLogString, "m_Portals");
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_OcclusionBakeSettings", GetExportOcclusionBakeSettings(container).ExportYAML(container));
			node.Add("m_SceneGUID", GetExportSceneGUID(container).ExportYAML(container));
			node.Add("m_OcclusionCullingData", GetExportOcclusionCullingData(container).ExportYAML(container));
			return node;
		}

		private OcclusionBakeSettings GetExportOcclusionBakeSettings(IExportContainer container)
		{
			if (IsReadAnything(container.Version) && IsReadOcclusionBakeSettings(container.Flags))
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
		private EngineGUID GetExportSceneGUID(IExportContainer container)
		{
			if(IsReadAnything(container.Version) && !IsReadPVSData(container.Version))
			{
				return SceneGUID;
			}
			else
			{
				SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
				return scene.GUID;
			}
		}
		private PPtr<OcclusionCullingData> GetExportOcclusionCullingData(IExportContainer container)
		{
			if(!IsReadAnything(container.Version))
			{
				return default;
			}

			if(IsReadPVSData(container.Version))
			{
				if (m_PVSData.Length == 0)
				{
					return default;
				}
				else
				{
					SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
					PPtr<OcclusionCullingData> occlusionData = PPtr<OcclusionCullingData>.CreateVirtualPointer(scene.OcclusionCullingData);
					return occlusionData;
				}
			}
			else
			{
				if (Classes.OcclusionCullingData.IsReadStaticRenderers(container.Flags))
				{
					return OcclusionCullingData;
				}
				else
				{
#warning TODO: OcclusionCullingData has to find all corresponding OcclusionCullingSettings and fill IDs itself
					OcclusionCullingData data = OcclusionCullingData.FindObject(container);
					if(data != null)
					{
						data.SetIDs(container, SceneGUID, StaticRenderers, Portals);
					}
					return OcclusionCullingData;
				}
			}
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
		
		public const string SceneExportFolder = "Scenes";

		public OcclusionBakeSettings OcclusionBakeSettings;
		public EngineGUID SceneGUID;
		public PPtr<OcclusionCullingData> OcclusionCullingData;

		private byte[] m_PVSData;
		private PPtr<Renderer>[] m_staticRenderers;
		private PPtr<OcclusionPortal>[] m_portals;
	}
}
