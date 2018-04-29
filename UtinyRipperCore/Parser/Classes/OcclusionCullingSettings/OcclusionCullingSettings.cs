using System.Collections.Generic;
using UtinyRipper.AssetExporters;
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
		public OcclusionCullingSettings(AssetInfo assetInfo):
			base(assetInfo)
		{
			OcclusionBakeSettings.SmallestOccluder = 5.0f;
			OcclusionBakeSettings.SmallestHole = 0.25f;
			OcclusionBakeSettings.BackfaceThreshold = 100.0f;
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
				if(IsReadQueryMode(stream.Version))
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
			foreach(Object @object in base.FetchDependencies(file, isLog))
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

		protected override YAMLMappingNode ExportYAMLRoot(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = base.ExportYAMLRoot(exporter);
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_OcclusionBakeSettings", OcclusionBakeSettings.ExportYAML(exporter));
			node.Add("m_SceneGUID", SceneGUID.ExportYAML(exporter));
			node.Add("m_OcclusionCullingData", OcclusionCullingData.ExportYAML(exporter));
			return node;
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

		public OcclusionBakeSettings OcclusionBakeSettings;
		public UtinyGUID SceneGUID;
		public PPtr<OcclusionCullingData> OcclusionCullingData;

		private byte[] m_PVSData;
		private PPtr<Renderer>[] m_staticRenderers;
		private PPtr<OcclusionPortal>[] m_portals;
	}
}
