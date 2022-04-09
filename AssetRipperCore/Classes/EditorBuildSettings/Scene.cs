using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;


namespace AssetRipper.Core.Classes.EditorBuildSettings
{
	public sealed class Scene : UnityAssetBase, IEditorScene
	{
		public Scene() { }

		public Scene(bool enabled, string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentNullException(nameof(path));
			}

			Enabled = enabled;
			Path = path;
			m_GUID = new();
		}

		public Scene(string path, UnityGUID guid)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			Enabled = true;
			Path = path;
			m_GUID = guid;
		}

		/// <summary>
		/// 5.6.0b10 and greater
		/// </summary>
		public static bool HasGuid(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 10);

		public override void Read(AssetReader reader)
		{
			Enabled = reader.ReadBoolean();
			reader.AlignStream();

			Path = reader.ReadString();
			if (HasGuid(reader.Version))
			{
				m_GUID.Read(reader);
			}
		}

		public override YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(EnabledName, Enabled);
			node.Add(PathName, Path);
			node.Add(GuidName, m_GUID.ExportYAML(container));
			return node;
		}

		public bool Enabled { get; set; }
		public string Path { get; set; }

		public const string EnabledName = "enabled";
		public const string PathName = "path";
		public const string GuidName = "guid";

		public UnityGUID m_GUID = new();

		public UnityGUID GUID
		{
			get => m_GUID;
			set => m_GUID = value;
		}
	}
}
