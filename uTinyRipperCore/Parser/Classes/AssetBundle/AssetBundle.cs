using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.Classes.AssetBundles;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes
{
	public sealed class AssetBundle : NamedObject
	{
		public AssetBundle(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
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

		/// <summary>
		/// 2.5.0 and greater
		/// </summary>
		public static bool HasPreloadTable(Version version) => version.IsGreaterEqual(2, 5);
		/// <summary>
		/// 3.4.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasScriptCampatibility(Version version) => version.IsGreaterEqual(3, 4) && version.IsLess(5);
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasClassCampatibility(Version version) => version.IsGreaterEqual(3, 5) && version.IsLess(5);
		/// <summary>
		/// 5.4.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasClassVersionMap(Version version) => version.IsGreaterEqual(5, 4) && version.IsLess(5, 5);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasRuntimeCompatibility(Version version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasAssetBundleName(Version version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0b2
		/// </summary>
		public static bool HasIsStreamedSceneAssetBundle(Version version) => version.IsGreaterEqual(5, 0, 0, VersionType.Beta, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasExplicitDataLayout(Version version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasPathFlags(Version version) => version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasSceneHashes(Version version) => version.IsGreaterEqual(2017, 3);

		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasPathExtension(Version version) => version.IsGreaterEqual(5);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasPreloadTable(reader.Version))
			{
				PreloadTable = reader.ReadAssetArray<PPtr<Object>>();
			}

			Container = reader.ReadKVPStringTArray<AssetBundles.AssetInfo>();
			MainAsset.Read(reader);

			if (HasScriptCampatibility(reader.Version))
			{
				ScriptCampatibility = reader.ReadAssetArray<AssetBundleScriptInfo>();
			}
			if (HasClassCampatibility(reader.Version))
			{
				ClassCampatibility = reader.ReadKVPInt32UInt32Array();
			}

			if (HasClassVersionMap(reader.Version))
			{
				ClassVersionMap = new Dictionary<int, int>();
				ClassVersionMap.Read(reader);
			}

			if (HasRuntimeCompatibility(reader.Version))
			{
				RuntimeCompatibility = reader.ReadUInt32();
			}

			if (HasAssetBundleName(reader.Version))
			{
				AssetBundleName = reader.ReadString();
				Dependencies = reader.ReadStringArray();
			}
			if (HasIsStreamedSceneAssetBundle(reader.Version))
			{
				IsStreamedSceneAssetBundle = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasExplicitDataLayout(reader.Version))
			{
				ExplicitDataLayout = reader.ReadInt32();
			}
			if (HasPathFlags(reader.Version))
			{
				PathFlags = reader.ReadInt32();
			}

			if (HasSceneHashes(reader.Version))
			{
				SceneHashes = new Dictionary<string, string>();
				SceneHashes.Read(reader);
			}
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object> asset in context.FetchDependencies(Container.Select(t => t.Value), ContainerName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public override string ExportExtension => throw new NotSupportedException();

		public PPtr<Object>[] PreloadTable { get; set; }
		public KeyValuePair<string, AssetBundles.AssetInfo>[] Container { get; set; }
		public AssetBundleScriptInfo[] ScriptCampatibility { get; set; }
		public KeyValuePair<int, uint>[] ClassCampatibility { get; set; }
		public Dictionary<int, int> ClassVersionMap { get; set; }
		public uint RuntimeCompatibility { get; set; }
		public string AssetBundleName { get; set; }
		public string[] Dependencies { get; set; }
		public bool IsStreamedSceneAssetBundle { get; set; }
		public int ExplicitDataLayout { get; set; }
		public int PathFlags { get; set; }
		public Dictionary<string, string> SceneHashes { get; set; }

		public const string ContainerName = "m_Container";

		public AssetBundles.AssetInfo MainAsset;
	}
}
