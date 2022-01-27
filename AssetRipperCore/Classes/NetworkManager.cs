using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes
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
		public static bool HasNetworkManager(UnityVersion version) => version.IsGreaterEqual(2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DebugLevel = reader.ReadInt32();
			Sendrate = reader.ReadSingle();
			m_assetToPrefab = new Dictionary<UnityGUID, PPtr<GameObject.GameObject>>();
			m_assetToPrefab.Read(reader);
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(AssetToPrefab.Values, AssetToPrefabName))
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

		private Dictionary<UnityGUID, PPtr<GameObject.GameObject>> m_assetToPrefab = new();
	}
}
