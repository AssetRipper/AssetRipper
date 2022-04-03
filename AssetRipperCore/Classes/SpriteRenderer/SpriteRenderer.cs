using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.SpriteRenderer
{
	public sealed class SpriteRenderer : Renderer.Renderer
	{
		public SpriteRenderer(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 5.3.0 and greater
		/// </summary>
		public static bool HasFlip(UnityVersion version) => version.IsGreaterEqual(5, 3);
		/// <summary>
		/// 5.6.0b3 and greater
		/// </summary>
		public static bool HasDrawMode(UnityVersion version) => version.IsGreaterEqual(5, 6, 0, UnityVersionType.Beta, 3);
		/// <summary>
		/// 5.6.1p2 to 5.6.x
		/// 2017.1.0b5 and greater
		/// </summary>
		public static bool HasWasSpriteAssigned(UnityVersion version)
		{
			if (version.IsGreaterEqual(2017))
			{
				return version.IsGreaterEqual(2017, 1, 0, UnityVersionType.Beta, 5);
			}
			else
			{
				return version.IsGreaterEqual(5, 6, 1, UnityVersionType.Patch, 2);
			}
		}
		/// <summary>
		/// 2017.1 and greater
		/// </summary>
		public static bool HasMaskInteraction(UnityVersion version) => version.IsGreaterEqual(2017);
		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasSpriteSortPoint(UnityVersion version) => version.IsGreaterEqual(2018, 2);

		/// <summary>
		/// 4.5.0 and greater
		/// </summary>
		private static bool IsAlignColorRGBAf(UnityVersion version) => version.IsGreaterEqual(4, 5);
		/// <summary>
		/// 5.4.0 and greater
		/// </summary>
		private static bool IsAlignFlip(UnityVersion version) => version.IsGreaterEqual(5, 4);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Sprite.Read(reader);
			Color.Read(reader);
			if (IsAlignColorRGBAf(reader.Version))
			{
				reader.AlignStream();
			}

			if (HasFlip(reader.Version))
			{
				FlipX = reader.ReadBoolean();
				FlipY = reader.ReadBoolean();
				if (IsAlignFlip(reader.Version))
				{
					reader.AlignStream();
				}
			}

			if (HasDrawMode(reader.Version))
			{
				DrawMode = (SpriteDrawMode)reader.ReadInt32();
				Size.Read(reader);
				AdaptiveModeThreshold = reader.ReadSingle();
				SpriteTileMode = (SpriteTileMode)reader.ReadInt32();
			}
			if (HasWasSpriteAssigned(reader.Version))
			{
				WasSpriteAssigned = reader.ReadBoolean();
				reader.AlignStream();
			}
			if (HasMaskInteraction(reader.Version))
			{
				MaskInteraction = (SpriteMaskInteraction)reader.ReadInt32();
			}
			if (HasSpriteSortPoint(reader.Version))
			{
				SpriteSortPoint = (SpriteSortPoint)reader.ReadInt32();
			}
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			yield return context.FetchDependency(Sprite, SpriteName);
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			node.Add(SpriteName, Sprite.ExportYAML(container));
			node.Add(ColorName, Color.ExportYAML(container));
			node.Add(FlipXName, FlipX);
			node.Add(FlipYName, FlipY);
			node.Add(DrawModeName, (int)DrawMode);
			node.Add(SizeName, (HasDrawMode(container.Version) ? Size : Vector2f.One).ExportYAML(container));
			node.Add(AdaptiveModeThresholdName, AdaptiveModeThreshold);
			node.Add(SpriteTileModeName, (int)SpriteTileMode);
			node.Add(WasSpriteAssignedName, WasSpriteAssigned);
			node.Add(MaskInteractionName, (int)MaskInteraction);
			if (HasSpriteSortPoint(container.ExportVersion))
			{
				node.Add(SpriteSortPointName, (int)SpriteSortPoint);
			}
			return node;
		}

		public bool FlipX { get; set; }
		public bool FlipY { get; set; }
		public SpriteDrawMode DrawMode { get; set; }
		public float AdaptiveModeThreshold { get; set; }
		public SpriteTileMode SpriteTileMode { get; set; }
		public bool WasSpriteAssigned { get; set; }
		public SpriteMaskInteraction MaskInteraction { get; set; }
		public SpriteSortPoint SpriteSortPoint { get; set; }

		public const string SpriteName = "m_Sprite";
		public const string ColorName = "m_Color";
		public const string FlipXName = "m_FlipX";
		public const string FlipYName = "m_FlipY";
		public const string DrawModeName = "m_DrawMode";
		public const string SizeName = "m_Size";
		public const string AdaptiveModeThresholdName = "m_AdaptiveModeThreshold";
		public const string SpriteTileModeName = "m_SpriteTileMode";
		public const string WasSpriteAssignedName = "m_WasSpriteAssigned";
		public const string MaskInteractionName = "m_MaskInteraction";
		public const string SpriteSortPointName = "m_SpriteSortPoint";

		public PPtr<Sprite.Sprite> Sprite = new();
		public ColorRGBAf Color = new();
		public Vector2f Size = new();
	}
}
