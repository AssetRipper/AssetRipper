using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Import.Structure.Assembly.Managers;
using AssetRipper.Primitives;
using AssetRipper.Processing;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

public class SpriteProcessorTests
{
	[Test]
	public void DirectSpriteAtlasIsPreferredWhenSpriteIsPackedIntoMultipleAtlases()
	{
		ProcessedAssetCollection collection = AssetCreator.CreateCollection(UnityVersion.V_2020);
		ITexture2D texture = collection.CreateTexture2D();
		ISprite sprite = collection.CreateSprite();
		ISpriteAtlas expectedAtlas = collection.CreateSpriteAtlas();
		ISpriteAtlas duplicateAtlas = collection.CreateSpriteAtlas();

		sprite.RD.Texture.SetAsset(collection, texture);
		sprite.SpriteAtlasP = expectedAtlas;
		AddSprite(expectedAtlas, sprite, texture);
		AddSprite(duplicateAtlas, sprite, texture);

		GameData gameData = new((GameBundle)collection.Bundle, collection.Version, new BaseManager(_ => { }), null);
		Assert.DoesNotThrow(() => new SpriteProcessor().Process(gameData));

		SpriteInformationObject information = (SpriteInformationObject)texture.MainAsset!;
		Assert.That(information.Sprites[sprite], Is.SameAs(expectedAtlas));
	}

	private static void AddSprite(ISpriteAtlas atlas, ISprite sprite, ITexture2D texture)
	{
		atlas.PackedSpritesP.Add(sprite);
		atlas.RenderDataMap.AddNew().Value.Texture.SetAsset(atlas.Collection, texture);
	}
}
