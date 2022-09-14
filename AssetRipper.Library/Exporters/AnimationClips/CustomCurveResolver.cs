using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Utils;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using System.Linq;
using BindingCustomType = AssetRipper.Core.Classes.AnimationClip.GenericBinding.BindingCustomType;

namespace AssetRipper.Library.Exporters.AnimationClips
{
	public sealed class CustomCurveResolver
	{
		public CustomCurveResolver(IAnimationClip clip)
		{
			if (clip == null)
			{
				throw new ArgumentNullException(nameof(clip));
			}
			m_clip = clip;
		}

		public string ToAttributeName(BindingCustomType type, uint attribute, string path)
		{
			switch (type)
			{
				case BindingCustomType.BlendShape:
					{
						const string Prefix = "blendShape.";
						if (AnimationClipConverter.UnknownPathRegex.IsMatch(path))
						{
							return Prefix + attribute;
						}

						foreach (IGameObject root in Roots)
						{
							ITransform rootTransform = root.GetTransform();
							ITransform? child = rootTransform.FindChild(path);
							if (child == null)
							{
								continue;
							}
							ISkinnedMeshRenderer? skin = child.GetGameObject().TryGetComponent<ISkinnedMeshRenderer>();
							if (skin == null)
							{
								continue;
							}
							IMesh? mesh = skin.Mesh_C137.TryGetAsset(skin.SerializedFile);
							if (mesh == null)
							{
								continue;
							}
							string? shapeName = mesh.FindBlendShapeNameByCRC(attribute);
							if (shapeName == null)
							{
								continue;
							}

							return Prefix + shapeName;
						}
						return Prefix + attribute;
					}

				case BindingCustomType.Renderer:
					return "m_Materials"
						+ "." + "Array" //from the common string
						+ "." + "data" //from the common string
						+ $"[{attribute}]";

				case BindingCustomType.RendererMaterial:
					{
						const string Prefix = "material.";
						if (AnimationClipConverter.UnknownPathRegex.IsMatch(path))
						{
							return Prefix + attribute;
						}

						foreach (IGameObject root in Roots)
						{
							ITransform rootTransform = root.GetTransform();
							ITransform? child = rootTransform.FindChild(path);
							if (child == null)
							{
								continue;
							}

							uint crc28 = attribute & 0xFFFFFFF;
							IRenderer? renderer = child.GetGameObject().TryGetComponent<IRenderer>();
							if (renderer == null)
							{
								continue;
							}
							string? property = renderer.FindMaterialPropertyNameByCRC28(crc28);
							if (property == null)
							{
								continue;
							}

							if ((attribute & 0x80000000) != 0)
							{
								return Prefix + property;
							}

							uint subPropIndex = (attribute >> 28) & 3;
							bool isRgba = (attribute & 0x40000000) != 0;
							char subProperty = subPropIndex switch
							{
								0 => isRgba ? 'r' : 'x',
								1 => isRgba ? 'g' : 'y',
								2 => isRgba ? 'b' : 'z',
								_ => isRgba ? 'a' : 'w',
							};
							return Prefix + property + "." + subProperty;
						}
						return Prefix + attribute;
					}

				case BindingCustomType.SpriteRenderer:
					{
						if (attribute == 0)
						{
							return "m_Sprite";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.MonoBehaviour:
					{
						if (attribute == CrcUtils.CalculateDigestAscii("m_Enabled"))
						{
							return "m_Enabled";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.Light:
					{
						const string ColorR = "m_Color" + "." + "r";
						if (attribute == CrcUtils.CalculateDigestAscii(ColorR))
						{
							return ColorR;
						}
						const string ColorG = "m_Color" + "." + "g";
						if (attribute == CrcUtils.CalculateDigestAscii(ColorG))
						{
							return ColorG;
						}
						const string ColorB = "m_Color" + "." + "b";
						if (attribute == CrcUtils.CalculateDigestAscii(ColorB))
						{
							return ColorB;
						}
						const string ColorA = "m_Color" + "." + "a";
						if (attribute == CrcUtils.CalculateDigestAscii(ColorA))
						{
							return ColorA;
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_CookieSize"))
						{
							return "m_CookieSize";
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_DrawHalo"))
						{
							return "m_DrawHalo";
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_Intensity"))
						{
							return "m_Intensity";
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_Range"))
						{
							return "m_Range";
						}
						const string ShadowsStrength = "m_Shadows" + "." + "m_Strength";
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsStrength))
						{
							return ShadowsStrength;
						}
						const string ShadowsBias = "m_Shadows" + "." + "m_Bias";
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsBias))
						{
							return ShadowsBias;
						}
						const string ShadowsNormalBias = "m_Shadows" + "." + "m_NormalBias";
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsNormalBias))
						{
							return ShadowsNormalBias;
						}
						const string ShadowsNearPlane = "m_Shadows" + "." + "m_NearPlane";
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsNearPlane))
						{
							return ShadowsNearPlane;
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_SpotAngle"))
						{
							return "m_SpotAngle";
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_InnerSpotAngle"))
						{
							return "m_InnerSpotAngle";
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_ColorTemperature"))
						{
							return "m_ColorTemperature";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RendererShadows:
					{
						if (attribute == CrcUtils.CalculateDigestAscii("m_ReceiveShadows"))
						{
							return "m_ReceiveShadows";
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_SortingOrder"))
						{
							return "m_SortingOrder";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.ParticleSystem:
					return "ParticleSystem_" + attribute;
				/*{
#warning TODO: ordinal propertyName
				}
				throw new ArgumentException($"Unknown attribute {attribute} for {_this}");*/

				case BindingCustomType.RectTransform:
					{
						string LocalPositionZ = "m_LocalPosition" + "." + "z";
						if (attribute == CrcUtils.CalculateDigestAscii(LocalPositionZ))
						{
							return LocalPositionZ;
						}
						string AnchoredPositionX = "m_AnchoredPosition" + "." + "x";
						if (attribute == CrcUtils.CalculateDigestAscii(AnchoredPositionX))
						{
							return AnchoredPositionX;
						}
						string AnchoredPositionY = "m_AnchoredPosition" + "." + "y";
						if (attribute == CrcUtils.CalculateDigestAscii(AnchoredPositionY))
						{
							return AnchoredPositionY;
						}
						string AnchorMinX = "m_AnchorMin" + "." + "x";
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMinX))
						{
							return AnchorMinX;
						}
						string AnchorMinY = "m_AnchorMin" + "." + "y";
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMinY))
						{
							return AnchorMinY;
						}
						string AnchorMaxX = "m_AnchorMax" + "." + "x";
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMaxX))
						{
							return AnchorMaxX;
						}
						string AnchorMaxY = "m_AnchorMax" + "." + "y";
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMaxY))
						{
							return AnchorMaxY;
						}
						string SizeDeltaX = "m_SizeDelta" + "." + "x";
						if (attribute == CrcUtils.CalculateDigestAscii(SizeDeltaX))
						{
							return SizeDeltaX;
						}
						string SizeDeltaY = "m_SizeDelta" + "." + "y";
						if (attribute == CrcUtils.CalculateDigestAscii(SizeDeltaY))
						{
							return SizeDeltaY;
						}
						string PivotX = "m_Pivot" + "." + "x";
						if (attribute == CrcUtils.CalculateDigestAscii(PivotX))
						{
							return PivotX;
						}
						string PivotY = "m_Pivot" + "." + "y";
						if (attribute == CrcUtils.CalculateDigestAscii(PivotY))
						{
							return PivotY;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.LineRenderer:
					{
						const string ParametersWidthMultiplier = "m_Parameters" + "." + "widthMultiplier";
						if (attribute == CrcUtils.CalculateDigestAscii(ParametersWidthMultiplier))
						{
							return ParametersWidthMultiplier;
						}
					}
#warning TODO: old versions animate all properties as custom curves
					return "LineRenderer_" + attribute;

#warning TODO:
				case BindingCustomType.TrailRenderer:
					{
						const string ParametersWidthMultiplier = "m_Parameters" + "." + "widthMultiplier";
						if (attribute == CrcUtils.CalculateDigestAscii(ParametersWidthMultiplier))
						{
							return ParametersWidthMultiplier;
						}
					}
#warning TODO: old versions animate all properties as custom curves
					return "TrailRenderer_" + attribute;

#warning TODO:
				case BindingCustomType.PositionConstraint:
					{
						uint property = attribute & 0xF;
						switch (property)
						{
							case 0:
								return "m_RestTranslation.x";
							case 1:
								return "m_RestTranslation.y";
							case 2:
								return "m_RestTranslation.z";
							case 3:
								return "m_Weight";
							case 4:
								return "m_TranslationOffset.x";
							case 5:
								return "m_TranslationOffset.y";
							case 6:
								return "m_TranslationOffset.z";
							case 7:
								return "m_AffectTranslationX";
							case 8:
								return "m_AffectTranslationY";
							case 9:
								return "m_AffectTranslationZ";
							case 10:
								return "m_Active";
							case 11:
								return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
							case 12:
								return $"m_Sources.Array.data[{attribute >> 8}].weight";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.RotationConstraint:
					{
						uint property = attribute & 0xF;
						switch (property)
						{
							case 0:
								return "m_RestRotation.x";
							case 1:
								return "m_RestRotation.y";
							case 2:
								return "m_RestRotation.z";
							case 3:
								return "m_Weight";
							case 4:
								return "m_RotationOffset.x";
							case 5:
								return "m_RotationOffset.y";
							case 6:
								return "m_RotationOffset.z";
							case 7:
								return "m_AffectRotationX";
							case 8:
								return "m_AffectRotationY";
							case 9:
								return "m_AffectRotationZ";
							case 10:
								return "m_Active";
							case 11:
								return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
							case 12:
								return $"m_Sources.Array.data[{attribute >> 8}].weight";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.ScaleConstraint:
					{
						uint property = attribute & 0xF;
						switch (property)
						{
							case 0:
								return "m_ScaleAtRest.x";
							case 1:
								return "m_ScaleAtRest.y";
							case 2:
								return "m_ScaleAtRest.z";
							case 3:
								return "m_Weight";
							case 4:
								return "m_ScalingOffset.x";
							case 5:
								return "m_ScalingOffset.y";
							case 6:
								return "m_ScalingOffset.z";
							case 7:
								return "m_AffectScalingX";
							case 8:
								return "m_AffectScalingY";
							case 9:
								return "m_AffectScalingZ";
							case 10:
								return "m_Active";
							case 11:
								return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
							case 12:
								return $"m_Sources.Array.data[{attribute >> 8}].weight";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.AimConstraint:
					{
						uint property = attribute & 0xF;
						switch (property)
						{
							case 0:
								return "m_Weight";
							case 1:
								return "m_AffectRotationX";
							case 2:
								return "m_AffectRotationY";
							case 3:
								return "m_AffectRotationZ";
							case 4:
								return "m_Active";
							case 5:
								return "m_WorldUpObject";
							case 6:
								return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
							case 7:
								return $"m_Sources.Array.data[{attribute >> 8}].weight";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.ParentConstraint:
					{
						uint property = attribute & 0xF;
						switch (property)
						{
							case 0:
								return "m_Weight";
							case 1:
								return "m_AffectTranslationX";
							case 2:
								return "m_AffectTranslationY";
							case 3:
								return "m_AffectTranslationZ";
							case 4:
								return "m_AffectRotationX";
							case 5:
								return "m_AffectRotationY";
							case 6:
								return "m_AffectRotationZ";
							case 7:
								return "m_Active";
							case 8:
								return $"m_TranslationOffsets.Array.data[{attribute >> 8}].x";
							case 9:
								return $"m_TranslationOffsets.Array.data[{attribute >> 8}].y";
							case 10:
								return $"m_TranslationOffsets.Array.data[{attribute >> 8}].z";
							case 11:
								return $"m_RotationOffsets.Array.data[{attribute >> 8}].x";
							case 12:
								return $"m_RotationOffsets.Array.data[{attribute >> 8}].y";
							case 13:
								return $"m_RotationOffsets.Array.data[{attribute >> 8}].z";
							case 14:
								return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
							case 15:
								return $"m_Sources.Array.data[{attribute >> 8}].weight";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.LookAtConstraint:
					{
						uint property = attribute & 0xF;
						switch (property)
						{
							case 0:
								return "m_Weight";
							case 1:
								return "m_Active";
							case 2:
								return "m_WorldUpObject";
							case 3:
								return $"m_Sources.Array.data[{attribute >> 8}].sourceTransform";
							case 4:
								return $"m_Sources.Array.data[{attribute >> 8}].weight";
							case 5:
								return "m_Roll";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.Camera:
					{
						if (attribute == CrcUtils.CalculateDigestAscii("field of view"))
						{
							return "field of view";
						}
						if (attribute == CrcUtils.CalculateDigestAscii("m_FocalLength"))
						{
							return "m_FocalLength";
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.VisualEffect:
					return "VisualEffect_" + attribute;

				case BindingCustomType.ParticleForceField:
					return "ParticleForceField_" + attribute;

				case BindingCustomType.UserDefined:
					return "UserDefined_" + attribute;

				case BindingCustomType.MeshFilter:
					return "MeshFilter_" + attribute;

				default:
					throw new ArgumentException($"Binding type {type} not implemented", nameof(type));
			}
		}

		private IGameObject[] Roots
		{
			get
			{
				if (!m_rootInited)
				{
					m_roots = m_clip.FindRoots().ToArray();
					m_rootInited = true;
				}
				return m_roots;
			}
		}

		private readonly IAnimationClip m_clip;
		private IGameObject[] m_roots = Array.Empty<IGameObject>();
		private bool m_rootInited = false;
	}
}
