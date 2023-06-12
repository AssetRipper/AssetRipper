using AssetRipper.SourceGenerated;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
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
			int i;
			string? foundPath;

			switch (type)
			{
				case BindingCustomType.BlendShape:
					{
						const string Prefix = "blendShape.";

						if (UnknownPathRegex().IsMatch(path))
						{
							return Prefix + attribute;
						}

						for (i = 0; i < Roots.Length; i++)
						{
							if (!Roots[i].TryGetChildComponent(path, out ISkinnedMeshRenderer? skin))
							{
								continue;
							}

							if (skin.Mesh_C137P is not IMesh mesh)
							{
								continue;
							}

							if (mesh.FindBlendShapeNameByCRC(attribute) is not string shapeName)
							{
								continue;
							}

							return Prefix + shapeName;
						}

						return Prefix + attribute;
					}

				case BindingCustomType.Renderer:
					return $"m_Materials.Array.data[{attribute}]"; //from the common string

				case BindingCustomType.RendererMaterial:
					{
						const string Prefix = "material.";

						if (UnknownPathRegex().IsMatch(path))
						{
							return Prefix + attribute;
						}

						uint crc28 = attribute & 0xFFFFFFF;
						bool usesRgba = (attribute & (1 << 30)) != 0;
						bool hasNoSubProperty = (attribute & (1 << 31)) != 0;
						uint subPropetyIndex = (attribute >> 28) & 3;

						for (i = 0; i < Roots.Length; i++)
						{
							if (!Roots[i].TryGetChildComponent(path, out IRenderer? renderer))
							{
								continue;
							}

							if (renderer.FindMaterialPropertyNameByCRC28(crc28) is not string property)
							{
								continue;
							}

							if (hasNoSubProperty)
							{
								return Prefix + property;
							}

							char subProperty = subPropetyIndex switch
							{
								0 => usesRgba ? 'r' : 'x',
								1 => usesRgba ? 'g' : 'y',
								2 => usesRgba ? 'b' : 'z',
								_ => usesRgba ? 'a' : 'w',
							};

							return Prefix + property + "." + subProperty;
						}

						return Prefix + attribute;
					}

				case BindingCustomType.SpriteRenderer:
					{
						// Not Crc
						return attribute switch
						{
							0 => "m_Sprite",
							
							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}"),
						};
					}
					
				case BindingCustomType.MonoBehaviour:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.MonoBehaviour, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

				case BindingCustomType.Light:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.Light, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

				case BindingCustomType.RendererShadows:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.Renderer, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

				case BindingCustomType.ParticleSystem:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.ParticleSystem, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

				case BindingCustomType.RectTransform:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.RectTransform, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

				case BindingCustomType.LineRenderer:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.LineRenderer, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

				case BindingCustomType.TrailRenderer:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.TrailRenderer, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

				case BindingCustomType.PositionConstraint:
					{
						// Not Crc

						uint property = attribute & 0xF; // 0...1111 (max 15)

						return property switch
						{
							0u  => "m_RestTranslation.x",
							1u  => "m_RestTranslation.y",
							2u  => "m_RestTranslation.z",
							3u  => "m_Weight",
							4u  => "m_TranslationOffset.x",
							5u  => "m_TranslationOffset.y",
							6u  => "m_TranslationOffset.z",
							7u  => "m_AffectTranslationX",
							8u  => "m_AffectTranslationY",
							9u  => "m_AffectTranslationZ",
							10u => "m_Active",
							11u => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							12u => $"m_Sources.Array.data[{attribute >> 8}].weight",

							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}"),
						};
					}

				case BindingCustomType.RotationConstraint:
					{
						// Not Crc

						uint property = attribute & 0xF; // 0...1111 (max 15)

						return property switch
						{
							0u  => "m_RestRotation.x",
							1u  => "m_RestRotation.y",
							2u  => "m_RestRotation.z",
							3u  => "m_Weight",
							4u  => "m_RotationOffset.x",
							5u  => "m_RotationOffset.y",
							6u  => "m_RotationOffset.z",
							7u  => "m_AffectRotationX",
							8u  => "m_AffectRotationY",
							9u  => "m_AffectRotationZ",
							10u => "m_Active",
							11u => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							12u => $"m_Sources.Array.data[{attribute >> 8}].weight",

							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}"),
						};
					}

				case BindingCustomType.ScaleConstraint:
					{
						// Not Crc

						uint property = attribute & 0xF; // 0...1111 (max 15)

						return property switch
						{
							0u  => "m_ScaleAtRest.x",
							1u  => "m_ScaleAtRest.y",
							2u  => "m_ScaleAtRest.z",
							3u  => "m_Weight",
							4u  => "m_ScalingOffset.x",
							5u  => "m_ScalingOffset.y",
							6u  => "m_ScalingOffset.z",
							7u  => "m_AffectScalingX",
							8u  => "m_AffectScalingY",
							9u  => "m_AffectScalingZ",
							10u => "m_Active",
							11u => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							12u => $"m_Sources.Array.data[{attribute >> 8}].weight",

							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}"),
						};
					}

				case BindingCustomType.AimConstraint:
					{
						// Not Crc

						uint property = attribute & 0xF; // 0...1111 (max 15)

						return property switch
						{
							0u => "m_Weight",
							1u => "m_AffectRotationX",
							2u => "m_AffectRotationY",
							3u => "m_AffectRotationZ",
							4u => "m_Active",
							5u => "m_WorldUpObject",
							6u => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							7u => $"m_Sources.Array.data[{attribute >> 8}].weight",

							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}"),
						};
					}

				case BindingCustomType.ParentConstraint:
					{
						// Not Crc

						uint property = attribute & 0xF; // 0...1111 (max 15)

						return property switch
						{
							0u  => "m_Weight",
							1u  => "m_AffectTranslationX",
							2u  => "m_AffectTranslationY",
							3u  => "m_AffectTranslationZ",
							4u  => "m_AffectRotationX",
							5u  => "m_AffectRotationY",
							6u  => "m_AffectRotationZ",
							7u  => "m_Active",
							8u  => $"m_TranslationOffsets.Array.data[{attribute >> 8}].x",
							9u  => $"m_TranslationOffsets.Array.data[{attribute >> 8}].y",
							10u => $"m_TranslationOffsets.Array.data[{attribute >> 8}].z",
							11u => $"m_RotationOffsets.Array.data[{attribute >> 8}].x",
							12u => $"m_RotationOffsets.Array.data[{attribute >> 8}].y",
							13u => $"m_RotationOffsets.Array.data[{attribute >> 8}].z",
							14u => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							15u => $"m_Sources.Array.data[{attribute >> 8}].weight",

							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}"),
						};
					}

				case BindingCustomType.LookAtConstraint:
					{
						// Not Crc

						uint property = attribute & 0xF;

						return property switch
						{
							0u => "m_Weight",
							1u => "m_Active",
							2u => "m_WorldUpObject",
							3u => $"m_Sources.Array.data[{attribute >> 8}].sourceTransform",
							4u => $"m_Sources.Array.data[{attribute >> 8}].weight",
							5u => "m_Roll",

							_ => throw new ArgumentException($"Unknown attribute {attribute} for {type}"),
						};
					}

				case BindingCustomType.Camera:
					{
						if (!FieldHashes.TryGetPath(ClassIDType.Camera, attribute, out foundPath))
						{
							throw new ArgumentException($"Unknown attribute {attribute} for {type}");
						}

						return foundPath;
					}

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
				return m_roots ??= m_clip.FindRoots().ToArray();
			}
		}

		private readonly IAnimationClip m_clip;
		private IGameObject[]? m_roots;

		[GeneratedRegex("^path_[0-9]{1,10}$", RegexOptions.Compiled)]
		private static partial Regex UnknownPathRegex();
	}
}
