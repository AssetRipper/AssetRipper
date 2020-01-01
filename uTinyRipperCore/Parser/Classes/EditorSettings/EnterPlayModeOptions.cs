using System;

namespace uTinyRipper.Classes.EditorSettingss
{
	[Flags]
	public enum EnterPlayModeOptions
	{
        None                = 0,
        DisableDomainReload = 1,
        DisableSceneReload  = 2,
	}
}
