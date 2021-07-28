using System;

namespace AssetRipper.Classes.EditorSettings
{
	[Flags]
	public enum EnterPlayModeOptions
	{
        None                = 0,
        DisableDomainReload = 1,
        DisableSceneReload  = 2,
	}
}
