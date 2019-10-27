using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct LightmapData : IAssetReadable, IYAMLExportable, IDependent
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

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield return context.FetchDependency(Lightmap, LightmapName);
			yield return context.FetchDependency(DirLightmap, DirLightmapName);
			yield return context.FetchDependency(ShadowMask, ShadowMaskName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(container.ExportVersion));
			node.Add(LightmapName, Lightmap.ExportYAML(container));
			node.Add(DirLightmapName, DirLightmap.ExportYAML(container));
			node.Add(ShadowMaskName, ShadowMask.ExportYAML(container));
			return node;
		}

		public const string LightmapName = "m_Lightmap";
		public const string DirLightmapName = "m_DirLightmap";
		public const string ShadowMaskName = "m_ShadowMask";

		public PPtr<Texture2D> Lightmap;
		/// <summary>
		/// IndirectLightmap previously
		/// </summary>
		public PPtr<Texture2D> DirLightmap;
		public PPtr<Texture2D> ShadowMask;
	}
}
