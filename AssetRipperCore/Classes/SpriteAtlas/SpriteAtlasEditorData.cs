using AssetRipper.Core.Classes.Meta.Importers.Texture;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Texture2D;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;


namespace AssetRipper.Core.Classes.SpriteAtlas
{
	public sealed class SpriteAtlasEditorData : IAssetReadable, IYAMLExportable, IDependent
	{
		public SpriteAtlasEditorData() { }

		public SpriteAtlasEditorData(IReadOnlyList<PPtr<Sprite.Sprite>> packables)
		{
			TextureSettings = new TextureSettings(true);
			PlatformSettings = Array.Empty<TextureImporterPlatformSettings>();
			PackingSettings = new PackingSettings(true);
			VariantMultiplier = 1;
			Packables = new PPtr<Object.Object>[packables.Count];
			for (int i = 0; i < packables.Count; i++)
			{
				Packables[i] = packables[i].CastTo<Object.Object>();
			}
			BindAsDefault = true;
		}

		public static int ToSerializedVersion(UnityVersion version)
		{
			// PackingParameters was renamed to PackingSettings
			// DefaultPlatformSettings has been added
			if (version.IsGreaterEqual(2018, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// (2018.4.9 to 2018.4.15) or (2019.2.9 to 2019.2.18) or (2019.3.0 to 2019.3.1)
		/// </summary>
		public static bool HasStoredHash(UnityVersion version)
		{
			if (version.IsGreaterEqual(2019))
			{
				if (version.IsGreaterEqual(2019, 3))
				{
					return version.IsLessEqual(2019, 3, 1);
				}
				else if (version.IsGreaterEqual(2019, 2, 9))
				{
					return version.IsLessEqual(2019, 2, 18);
				}
			}
			else if (version.IsGreaterEqual(2018, 4, 9))
			{
				return version.IsLessEqual(2018, 4, 15);
			}
			return false;
		}

		public void Read(AssetReader reader)
		{
			TextureSettings.Read(reader);
			PlatformSettings = reader.ReadAssetArray<TextureImporterPlatformSettings>();
			PackingSettings.Read(reader);
			VariantMultiplier = reader.ReadSingle();
			Packables = reader.ReadAssetArray<PPtr<Object.Object>>();
			BindAsDefault = reader.ReadBoolean();
			if (HasStoredHash(reader.Version))
			{
				StoredHash.Read(reader);
			}
			reader.AlignStream();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(Packables, PackablesName))
			{
				yield return asset;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(TextureSettingsName, TextureSettings.ExportYAML(container));
			node.Add(PlatformSettingsName, GetPlatformSettings(container).ExportYAML(container));
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

		public IReadOnlyList<TextureImporterPlatformSettings> GetPlatformSettings(IExportContainer container)
		{
			if (ToSerializedVersion(container.ExportVersion) > 1)
			{
				if (ToSerializedVersion(container.Version) > 1)
				{
					return PlatformSettings;
				}
				else
				{
					TextureImporterPlatformSettings[] settings = new TextureImporterPlatformSettings[PlatformSettings.Length + 1];
					TextureImporterPlatformSettings setting = new TextureImporterPlatformSettings(container.ExportLayout);
					setting.TextureFormat = TextureFormat.Automatic;
					setting.ForceMaximumCompressionQuality_BC6H_BC7 = true;
					settings[0] = setting;
					Array.Copy(PlatformSettings, 0, settings, 1, PlatformSettings.Length);
					return settings;
				}
			}
			else
			{
				return PlatformSettings;
			}
		}

		private string GetPackingSettingsName(UnityVersion version)
		{
			return ToSerializedVersion(version) > 1 ? PackingSettingsName : PackingParametersName;
		}

		public TextureImporterPlatformSettings[] PlatformSettings { get; set; }
		public float VariantMultiplier { get; set; }
		public PPtr<Object.Object>[] Packables { get; set; }
		public bool BindAsDefault { get; set; }

		public const string TextureSettingsName = "textureSettings";
		public const string PlatformSettingsName = "platformSettings";
		public const string PackingParametersName = "packingParameters";
		public const string PackingSettingsName = "packingSettings";
		public const string VariantMultiplierName = "variantMultiplier";
		public const string PackablesName = "packables";
		public const string BindAsDefaultName = "bindAsDefault";
		public const string StoredHashName = "storedHash";

		public TextureSettings TextureSettings = new();
		/// <summary>
		/// PackingParameters previously
		/// </summary>
		public PackingSettings PackingSettings = new();
		public Hash128 StoredHash = new();
	}
}
