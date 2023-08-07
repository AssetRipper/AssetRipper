using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.GenericBinding;
using System.Text.RegularExpressions;

namespace AssetRipper.Processing.AnimationClips
{
	public sealed partial class CustomCurveResolver
	{
		public CustomCurveResolver(IAnimationClip clip)
		{
			m_clip = clip ?? throw new ArgumentNullException(nameof(clip));
		}

		public string ToAttributeName(BindingCustomType type, uint attribute, string path)
		{
			switch (type)
			{
				case BindingCustomType.BlendShape:
					{
						const string Prefix = "blendShape.";
						if (UnknownPathRegex().IsMatch(path))
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
							IMesh? mesh = skin.Mesh_C137P;
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
						if (UnknownPathRegex().IsMatch(path))
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

							uint subPropIndex = attribute >> 28 & 3;
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
						if (FieldHashes.TryGetPath(ClassIDType.MonoBehaviour, attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.Light:
					{
						if (FieldHashes.TryGetPath(ClassIDType.Light, attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RendererShadows:
					{
						if (FieldHashes.TryGetPath(ClassIDType.Renderer, attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.ParticleSystem:
					{
						if (FieldHashes.TryGetPath(ClassIDType.ParticleSystem, attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RectTransform:
					{
						if (FieldHashes.TryGetPath(ClassIDType.RectTransform, attribute, out string? foundPath))	
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.LineRenderer:
					{
						if (FieldHashes.TryGetPath(ClassIDType.LineRenderer, attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.TrailRenderer:
					{
						if (FieldHashes.TryGetPath(ClassIDType.TrailRenderer, attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.PositionConstraint:
					{
						uint property = attribute & 0xF;
						return property switch
						{
							0 => "m_RestTranslation.x",
							1 => "m_RestTranslation.y",
							2 => "m_RestTranslation.z",
							3 => "m_Weight",
							4 => "m_TranslationOffset.x",
							5 => "m_TranslationOffset.y",
							6 => "m_TranslationOffset.z",
							7 => "m_AffectTranslationX",
							8 => "m_AffectTranslationY",
							9 => "m_AffectTranslationZ",
							10 => "m_Active",
							11 => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							12 => $"m_Sources.Array.data[{attribute >> 8}].weight",
							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}")
						};
					}

				case BindingCustomType.RotationConstraint:
					{
						uint property = attribute & 0xF;
						return property switch
						{
							0 => "m_RestRotation.x",
							1 => "m_RestRotation.y",
							2 => "m_RestRotation.z",
							3 => "m_Weight",
							4 => "m_RotationOffset.x",
							5 => "m_RotationOffset.y",
							6 => "m_RotationOffset.z",
							7 => "m_AffectRotationX",
							8 => "m_AffectRotationY",
							9 => "m_AffectRotationZ",
							10 => "m_Active",
							11 => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							12 => $"m_Sources.Array.data[{attribute >> 8}].weight",
							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}")
						};
					}

				case BindingCustomType.ScaleConstraint:
					{
						uint property = attribute & 0xF;
						return property switch
						{
							0 => "m_ScaleAtRest.x",
							1 => "m_ScaleAtRest.y",
							2 => "m_ScaleAtRest.z",
							3 => "m_Weight",
							4 => "m_ScalingOffset.x",
							5 => "m_ScalingOffset.y",
							6 => "m_ScalingOffset.z",
							7 => "m_AffectScalingX",
							8 => "m_AffectScalingY",
							9 => "m_AffectScalingZ",
							10 => "m_Active",
							11 => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							12 => $"m_Sources.Array.data[{attribute >> 8}].weight",
							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}")
						};
					}

				case BindingCustomType.AimConstraint:
					{
						uint property = attribute & 0xF;
						return property switch
						{
							0 => "m_Weight",
							1 => "m_AffectRotationX",
							2 => "m_AffectRotationY",
							3 => "m_AffectRotationZ",
							4 => "m_Active",
							5 => "m_WorldUpObject",
							6 => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							7 => $"m_Sources.Array.data[{attribute >> 8}].weight",
							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}")
						};
					}

#warning TODO:
				case BindingCustomType.ParentConstraint:
					{
						uint property = attribute & 0xF;
						return property switch
						{
							0 => "m_Weight",
							1 => "m_AffectTranslationX",
							2 => "m_AffectTranslationY",
							3 => "m_AffectTranslationZ",
							4 => "m_AffectRotationX",
							5 => "m_AffectRotationY",
							6 => "m_AffectRotationZ",
							7 => "m_Active",
							8 => $"m_TranslationOffsets.Array.data[{attribute >> 8}].x",
							9 => $"m_TranslationOffsets.Array.data[{attribute >> 8}].y",
							10 => $"m_TranslationOffsets.Array.data[{attribute >> 8}].z",
							11 => $"m_RotationOffsets.Array.data[{attribute >> 8}].x",
							12 => $"m_RotationOffsets.Array.data[{attribute >> 8}].y",
							13 => $"m_RotationOffsets.Array.data[{attribute >> 8}].z",
							14 => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							15 => $"m_Sources.Array.data[{attribute >> 8}].weight",
							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}")
						};
					}

				case BindingCustomType.LookAtConstraint:
					{
						uint property = attribute & 0xF;
						return property switch
						{
							0 => "m_Weight",
							1 => "m_Active",
							2 => "m_WorldUpObject",
							3 => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							4 => $"m_Sources.Array.data[{attribute >> 8}].weight",
							5 => "m_Roll",
							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}")
						};
					}

				case BindingCustomType.Camera:
					{
						if (FieldHashes.TryGetPath(ClassIDType.Camera, attribute, out string? foundPath))
						{
							return foundPath;
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
				m_roots ??= m_clip.FindRoots().ToArray();
				return m_roots;
			}
		}

		private readonly IAnimationClip m_clip;
		private IGameObject[]? m_roots;

		[GeneratedRegex("^path_[0-9]{1,10}$", RegexOptions.Compiled)]
		private static partial Regex UnknownPathRegex();
	}
}
