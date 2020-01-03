using System;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Converters;
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

		public Scene(string path, GUID guid)
		{
			if (path == null)
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
		public static bool HasGuid(Version version) => version.IsGreaterEqual(5, 6);

		public void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream();
			
			Path = reader.ReadString();
			GUID.Read(reader);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EnabledName, Enabled);
			node.Add(PathName, Path);
			node.Add(GuidName, GUID.ExportYAML(container));
			return node;
		}

		public bool Enabled { get; set; }
		public string Path { get; set; }

		public const string EnabledName = "enabled";
		public const string PathName = "path";
		public const string GuidName = "guid";

		public GUID GUID;
	}
}
