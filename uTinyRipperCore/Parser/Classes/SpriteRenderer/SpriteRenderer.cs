using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.SpriteRenderers;
using uTinyRipper.Exporter.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes
{
	public sealed class SpriteRenderer : Renderer
	{
		public SpriteRenderer(AssetInfo assetInfo):
			base(assetInfo)
		{
		}

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool IsReadFlip(Version version)
		{
			return version.IsGreaterEqual(5, 3);
		}
		/// <summary>
		/// 5.6.0 and greater
		/// </summary>
		public static bool IsReadDrawMode(Version version)
		{
			return version.IsGreaterEqual(5, 6);
		}
		/// <summary>
		/// 5.6.2 to 5.6.x
		/// 2017.1.0b5 and greater
		/// </summary>
		public static bool IsReadWasSpriteAssigned(Version version)
		{
			if(version.IsGreaterEqual(2017))
			{
				return version.IsGreaterEqual(2017, 1, 0, VersionType.Beta, 5);
			}
			else
			{
				return version.IsGreaterEqual(5, 6, 2);
			}
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool IsReadMaskInteraction(Version version)
		{
			return version.IsGreaterEqual(2017);
		}
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool IsReadSpriteSortPoint(Version version)
		{
			return version.IsGreaterEqual(2018, 2);
		}
		
		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlignColor(Version version)
		{
			return version.IsGreaterEqual(4, 5);
		}
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsAlignFlip(Version version)
		{
			return version.IsGreaterEqual(5, 4);
		}

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Sprite.Read(reader);
			Color.Read(reader);
			if (IsAlignColor(reader.Version))
			{
				reader.AlignStream(AlignType.Align4);
			}

			if(IsReadFlip(reader.Version))
			{
				FlipX = reader.ReadBoolean();
				FlipY = reader.ReadBoolean();
				if(IsAlignFlip(reader.Version))
				{
					reader.AlignStream(AlignType.Align4);
				}
			}

			if(IsReadDrawMode(reader.Version))
			{
				DrawMode = (SpriteDrawMode)reader.ReadInt32();
				Size.Read(reader);
				AdaptiveModeThreshold = reader.ReadSingle();
				SpriteTileMode = (SpriteTileMode)reader.ReadInt32();
			}
			if(IsReadWasSpriteAssigned(reader.Version))
			{
				WasSpriteAssigned = reader.ReadBoolean();
				reader.AlignStream(AlignType.Align4);
			}
			if(IsReadMaskInteraction(reader.Version))
			{
				MaskInteraction = (SpriteMaskInteraction)reader.ReadInt32();
			}
			if(IsReadSpriteSortPoint(reader.Version))
			{
				SpriteSortPoint = (SpriteSortPoint)reader.ReadInt32();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object asset in base.FetchDependencies(file, isLog))
			{
				yield return asset;
			}
			
			yield return Sprite.FetchDependency(file, isLog, ToLogString, "m_Sprite");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add("m_Sprite", Sprite.ExportYAML(container));
			node.Add("m_Color", Color.ExportYAML(container));
			node.Add("m_FlipX", FlipX);
			node.Add("m_FlipY", FlipY);
			node.Add("m_DrawMode", (int)DrawMode);
			node.Add("m_Size", (IsReadDrawMode(container.Version) ? Size : Vector2f.One).ExportYAML(container));
			node.Add("m_AdaptiveModeThreshold", AdaptiveModeThreshold);
			node.Add("m_SpriteTileMode", (int)SpriteTileMode);
			node.Add("m_WasSpriteAssigned", WasSpriteAssigned);
			node.Add("m_MaskInteraction", (int)MaskInteraction);
			return node;
		}

		public bool FlipX { get; private set; }
		public bool FlipY { get; private set; }
		public SpriteDrawMode DrawMode { get; private set; }
		public float AdaptiveModeThreshold { get; private set; }
		public SpriteTileMode SpriteTileMode { get; private set; }
		public bool WasSpriteAssigned { get; private set; }
		public SpriteMaskInteraction MaskInteraction { get; private set; }
		public SpriteSortPoint SpriteSortPoint { get; private set; }

		public PPtr<Sprite> Sprite;
		public ColorRGBAf Color;
		public Vector2f Size;
	}
}
