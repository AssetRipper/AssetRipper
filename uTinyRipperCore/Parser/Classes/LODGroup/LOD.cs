using System.Collections.Generic;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.LODGroups
{
	public struct LOD : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 5.0.0 to 5.1.0 exclusive
		/// </summary>
		public static bool HasFadeMode(Version version) => version.IsGreaterEqual(5) && version.IsLess(5, 1);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasFadeTransitionWidth(Version version) => version.IsGreaterEqual(5);

		public void Read(AssetReader reader)
		{
			ScreenRelativeHeight = reader.ReadSingle();
			if (HasFadeMode(reader.Version))
			{
				FadeMode = (LODFadeMode)reader.ReadInt32();
			}
			if (HasFadeTransitionWidth(reader.Version))
			{
				FadeTransitionWidth = reader.ReadSingle();
			}
			Renderers = reader.ReadAssetArray<LODRenderer>();
		}

		public IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in context.FetchDependencies(Renderers, RenderersName))
			{
				yield return asset;
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ScreenRelativeHeightName, ScreenRelativeHeight);
			node.Add(FadeTransitionWidthName, FadeTransitionWidth);
			node.Add(RenderersName, Renderers.ExportYAML(container));
			return node;
		}

		public float ScreenRelativeHeight { get; set; }
		public LODFadeMode FadeMode { get; set; }
		public float FadeTransitionWidth { get; set; }
		public LODRenderer[] Renderers { get; set; }

		public const string ScreenRelativeHeightName = "screenRelativeHeight";
		public const string FadeModeName = "fadeMode";
		public const string FadeTransitionWidthName = "fadeTransitionWidth";
		public const string RenderersName = "renderers";
	}
}
