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

		/// <summary>
		/// Less than 5.5.0
		/// </summary>
		public static bool IsReadPVSData(Version version)
		{
			return version.IsLess(5, 5);
		}
		/// <summary>
		/// Less than 4.3.0
		/// </summary>
		public static bool IsReadQueryMode(Version version)
		{
			return version.IsLess(4, 3);
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

			if (IsReadPVSData(stream.Version))
			{
				m_PVSData = stream.ReadByteArray();
				stream.AlignStream(AlignType.Align4);
				if (IsReadQueryMode(stream.Version))
				{
					QueryMode = stream.ReadInt32();
				}

				m_staticRenderers = stream.ReadArray<PPtr<Renderer>>();
				m_portals = stream.ReadArray<PPtr<OcclusionPortal>>();

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
			node.Add("m_OcclusionBakeSettings", GetExportOcclusionBakeSettings(container.Flags).ExportYAML(container));
			node.Add("m_SceneGUID", GetExportSceneGUID(container).ExportYAML(container));
			node.Add("m_OcclusionCullingData", GetExportOcclusionCullingData(container));
			return node;
		}

		private OcclusionBakeSettings GetExportOcclusionBakeSettings(TransferInstructionFlags flags)
		{
			if (IsReadOcclusionBakeSettings(flags))
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
		private UtinyGUID GetExportSceneGUID(IExportContainer container)
		{
			if(IsReadPVSData(container.Version))
			{
				SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
				return scene.GUID;
			}
			else
			{
				return SceneGUID;
			}
		}
		private YAMLNode GetExportOcclusionCullingData(IExportContainer container)
		{
			if(IsReadPVSData(container.Version))
			{
				if (m_PVSData.Length == 0)
				{
					return ExportPointer.EmptyPointer.ExportYAML(container);
				}
				else
				{
#warning HACK!!!
					AssetInfo dataAssetInfo = new AssetInfo(File, 0, ClassIDType.OcclusionCullingData);
					OcclusionCullingData ocData = new OcclusionCullingData(dataAssetInfo, container, m_PVSData, SceneGUID, StaticRenderers, Portals);
					SceneExportCollection scene = (SceneExportCollection)container.CurrentCollection;
					scene.OcclusionCullingData = ocData;

					var exPointer = container.CreateExportPointer(ocData);
					return exPointer.ExportYAML(container);
				}
			}
			else
			{
#warning TODO: OcclusionCullingData has to find all corresponding OcclusionCullingSettings and fill IDs itself
				if (Classes.OcclusionCullingData.IsReadStaticRenderers(container.Flags))
				{
					return OcclusionCullingData.ExportYAML(container);
				}
				else
				{
					OcclusionCullingData data = OcclusionCullingData.FindObject(container);
					if(data != null)
					{
						data.SetIDs(container, SceneGUID, StaticRenderers, Portals);
					}
					return OcclusionCullingData.ExportYAML(container);
				}
			}
		}

		public IReadOnlyList<byte> PVSData => m_PVSData;
		public int QueryMode { get; private set; }
		/// <summary>
		/// PVSObjectsArray previously
		/// </summary>
		public IReadOnlyList<PPtr<Renderer>> StaticRenderers => m_staticRenderers;
		/// <summary>
		/// PVSPortalsArray previously
		/// </summary>
		public IReadOnlyList<PPtr<OcclusionPortal>> Portals => m_portals;
		
		public const string SceneExportFolder = "Scenes";

		public OcclusionBakeSettings OcclusionBakeSettings;
		public UtinyGUID SceneGUID;
		public PPtr<OcclusionCullingData> OcclusionCullingData;

		private byte[] m_PVSData;
		private PPtr<Renderer>[] m_staticRenderers;
		private PPtr<OcclusionPortal>[] m_portals;
	}
}
