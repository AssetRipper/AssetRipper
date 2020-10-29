using System.Collections.Generic;
using uTinyRipper.Converters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.LightmapSettingss
{
	public struct LightmapData : IAsset, IDependent
	{
		public static int ToSerializedVersion(Version version)
		{
			// NOTE: unknown conversion
			if (version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 3.0.0 to 5.6.0b1
		/// </summary>
		public static bool HasIndirectLightmap(Version version) => version.IsGreaterEqual(3) && version.IsLessEqual(5, 6, 0, VersionType.Beta, 1);
		/// <summary>
		/// 5.0.0bx (NOTE: unknown version)
		/// </summary>
		public static bool HasLightInd(Version version) => version.IsEqual(5, 0, 0, VersionType.Beta);
		/// <summary>
		/// 5.6.0b2 and greater
		/// </summary>
		public static bool HasDirLightmap(Version version) => version.IsGreaterEqual(5, 6, 0, VersionType.Beta, 2);

		public void Read(AssetReader reader)
		{
			Lightmap.Read(reader);
			if (HasIndirectLightmap(reader.Version))
			{
				IndirectLightmap.Read(reader);
			}
			if (HasLightInd(reader.Version))
			{
				LightInd.Read(reader);
				DirInd.Read(reader);
			}
			if (HasDirLightmap(reader.Version))
			{
				DirLightmap.Read(reader);
				ShadowMask.Read(reader);
			}
		}

		public void Write(AssetWriter writer)
		{
			Lightmap.Write(writer);
			if (HasIndirectLightmap(writer.Version))
			{
				IndirectLightmap.Write(writer);
			}
			if (HasLightInd(writer.Version))
			{
				LightInd.Write(writer);
				DirInd.Write(writer);
			}
			if (HasDirLightmap(writer.Version))
			{
				DirLightmap.Write(writer);
				ShadowMask.Write(writer);
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Lightmap, LightmapName);
			if (HasIndirectLightmap(context.Version))
			{
				yield return context.FetchDependency(IndirectLightmap, IndirectLightmapName);
			}
			if (HasLightInd(context.Version))
			{
				yield return context.FetchDependency(LightInd, LightIndName);
				yield return context.FetchDependency(DirInd, DirIndName);
			}
			if (HasDirLightmap(context.Version))
			{
				yield return context.FetchDependency(DirLightmap, DirLightmapName);
				yield return context.FetchDependency(ShadowMask, ShadowMaskName);
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(LightmapName, Lightmap.ExportYAML(container));
			if (HasIndirectLightmap(container.ExportVersion))
			{
				node.Add(IndirectLightmapName, IndirectLightmap.ExportYAML(container));
			}
			if (HasLightInd(container.ExportVersion))
			{
				node.Add(LightIndName, LightInd.ExportYAML(container));
				node.Add(DirIndName, DirInd.ExportYAML(container));
			}
			if (HasDirLightmap(container.ExportVersion))
			{
				node.Add(DirLightmapName, DirLightmap.ExportYAML(container));
				node.Add(ShadowMaskName, ShadowMask.ExportYAML(container));
			}
			return node;
		}

		public const string LightmapName = "m_Lightmap";
		public const string IndirectLightmapName = "m_IndirectLightmap";
		public const string LightIndName = "m_LightInd";
		public const string DirIndName = "m_DirInd";
		public const string DirLightmapName = "m_DirLightmap";
		public const string ShadowMaskName = "m_ShadowMask";

		public PPtr<Texture2D> Lightmap;
		public PPtr<Texture2D> IndirectLightmap;
		public PPtr<Texture2D> LightInd;
		public PPtr<Texture2D> DirInd;
		public PPtr<Texture2D> DirLightmap;
		public PPtr<Texture2D> ShadowMask;
	}
}
