using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.SpriteRenderer;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.SpriteMask
{
	public sealed class SpriteMask : Renderer.Renderer
	{
		public SpriteMask(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// 2019.3.0a3 and greater
		/// </summary>
		public static bool HasFrontSortingLayerID(UnityVersion version) => version.IsGreaterEqual(2019, 3, 0, UnityVersionType.Alpha, 3);

		/// <summary>
		/// 2019.3.0a3 and greater
		/// </summary>
		public static bool HasBackSortingLayerID(UnityVersion version) => version.IsGreaterEqual(2019, 3, 0, UnityVersionType.Alpha, 3);

		/// <summary>
		/// 2018.2 and greater
		/// </summary>
		public static bool HasSpriteSortPoint(UnityVersion version) => version.IsGreaterEqual(2018, 2);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			Sprite.Read(reader);
			MaskAlphaCutoff = reader.ReadSingle();
			if (HasFrontSortingLayerID(reader.Version))
			{
				FrontSortingLayerID = reader.ReadInt32();
			}
			if (HasBackSortingLayerID(reader.Version))
			{
				BackSortingLayerID = reader.ReadInt32();
			}

			FrontSortingLayer = reader.ReadInt16();
			BackSortingLayer = reader.ReadInt16();
			FrontSortingOrder = reader.ReadInt16();
			BackSortingOrder = reader.ReadInt16();

			IsCustomRangeActive = reader.ReadBoolean();
			reader.AlignStream();

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
			node.Add(MaskAlphaCutoffName, MaskAlphaCutoff);
			// Editor always exposes the sorting layer IDs,
			// however they weren't exposed to the Player until 2019.3.0a3.
			node.Add(FrontSortingLayerID, FrontSortingLayerID);
			node.Add(BackSortingLayerID, BackSortingLayerID);
			node.Add(FrontSortingLayerName, FrontSortingLayer);
			node.Add(BackSortingLayerName, BackSortingLayer);
			node.Add(FrontSortingOrderName, FrontSortingOrder);
			node.Add(BackSortingLayerName, BackSortingOrder);
			node.Add(IsCustomRangeActiveName, IsCustomRangeActive);
			if (HasSpriteSortPoint(container.ExportVersion))
			{
				node.Add(SpriteSortPointName, (int)SpriteSortPoint);
			}
			return node;
		}

		public float MaskAlphaCutoff { get; set; }
		public int FrontSortingLayerID { get; set; }
		public int BackSortingLayerID { get; set; }
		public short FrontSortingLayer { get; set; }
		public short BackSortingLayer { get; set; }
		public short FrontSortingOrder { get; set; }
		public short BackSortingOrder { get; set; }
		public bool IsCustomRangeActive { get; set; }
		public SpriteSortPoint SpriteSortPoint { get; set; }

		public const string SpriteName = "m_Sprite";
		public const string MaskAlphaCutoffName = "m_MaskAlphaCutoff";
		public const string FrontSortingLayerIDName = "m_FrontSortingLayerID";
		public const string BackSortingLayerIDName = "m_BackSortingLayerID";
		public const string FrontSortingLayerName = "m_FrontSortingLayer";
		public const string BackSortingLayerName = "m_BackSortingLayer";
		public const string FrontSortingOrderName = "m_FrontSortingOrder";
		public const string BackSortingOrderName = "m_BackSortingOrder";
		public const string IsCustomRangeActiveName = "m_IsCustomRangeActive";
		public const string SpriteSortPointName = "m_SpriteSortPoint";

		public PPtr<Sprite.Sprite> Sprite = new();
	}
}
