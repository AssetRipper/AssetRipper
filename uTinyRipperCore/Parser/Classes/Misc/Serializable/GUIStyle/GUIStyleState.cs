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
			m_scaledBackgrounds = new PPtr<Texture2D>[0];
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
			m_scaledBackgrounds = new PPtr<Texture2D>[0];
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
			node.Add("m_Background", Background.ExportYAML(container));
			node.Add("m_ScaledBackgrounds", m_scaledBackgrounds.ExportYAML(container));
			node.Add("m_TextColor", TextColor.ExportYAML(container));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield break;
		}
		
		public IReadOnlyList<PPtr<Texture2D>> ScaledBackgrounds => m_scaledBackgrounds;

		public PPtr<Texture2D> Background;
		public ColorRGBAf TextColor;

		private PPtr<Texture2D>[] m_scaledBackgrounds;
	}
}
