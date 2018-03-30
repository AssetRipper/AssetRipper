using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.Materials
{
	public struct UnityPropertySheet : IAssetReadable, IYAMLExportable
	{
		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 3;
			}

			if(version.IsGreaterEqual(2017, 3))
			{
				return 3;
			}
			// min version is 2
			return 2;
		}

		public void Read(AssetStream stream)
		{
			m_texEnvs = new Dictionary<FastPropertyName, UnityTexEnv>();
			m_floats = new Dictionary<FastPropertyName, float>();
			m_colors = new Dictionary<FastPropertyName, ColorRGBAf>();

			m_texEnvs.Read(stream);
			m_floats.Read(stream);
			m_colors.Read(stream);
		}
		
		public YAMLNode ExportYAML(IAssetsExporter exporter)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
			YAMLMappingNode node = new YAMLMappingNode();
			node.AddSerializedVersion(GetSerializedVersion(exporter.Version));
			node.Add("m_TexEnvs", m_texEnvs.ExportYAML(exporter));
			node.Add("m_Floats", m_floats.ExportYAML(exporter));
			node.Add("m_Colors", m_colors.ExportYAML(exporter));
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach(UnityTexEnv env in m_texEnvs.Values)
			{
				foreach(Object @object in env.FetchDependencies(file, isLog))
				{
					yield return @object;
				}
			}
		}

		public IReadOnlyDictionary<FastPropertyName, UnityTexEnv> TexEnvs => m_texEnvs;
		public IReadOnlyDictionary<FastPropertyName, float> Floats => m_floats;
		public IReadOnlyDictionary<FastPropertyName, ColorRGBAf> Colors => m_colors;

		private Dictionary<FastPropertyName, UnityTexEnv> m_texEnvs;
		private Dictionary<FastPropertyName, float> m_floats;
		private Dictionary<FastPropertyName, ColorRGBAf> m_colors;
	}
}
