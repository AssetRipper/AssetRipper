using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.AnimationClip.GenericBinding;
using AssetRipper.Core.Classes.Camera;
using AssetRipper.Core.Classes.GameObject;
using AssetRipper.Core.Classes.Light;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Renderer;
using AssetRipper.Core.Classes.SpriteRenderer;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.Utils;
using System;
using System.Linq;
using UnityVersion = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Core.Converters.AnimationClip
{
	public sealed class CustomCurveResolver
	{
		public CustomCurveResolver(AssetRipper.Core.Classes.AnimationClip.AnimationClip clip)
		{
			if (clip == null)
			{
				throw new ArgumentNullException(nameof(clip));
			}
			m_clip = clip;
		}

		public string ToAttributeName(LayoutInfo layout, BindingCustomType type, uint attribute, string path)
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

						foreach (AssetRipper.Core.Classes.GameObject.GameObject root in Roots)
						{
							ITransform rootTransform = root.GetTransform();
							ITransform child = rootTransform.FindChild(path);
							if (child == null)
							{
								continue;
							}
							SkinnedMeshRenderer skin = child.GetGameObject().FindComponent<SkinnedMeshRenderer>();
							if (skin == null)
							{
								continue;
							}
							AssetRipper.Core.Classes.Mesh.Mesh mesh = skin.Mesh.FindAsset(skin.SerializedFile);
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

						foreach (AssetRipper.Core.Classes.GameObject.GameObject root in Roots)
						{
							ITransform rootTransform = root.GetTransform();
							ITransform child = rootTransform.FindChild(path);
							if (child == null)
							{
								continue;
							}

							uint crc28 = attribute & 0xFFFFFFF;
							Renderer renderer = child.GetGameObject().FindComponent<Renderer>();
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

							uint subPropIndex = attribute >> 28 & 3;
							bool isRgba = (attribute & 0x40000000) != 0;
							var subProperty = subPropIndex switch
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
							return SpriteRenderer.SpriteName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.MonoBehaviour:
					{
						if (attribute == CrcUtils.CalculateDigestAscii(Behaviour.EnabledName))
						{
							return Behaviour.EnabledName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.Light:
					{
						const string ColorR = Light.ColorName + "." + ColorRGBAf.RName;
						if (attribute == CrcUtils.CalculateDigestAscii(ColorR))
						{
							return ColorR;
						}
						const string ColorG = Light.ColorName + "." + ColorRGBAf.GName;
						if (attribute == CrcUtils.CalculateDigestAscii(ColorG))
						{
							return ColorG;
						}
						const string ColorB = Light.ColorName + "." + ColorRGBAf.BName;
						if (attribute == CrcUtils.CalculateDigestAscii(ColorB))
						{
							return ColorB;
						}
						const string ColorA = Light.ColorName + "." + ColorRGBAf.AName;
						if (attribute == CrcUtils.CalculateDigestAscii(ColorA))
						{
							return ColorA;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Light.CookieSizeName))
						{
							return Light.CookieSizeName;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Light.DrawHaloName))
						{
							return Light.DrawHaloName;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Light.IntensityName))
						{
							return Light.IntensityName;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Light.RangeName))
						{
							return Light.RangeName;
						}
						const string ShadowsStrength = Light.ShadowsName + "." + ShadowSettings.StrengthName;
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsStrength))
						{
							return ShadowsStrength;
						}
						const string ShadowsBias = Light.ShadowsName + "." + ShadowSettings.BiasName;
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsBias))
						{
							return ShadowsBias;
						}
						const string ShadowsNormalBias = Light.ShadowsName + "." + ShadowSettings.NormalBiasName;
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsNormalBias))
						{
							return ShadowsNormalBias;
						}
						const string ShadowsNearPlane = Light.ShadowsName + "." + ShadowSettings.NearPlaneName;
						if (attribute == CrcUtils.CalculateDigestAscii(ShadowsNearPlane))
						{
							return ShadowsNearPlane;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Light.SpotAngleName))
						{
							return Light.SpotAngleName;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Light.ColorTemperatureName))
						{
							return Light.ColorTemperatureName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RendererShadows:
					{
						if (attribute == CrcUtils.CalculateDigestAscii(Renderer.ReceiveShadowsName))
						{
							return Renderer.ReceiveShadowsName;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Renderer.SortingOrderName))
						{
							return Renderer.SortingOrderName;
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
						string LocalPositionZ = Transform.LocalPositionName + "." + Vector3f.ZName;
						if (attribute == CrcUtils.CalculateDigestAscii(LocalPositionZ))
						{
							return LocalPositionZ;
						}
						string AnchoredPositionX = RectTransform.AnchoredPositionName + "." + Vector2f.XName;
						if (attribute == CrcUtils.CalculateDigestAscii(AnchoredPositionX))
						{
							return AnchoredPositionX;
						}
						string AnchoredPositionY = RectTransform.AnchoredPositionName + "." + Vector2f.YName;
						if (attribute == CrcUtils.CalculateDigestAscii(AnchoredPositionY))
						{
							return AnchoredPositionY;
						}
						string AnchorMinX = RectTransform.AnchorMinName + "." + Vector2f.XName;
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMinX))
						{
							return AnchorMinX;
						}
						string AnchorMinY = RectTransform.AnchorMinName + "." + Vector2f.YName;
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMinY))
						{
							return AnchorMinY;
						}
						string AnchorMaxX = RectTransform.AnchorMaxName + "." + Vector2f.XName;
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMaxX))
						{
							return AnchorMaxX;
						}
						string AnchorMaxY = RectTransform.AnchorMaxName + "." + Vector2f.YName;
						if (attribute == CrcUtils.CalculateDigestAscii(AnchorMaxY))
						{
							return AnchorMaxY;
						}
						string SizeDeltaX = RectTransform.SizeDeltaName + "." + Vector2f.XName;
						if (attribute == CrcUtils.CalculateDigestAscii(SizeDeltaX))
						{
							return SizeDeltaX;
						}
						string SizeDeltaY = RectTransform.SizeDeltaName + "." + Vector2f.YName;
						if (attribute == CrcUtils.CalculateDigestAscii(SizeDeltaY))
						{
							return SizeDeltaY;
						}
						string PivotX = RectTransform.PivotName + "." + Vector2f.XName;
						if (attribute == CrcUtils.CalculateDigestAscii(PivotX))
						{
							return PivotX;
						}
						string PivotY = RectTransform.PivotName + "." + Vector2f.YName;
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
						if (attribute == CrcUtils.CalculateDigestAscii(Camera.FieldOfViewName))
						{
							return Camera.FieldOfViewName;
						}
						if (attribute == CrcUtils.CalculateDigestAscii(Camera.FocalLengthName))
						{
							return Camera.FocalLengthName;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

#warning TODO: Find the actual name of this custom type and implement its attribute names
				case BindingCustomType.Unknown38:
					return "Unknown38_" + attribute;

				default:
					throw new ArgumentException($"Binding type {type} not implemented", nameof(type));
			}
		}

		private AssetRipper.Core.Classes.GameObject.GameObject[] Roots
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

		private UnityVersion Version => m_clip.SerializedFile.Version;

		private readonly AssetRipper.Core.Classes.AnimationClip.AnimationClip m_clip = null;

		private AssetRipper.Core.Classes.GameObject.GameObject[] m_roots = null;
		private bool m_rootInited = false;
	}
}
