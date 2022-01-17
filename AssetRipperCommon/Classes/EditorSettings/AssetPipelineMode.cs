namespace AssetRipper.Core.Classes.EditorSettings
{
	/// <summary>
	/// Selects the Assetpipeline mode to use.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/EditorSettings.bindings.cs"/>
	/// </summary>
	public enum AssetPipelineMode
	{
		/// <summary>
		/// Use this if you want to use assetpipeline version 1.
		/// </summary>
		Version1 = 0,
		/// <summary>
		/// Use this if you want to use assetpipeline version 2.
		/// </summary>
		Version2 = 1,
	}
}
