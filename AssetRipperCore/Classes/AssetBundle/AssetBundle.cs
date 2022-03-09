using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Classes.AssetBundle
{
	public sealed class AssetBundle : NamedObject, IAssetBundle
	{
		public AssetBundle(Parser.Asset.AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
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
		public static bool HasPreloadTable(UnityVersion version) => version.IsGreaterEqual(2, 5);
		/// <summary>
		/// 3.4.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasScriptCampatibility(UnityVersion version) => version.IsGreaterEqual(3, 4) && version.IsLess(5);
		/// <summary>
		/// 3.5.0 to 5.0.0 exclusive
		/// </summary>
		public static bool HasClassCampatibility(UnityVersion version) => version.IsGreaterEqual(3, 5) && version.IsLess(5);
		/// <summary>
		/// 5.4.0 to 5.5.0 exclusive
		/// </summary>
		public static bool HasClassVersionMap(UnityVersion version) => version.IsGreaterEqual(5, 4) && version.IsLess(5, 5);
		/// <summary>
		/// 4.2.0 and greater
		/// </summary>
		public static bool HasRuntimeCompatibility(UnityVersion version) => version.IsGreaterEqual(4, 2);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasAssetBundleName(UnityVersion version) => version.IsGreaterEqual(5);
		/// <summary>
		/// 5.0.0b2
		/// </summary>
		public static bool HasIsStreamedSceneAssetBundle(UnityVersion version) => version.IsGreaterEqual(5, 0, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasExplicitDataLayout(UnityVersion version) => version.IsGreaterEqual(2017, 3);
		/// <summary>
		/// 2017.1.0b2 and greater
		/// </summary>
		public static bool HasPathFlags(UnityVersion version) => version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 2);
		/// <summary>
		/// 2017.3 and greater
		/// </summary>
		public static bool HasSceneHashes(UnityVersion version) => version.IsGreaterEqual(2017, 3);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasPreloadTable(reader.Version))
			{
				PreloadTable = reader.ReadAssetArray<PPtr<Object.Object>>();
			}

			Container = reader.ReadKVPStringTArray<AssetInfo>();
			MainAsset.Read(reader);

			if (HasScriptCampatibility(reader.Version))
			{
				ScriptCompatibility = reader.ReadAssetArray<AssetBundleScriptInfo>();
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

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Container.Select(t => t.Value), ContainerName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public NullableKeyValuePair<Utf8StringBase, IAssetInfo>[] GetAssets()
		{
			return Container.Select(t => new NullableKeyValuePair<Utf8StringBase, IAssetInfo>(new Utf8StringLegacy(t.Key), t.Value)).ToArray();
		}

		public override string ExportExtension => throw new NotSupportedException();

		public PPtr<Object.Object>[] PreloadTable { get; set; }
		public KeyValuePair<string, AssetInfo>[] Container { get; set; }
		public AssetBundleScriptInfo[] ScriptCompatibility { get; set; }
		public KeyValuePair<int, uint>[] ClassCampatibility { get; set; }
		public Dictionary<int, int> ClassVersionMap { get; set; }
		public uint RuntimeCompatibility { get; set; }
		public string AssetBundleName { get; set; }
		public string[] Dependencies { get; set; }
		public bool IsStreamedSceneAssetBundle { get; set; }
		public int ExplicitDataLayout { get; set; }
		public int PathFlags { get; set; }
		public Dictionary<string, string> SceneHashes { get; set; }

		bool IAssetBundle.HasAssetBundleName => HasAssetBundleName(SerializedFile.Version);

		public const string ContainerName = "m_Container";

		public AssetInfo MainAsset = new();
	}
}
