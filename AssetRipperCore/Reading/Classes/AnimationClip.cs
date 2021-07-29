using AssetRipper.IO;
using AssetRipper.IO.Extensions;

namespace AssetRipper.Reading.Classes
{
	public sealed class AnimationClip : NamedObject
    {
        public AnimationType m_AnimationType;
        public bool m_Legacy;
        public bool m_Compressed;
        public bool m_UseHighQualityCurve;
        public QuaternionCurve[] m_RotationCurves;
        public CompressedAnimationCurve[] m_CompressedRotationCurves;
        public Vector3Curve[] m_EulerCurves;
        public Vector3Curve[] m_PositionCurves;
        public Vector3Curve[] m_ScaleCurves;
        public FloatCurve[] m_FloatCurves;
        public PPtrCurve[] m_PPtrCurves;
        public float m_SampleRate;
        public int m_WrapMode;
        public AABB m_Bounds;
        public uint m_MuscleClipSize;
        public ClipMuscleConstant m_MuscleClip;
        public AnimationClipBindingConstant m_ClipBindingConstant;
        public AnimationEvent[] m_Events;


        public AnimationClip(ObjectReader reader) : base(reader)
        {
            if (version[0] >= 5)//5.0 and up
            {
                m_Legacy = reader.ReadBoolean();
            }
            else if (version[0] >= 4)//4.0 and up
            {
                m_AnimationType = (AnimationType)reader.ReadInt32();
                if (m_AnimationType == AnimationType.kLegacy)
                    m_Legacy = true;
            }
            else
            {
                m_Legacy = true;
            }
            m_Compressed = reader.ReadBoolean();
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3))//4.3 and up
            {
                m_UseHighQualityCurve = reader.ReadBoolean();
            }
            reader.AlignStream();
            int numRCurves = reader.ReadInt32();
            m_RotationCurves = new QuaternionCurve[numRCurves];
            for (int i = 0; i < numRCurves; i++)
            {
                m_RotationCurves[i] = new QuaternionCurve(reader);
            }

            int numCRCurves = reader.ReadInt32();
            m_CompressedRotationCurves = new CompressedAnimationCurve[numCRCurves];
            for (int i = 0; i < numCRCurves; i++)
            {
                m_CompressedRotationCurves[i] = new CompressedAnimationCurve(reader);
            }

            if (version[0] > 5 || (version[0] == 5 && version[1] >= 3))//5.3 and up
            {
                int numEulerCurves = reader.ReadInt32();
                m_EulerCurves = new Vector3Curve[numEulerCurves];
                for (int i = 0; i < numEulerCurves; i++)
                {
                    m_EulerCurves[i] = new Vector3Curve(reader);
                }
            }

            int numPCurves = reader.ReadInt32();
            m_PositionCurves = new Vector3Curve[numPCurves];
            for (int i = 0; i < numPCurves; i++)
            {
                m_PositionCurves[i] = new Vector3Curve(reader);
            }

            int numSCurves = reader.ReadInt32();
            m_ScaleCurves = new Vector3Curve[numSCurves];
            for (int i = 0; i < numSCurves; i++)
            {
                m_ScaleCurves[i] = new Vector3Curve(reader);
            }

            int numFCurves = reader.ReadInt32();
            m_FloatCurves = new FloatCurve[numFCurves];
            for (int i = 0; i < numFCurves; i++)
            {
                m_FloatCurves[i] = new FloatCurve(reader);
            }

            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                int numPtrCurves = reader.ReadInt32();
                m_PPtrCurves = new PPtrCurve[numPtrCurves];
                for (int i = 0; i < numPtrCurves; i++)
                {
                    m_PPtrCurves[i] = new PPtrCurve(reader);
                }
            }

            m_SampleRate = reader.ReadSingle();
            m_WrapMode = reader.ReadInt32();
            if (version[0] > 3 || (version[0] == 3 && version[1] >= 4)) //3.4 and up
            {
                m_Bounds = new AABB(reader);
            }
            if (version[0] >= 4)//4.0 and up
            {
                m_MuscleClipSize = reader.ReadUInt32();
                m_MuscleClip = new ClipMuscleConstant(reader);
            }
            if (version[0] > 4 || (version[0] == 4 && version[1] >= 3)) //4.3 and up
            {
                m_ClipBindingConstant = new AnimationClipBindingConstant(reader);
            }
            if (version[0] > 2018 || (version[0] == 2018 && version[1] >= 3)) //2018.3 and up
            {
                var m_HasGenericRootTransform = reader.ReadBoolean();
                var m_HasMotionFloatCurves = reader.ReadBoolean();
                reader.AlignStream();
            }
            int numEvents = reader.ReadInt32();
            m_Events = new AnimationEvent[numEvents];
            for (int i = 0; i < numEvents; i++)
            {
                m_Events[i] = new AnimationEvent(reader);
            }
            if (version[0] >= 2017) //2017 and up
            {
                reader.AlignStream();
            }
        }
    }
}
