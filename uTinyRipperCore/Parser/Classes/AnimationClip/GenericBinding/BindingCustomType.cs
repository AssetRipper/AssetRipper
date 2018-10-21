using SevenZip;
using System;
using uTinyRipper.Classes.Lights;

namespace uTinyRipper.Classes.AnimationClips
{
	public enum BindingCustomType : byte
	{
		None				= 0,
		Transform			= 4,
		AnimatorMuscle		= 8,
		BlendShape			= 20,
		Renderer			= 21,
		RendererMaterial	= 22,
		SpriteRenderer		= 23,
		MonoBehaviour		= 24,
		Light				= 25,
		RendererShadows		= 26,
		ParticleSystem		= 27,
		RectTransform		= 28,
		LineRenderer		= 29,
		TrailRenderer		= 30,
#warning TODO:
	}

	public static class BindingCustomTypeExtensions
	{
		public static string ToAttributeName(this BindingCustomType _this, uint attribute)
		{
			switch (_this)
			{
				case BindingCustomType.BlendShape:
					return "blendShape." + attribute.ToString()/* + gameObject.GetComponent<SkinnedMeshRenderer>().Mesh.Shapes.GetByCrc(attribute)*/;

				case BindingCustomType.Renderer:
					{
						string properyName = Renderer.MaterialsName + "." + nameof(TreeNodeType.Array) + "." + nameof(TreeNodeType.data) + $"[{attribute}]";
						if(attribute == CRC.CalculateDigestAscii(properyName))
						{
							return properyName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {_this}");

				case BindingCustomType.RendererMaterial:
					return "material." + attribute /*some name*/;

				case BindingCustomType.SpriteRenderer:
					{
						if (attribute == 0)
						{
							return SpriteRenderer.SpriteName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {_this}");

				case BindingCustomType.MonoBehaviour:
					{
						if (attribute == CRC.CalculateDigestAscii(Behaviour.EnabledName))
						{
							return Behaviour.EnabledName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {_this}");

				case BindingCustomType.Light:
					{
						const string ColorR = Light.ColorName + "." + ColorRGBAf.RName;
						if (attribute == CRC.CalculateDigestAscii(ColorR))
						{
							return ColorR;
						}
						const string ColorG = Light.ColorName + "." + ColorRGBAf.GName;
						if (attribute == CRC.CalculateDigestAscii(ColorG))
						{
							return ColorG;
						}
						const string ColorB = Light.ColorName + "." + ColorRGBAf.BName;
						if (attribute == CRC.CalculateDigestAscii(ColorB))
						{
							return ColorB;
						}
						const string ColorA = Light.ColorName + "." + ColorRGBAf.AName;
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
					throw new ArgumentException($"Unknown attribute {attribute} for {_this}");

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
					throw new ArgumentException($"Unknown attribute {attribute} for {_this}");

				case BindingCustomType.ParticleSystem:
					return "ParticleSystem_" + attribute;
				/*{
					// TODO: ordinal propertyName
				}
				throw new ArgumentException($"Unknown attribute {attribute} for {_this}");*/

				case BindingCustomType.RectTransform:
					{
						const string LocalPositionZ = Transform.LocalPositionName + "." + Vector3f.ZName;
						if (attribute == CRC.CalculateDigestAscii(LocalPositionZ))
						{
							return LocalPositionZ;
						}
						const string AnchoredPositionX = RectTransform.AnchoredPositionName + "." + Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(AnchoredPositionX))
						{
							return AnchoredPositionX;
						}
						const string AnchoredPositionY = RectTransform.AnchoredPositionName + "." + Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(AnchoredPositionY))
						{
							return AnchoredPositionY;
						}
						const string AnchorMinX = RectTransform.AnchorMinName + "." + Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMinX))
						{
							return AnchorMinX;
						}
						const string AnchorMinY = RectTransform.AnchorMinName + "." + Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMinY))
						{
							return AnchorMinY;
						}
						const string AnchorMaxX = RectTransform.AnchorMaxName + "." + Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMaxX))
						{
							return AnchorMaxX;
						}
						const string AnchorMaxY = RectTransform.AnchorMaxName + "." + Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(AnchorMaxY))
						{
							return AnchorMaxY;
						}
						const string SizeDeltaX = RectTransform.SizeDeltaName + "." + Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(SizeDeltaX))
						{
							return SizeDeltaX;
						}
						const string SizeDeltaY = RectTransform.SizeDeltaName + "." + Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(SizeDeltaY))
						{
							return SizeDeltaY;
						}
						const string PivotX = RectTransform.PivotName + "." + Vector2f.XName;
						if (attribute == CRC.CalculateDigestAscii(PivotX))
						{
							return PivotX;
						}
						const string PivotY = RectTransform.PivotName + "." + Vector2f.YName;
						if (attribute == CRC.CalculateDigestAscii(PivotY))
						{
							return PivotY;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {_this}");



				case BindingCustomType.LineRenderer:
#warning TODO:
					return "LineRenderer_" + attribute;
				/*{
					const string ParametersWidthMultiplier = LineRenderer.ParametersName + "." + LineRendererParameters.WidthMultiplier;
					if (attribute == CRC.CalculateDigestAscii(ParametersWidthMultiplier))
					{
						return ParametersWidthMultiplier;
					}
				}
				throw new ArgumentException($"Unknown attribute {attribute} for {_this}");*/

				case BindingCustomType.TrailRenderer:
#warning TODO:
					return "TrailRenderer_" + attribute;
				/*{
					const string ParametersWidthMultiplier = TrailRenderer.ParametersName + "." + TrailRendererParameters.WidthMultiplier;
					if (attribute == CRC.CalculateDigestAscii(ParametersWidthMultiplier))
					{
						return ParametersWidthMultiplier;
					}
				}
				throw new ArgumentException($"Unknown attribute {attribute} for {_this}");*/

#warning TODO:
				default:
					return "unsupported_" + attribute;
					//throw new ArgumentException(_this.ToString());
			}
		}
	}
}
