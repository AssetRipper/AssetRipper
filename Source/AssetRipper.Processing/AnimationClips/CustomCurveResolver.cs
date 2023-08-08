using AssetRipper.Checksum;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_108;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_1183024399;
using AssetRipper.SourceGenerated.Classes.ClassID_120;
using AssetRipper.SourceGenerated.Classes.ClassID_137;
using AssetRipper.SourceGenerated.Classes.ClassID_1773428102;
using AssetRipper.SourceGenerated.Classes.ClassID_1818360608;
using AssetRipper.SourceGenerated.Classes.ClassID_1818360609;
using AssetRipper.SourceGenerated.Classes.ClassID_1818360610;
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
using AssetRipper.SourceGenerated.Classes.ClassID_895512359;
using AssetRipper.SourceGenerated.Classes.ClassID_96;
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
						if (MonoBehaviour.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.Light:
					{
						if (Light.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RendererShadows:
					{
						if (Renderer.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.ParticleSystem:
					{
						if (ParticleSystem.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RectTransform:
					{
						if (RectTransform.TryGetPath(attribute, out string? foundPath))	
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.LineRenderer:
					{
						if (LineRenderer.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.TrailRenderer:
					{
						if (TrailRenderer.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.PositionConstraint:
					{
						if (PositionConstraint.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.RotationConstraint:
					{
						if (RotationConstraint.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.ScaleConstraint:
					{
						if (ScaleConstraint.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.AimConstraint:
					{
						if (AimConstraint.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.ParentConstraint:
					{
						if (ParentConstraint.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.LookAtConstraint:
					{
						if (LookAtConstraint.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");
				
				case BindingCustomType.Camera:
					{
						if (Camera.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				// TryGetPath may not work here
				case BindingCustomType.VisualEffect:
					{
						if (VisualEffect.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				// TryGetPath may not work here
				case BindingCustomType.ParticleForceField:
					{
						if (ParticleSystemForceField.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

				case BindingCustomType.UserDefined:
					return "UserDefined_" + Crc32Algorithm.ReverseAscii(attribute);
				
				// TryGetPath may not work here
				case BindingCustomType.MeshFilter:
					{
						if (MeshFilter.TryGetPath(attribute, out string? foundPath))
						{
							return foundPath;
						}
					}
					throw new ArgumentException($"Unknown attribute {attribute} for {type}");

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
