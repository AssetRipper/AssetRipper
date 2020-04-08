using System;
using System.Collections.Generic;
using uTinyRipper.Classes.SpriteAtlases;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.YAML;
using uTinyRipper.Converters;
using uTinyRipper.Classes.Misc;
using uTinyRipper.Classes.TextureImporters;

namespace uTinyRipper.Classes
{
	public sealed class Sprite : NamedObject
	{
		public Sprite(AssetInfo assetInfo) :
			base(assetInfo)
		{
		}

		public static int ToSerializedVersion(Version version)
		{
#warning TODO:
			/*if (version.IsGreaterEqual(2018))
			{
				return 2;
			}*/
			return 1;
		}

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		public static bool HasBorder(Version version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.4.1p3 and greater
		/// </summary>
		public static bool HasPivot(Version version) => version.IsGreaterEqual(5, 4, 1, VersionType.Patch, 3);
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasPolygon(Version version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasAtlasName(TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasRendererData(Version version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 2017.1 and greater and Not Release
		/// </summary>
		public static bool HasAtlasRD(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2017);
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasPhysicsShape(Version version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool HasBones(Version version) => version.IsGreaterEqual(2018);
		/// <summary>
		/// and greater and Not Release
		/// </summary>
		public static bool HasSpriteID(Version version, TransferInstructionFlags flags) => !flags.IsRelease() && version.IsGreaterEqual(2018);

		public SpriteMetaData GenerateSpriteMetaData(IExportContainer container, SpriteAtlas atlas)
		{
			return SpriteConverter.GenerateSpriteMetaData(container, atlas, this);
		}

		public void GetSpriteCoordinatesInAtlas(SpriteAtlas atlas, out Rectf sAtlasRect, out Vector2f sAtlasPivot, out Vector4f sAtlasBorder)
		{
			// sprite values are relative to original image (image, it was created from).
			// since atlas shuffle and crop sprite images, we need to recalculate those values.
			// if sprite doesn't belong to an atlas, consider its image as single sprite atlas

			Vector2f cropBotLeft;
			Vector2f cropTopRight;
			if (atlas == null)
			{
				Vector2f spriteOffset = RD.TextureRect.Position;
				Vector2f spriteSize = RD.TextureRect.Size;
				sAtlasRect = new Rectf(spriteOffset, spriteSize);
				cropBotLeft = RD.TextureRectOffset;
			}
			else
			{
				SpriteAtlasData atlasData = atlas.RenderDataMap[RenderDataKey];
				Vector2f spriteOffset = atlasData.TextureRect.Position;
				Vector2f spriteSize = atlasData.TextureRect.Size;
				sAtlasRect = new Rectf(spriteOffset, spriteSize);
				cropBotLeft = atlasData.TextureRectOffset;
			}

			Vector2f sizeDelta = Rect.Size - sAtlasRect.Size;
			cropTopRight = new Vector2f(sizeDelta.X - cropBotLeft.X, sizeDelta.Y - cropBotLeft.Y);

			Vector2f pivot = Pivot;
			if (!HasPivot(File.Version))
			{
				Vector2f center = new Vector2f(Rect.Size.X / 2.0f, Rect.Size.Y / 2.0f);
				Vector2f pivotOffset = center + Offset;
				pivot = new Vector2f(pivotOffset.X / Rect.Size.X, pivotOffset.Y / Rect.Size.Y);
			}

			Vector2f pivotPosition = new Vector2f(pivot.X * Rect.Size.X, pivot.Y * Rect.Size.Y);
			Vector2f aAtlasPivotPosition = pivotPosition - cropBotLeft;
			sAtlasPivot = new Vector2f(aAtlasPivotPosition.X / sAtlasRect.Size.X, aAtlasPivotPosition.Y / sAtlasRect.Size.Y);

			float borderL = Border.X == 0.0f ? 0.0f : Border.X - cropBotLeft.X;
			float borderB = Border.Y == 0.0f ? 0.0f : Border.Y - cropBotLeft.Y;
			float borderR = Border.Z == 0.0f ? 0.0f : Border.Z - cropTopRight.X;
			float borderT = Border.W == 0.0f ? 0.0f : Border.W - cropTopRight.Y;
			sAtlasBorder = new Vector4f(borderL, borderB, borderR, borderT);
		}

		public Vector2f[][] GenerateOutline(SpriteAtlas atlas, Rectf rect, Vector2f pivot)
		{
			Vector2f[][] outlines = RD.GenerateOutline(File.Version);
			float pivotShiftX = rect.Width * pivot.X - rect.Width * 0.5f;
			float pivotShiftY = rect.Height * pivot.Y - rect.Height * 0.5f;
			Vector2f pivotShift = new Vector2f(pivotShiftX, pivotShiftY);
			foreach (Vector2f[] outline in outlines)
			{
				for (int i = 0; i < outline.Length; i++)
				{
					Vector2f point = outline[i] * PixelsToUnits;
					outline[i] = point + pivotShift;
				}
			}
			return FixRotation(atlas, outlines);
		}

		public Vector2f[][] GeneratePhysicsShape(SpriteAtlas atlas, Rectf rect, Vector2f pivot)
		{
			if (PhysicsShape.Length > 0)
			{
				Vector2f[][] shape = new Vector2f[PhysicsShape.Length][];
				float pivotShiftX = rect.Width * pivot.X - rect.Width * 0.5f;
				float pivotShiftY = rect.Height * pivot.Y - rect.Height * 0.5f;
				Vector2f pivotShift = new Vector2f(pivotShiftX, pivotShiftY);
				for (int i = 0; i < PhysicsShape.Length; i++)
				{
					shape[i] = new Vector2f[PhysicsShape[i].Length];
					for (int j = 0; j < PhysicsShape[i].Length; j++)
					{
						Vector2f point = PhysicsShape[i][j] * PixelsToUnits;
						shape[i][j] = point + pivotShift;
					}
				}
				return FixRotation(atlas, shape);
			}
			return Array.Empty<Vector2f[]>();
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Rect.Read(reader);
			Offset.Read(reader);
			if (HasBorder(reader.Version))
			{
				Border.Read(reader);
			}
			PixelsToUnits = reader.ReadSingle();
			if (HasPivot(reader.Version))
			{
				Pivot.Read(reader);
			}
			Extrude = reader.ReadUInt32();
			if (HasPolygon(reader.Version))
			{
				IsPolygon = reader.ReadBoolean();
				reader.AlignStream();
			}
#if UNIVERSAL
			if (HasAtlasName(reader.Flags))
			{
				AtlasName = reader.ReadString();
				PackingTag = reader.ReadString();
			}
#endif

			if (HasRendererData(reader.Version))
			{
				RenderDataKey = reader.ReadTupleTLong<UnityGUID>();
				AtlasTags = reader.ReadStringArray();
				SpriteAtlas.Read(reader);
			}
			RD.Read(reader);
#if UNIVERSAL
			if (HasAtlasRD(reader.Version, reader.Flags))
			{
				AtlasRD.Read(reader);
			}
#endif
			reader.AlignStream();

			if (HasPhysicsShape(reader.Version))
			{
				PhysicsShape = reader.ReadAssetArrayArray<Vector2f>();
			}

			if (HasBones(reader.Version))
			{
				Bones = reader.ReadAssetArray<SpriteBone>();
			}
#if UNIVERSAL
			if (HasSpriteID(reader.Version, reader.Flags))
			{
				SpriteID = reader.ReadString();
			}
#endif
		}

		public override IEnumerable<PPtr<Object>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<Object> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(SpriteAtlas, SpriteAtlasName);
			foreach (PPtr<Object> asset in context.FetchDependencies(RD, RDName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		private Vector2f[][] FixRotation(SpriteAtlas atlas, Vector2f[][] outlines)
		{
			bool isPacked = RD.IsPacked;
			SpritePackingRotation rotation = RD.PackingRotation;
			if (atlas != null)
			{
				SpriteAtlasData atlasData = atlas.RenderDataMap[RenderDataKey];
				isPacked = atlasData.IsPacked;
				rotation = atlasData.PackingRotation;
			}

			if (isPacked)
			{
				switch (rotation)
				{
					case SpritePackingRotation.FlipHorizontal:
						{
							foreach(Vector2f[] outline in outlines)
							{
								for (int i = 0; i < outline.Length; i++)
								{
									Vector2f vertex = outline[i];
									outline[i] = new Vector2f(-vertex.X, vertex.Y);
								}
							}
						}
						break;

					case SpritePackingRotation.FlipVertical:
						{
							foreach (Vector2f[] outline in outlines)
							{
								for (int i = 0; i < outline.Length; i++)
								{
									Vector2f vertex = outline[i];
									outline[i] = new Vector2f(vertex.X, -vertex.Y);
								}
							}
						}
						break;

					case SpritePackingRotation.Rotate90:
						{
							foreach (Vector2f[] outline in outlines)
							{
								for (int i = 0; i < outline.Length; i++)
								{
									Vector2f vertex = outline[i];
									outline[i] = new Vector2f(vertex.Y, vertex.X);
								}
							}
						}
						break;

					case SpritePackingRotation.Rotate180:
						{
							foreach (Vector2f[] outline in outlines)
							{
								for (int i = 0; i < outline.Length; i++)
								{
									Vector2f vertex = outline[i];
									outline[i] = new Vector2f(-vertex.X, -vertex.Y);
								}
							}
						}
						break;
				}
			}
			return outlines;
		}

		public float PixelsToUnits { get; set; }
		public uint Extrude { get; set; }
		public bool IsPolygon { get; set; }
#if UNIVERSAL
		public string AtlasName { get; set; }
		public string PackingTag { get; set; }
#endif
		public string[] AtlasTags { get; set; }
		public Vector2f[][] PhysicsShape { get; set; }
		public SpriteBone[] Bones { get; set; }
#if UNIVERSAL
		public string SpriteID { get; set; }
#endif

		/// <summary>
		/// Rectangle of the sprite in the source image (image, that was used to create an atlas)
		/// </summary>
		public Rectf Rect;
		/// <summary>
		/// Pivot pixel offset relative to sprite center
		/// </summary>
		public Vector2f Offset;
		/// <summary>
		/// Border of the sprite relative to original rectangle Rect
		/// </summary>
		public Vector4f Border;
		/// <summary>
		/// Pivot, relative to left-bottom of original sprite rectangle Rect
		/// </summary>
		public Vector2f Pivot;
		public Tuple<UnityGUID, long> RenderDataKey;
		public PPtr<SpriteAtlas> SpriteAtlas;
		public SpriteRenderData RD;
#if UNIVERSAL
		public SpriteRenderData AtlasRD;
#endif

		public const string SpriteAtlasName = "m_SpriteAtlas";
		public const string RDName = "m_RD";
	}
}
