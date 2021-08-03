using AssetRipper.Core.IO;

namespace AssetRipper.Core.Reading.Classes
{
	public sealed class AnimatorOverrideController : RuntimeAnimatorController
    {
        public PPtr<RuntimeAnimatorController> m_Controller;
        public AnimationClipOverride[] m_Clips;

        public AnimatorOverrideController(ObjectReader reader) : base(reader)
        {
            m_Controller = new PPtr<RuntimeAnimatorController>(reader);

            int numOverrides = reader.ReadInt32();
            m_Clips = new AnimationClipOverride[numOverrides];
            for (int i = 0; i < numOverrides; i++)
            {
                m_Clips[i] = new AnimationClipOverride(reader);
            }
        }
    }
}
