namespace AssetRipper.Export.UnityProjects.EngineAssets
{
	internal readonly struct EngineBuiltInAssetInfo
	{
		public EngineBuiltInAssetInfo(UnityVersion version, EngineBuiltInAsset asset)
		{
			m_variations = new List<KeyValuePair<UnityVersion, EngineBuiltInAsset>>(1)
			{
				new KeyValuePair<UnityVersion, EngineBuiltInAsset>(version, asset)
			};
		}

		public void AddVariation(UnityVersion version, EngineBuiltInAsset asset)
		{
			KeyValuePair<UnityVersion, EngineBuiltInAsset> kvp = new KeyValuePair<UnityVersion, EngineBuiltInAsset>(version, asset);
			for (int i = 0; i < m_variations.Count; i++)
			{
				UnityVersion key = m_variations[i].Key;
				if (key < version)
				{
					m_variations.Insert(i, kvp);
					return;
				}
			}
			m_variations.Add(kvp);
		}

		public bool ContainsAsset(UnityVersion version)
		{
			foreach ((UnityVersion variationVersion, _) in m_variations)
			{
				if (version >= variationVersion)
				{
					return true;
				}
			}
			return false;
		}

		public EngineBuiltInAsset GetAsset(UnityVersion version)
		{
			foreach (KeyValuePair<UnityVersion, EngineBuiltInAsset> kvp in m_variations)
			{
				if (version >= kvp.Key)
				{
					return kvp.Value;
				}
			}
			throw new Exception($"There is no asset for {version} version");
		}

		public bool TryGetAsset(UnityVersion version, out EngineBuiltInAsset asset)
		{
			foreach ((UnityVersion variationVersion, EngineBuiltInAsset variationAsset) in m_variations)
			{
				if (version >= variationVersion)
				{
					asset = variationAsset;
					return true;
				}
			}
			asset = default;
			return false;
		}

		private readonly List<KeyValuePair<UnityVersion, EngineBuiltInAsset>> m_variations;
	}
}
