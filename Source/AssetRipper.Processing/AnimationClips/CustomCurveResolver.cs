using AssetRipper.Checksum;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_108;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_120;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_198;
using AssetRipper.SourceGenerated.Classes.ClassID_20;
using AssetRipper.SourceGenerated.Classes.ClassID_2083052967;
using AssetRipper.SourceGenerated.Classes.ClassID_224;
using AssetRipper.SourceGenerated.Classes.ClassID_25;
using AssetRipper.SourceGenerated.Classes.ClassID_33;
using AssetRipper.SourceGenerated.Classes.ClassID_330;
using AssetRipper.SourceGenerated.Classes.ClassID_4;
using AssetRipper.SourceGenerated.Classes.ClassID_43;
using AssetRipper.SourceGenerated.Classes.ClassID_74;
using AssetRipper.SourceGenerated.Classes.ClassID_96;
using AssetRipper.SourceGenerated.Extensions;
using AssetRipper.SourceGenerated.Extensions.Enums.AnimationClip.GenericBinding;
using System.Text.RegularExpressions;

namespace AssetRipper.Processing.AnimationClips
{
	/// <summary>
	/// Resolves the attribute names for custom curves
	/// </summary>
	/// <remarks>
	/// This has to remain a class due to the lazy initialization of <see cref="CustomCurveResolver.Roots"/>.
	/// </remarks>
	public partial class CustomCurveResolver
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
						if (UnknownPathRegex.IsMatch(path))
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
							ISkinnedMeshRenderer? skin = child.GameObject_C4P?.TryGetComponent<ISkinnedMeshRenderer>();
							if (skin == null)
							{
								continue;
							}
							IMesh? mesh = skin.MeshP;
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
						if (UnknownPathRegex.IsMatch(path))
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
							IRenderer? renderer = child.GameObject_C4P?.TryGetComponent<IRenderer>();
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
					return ThrowUnknownAttributeException(type, attribute);

				case BindingCustomType.MonoBehaviour:
					{
						if (MonoBehaviour.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					return ThrowUnknownAttributeException(type, attribute);

				case BindingCustomType.Light:
					{
						if (Light.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					return ThrowUnknownAttributeException(type, attribute);

				case BindingCustomType.RendererShadows:
					{
						if (Renderer.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					return ThrowUnknownAttributeException(type, attribute);

				case BindingCustomType.ParticleSystem:
					{
						if (ParticleSystem.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
						else
						{
							//This has at least one ordinal property name,
							//So the precalculated hashes are insufficient for recovery.
							//Binary analysis may be required.
							//Example failed attributes:
							//0x45ECBD03 (1173142787)
							//0x483A8AB1 (1211796145)
							//0x72FE5CE2 (1929272546)
							//https://github.com/AssetRipper/AssetRipper/issues/1025
						}
					}
					return Crc32Algorithm.ReverseAscii(attribute, $"ParticleSystem_0x{attribute:X}_");

				case BindingCustomType.RectTransform:
					{
						if (RectTransform.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					return ThrowUnknownAttributeException(type, attribute);

				case BindingCustomType.LineRenderer:
					{
						if (LineRenderer.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					return ThrowUnknownAttributeException(type, attribute);

				case BindingCustomType.TrailRenderer:
					{
						if (TrailRenderer.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					return ThrowUnknownAttributeException(type, attribute);

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
							_ => ThrowUnknownAttributeException(type, attribute)
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
							_ => ThrowUnknownAttributeException(type, attribute)
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
							_ => ThrowUnknownAttributeException(type, attribute)
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
							_ => ThrowUnknownAttributeException(type, attribute)
						};
					}

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
							_ => ThrowUnknownAttributeException(type, attribute)
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
							_ => ThrowUnknownAttributeException(type, attribute)
						};
					}

				case BindingCustomType.Camera:
					{
						if (Camera.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					return ThrowUnknownAttributeException(type, attribute);

				case BindingCustomType.VisualEffect:
					{
						if (VisualEffect.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
						else
						{
							//This has at least one ordinal property name,
							//So the precalculated hashes are insufficient for recovery.
							//Binary analysis may be required.
							//Example failed attributes:
							//0xF781B1D9 (4152472025)
							//0x4CB2F934 (1286797620)
							//https://github.com/AssetRipper/AssetRipper/issues/1047
						}
					}
					return Crc32Algorithm.ReverseAscii(attribute, $"VisualEffect_0x{attribute:X}_");

				case BindingCustomType.ParticleForceField:
					{
						if (ParticleSystemForceField.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
						else
						{
							//This has at least one ordinal property name,
							//So the precalculated hashes are insufficient for recovery.
							//Binary analysis may be required.
							//Example failed attributes:
							//0x8D909E70 (2375065200)
							//https://github.com/AssetRipper/AssetRipper/issues/1239
						}
					}
					return Crc32Algorithm.ReverseAscii(attribute, $"ParticleForceField_0x{attribute:X}_");

				case BindingCustomType.UserDefined:
					return Crc32Algorithm.ReverseAscii(attribute, $"UserDefined_0x{attribute:X}_");

				case BindingCustomType.MeshFilter:
					{
						if (MeshFilter.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
						else
						{
							//This has at least one ordinal property name,
							//So the precalculated hashes are insufficient for recovery.
							//Binary analysis may be required.
							//Example failed attributes:
							//0xDFF2AF49 (3757223753)
						}
					}
					return Crc32Algorithm.ReverseAscii(attribute, $"MeshFilter_0x{attribute:X}_");

				default:
					throw new ArgumentException($"Binding type {type} not implemented", nameof(type));
			}

			[DoesNotReturn]
			static string ThrowUnknownAttributeException(BindingCustomType type, uint attribute)
			{
				throw new ArgumentException($"Unknown attribute 0x{attribute:X} ({attribute}) for {type}");
			}
		}

		[field: MaybeNull]
		private IGameObject[] Roots
		{
			get
			{
				field ??= m_clip.FindRoots().ToArray();
				return field;
			}
		}

		private readonly IAnimationClip m_clip;

		[GeneratedRegex("^path_[0-9]{1,10}$", RegexOptions.Compiled)]
		private static partial Regex UnknownPathRegex { get; }
	}
}
