using System.Collections.Generic;
using uTinyRipper.Project;
using uTinyRipper.YAML;
using uTinyRipper.Converters;

namespace uTinyRipper.Classes.GUIStyles
{
	public struct GUIStyleState : IAsset
	{
		public GUIStyleState(bool _)
		{
			Background = default;
			TextColor = default;
			m_scaledBackgrounds = System.Array.Empty<PPtr<Texture2D>>();
		}

		public GUIStyleState(GUIStyleState copy)
		{
			Background = copy.Background;
			TextColor = copy.TextColor;
			m_scaledBackgrounds = new PPtr<Texture2D>[copy.ScaledBackgrounds.Count];
			for(int i = 0; i < copy.ScaledBackgrounds.Count; i++)
			{
				m_scaledBackgrounds[i] = copy.ScaledBackgrounds[i];
			}
		}

		public void Read(AssetReader reader)
		{
			Background.Read(reader);
			m_scaledBackgrounds = System.Array.Empty<PPtr<Texture2D>>();
			//m_scaledBackgrounds = stream.ReadArray<PPtr<Texture2D>>();
			TextColor.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Background.Write(writer);
			//writer.WriteAssetArray(m_scaledBackgrounds);
			TextColor.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(BackgroundName, Background.ExportYAML(container));
			node.Add(ScaledBackgroundsName, m_scaledBackgrounds.ExportYAML(container));
			node.Add(TextColorName, TextColor.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(IDependencyContext context)
		{
			yield break;
		}
		
		public IReadOnlyList<PPtr<Texture2D>> ScaledBackgrounds => m_scaledBackgrounds;

		public const string BackgroundName = "m_Background";
		public const string ScaledBackgroundsName = "m_ScaledBackgrounds";
		public const string TextColorName = "m_TextColor";

		public PPtr<Texture2D> Background;
		public ColorRGBAf TextColor;

		private PPtr<Texture2D>[] m_scaledBackgrounds;
	}
}
