using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.Shaders
{
	public struct ShaderCompilationInfo : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.2.0 adn greater
		/// </summary>
		public static bool IsReadHasFixedFunctionShaders(Version version)
		{
			return version.IsGreaterEqual(5, 2);
		}

		public void Read(AssetReader reader)
		{
			m_snippets = new Dictionary<int, ShaderSnippet>();
			m_snippets.Read(reader);
			MeshComponentsFromSnippets = reader.ReadInt32();
			HasSurfaceShaders = reader.ReadBoolean();
			if (IsReadHasFixedFunctionShaders(reader.Version))
			{
				HasFixedFunctionShaders = reader.ReadBoolean();
			}
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(SnippetsName, Snippets.ExportYAML(container));
			node.Add(MeshComponentsFromSnippetsName, MeshComponentsFromSnippets);
			node.Add(HasSurfaceShadersName, HasSurfaceShaders);
			node.Add(HasFixedFunctionShadersName, HasFixedFunctionShaders);
			return node;
		}

		public IReadOnlyDictionary<int, ShaderSnippet> Snippets => m_snippets;
		public int MeshComponentsFromSnippets { get; private set; }
		public bool HasSurfaceShaders { get; private set; }
		public bool HasFixedFunctionShaders { get; private set; }

		public const string SnippetsName = "m_Snippets";
		public const string MeshComponentsFromSnippetsName = "m_MeshComponentsFromSnippets";
		public const string HasSurfaceShadersName = "m_HasSurfaceShaders";
		public const string HasFixedFunctionShadersName = "m_HasFixedFunctionShaders";

		private Dictionary<int, ShaderSnippet> m_snippets;
	}
}
