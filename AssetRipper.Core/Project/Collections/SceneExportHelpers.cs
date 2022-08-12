using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using System.IO;
using System.Text.RegularExpressions;

namespace AssetRipper.Core.Project.Collections
{
	public static class SceneExportHelpers
	{
		private const string AssetsName = "Assets/";
		private const string LevelName = "level";
		private const string MainSceneName = "maindata";

		private static readonly Regex s_sceneNameFormat = new Regex($"^{LevelName}(0|[1-9][0-9]*)$");

		public static int FileNameToSceneIndex(string name, UnityVersion version)
		{
			if (HasMainData(version))
			{
				if (name == MainSceneName)
				{
					return 0;
				}

				string indexStr = name.Substring(LevelName.Length);
				return int.Parse(indexStr) + 1;
			}
			else
			{
				string indexStr = name.Substring(LevelName.Length);
				return int.Parse(indexStr);
			}
		}

		/// <summary>
		/// Less than 5.3.0
		/// </summary>
		public static bool HasMainData(UnityVersion version) => version.IsLess(5, 3);

		/// <summary>
		/// GameObject, Classes Inherited From Level Game Manager, Monobehaviours with GameObjects, Components
		/// </summary>
		public static bool IsSceneCompatible(IUnityObjectBase asset)
		{
			return asset switch
			{
				IGameObject => true,
				ILevelGameManager => true,
				IMonoBehaviour monoBeh => monoBeh.IsSceneObject(),
				IComponent => true,
				_ => false
			};
		}

		public static string SceneIndexToFileName(int index, UnityVersion version)
		{
			if (HasMainData(version))
			{
				if (index == 0)
				{
					return MainSceneName;
				}
				return LevelName + (index - 1).ToString();
			}
			return LevelName + index.ToString();
		}

		internal static string GetSceneSubPath(IExportContainer container, ISerializedFile serializedFile, out bool isRegular)
		{
			if (IsSceneName(serializedFile.Name))
			{
				int index = FileNameToSceneIndex(serializedFile.Name, serializedFile.Version);
				string scenePath = container.SceneIndexToName(index);
				if (scenePath.StartsWith(AssetsName, StringComparison.Ordinal))
				{
					string extension = Path.GetExtension(scenePath);
					isRegular = true;
					return scenePath.Substring(AssetsName.Length, scenePath.Length - AssetsName.Length - extension.Length);
				}
				else if (Path.IsPathRooted(scenePath))
				{
					// pull/uTiny 617
					// NOTE: absolute project path may contain Assets/ in its name so in this case we get incorrect scene path, but there is no way to bypass this issue
					int assetIndex = scenePath.IndexOf(AssetsName);
					string extension = Path.GetExtension(scenePath);
					isRegular = true;
					return scenePath.Substring(assetIndex + AssetsName.Length, scenePath.Length - assetIndex - AssetsName.Length - extension.Length);
				}
				else if (scenePath.Length == 0)
				{
					// if you build a game without included scenes, Unity create one with empty name
					isRegular = false;
					return serializedFile.Name;
				}
				else
				{
					isRegular = false;
					return scenePath;
				}
			}
			isRegular = false;
			return serializedFile.Name;
		}

		internal static bool IsDuplicate(IExportContainer container, ISerializedFile serializedFile)
		{
			if (IsSceneName(serializedFile.Name))
			{
				int index = FileNameToSceneIndex(serializedFile.Name, serializedFile.Version);
				return container.IsSceneDuplicate(index);
			}
			return false;
		}

		private static bool IsSceneName(string name) => name == MainSceneName || s_sceneNameFormat.IsMatch(name);
	}
}
