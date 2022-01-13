using System;

namespace AssetRipper.Core.Classes.EditorSettings
{
	[Flags]
	public enum EnterPlayModeOptions
	{
		None = 0,
		DisableDomainReload = 1,
		DisableSceneReload = 2,
	}
}
