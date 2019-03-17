using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.AssetExporters.Classes;
using uTinyRipper.Classes.Textures;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.SpriteAtlases
{
	public class SpriteAtlasEditorData : IAssetReadable, IYAMLExportable
	{
		public SpriteAtlasEditorData()
		{
		}

		public SpriteAtlasEditorData(IReadOnlyList<PPtr<Sprite>> packables)
		{
			TextureSettings = new TextureSettings(true);
			m_platformSettings = new TextureImporterPlatformSettings[0];
			PackingSettings = new PackingSettings(true);
			VariantMultiplier = 1;
			m_packables = new PPtr<Object>[packables.Count];
			for (int i = 0; i < packables.Count; i++)
			{
				m_packables[i] = packables[i].CastTo<Object>();
			}
			BindAsDefault = true;
		}

		private static int GetSerializedVersion(Version version)
		{
			// packingParameters was renamed to packingSettings. Added DefaultPlatformSettings
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
			reader.AlignStream(AlignType.Align4);
			
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (PPtr<Object> packable in Packables)
			{
				yield return packable.FetchDependency(file, isLog, () => nameof(SpriteAtlasEditorData), "packables");
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
					settings[0] = new TextureImporterPlatformSettings(TextureFormat.Automatic);
					Array.Copy(m_platformSettings, 0, settings, 1, m_platformSettings.Length);
					return settings;
				}
			}
			else
			{
				if (GetSerializedVersion(version) > 1)
				{
					List<TextureImporterPlatformSettings> settings = new List<TextureImporterPlatformSettings>();
					foreach(TextureImporterPlatformSettings setting in m_platformSettings)
					{
						if (setting.BuildTarget != TextureImporterPlatformSettings.DefaultTexturePlatformName)
						{
							settings.Add(setting);
						}
					}
					return settings;
				}
				else
				{
					return m_platformSettings;
				}
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

		public TextureSettings TextureSettings;
		/// <summary>
		/// PackingParameters previously
		/// </summary>
		public PackingSettings PackingSettings;

		private TextureImporterPlatformSettings[] m_platformSettings;
		private PPtr<Object>[] m_packables;
	}
}
