using AssetRipper.SourceGenerated.Classes.ClassID_19;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class Physics2DSettings
	{
		public static void ConvertToEditorFormat(this IPhysics2DSettings settings)
		{
			settings.AlwaysShowColliders_C19 = false;
			settings.ColliderAABBColor_C19?.SetValues(1.0f, 1.0f, 0.0f, 0.2509804f);
			settings.ColliderAsleepColor_C19?.SetValues(0.5686275f, 0.95686275f, 0.54509807f, 0.36078432f);
			settings.ColliderAwakeColor_C19?.SetValues(0.5686275f, 0.95686275f, 0.54509807f, 0.7529412f);
			settings.ColliderContactColor_C19?.SetValues(1.0f, 0.0f, 1.0f, 0.6862745f);
			settings.ContactArrowScale_C19 = 0.2f;
			settings.ShowColliderAABB_C19 = false;
			settings.ShowColliderContacts_C19 = false;
			settings.ShowColliderSleep_C19 = true;
		}
	}
}
