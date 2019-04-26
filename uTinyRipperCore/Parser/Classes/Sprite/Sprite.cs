using System;
using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.SpriteAtlases;
using uTinyRipper.Classes.Sprites;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

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
		public static bool IsReadBorder(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.4.1p3 and greater
		/// </summary>
		public static bool IsReadPivot(Version version)
		{
			return version.IsGreaterEqual(5, 4, 1, VersionType.Patch, 3);
		}
		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadPolygon(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadAtlasName(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadRendererData(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		/// <summary>
		/// Not Release
		/// </summary>
		public static bool IsReadAtlasRD(TransferInstructionFlags flags)
		{
			return !flags.IsRelease();
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadPhysicsShape(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		/// <summary>
		/// 2018.1 and greater
		/// </summary>
		public static bool IsReadBones(Version version)
		{
			return version.IsGreaterEqual(2018);
		}
		/// <summary>
		/// and greater and Not Release
		/// </summary>
		public static bool IsReadSpriteID(Version version, TransferInstructionFlags flags)
		{
			return !flags.IsRelease() && version.IsGreaterEqual(2018);
		}

		private static int GetSerializedVersion(Version version)
		{
			if (Config.IsExportTopmostSerializedVersion)
			{
				// return 2;
				return 1;
			}

			if (version.IsGreaterEqual(2018))
			{
				return 2;
			}
			return 1;
		}

		public void GetExportPosition(SpriteAtlas atlas, out Rectf rect, out Vector2f pivot, out Vector4f border)
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

		public IReadOnlyList<IReadOnlyList<Vector2f>> GenerateOutline(SpriteAtlas atlas, Rectf rect, Vector2f pivot)
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

		public IReadOnlyList<IReadOnlyList<Vector2f>> GeneratePhysicsShape(SpriteAtlas atlas, Rectf rect, Vector2f pivot)
		{
			if (IsReadPhysicsShape(File.Version))
			{
				Vector2f[][] shape = new Vector2f[PhysicsShape.Count][];
				float pivotShiftX = rect.Width * pivot.X - rect.Width * 0.5f;
				float pivotShiftY = rect.Height * pivot.Y - rect.Height * 0.5f;
				Vector2f pivotShift = new Vector2f(pivotShiftX, pivotShiftY);
				for (int i = 0; i < PhysicsShape.Count; i++)
				{
					shape[i] = new Vector2f[PhysicsShape[i].Count];
					for (int j = 0; j < PhysicsShape[i].Count; j++)
					{
						Vector2f point = PhysicsShape[i][j] * PixelsToUnits;
						shape[i][j] = point + pivotShift;
					}
				}
				return FixRotation(atlas, shape);
			}
			else
			{
				return new Vector2f[0][];
			}
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Rect.Read(reader);
			Offset.Read(reader);
			if (IsReadBorder(reader.Version))
			{
				Border.Read(reader);
			}
			PixelsToUnits = reader.ReadSingle();
			if (IsReadPivot(reader.Version))
			{
				Pivot.Read(reader);
			}
			Extrude = reader.ReadUInt32();
			if (IsReadPolygon(reader.Version))
			{
				IsPolygon = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
#if UNIVERSAL
			if (IsReadAtlasName(reader.Flags))
			{
				AtlasName = reader.ReadString();
				PackingTag = reader.ReadString();
			}
#endif

			if (IsReadRendererData(reader.Version))
			{
				RenderDataKey = reader.ReadTupleTLong<EngineGUID>();
				m_atlasTags = reader.ReadStringArray();
				SpriteAtlas.Read(reader);
			}
			RD.Read(reader);
#if UNIVERSAL
			if (IsReadAtlasRD(reader.Flags))
			{
				AtlasRD.Read(reader);
			}
#endif
			reader.AlignStream(AlignType.Align4);

			if (IsReadPhysicsShape(reader.Version))
			{
				int count = reader.ReadInt32();
				m_physicsShape = new Vector2f[count][];
				for (int i = 0; i < count; i++)
				{
					m_physicsShape[i] = reader.ReadAssetArray<Vector2f>();
				}
			}

			if (IsReadBones(reader.Version))
			{
				m_bones = reader.ReadAssetArray<SpriteBone>();
			}
#if UNIVERSAL
			if (IsReadSpriteID(reader.Version, reader.Flags))
			{
				SpriteID = reader.ReadString();
			}
#endif
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			if (!SpriteAtlas.IsNull)
			{
				yield return SpriteAtlas.FetchDependency(file, isLog, ToLogString, "SpriteAtlas");
			}
			foreach (Object asset in RD.FetchDependencies(file))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		private IReadOnlyList<IReadOnlyList<Vector2f>> FixRotation(SpriteAtlas atlas, Vector2f[][] outlines)
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

		public float PixelsToUnits { get; private set; }
		public uint Extrude { get; private set; }
		public bool IsPolygon { get; private set; }
#if UNIVERSAL
		public string AtlasName { get; private set; }
		public string PackingTag { get; private set; }
#endif
		public IReadOnlyList<string> AtlasTags => m_atlasTags;
		public IReadOnlyList<IReadOnlyList<Vector2f>> PhysicsShape => m_physicsShape;
		public IReadOnlyList<SpriteBone> Bones => m_bones;
#if UNIVERSAL
		public string SpriteID { get; private set; }
#endif

		public Rectf Rect;
		public Vector2f Offset;
		public Vector4f Border;
		public Vector2f Pivot;
		public Tuple<EngineGUID, long> RenderDataKey;
		public PPtr<SpriteAtlas> SpriteAtlas;
		public SpriteRenderData RD;
#if UNIVERSAL
		public SpriteRenderData AtlasRD;
#endif

		private string[] m_atlasTags;
		private Vector2f[][] m_physicsShape;
		private SpriteBone[] m_bones;
	}
}
