using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Classes.SpriteRenderers;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes
{
	public class SpriteRenderer : Renderer
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

		public override void Read(AssetStream stream)
		{
			base.Read(stream);

			Sprite.Read(stream);
			Color.Read(stream);
			if (IsAlignColor(stream.Version))
			{
				stream.AlignStream(AlignType.Align4);
			}

			if(IsReadFlip(stream.Version))
			{
				FlipX = stream.ReadBoolean();
				FlipY = stream.ReadBoolean();
				if(IsAlignFlip(stream.Version))
				{
					stream.AlignStream(AlignType.Align4);
				}
			}

			if(IsReadDrawMode(stream.Version))
			{
				DrawMode = (SpriteDrawMode)stream.ReadInt32();
				Size.Read(stream);
				AdaptiveModeThreshold = stream.ReadSingle();
				SpriteTileMode = (SpriteTileMode)stream.ReadInt32();
			}
			if(IsReadWasSpriteAssigned(stream.Version))
			{
				WasSpriteAssigned = stream.ReadBoolean();
				stream.AlignStream(AlignType.Align4);
			}
			if(IsReadMaskInteraction(stream.Version))
			{
				MaskInteraction = (SpriteMaskInteraction)stream.ReadInt32();
			}
		}

		public override IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			foreach (Object @object in base.FetchDependencies(file, isLog))
			{
				yield return @object;
			}
			
			yield return Sprite.FetchDependency(file, isLog, ToLogString, "m_Sprite");
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
#warning TODO: values acording to read version (current 2017.3.0f3)
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

		public PPtr<Sprite> Sprite;
		public ColorRGBAf Color;
		public Vector2f Size;
	}
}
