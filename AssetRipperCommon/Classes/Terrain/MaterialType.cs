namespace AssetRipper.Core.Classes.Terrain
{
	/// <summary>
	/// The type of the material used to render a terrain object. Could be one of the built-in types or custom.<br/>
	/// <see href="https://github.com/Unity-Technologies/UnityCsReference/blob/master/Modules/Terrain/Public/Terrain.deprecated.cs"/>
	/// </summary>
	public enum MaterialType
	{
		/// <summary>
		/// A built-in material that uses the standard physically-based lighting model. Inputs supported: smoothness, metallic / specular, normal.
		/// </summary>
		BuiltInStandard = 0,
		/// <summary>
		/// A built-in material that uses the legacy Lambert (diffuse) lighting model and has optional normal map support.
		/// </summary>
		BuiltInLegacyDiffuse = 1,
		/// <summary>
		/// A built-in material that uses the legacy BlinnPhong (specular) lighting model and has optional normal map support.
		/// </summary>
		BuiltInLegacySpecular = 2,
		/// <summary>
		/// Use a custom material given by Terrain.materialTemplate.
		/// </summary>
		Custom = 3,
	}
}
