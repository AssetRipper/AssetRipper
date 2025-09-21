using AssetRipper.Assets;
using AssetRipper.Assets.Collections;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_1001;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_141;
using AssetRipper.SourceGenerated.Classes.ClassID_2;
using AssetRipper.SourceGenerated.Classes.ClassID_3;
using AssetRipper.SourceGenerated.Extensions;
using System.Text.RegularExpressions;

namespace AssetRipper.Processing.Scenes;

public static partial class SceneHelpers
{
	private const string AssetsName = "Assets/";
	private const string LevelName = "level";
	private const string MainSceneName = "maindata";

	public static bool TryGetFileNameToSceneIndex(string name, UnityVersion version, out int index)
	{
		if (HasMainData(version))
		{
			if (name == MainSceneName)
			{
				index = 0;
				return true;
			}

			if (SceneNameFormat.IsMatch(name))
			{
				index = int.Parse(name.AsSpan(LevelName.Length)) + 1;
				return true;
			}
		}
		else
		{
			if (SceneNameFormat.IsMatch(name))
			{
				index = int.Parse(name.AsSpan(LevelName.Length));
				return true;
			}
		}

		index = -1;
		return false;
	}

	/// <summary>
	/// Less than 5.3.0
	/// </summary>
	public static bool HasMainData(UnityVersion version) => version.LessThan(5, 3);

	/// <summary>
	/// GameObjects, Classes inheriting from LevelGameManager, MonoBehaviours with GameObjects, Components, and PrefabInstances
	/// </summary>
	public static bool IsSceneCompatible(IUnityObjectBase asset)
	{
		return asset switch
		{
			IGameObject => true,
			ILevelGameManager => true,
			IMonoBehaviour monoBeh => monoBeh.IsComponentOnGameObject(),
			IComponent => true,
			IPrefabInstance => true,
			_ => false,
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
			return $"{LevelName}{index - 1}";
		}
		return $"{LevelName}{index}";
	}

	public static bool TryGetScenePath(AssetCollection collection, [NotNullWhen(true)] IBuildSettings? buildSettings, [NotNullWhen(true)] out string? result)
	{
		if (buildSettings is not null && TryGetFileNameToSceneIndex(collection.Name, collection.OriginalVersion, out int index))
		{
			if (index >= buildSettings.Scenes.Count)
			{
				//This can happen in the following situation:
				//1. A game is built with N scenes and published to a distribution platform.
				//2. One of the scenes is removed from the project, for whatever reason.
				//3. The game is built again, with the new scene list.
				//4. When updating the game, the developer forgets to delete the Nth scene file.
				//5. Now, there are N-1 scenes in the BuildSettings, but N scene files for AssetRipper to find.
				result = null;
				return false;
			}
			string scenePath = buildSettings.Scenes[index].String;
			if (scenePath.StartsWith(AssetsName, StringComparison.Ordinal))
			{
				string extension = Path.GetExtension(scenePath);
				result = scenePath[..^extension.Length];
				return true;
			}
			else if (Path.IsPathRooted(scenePath))
			{
				// pull/uTiny 617
				// NOTE: absolute project path may contain Assets/ in its name so in this case we get incorrect scene path, but there is no way to bypass this issue
				int assetIndex = scenePath.IndexOf(AssetsName);
				string extension = Path.GetExtension(scenePath);
				result = scenePath.Substring(assetIndex, scenePath.Length - assetIndex - extension.Length);
				return true;
			}
			else if (scenePath.Length == 0)
			{
				// If a game is built without included scenes, Unity creates one with empty name.
				result = null;
				return false;
			}
			else
			{
				result = Path.Join("Assets", "Scenes", scenePath);
				return true;
			}
		}
		result = null;
		return false;
	}

	public static bool IsSceneDuplicate(int sceneIndex, IBuildSettings? buildSettings)
	{
		if (buildSettings == null)
		{
			return false;
		}

		string sceneName = buildSettings.Scenes[sceneIndex].String;
		for (int i = 0; i < buildSettings.Scenes.Count; i++)
		{
			if (buildSettings.Scenes[i] == sceneName)
			{
				if (i != sceneIndex)
				{
					return true;
				}
			}
		}
		return false;
	}

	[GeneratedRegex("^level(0|([1-9][0-9]*))$")]
	private static partial Regex SceneNameFormat { get; }
}
