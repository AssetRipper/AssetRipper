using AssetRipper.SourceGenerated.Classes.ClassID_19;

namespace AssetRipper.SourceGenerated.Extensions;

public static class Physics2DSettingsExtensions
{
	public static void ConvertToEditorFormat(this IPhysics2DSettings settings)
	{
		settings.AlwaysShowColliders = false;
		settings.ColliderAABBColor?.SetValues(1.0f, 1.0f, 0.0f, 0.2509804f);
		settings.ColliderAsleepColor?.SetValues(0.5686275f, 0.95686275f, 0.54509807f, 0.36078432f);
		settings.ColliderAwakeColor?.SetValues(0.5686275f, 0.95686275f, 0.54509807f, 0.7529412f);
		settings.ColliderContactColor?.SetValues(1.0f, 0.0f, 1.0f, 0.6862745f);
		settings.ContactArrowScale = 0.2f;
		settings.ShowColliderAABB = false;
		settings.ShowColliderContacts = false;
		settings.ShowColliderSleep = true;
	}
}
