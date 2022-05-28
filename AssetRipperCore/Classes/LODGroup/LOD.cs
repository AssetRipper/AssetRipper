using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.LODGroup
{
	public sealed class LOD : IAssetReadable, IYamlExportable, IDependent
	{
		/// <summary>
		/// 5.0.0 to 5.1.0 exclusive
		/// </summary>
		public static bool HasFadeMode(UnityVersion version) => version.IsGreaterEqual(5) && version.IsLess(5, 1);
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool HasFadeTransitionWidth(UnityVersion version) => version.IsGreaterEqual(5);

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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray(Renderers, RenderersName))
			{
				yield return asset;
			}
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(ScreenRelativeHeightName, ScreenRelativeHeight);
			node.Add(FadeTransitionWidthName, FadeTransitionWidth);
			node.Add(RenderersName, Renderers.ExportYaml(container));
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
