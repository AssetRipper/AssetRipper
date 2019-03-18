using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.LODGroups
{
	public struct LOD : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0 to 5.1.0 exclusive
		/// </summary>
		public static bool IsReadFadeMode(Version version)
		{
			return version.IsGreaterEqual(5) && version.IsLess(5, 1);
		}
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadFadeTransitionWidth(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetReader reader)
		{
			ScreenRelativeHeight = reader.ReadSingle();
			if (IsReadFadeMode(reader.Version))
			{
				FadeMode = (LODFadeMode)reader.ReadInt32();
			}
			if (IsReadFadeTransitionWidth(reader.Version))
			{
				FadeTransitionWidth = reader.ReadSingle();
			}
			m_renderers = reader.ReadAssetArray<LODRenderer>();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (LODRenderer renderer in Renderers)
			{
				foreach (Object asset in renderer.FetchDependencies(file, isLog))
				{
					yield return asset;
				}
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

		public float ScreenRelativeHeight { get; private set; }
		public LODFadeMode FadeMode { get; private set; }
		public float FadeTransitionWidth { get; private set; }
		public IReadOnlyList<LODRenderer> Renderers => m_renderers;

		public const string ScreenRelativeHeightName = "screenRelativeHeight";
		public const string FadeModeName = "fadeMode";
		public const string FadeTransitionWidthName = "fadeTransitionWidth";
		public const string RenderersName = "renderers";

		private LODRenderer[] m_renderers;
	}
}
