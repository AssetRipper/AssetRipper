using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.AssetBundles;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
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
		/// <summary>
		/// 5.0.0b2
		/// </summary>
		public static bool IsReadIsStreamedSceneAssetBundle(Version version)
		{
			return version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadExplicitDataLayout(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
		}
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool IsReadPathFlags(Version version)
		{
			return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		}
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool IsReadSceneHashes(Version version)
		{
			return version.IsGreaterEqual(2017, 3);
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

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadPreloadTable(reader.Version))
			{
				m_preloadTable = reader.ReadAssetArray<PPtr<Object>>();
			}

			m_container = reader.ReadStringTKVPArray<AssetBundles.AssetInfo>();
			MainAsset.Read(reader);

			if(IsReadScriptCampatibility(reader.Version))
			{
				m_scriptCampatibility = reader.ReadAssetArray<AssetBundleScriptInfo>();
			}
			if (IsReadClassCampatibility(reader.Version))
			{
				m_classCampatibility = reader.ReadInt32KVPUInt32Array();
			}

			if (IsReadClassVersionMap(reader.Version))
			{
				m_classVersionMap = new Dictionary<int, int>();
				m_classVersionMap.Read(reader);
			}

			if (IsReadRuntimeCompatibility(reader.Version))
			{
				RuntimeCompatibility = reader.ReadUInt32();
			}

			if(IsReadAssetBundleName(reader.Version))
			{
				AssetBundleName = reader.ReadString();
				m_dependencies = reader.ReadStringArray();
			}
			if (IsReadIsStreamedSceneAssetBundle(reader.Version))
			{
				IsStreamedSceneAssetBundle = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if(IsReadExplicitDataLayout(reader.Version))
			{
				ExplicitDataLayout = reader.ReadInt32();
			}
			if (IsReadPathFlags(reader.Version))
			{
				PathFlags = reader.ReadInt32();
			}

			if(IsReadSceneHashes(reader.Version))
			{
				m_sceneHashes = new Dictionary<string, string>();
				m_sceneHashes.Read(reader);
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			foreach (AssetBundles.AssetInfo container in m_container.Select(t => t.Value))
			{
				foreach (Object asset in container.FetchDependencies(file, isLog))
				{
					yield return asset;
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
