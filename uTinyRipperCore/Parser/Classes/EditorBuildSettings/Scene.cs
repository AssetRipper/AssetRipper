using System;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.EditorBuildSettingss
{
	public struct Scene : IAssetReadable, IYAMLExportable
	{
		public Scene(bool enabled, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			Enabled = enabled;
			Path = path;
			GUID = default;
		}

		public Scene(string path, EngineGUID guid)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			Enabled = true;
			Path = path;
			GUID = guid;
		}

		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadGuid(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}

		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream(AlignType.Align4);
			
			Path = reader.ReadString();
			GUID.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("enabled", Enabled);
			node.Add("path", Path);
			node.Add("guid", GUID.ExportYAML(container));
			return node;
		}

		public bool Enabled { get; private set; }
		public string Path { get; private set; }

		public EngineGUID GUID;
	}
}
