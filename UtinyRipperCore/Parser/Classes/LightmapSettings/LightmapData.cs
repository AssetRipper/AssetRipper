using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.LightmapSettingss
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

		public void Read(AssetStream stream)
		{
			Lightmap.Read(stream);
			if(IsReadDirLightmap(stream.Version))
			{
				DirLightmap.Read(stream);
			}
			if(IsReadShadowMask(stream.Version))
			{
				ShadowMask.Read(stream);
			}
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return Lightmap.FetchDependency(file, isLog, () => nameof(LightmapData), "m_Lightmap");
			yield return DirLightmap.FetchDependency(file, isLog, () => nameof(LightmapData), "m_DirLightmap");
			yield return ShadowMask.FetchDependency(file, isLog, () => nameof(LightmapData), "m_ShadowMask");
		}

		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_Lightmap", Lightmap.ExportYAML(exporter));
			node.Add("m_DirLightmap", DirLightmap.ExportYAML(exporter));
			node.Add("m_ShadowMask", ShadowMask.ExportYAML(exporter));
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
