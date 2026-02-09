using AssetRipper.SourceGenerated.Classes.ClassID_78;

namespace AssetRipper.SourceGenerated.Extensions;

public static class TagManagerExtensions
{
	/// <summary>
	/// 5.0.0 to 5.5.0 exclusive
	/// </summary>
	public static bool IsBrokenCustomTags(UnityVersion version) => version.GreaterThanOrEquals(5) && version.LessThan(5, 5);

	/// <summary>
	/// 5.0.0 to 5.5.0 exclusive
	/// </summary>
	public static bool IsBrokenCustomTags(this ITagManager tagManager) => IsBrokenCustomTags(tagManager.Collection.Version);

	public static string TagIDToName(this ITagManager? tagManager, int tagID)
	{
		switch (tagID)
		{
			case 0:
				return TagManagerConstants.UntaggedTag;
			case 1:
				return TagManagerConstants.RespawnTag;
			case 2:
				return TagManagerConstants.FinishTag;
			case 3:
				return TagManagerConstants.EditorOnlyTag;
			//case 4:
			case 5:
				return TagManagerConstants.MainCameraTag;
			case 6:
				return TagManagerConstants.PlayerTag;
			case 7:
				return TagManagerConstants.GameControllerTag;
		}
		if (tagManager != null)
		{
			// Unity doesn't verify tagID on export?
			int tagIndex = tagID - 20000;
			if (tagIndex < tagManager.Tags.Count)
			{
				if (tagIndex >= 0)
				{
					return tagManager.Tags[tagIndex].String;
				}
				else if (!tagManager.IsBrokenCustomTags())
				{
					throw new Exception($"Unknown default tag {tagID}");
				}
			}
		}
		return $"unknown_{tagID}";
	}

	public static ushort TagNameToID(this ITagManager? tagManager, string tagName)
	{
		switch (tagName)
		{
			case TagManagerConstants.UntaggedTag:
				return 0;
			case TagManagerConstants.RespawnTag:
				return 1;
			case TagManagerConstants.FinishTag:
				return 2;
			case TagManagerConstants.EditorOnlyTag:
				return 3;
			case TagManagerConstants.MainCameraTag:
				return 5;
			case TagManagerConstants.PlayerTag:
				return 6;
			case TagManagerConstants.GameControllerTag:
				return 7;
		}
		if (tagManager != null)
		{
			for (int i = 0; i < tagManager.Tags.Count; i++)
			{
				if (tagManager.Tags[i] == tagName)
				{
					return (ushort)(20000 + i);
				}
			}
		}
		return 0;
	}
}
