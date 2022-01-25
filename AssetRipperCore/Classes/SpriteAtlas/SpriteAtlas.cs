using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using AssetRipper.Core.YAML.Extensions;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.SpriteAtlas
{
	public sealed class SpriteAtlas : NamedObject, ISpriteAtlas
	{
		public SpriteAtlas(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Not Release
		/// </summary>
		public static bool HasEditorData(TransferInstructionFlags flags) => !flags.IsRelease();
		/// <summary>
		/// Release
		/// </summary>
		public static bool HasRenderDataMap(TransferInstructionFlags flags) => flags.IsRelease();

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			if (HasEditorData(reader.Flags))
			{
				EditorData = reader.ReadAsset<SpriteAtlasEditorData>();
				MasterAtlas.Read(reader);
			}
			PackedSprites = reader.ReadAssetArray<PPtr<Sprite.Sprite>>();
			PackedSpriteNamesToIndex = reader.ReadStringArray();
			if (HasRenderDataMap(reader.Flags))
			{
				RenderDataMap.Read(reader);
			}
			Tag = reader.ReadString();
			IsVariant = reader.ReadBoolean();
			reader.AlignStream();
		}

		public override IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			foreach (PPtr<IUnityObjectBase> asset in base.FetchDependencies(context))
			{
				yield return asset;
			}

			if (HasEditorData(context.Flags))
			{
				foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromDependent(EditorData, EditorDataName))
				{
					yield return asset;
				}
				yield return context.FetchDependency(MasterAtlas, MasterAtlasName);
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependencies(PackedSprites, PackedSpritesName))
			{
				yield return asset;
			}
			foreach (PPtr<IUnityObjectBase> asset in context.FetchDependenciesFromArray((IEnumerable<SpriteAtlasData>)RenderDataMap.Values, RenderDataMapName))
			{
				yield return asset;
			}
		}

		protected override YAMLMappingNode ExportYAMLRoot(IExportContainer container)
		{
			YAMLMappingNode node = base.ExportYAMLRoot(container);
			if (HasEditorData(container.ExportFlags))
			{
				node.Add(EditorDataName, GetEditorData(container.Flags).ExportYAML(container));
				node.Add(MasterAtlasName, MasterAtlas.ExportYAML(container));
			}
			node.Add(PackedSpritesName, PackedSprites.ExportYAML(container));
			node.Add(PackedSpriteNamesToIndexName, PackedSpriteNamesToIndex.ExportYAML());
			if (HasRenderDataMap(container.ExportFlags))
			{
				node.Add(RenderDataMapName, RenderDataMap.ExportYAML(container));
			}
			node.Add(TagName, Tag);
			node.Add(IsVariantName, IsVariant);
			return node;
		}

		public SpriteAtlasEditorData GetEditorData(TransferInstructionFlags flags)
		{
			if (HasEditorData(flags))
			{
				return EditorData;
			}
			return new SpriteAtlasEditorData(PackedSprites);
		}

		public override string ExportExtension => "spriteatlas";

		public SpriteAtlasEditorData EditorData { get; set; }
		public PPtr<Sprite.Sprite>[] PackedSprites { get; set; }
		public string[] PackedSpriteNamesToIndex { get; set; }
		public Dictionary<Tuple<UnityGUID, long>, SpriteAtlasData> RenderDataMap { get; set; } = new Dictionary<Tuple<UnityGUID, long>, SpriteAtlasData>();
		public string Tag { get; set; }
		public bool IsVariant { get; set; }

		public const string EditorDataName = "m_EditorData";
		public const string MasterAtlasName = "m_MasterAtlas";
		public const string PackedSpritesName = "m_PackedSprites";
		public const string PackedSpriteNamesToIndexName = "m_PackedSpriteNamesToIndex";
		public const string RenderDataMapName = "m_RenderDataMap";
		public const string TagName = "m_Tag";
		public const string IsVariantName = "m_IsVariant";

		public PPtr<SpriteAtlas> MasterAtlas;
	}
}
