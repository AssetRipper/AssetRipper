namespace AssetRipper.Core.Classes.Avatar
{
	/// <summary>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Animation/ScriptBindings/Avatar.bindings.cs"/>
	/// </summary>
	public enum BodyDoF
	{
		SpineFrontBack = 0,
		SpineLeftRight = 1,
		SpineRollLeftRight = 2,
#warning TODO: check since reference has this as 3 not 4
		ChestFrontBack = 4,
		ChestLeftRight = 5,
		ChestRollLeftRight = 6,
		UpperChestFrontBack = 7,
		UpperChestLeftRight = 8,
		UpperChestRollLeftRight = 9,
		LastBodyDoF = 10,
	}
}
