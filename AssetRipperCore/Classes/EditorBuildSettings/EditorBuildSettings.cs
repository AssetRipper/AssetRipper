using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using System.Linq;


namespace AssetRipper.Core.Classes.EditorBuildSettings
{
	public sealed class EditorBuildSettings : Object.Object, IEditorBuildSettings
	{
		public EditorBuildSettings(AssetInfo assetInfo) : base(assetInfo) { }

		public static int ToSerializedVersion(UnityVersion version)
		{
			// unknown version
			// KeyValuePairs to Scene class
			if (version.IsGreaterEqual(2, 5))
			{
				return 2;
			}
			return 1;
		}

		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasConfigObjects(UnityVersion version) => version.IsGreaterEqual(2018);

		/// <summary>
		/// 2.5.0 and greater (NOTE: unknown version)
		/// </summary>
		private static bool HasScenes(UnityVersion version) => version.IsGreaterEqual(2, 5);

		public void Initialize(IEnumerable<Scene> scenes)
		{
			if (scenes == null)
			{
				throw new ArgumentNullException(nameof(scenes));
			}
			m_Scenes = scenes.ToArray();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasScenes(reader.Version))
			{
				m_Scenes = reader.ReadAssetArray<Scene>();
			}
			else
			{
				Tuple<bool, string>[] scenes = reader.ReadTupleBoolStringArray();
				m_Scenes = scenes.Select(t => new Scene(t.Item1, t.Item2)).ToArray();
			}
			if (HasConfigObjects(reader.Version))
			{
				ConfigObjects = new Dictionary<string, PPtr<Object.Object>>();
				ConfigObjects.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(ToSerializedVersion(container.ExportVersion));
			node.Add(ScenesName, m_Scenes.ExportYAML(container));
			if (HasConfigObjects(container.ExportVersion))
			{
				node.Add(ConfigObjectsName, GetConfigObjects(container.Version).ExportYAML(container));
			}
			return node;
		}

		private IReadOnlyDictionary<string, PPtr<Object.Object>> GetConfigObjects(UnityVersion version)
		{
			return HasConfigObjects(version) ? ConfigObjects : new Dictionary<string, PPtr<Object.Object>>(0);
		}

		public void InitializeScenesArray(int length)
		{
			m_Scenes = new Scene[length];
			for (int i = 0; i < length; i++)
			{
				m_Scenes[i] = new Scene();
			}
		}

		public Scene[] m_Scenes;
		public IEditorScene[] Scenes
		{
			get => m_Scenes;
		}

		public Dictionary<string, PPtr<Object.Object>> ConfigObjects { get; set; } = new Dictionary<string, PPtr<Object.Object>>();

		public const string ScenesName = "m_Scenes";
		public const string ConfigObjectsName = "m_configObjects";
	}
}
