using System;
using System.Collections.Generic;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.TextureImporters;

namespace uTinyRipper.Classes.SpriteAtlases
{
	public class SpriteAtlasEditorData : IAssetReadable, IYAMLExportable, IDependent
	{
		public SpriteAtlasEditorData()
		{
		}

		public SpriteAtlasEditorData(IReadOnlyList<PPtr<Sprite>> packables)
		{
			TextureSettings = new TextureSettings(true);
			m_platformSettings = Array.Empty<TextureImporterPlatformSettings>();
			PackingSettings = new PackingSettings(true);
			VariantMultiplier = 1;
			m_packables = new PPtr<Object>[packables.Count];
			for (int i = 0; i < packables.Count; i++)
			{
				m_packables[i] = packables[i].CastTo<Object>();
			}
			BindAsDefault = true;
		}

		/// <summary>
		/// 2018.4.9 to 2019.1 exclusive or 2019.2.9 and greater
		/// </summary>
		public static bool HasStoredHash(Version version)
		{
			if (version.IsGreaterEqual(2019, 2, 9))
			{
				return true;
			}
			if (version.IsGreaterEqual(2019))
			{
				return false;
			}
			return version.IsGreaterEqual(2018, 4, 9);
		}

		private static int GetSerializedVersion(Version version)
		{
			// PackingParameters was renamed to PackingSettings
			// DefaultPlatformSettings has been added
			if (version.IsGreaterEqual(2018, 2))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			TextureSettings.Read(reader);
			m_platformSettings = reader.ReadAssetArray<TextureImporterPlatformSettings>();
			PackingSettings.Read(reader);
			VariantMultiplier = reader.ReadSingle();
			m_packables = reader.ReadAssetArray<PPtr<Object>>();
			BindAsDefault = reader.ReadBoolean();
			if (HasStoredHash(reader.Version))
			{
				StoredHash.Read(reader);
			}
			reader.AlignStream();
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in context.FetchDependencies(Packables, PackablesName))
			{
				yield return asset;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(PlatformSettingsName, GetPlatformSettings(container.Version, container.ExportVersion).ExportYAML(container));
			node.Add(GetPackingSettingsName(container.ExportVersion), PackingSettings.ExportYAML(container));
			node.Add(VariantMultiplierName, VariantMultiplier);
			node.Add(PackablesName, Packables.ExportYAML(container));
			node.Add(BindAsDefaultName, BindAsDefault);
			if (HasStoredHash(container.ExportVersion))
			{
				node.Add(StoredHashName, StoredHash.ExportYAML(container));
			}
			return node;
		}

		public IReadOnlyList<TextureImporterPlatformSettings> GetPlatformSettings(Version version, Version exportVersion)
		{
			if (GetSerializedVersion(exportVersion) > 1)
			{
				if (GetSerializedVersion(version) > 1)
				{
					return m_platformSettings;
				}
				else
				{
					TextureImporterPlatformSettings[] settings = new TextureImporterPlatformSettings[m_platformSettings.Length + 1];
					TextureImporterPlatformSettings setting = new TextureImporterPlatformSettings(exportVersion);
					setting.TextureFormat = TextureFormat.Automatic;
					setting.ForceMaximumCompressionQuality_BC6H_BC7 = true;
					settings[0] = setting;
					Array.Copy(m_platformSettings, 0, settings, 1, m_platformSettings.Length);
					return settings;
				}
			}
			else
			{
				return m_platformSettings;
			}
		}

		private string GetPackingSettingsName(Version version)
		{
			return GetSerializedVersion(version) > 1 ? PackingSettingsName : PackingParametersName;
		}

		public IReadOnlyList<TextureImporterPlatformSettings> PlatformSettings => m_platformSettings;
		public float VariantMultiplier { get; private set; }
		public IReadOnlyList<PPtr<Object>> Packables => m_packables;
		public bool BindAsDefault { get; private set; }

		public const string TextureSettingsName = "textureSettings";
		public const string PlatformSettingsName = "platformSettings";
		public const string PackingParametersName = "packingParameters";
		public const string PackingSettingsName = "packingSettings";
		public const string VariantMultiplierName = "variantMultiplier";
		public const string PackablesName = "packables";
		public const string BindAsDefaultName = "bindAsDefault";
		public const string StoredHashName = "storedHash";

		public TextureSettings TextureSettings;
		/// <summary>
		/// PackingParameters previously
		/// </summary>
		public PackingSettings PackingSettings;
		public Hash128 StoredHash;

		private TextureImporterPlatformSettings[] m_platformSettings;
		private PPtr<Object>[] m_packables;
	}
}
