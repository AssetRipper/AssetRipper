using SevenZip;
using System;
using System.Linq;
using uTinyRipper.Classes;
using uTinyRipper.Classes.AnimationClips;
using uTinyRipper.Classes.Lights;
using uTinyRipper.Layout;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Converters.AnimationClips
{
	public sealed class CustomCurveResolver
	{
		public CustomCurveResolver(AnimationClip clip)
		{
			if (clip == null)
			{
				throw new ArgumentNullException(nameof(clip));
			}
			m_clip = clip;
		}

		public string ToAttributeName(AssetLayout layout, BindingCustomType type, uint attribute, string path)
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

						foreach (GameObject root in Roots)
						{
							Transform rootTransform = root.GetTransform();
							Transform child = rootTransform.FindChild(path);
							if (child == null)
							{
								continue;
							}
							SkinnedMeshRenderer skin = child.GameObject.FindAsset(child.File).FindComponent<SkinnedMeshRenderer>();
							if (skin == null)
							{
								continue;
							}
							Mesh mesh = skin.Mesh.FindAsset(skin.File);
							if (mesh == null)
							{
								continue;
							}
							string shapeName = mesh.FindBlendShapeNameByCRC(attribute);
							if (shapeName == null)
							{
								continue;
							}

							return Prefix + shapeName;
						}
						return Prefix + attribute;
					}

				case BindingCustomType.Renderer:
					return Renderer.MaterialsName + "." + nameof(TreeNodeType.Array) + "." + nameof(TreeNodeType.data) + $"[{attribute}]";

				case BindingCustomType.RendererMaterial:
					{
						const string Prefix = "material.";
						if (AnimationClipConverter.UnknownPathRegex.IsMatch(path))
						{
							return Prefix + attribute;
						}

						foreach (GameObject root in Roots)
						{
							Transform rootTransform = root.GetTransform();
							Transform child = rootTransform.FindChild(path);
							if (child == null)
							{
								continue;
							}

							uint crc28 = attribute & 0xFFFFFFF;
							Renderer renderer = child.GameObject.FindAsset(child.File).FindComponent<Renderer>();
							if (renderer == null)
							{
								continue;
							}
							string property = renderer.FindMaterialPropertyNameByCRC28(crc28);
							if (property == null)
							{
								continue;
							}

							if ((attribute & 0x80000000) != 0)
							{
								return Prefix + property;
							}
							char subProperty;
							uint subPropIndex = attribute >> 28 & 3;
							bool isRgba = (attribute & 0x40000000) != 0;
							switch (subPropIndex)
							{
								case 0:
									subProperty = isRgba ? 'r' : 'x';
									break;
								case 1:
									subProperty = isRgba ? 'g' : 'y';
									break;
								case 2:
									subProperty = isRgba ? 'b' : 'z';
									break;

								default:
									subProperty = isRgba ? 'a' : 'w';
									break;
							}
							return Prefix + property + "." + subProperty;
						}
						return Prefix + attribute;
					}

				case BindingCustomType.SpriteRenderer:
					{
						if (attribute == 0)
						{
							return SpriteRenderer.SpriteName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.MonoBehaviour:
					{
						if (attribute == CRC.CalculateDigestAscii(layout.Behaviour.EnabledName))
						{
							return layout.Behaviour.EnabledName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.Light:
					{
						string ColorR = Light.ColorName + "." + layout.Serialized.ColorRGBAf.RName;
						if (attribute == CRC.CalculateDigestAscii(ColorR))
						{
							return ColorR;
						}
						string ColorG = Light.ColorName + "." + layout.Serialized.ColorRGBAf.GName;
						if (attribute == CRC.CalculateDigestAscii(ColorG))
						{
							return ColorG;
						}
						string ColorB = Light.ColorName + "." + layout.Serialized.ColorRGBAf.BName;
						if (attribute == CRC.CalculateDigestAscii(ColorB))
						{
							return ColorB;
						}
						string ColorA = Light.ColorName + "." + layout.Serialized.ColorRGBAf.AName;
						if (attribute == CRC.CalculateDigestAscii(ColorA))
						{
							return ColorA;
						}
						if (attribute == CRC.CalculateDigestAscii(Light.CookieSizeName))
						{
							return Light.CookieSizeName;
						}
						if (attribute == CRC.CalculateDigestAscii(Light.DrawHaloName))
						{
							return Light.DrawHaloName;
						}
						if (attribute == CRC.CalculateDigestAscii(Light.IntensityName))
						{
							return Light.IntensityName;
						}
						if (attribute == CRC.CalculateDigestAscii(Light.RangeName))
						{
							return Light.RangeName;
						}
						const string ShadowsStrength = Light.ShadowsName + "." + ShadowSettings.StrengthName;
						if (attribute == CRC.CalculateDigestAscii(ShadowsStrength))
						{
							return ShadowsStrength;
						}
						const string ShadowsBias = Light.ShadowsName + "." + ShadowSettings.BiasName;
						if (attribute == CRC.CalculateDigestAscii(ShadowsBias))
						{
							return ShadowsBias;
						}
						const string ShadowsNormalBias = Light.ShadowsName + "." + ShadowSettings.NormalBiasName;
						if (attribute == CRC.CalculateDigestAscii(ShadowsNormalBias))
						{
							return ShadowsNormalBias;
						}
						const string ShadowsNearPlane = Light.ShadowsName + "." + ShadowSettings.NearPlaneName;
						if (attribute == CRC.CalculateDigestAscii(ShadowsNearPlane))
						{
							return ShadowsNearPlane;
						}
						if (attribute == CRC.CalculateDigestAscii(Light.SpotAngleName))
						{
							return Light.SpotAngleName;
						}
						if (attribute == CRC.CalculateDigestAscii(Light.ColorTemperatureName))
						{
							return Light.ColorTemperatureName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RendererShadows:
					{
						if (attribute == CRC.CalculateDigestAscii(Renderer.ReceiveShadowsName))
						{
							return Renderer.ReceiveShadowsName;
						}
						if (attribute == CRC.CalculateDigestAscii(Renderer.SortingOrderName))
						{
							return Renderer.SortingOrderName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.ParticleSystem:
					return "ParticleSystem_" + attribute;
				/*{
					// TODO: ordinal propertyName
				}
				throw new ArgumentException($"Unknown attribute {attribute} for {_this}");*/

				case BindingCustomType.RectTransform:
					{
						string LocalPositionZ = layout.Transform.LocalPositionName + "." + layout.Serialized.Vector3f.ZName;
						if (attribute == CRC.CalculateDigestAscii(LocalPositionZ))
						{
							return LocalPositionZ;
						}
						string AnchoredPositionX = RectTransform.AnchoredPositionName + "." + layout.Serialized.Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(AnchoredPositionX))
						{
							return AnchoredPositionX;
						}
						string AnchoredPositionY = RectTransform.AnchoredPositionName + "." + layout.Serialized.Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(AnchoredPositionY))
						{
							return AnchoredPositionY;
						}
						string AnchorMinX = RectTransform.AnchorMinName + "." + layout.Serialized.Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMinX))
						{
							return AnchorMinX;
						}
						string AnchorMinY = RectTransform.AnchorMinName + "." + layout.Serialized.Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMinY))
						{
							return AnchorMinY;
						}
						string AnchorMaxX = RectTransform.AnchorMaxName + "." + layout.Serialized.Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMaxX))
						{
							return AnchorMaxX;
						}
						string AnchorMaxY = RectTransform.AnchorMaxName + "." + layout.Serialized.Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMaxY))
						{
							return AnchorMaxY;
						}
						string SizeDeltaX = RectTransform.SizeDeltaName + "." + layout.Serialized.Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(SizeDeltaX))
						{
							return SizeDeltaX;
						}
						string SizeDeltaY = RectTransform.SizeDeltaName + "." + layout.Serialized.Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(SizeDeltaY))
						{
							return SizeDeltaY;
						}
						string PivotX = RectTransform.PivotName + "." + layout.Serialized.Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(PivotX))
						{
							return PivotX;
						}
						string PivotY = RectTransform.PivotName + "." + layout.Serialized.Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(PivotY))
						{
							return PivotY;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO:
				case BindingCustomType.LineRenderer:
					{
						const string ParametersWidthMultiplier = "m_Parameters" + "." + "widthMultiplier";
						if (attribute == CRC.CalculateDigestAscii(ParametersWidthMultiplier))
						{
							return ParametersWidthMultiplier;
						}
					}
					// TODO: old versions animate all properties as custom curves
					return "LineRenderer_" + attribute;

#warning TODO:
				case BindingCustomType.TrailRenderer:
					{
						const string ParametersWidthMultiplier = "m_Parameters" + "." + "widthMultiplier";
						if (attribute == CRC.CalculateDigestAscii(ParametersWidthMultiplier))
						{
							return ParametersWidthMultiplier;
						}
					}
					// TODO: old versions animate all properties as custom curves
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
						if (attribute == CRC.CalculateDigestAscii(Camera.FieldOfViewName))
						{
							return Camera.FieldOfViewName;
						}
						if (attribute == CRC.CalculateDigestAscii(Camera.FocalLengthName))
						{
							return Camera.FocalLengthName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				default:
					throw new ArgumentException(type.ToString());
			}
		}

		private GameObject[] Roots
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

		private Version Version => m_clip.File.Version;

		private readonly AnimationClip m_clip = null;

		private GameObject[] m_roots = null;
		private bool m_rootInited = false;
	}
}
