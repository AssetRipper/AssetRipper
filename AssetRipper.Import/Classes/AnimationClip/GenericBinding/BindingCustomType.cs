namespace AssetRipper.Core.Classes.AnimationClip.GenericBinding
{
	public enum BindingCustomType : byte
	{
		None = 0,
		Transform = 4,
		AnimatorMuscle = 8,

		BlendShape = 20,
		Renderer = 21,
		RendererMaterial = 22,
		SpriteRenderer = 23,
		MonoBehaviour = 24,
		Light = 25,
		RendererShadows = 26,
		ParticleSystem = 27,
		RectTransform = 28,
		LineRenderer = 29,
		TrailRenderer = 30,
		PositionConstraint = 31,
		RotationConstraint = 32,
		ScaleConstraint = 33,
		AimConstraint = 34,
		ParentConstraint = 35,
		LookAtConstraint = 36,
		Camera = 37,
		VisualEffect = 38,
		ParticleForceField = 39,
		UserDefined = 40,
		MeshFilter = 41,
	}
}
/*BindType
kUnbound 0x0
kBindTransformPosition 0x1
kBindTransformRotation 0x2
kBindTransformScale 0x3
kBindTransformEuler 0x4
kMinSinglePropertyBinding 0x5
kBindFloat 0x5
kBindFloatToBool 0x6
kBindGameObjectActive 0x7
kBindMuscle 0x8
kBindScriptObjectReference 0x9
kBindFloatToInt 0xa
kBindDiscreteInt 0xb
kBlendShapeWeightBinding 0x14
kRendererMaterialPPtrBinding 0x15
kRendererMaterialPropertyBinding 0x16
kSpriteRendererPPtrBinding 0x17
kMonoBehaviourPropertyBinding 0x18
kLightPropertyBinding 0x19
kRendererOtherPropertyBinding 0x1a
kParticleSystemPropertyBindings 0x1b
kRectTransformPropertyBindings 0x1c
kLineRendererPropertyBindings 0x1d
kTrailRendererPropertyBindings 0x1e
kPositionConstraintPropertyBindings 0x1f
kRotationConstraintPropertyBindings 0x20
kScaleConstraintPropertyBindings 0x21
kAimConstraintPropertyBindings 0x22
kParentConstraintPropertyBindings 0x23
kLookAtConstraintPropertyBindings 0x24
kCameraPropertyBindings 0x25
kVisualEffectPropertyBindings 0x26
kParticleForceFieldPropertyBinding 0x27
kUserDefinedBinding 0x28
kMeshFilterBinding 0x29
kAllBindingCount 0x2a    */
