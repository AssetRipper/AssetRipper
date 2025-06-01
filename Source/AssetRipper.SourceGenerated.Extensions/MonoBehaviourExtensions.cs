using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;

namespace AssetRipper.SourceGenerated.Extensions;

public static class MonoBehaviourExtensions
{
	/// <summary>
	/// Does this MonoBehaviour belong to scene/prefab hierarchy? In other words, is <see cref="IMonoBehaviour.GameObject"/> a non-null pptr?
	/// </summary>
	public static bool IsComponentOnGameObject(this IMonoBehaviour monoBehaviour) => !monoBehaviour.GameObject.IsNull();

	public static bool TryGetScript(this IMonoBehaviour monoBehaviour, [NotNullWhen(true)] out IMonoScript? script)
	{
		script = monoBehaviour.ScriptP;
		return script is not null;
	}

	public static bool IsGuiSkin(this IMonoBehaviour monoBehaviour)
	{
		if (TryGetScript(monoBehaviour, out IMonoScript? script))
		{
			if (script.PathID is 12001 && script.Collection.Name == "unity default resources")
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsBrush(this IMonoBehaviour monoBehaviour)
	{
		if (TryGetScript(monoBehaviour, out IMonoScript? script))
		{
			if (script.PathID is 12146 && script.Collection.Name == "unity default resources")
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsTimelineAsset(this IMonoBehaviour monoBehaviour)
	{
		return monoBehaviour.IsType("UnityEngine.Timeline", "TimelineAsset");
	}

	public static bool IsPostProcessProfile(this IMonoBehaviour monoBehaviour)
	{
		return monoBehaviour.IsType("UnityEngine.Rendering.PostProcessing", "PostProcessProfile");
	}

	private static bool IsType(this IMonoBehaviour monoBehaviour, string @namespace, string name)
	{
		return TryGetScript(monoBehaviour, out IMonoScript? script) && script.IsType(@namespace, name);
	}
}
