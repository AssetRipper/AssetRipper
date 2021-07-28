using AssetRipper.Project;
using AssetRipper.Parser.Asset;
using AssetRipper.Classes.Misc;
using AssetRipper.Parser.Files;
using AssetRipper.Parser.Files.SerializedFiles;
using AssetRipper.IO.Asset;
using AssetRipper.IO.Extensions;
using AssetRipper.YAML;
using System.Collections.Generic;

namespace AssetRipper.Classes
{
	public sealed class NetworkManager : GlobalGameManager
	{
		public NetworkManager(AssetInfo assetInfo) : base(assetInfo) { }

		public NetworkManager(AssetInfo assetInfo, bool _) : this(assetInfo)
		{
			Sendrate = 15.0f;
			m_assetToPrefab = new Dictionary<UnityGUID, PPtr<GameObject.GameObject>>();
		}

		public static NetworkManager CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new NetworkManager(assetInfo));
		}

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool HasNetworkManager(Version version) => version.IsGreaterEqual(2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DebugLevel = reader.ReadInt32();
			Sendrate = reader.ReadSingle();
			m_assetToPrefab = new Dictionary<UnityGUID, PPtr<GameObject.GameObject>>();
			m_assetToPrefab.Read(reader);
		}

		public override IEnumerable<PPtr<Object.Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object.Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<Object.Object> asset in context.FetchDependencies(AssetToPrefab.Values, AssetToPrefabName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(DebugLevelName, DebugLevel);
			node.Add(SendrateName, Sendrate);
			node.Add(AssetToPrefabName, AssetToPrefab.ExportYAML(container));
			return node;
		}

		public int DebugLevel { get; set; }
		public float Sendrate { get; set; }
		public IReadOnlyDictionary<UnityGUID, PPtr<GameObject.GameObject>> AssetToPrefab => m_assetToPrefab;

		public const string DebugLevelName = "m_DebugLevel";
		public const string SendrateName = "m_Sendrate";
		public const string AssetToPrefabName = "m_AssetToPrefab";

		private Dictionary<UnityGUID, PPtr<GameObject.GameObject>> m_assetToPrefab;
	}
}
