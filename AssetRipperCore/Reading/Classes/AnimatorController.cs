using AssetRipper.IO.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Reading.Classes
{
	public sealed class AnimatorController : RuntimeAnimatorController
    {
        public PPtr<AnimationClip>[] m_AnimationClips;

        public AnimatorController(ObjectReader reader) : base(reader)
        {
            var m_ControllerSize = reader.ReadUInt32();
            var m_Controller = new ControllerConstant(reader);

            int tosSize = reader.ReadInt32();
            var m_TOS = new KeyValuePair<uint, string>[tosSize];
            for (int i = 0; i < tosSize; i++)
            {
                m_TOS[i] = new KeyValuePair<uint, string>(reader.ReadUInt32(), reader.ReadAlignedString());
            }

            int numClips = reader.ReadInt32();
            m_AnimationClips = new PPtr<AnimationClip>[numClips];
            for (int i = 0; i < numClips; i++)
            {
                m_AnimationClips[i] = new PPtr<AnimationClip>(reader);
            }
        }
    }
}
