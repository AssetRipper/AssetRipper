using System;
using System.Collections.Generic;
using System.Linq;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.EditorBuildSettingss;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class EditorBuildSettings : Object
	{
		public EditorBuildSettings(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static EditorBuildSettings CreateVirtualInstance(VirtualSerializedFile virtualFile)
		{
			return virtualFile.CreateAsset((assetInfo) => new EditorBuildSettings(assetInfo));
		}

		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadConfigObjects(Version version)
		{
			return version.IsGreaterEqual(2018);
		}

		private static bool IsReadScenes(Version version)
		{
#warning unknown
			return version.IsGreaterEqual(2, 5);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				return 2;
			}

			// KeyValuePairs to Scene class
			if (IsReadScenes(version))
			{
				return 2;
			}
			return 1;
		}

		public void Initialize(IEnumerable<Scene> scenes)
		{
			if (scenes == null)
			{
				throw new ArgumentNullException(nameof(scenes));
			}
			m_scenes = scenes.ToArray();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (IsReadScenes(reader.Version))
			{
				m_scenes = reader.ReadAssetArray<Scene>();
			}
			else
			{
				Tuple<bool, string>[] scenes = reader.ReadTupleBoolStringArray();
				m_scenes = scenes.Select(t => new Scene(t.Item1, t.Item2)).ToArray();
			}
			if(IsReadConfigObjects(reader.Version))
			{
				m_configObjects = new Dictionary<string, PPtr<Object>>();
				m_configObjects.Read(reader);
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.AddSerializedVersion(GetSerializedVersion(container.Version));
			node.Add("m_Scenes", Scenes.ExportYAML(container));
#warning TODO: 2018
			//node.Add("m_configObjects", ConfigObjects.ExportYAML(container));
			return node;
		}

		public IReadOnlyList<Scene> Scenes => m_scenes;
		public IReadOnlyDictionary<string, PPtr<Object>> ConfigObjects => m_configObjects;

		private Scene[] m_scenes;
		private Dictionary<string, PPtr<Object>> m_configObjects;
	}
}
