using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct LightmapData : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadDirLightmap(Version version)
		{
			return version.IsGreaterEqual(3);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadShadowMask(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			if (version.IsGreaterEqual(5, 6))
			{
				return 2;
			}
			return 1;
		}

		public void Read(AssetReader reader)
		{
			Lightmap.Read(reader);
			if(IsReadDirLightmap(reader.Version))
			{
				DirLightmap.Read(reader);
			}
			if(IsReadShadowMask(reader.Version))
			{
				ShadowMask.Read(reader);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Lightmap.FetchDependency(file, isLog, () => nameof(LightmapData), "m_Lightmap");
			yield return DirLightmap.FetchDependency(file, isLog, () => nameof(LightmapData), "m_DirLightmap");
			yield return ShadowMask.FetchDependency(file, isLog, () => nameof(LightmapData), "m_ShadowMask");
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Lightmap", Lightmap.ExportYAML(container));
			node.Add("m_DirLightmap", DirLightmap.ExportYAML(container));
			node.Add("m_ShadowMask", ShadowMask.ExportYAML(container));
			return node;
		}

		public PPtr<Texture2D> Lightmap;
		/// <summary>
		/// IndirectLightmap previously
		/// </summary>
		public PPtr<Texture2D> DirLightmap;
		public PPtr<Texture2D> ShadowMask;
	}
}
