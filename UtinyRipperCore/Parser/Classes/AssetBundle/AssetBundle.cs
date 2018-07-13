using System;
using System.Collections.Generic;
using System.Linq;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.AssetBundles;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public sealed class AssetBundle : NamedObject
	{
		public AssetBundle(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool IsReadPreloadTable(Version version)
		{
			return version.IsGreaterEqual(2, 5);
		}
		/// <summary>
		/// 3.4.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadScriptCampatibility(Version version)
		{
			return version.IsGreaterEqual(3, 4) && version.IsLess(5);
		}
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool IsReadClassCampatibility(Version version)
		{
			return version.IsGreaterEqual(3, 5) && version.IsLess(5);
		}
		/// <summary>
		/// 5.4.0 to 5.5.0 exclusive
		/// </summary>
		public static bool IsReadClassVersionMap(Version version)
		{
			return version.IsGreaterEqual(5, 4) && version.IsLess(5, 5);
		}
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool IsReadRuntimeCompatibility(Version version)
		{
			return version.IsGreaterEqual(4, 2);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadAssetBundleName(Version version)
		{
			return version.IsGreaterEqual(5);
		}
		public static bool IsReadIsStreamedSceneAssetBundle(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta, 1);
		}
		public static bool IsReadExplicitDataLayout(Version version)
		{
#warning unknown
			return version.IsGreaterEqual(2017, 4);
		}
		/// <summary>
		/// 2017.1.0b2 andgreater
		/// </summary>
		public static bool IsReadPathFlags(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		public static bool IsReadSceneHashes(Version version)
		{
#warning unknown
			return version.IsGreaterEqual(2017, 4);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

			if (version.IsGreaterEqual(4, 2))
			{
				return 3;
			}
			if (version.IsGreaterEqual(2, 5))
			{
				return 2;
			}
			return 1;
		}

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			if (IsReadPreloadTable(stream.Version))
			{
				m_preloadTable = stream.ReadArray<PPtr<Object>>();
			}

			m_container = stream.ReadStringKVPArray<AssetBundles.AssetInfo>();
			MainAsset.Read(stream);

			if(IsReadScriptCampatibility(stream.Version))
			{
				m_scriptCampatibility = stream.ReadArray<AssetBundleScriptInfo>();
			}
			if (IsReadClassCampatibility(stream.Version))
			{
				m_classCampatibility = stream.ReadInt32KVPUInt32Array();
			}

			if (IsReadClassVersionMap(stream.Version))
			{
				m_classVersionMap = new Dictionary<int, int>();
				m_classVersionMap.Read(stream);
			}

			if (IsReadRuntimeCompatibility(stream.Version))
			{
				RuntimeCompatibility = stream.ReadUInt32();
			}

			if(IsReadAssetBundleName(stream.Version))
			{
				AssetBundleName = stream.ReadStringAligned();
				m_dependencies = stream.ReadStringArray();
			}
			if (IsReadIsStreamedSceneAssetBundle(stream.Version))
			{
				IsStreamedSceneAssetBundle = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
			if(IsReadExplicitDataLayout(stream.Version))
			{
				ExplicitDataLayout = stream.ReadInt32();
			}
			if (IsReadPathFlags(stream.Version))
			{
				PathFlags = stream.ReadInt32();
			}

			if(IsReadSceneHashes(stream.Version))
			{
				m_sceneHashes = new Dictionary<string, string>();
				m_sceneHashes.Read(stream);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			foreach (AssetBundles.AssetInfo container in m_container.Select(t => t.Value))
			{
				foreach (Object @object in container.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public override string ExportExtension => throw new NotSupportedException();

		public IReadOnlyList<PPtr<Object>> PreloadTable => m_preloadTable;
		public ILookup<string, AssetBundles.AssetInfo> Container => m_container.ToLookup(t => t.Key, t => t.Value);
		public IReadOnlyList<AssetBundleScriptInfo> ScriptCampatibility => m_scriptCampatibility;
		public IReadOnlyList<KeyValuePair<int, uint>> ClassCampatibility => m_classCampatibility;
		public IReadOnlyDictionary<int, int> ClassVersionMap => m_classVersionMap;
		public uint RuntimeCompatibility { get; private set; }
		public string AssetBundleName { get; private set; }
		public IReadOnlyList<string> Dependencies  => m_dependencies;
		public bool IsStreamedSceneAssetBundle { get; private set; }
		public int ExplicitDataLayout { get; private set; }
		public int PathFlags { get; private set; }
		
		public AssetBundles.AssetInfo MainAsset;
		
		private PPtr<Object>[] m_preloadTable;
		private KeyValuePair<string, AssetBundles.AssetInfo>[] m_container;
		private AssetBundleScriptInfo[] m_scriptCampatibility;
		private KeyValuePair<int, uint>[] m_classCampatibility;
		private Dictionary<int, int> m_classVersionMap;
		private string[] m_dependencies;
		private Dictionary<string, string> m_sceneHashes;
	}
}
