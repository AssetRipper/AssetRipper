using AssetRipper.Core.Classes.Misc;

namespace AssetRipper.Core.Classes.EditorBuildSettings
{
	/// <summary>
	/// 2.5.0 and greater (NOTE: unknown version)<br/>
	/// Actually called Scene, but name conflicts with a different class in older unity versions
	/// </summary>
	public interface IEditorScene
	{
		public bool Enabled { get; set; }
		public string Path { get; set; }
		public UnityGUID GUID { get; set; }
	}
}
