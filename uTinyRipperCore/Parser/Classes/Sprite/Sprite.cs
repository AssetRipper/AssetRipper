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
		/// Not Release
		/// </summary>
		public static bool HasAtlasRD(TransferInstructionFlags flags) => !flags.IsRelease();
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

		private static int ToSerializedVersion(Version version)
		{
#warning TODO:
			/*if (version.IsGreaterEqual(2018))
			{
				return 2;
			}*/
			return 1;
		}

		public SpriteMetaData GenerateSpriteMetaData(IExportContainer container, SpriteAtlas atlas)
		{
			return SpriteConverter.GenerateSpriteMetaData(container, atlas, this);
		}

		public void GetCoordinates(SpriteAtlas atlas, out Rectf rect, out Vector2f pivot, out Vector4f border)
		{
			Vector2f rectOffset;
			if (atlas == null)
			{
				Vector2f textureOffset = RD.TextureRect.Position;
				Vector2f textureSize = RD.TextureRect.Size;
				rectOffset = RD.TextureRectOffset; // should be equal to RD.TextureRect.Position - Rect.Position
				rect = new Rectf(textureOffset, textureSize);
			}
			else
			{
				SpriteAtlasData atlasData = atlas.RenderDataMap[RenderDataKey];
				Vector2f textureOffset = atlasData.TextureRect.Position;
				Vector2f textureSize = atlasData.TextureRect.Size;
				rectOffset = atlasData.TextureRectOffset;
				rect = new Rectf(textureOffset, textureSize);
			}

			Vector2f sizeDif = Rect.Size - rect.Size;
			Vector2f pivotShiftSize = new Vector2f(Pivot.X * sizeDif.X, Pivot.Y * sizeDif.Y);
			Vector2f relPivotShiftPos = new Vector2f(rectOffset.X / rect.Size.X, rectOffset.Y / rect.Size.Y);
			Vector2f relPivotShiftSize = new Vector2f(pivotShiftSize.X / rect.Size.X, pivotShiftSize.Y / rect.Size.Y);
			pivot = Pivot - relPivotShiftPos + relPivotShiftSize;

			float borderL = Border.X == 0.0f ? 0.0f : Border.X - rectOffset.X;
			float borderB = Border.Y == 0.0f ? 0.0f : Border.Y - rectOffset.Y;
			float borderR = Border.Z == 0.0f ? 0.0f : Border.Z + rectOffset.X - sizeDif.X;
			float borderT = Border.W == 0.0f ? 0.0f : Border.W + rectOffset.Y - sizeDif.Y;
			border = new Vector4f(borderL, borderB, borderR, borderT);
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
				reader.AlignStream(AlignType.Align4);
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
				RenderDataKey = reader.ReadTupleTLong<GUID>();
				AtlasTags = reader.ReadStringArray();
				SpriteAtlas.Read(reader);
			}
			RD.Read(reader);
#if UNIVERSAL
			if (HasAtlasRD(reader.Flags))
			{
				AtlasRD.Read(reader);
			}
#endif
			reader.AlignStream(AlignType.Align4);

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

		public Rectf Rect;
		public Vector2f Offset;
		public Vector4f Border;
		public Vector2f Pivot;
		public Tuple<GUID, long> RenderDataKey;
		public PPtr<SpriteAtlas> SpriteAtlas;
		public SpriteRenderData RD;
#if UNIVERSAL
		public SpriteRenderData AtlasRD;
#endif

		public const string SpriteAtlasName = "m_SpriteAtlas";
		public const string RDName = "m_RD";
	}
}
