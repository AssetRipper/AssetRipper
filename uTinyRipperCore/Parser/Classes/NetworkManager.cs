using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes
{
	public sealed class NetworkManager : GlobalGameManager
	{
		public NetworkManager(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		public NetworkManager(AssetInfo assetInfo, bool _) :
			this(assetInfo)
		{
			Sendrate = 15.0f;
			m_assetToPrefab = new Dictionary<GUID, PPtr<GameObject>>();
		}

		public static NetworkManager CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new NetworkManager(assetInfo));
		}

		/// <summary>
		/// 2.0.0 and greater
		/// </summary>
		public static bool IsReadNetworkManager(Version version)
		{
			return version.IsGreaterEqual(2);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			DebugLevel = reader.ReadInt32();
			Sendrate = reader.ReadSingle();
			m_assetToPrefab = new Dictionary<GUID, PPtr<GameObject>>();
			m_assetToPrefab.Read(reader);
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}

			foreach(PPtr<GameObject> prefab in AssetToPrefab.Values)
			{
				yield return prefab.FetchDependency(file);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_DebugLevel", DebugLevel);
			node.Add("m_Sendrate", Sendrate);
			node.Add("m_AssetToPrefab", AssetToPrefab.ExportYAML(container));
			return node;
		}

		public int DebugLevel { get; private set; }
		public float Sendrate { get; private set; }
		public IReadOnlyDictionary<GUID, PPtr<GameObject>> AssetToPrefab => m_assetToPrefab;

		private Dictionary<GUID, PPtr<GameObject>> m_assetToPrefab;
	}
}
