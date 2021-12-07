using AssetRipper.Core.Classes.Misc;

namespace AssetRipper.Core.Classes.EditorBuildSettings
{
	/// <summary>
	/// 2.5.0 and greater (NOTE: unknown version)
	/// </summary>
	public interface IScene
	{
		public bool Enabled { get; set; }
		public string Path { get; set; }
		public UnityGUID GUID { get; set; }
	}
}
