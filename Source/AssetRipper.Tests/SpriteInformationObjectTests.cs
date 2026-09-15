using AssetRipper.Assets.Bundles;
using AssetRipper.Assets.Collections;
using AssetRipper.Primitives;
using AssetRipper.Processing.Textures;
using AssetRipper.SourceGenerated.Classes.ClassID_213;
using AssetRipper.SourceGenerated.Classes.ClassID_28;
using AssetRipper.SourceGenerated.Classes.ClassID_687078895;
using AssetRipper.SourceGenerated.Extensions;

namespace AssetRipper.Tests;

public class SpriteInformationObjectTests
{
	private static readonly UnityVersion Version = new(2020, 1);

	private static SpriteInformationObject CreateGroup(ProcessedAssetCollection collection, ITexture2D texture)
	{
		return collection.CreateAsset(-1, (assetInfo) => new SpriteInformationObject(assetInfo, texture));
	}

	/// <summary>
	/// A sprite atlas commonly spans several texture pages. Each page gets its own
	/// SpriteInformationObject, so more than one group references the same atlas.
	/// </summary>
	[Test]
	public void AtlasSpanningTwoTexturePagesDoesNotTripTheMainAssetAssertion()
	{
		ProcessedAssetCollection collection = new GameBundle().AddNewProcessedCollection("Collection", Version);

		ISpriteAtlas atlas = collection.CreateSpriteAtlas();
		ITexture2D pageOne = collection.CreateTexture2D();
		ITexture2D pageTwo = collection.CreateTexture2D();
		ISprite spriteOne = collection.CreateSprite();
		ISprite spriteTwo = collection.CreateSprite();

		SpriteInformationObject groupOne = CreateGroup(collection, pageOne);
		SpriteInformationObject groupTwo = CreateGroup(collection, pageTwo);
		groupOne.AddToDictionary(spriteOne, atlas);
		groupTwo.AddToDictionary(spriteTwo, atlas);

		groupOne.SetMainAsset();

		Assert.DoesNotThrow(groupTwo.SetMainAsset);
	}

	[Test]
	public void GroupStillOwnsItsTextureAndSprites()
	{
		ProcessedAssetCollection collection = new GameBundle().AddNewProcessedCollection("Collection", Version);

		ISpriteAtlas atlas = collection.CreateSpriteAtlas();
		ITexture2D texture = collection.CreateTexture2D();
		ISprite sprite = collection.CreateSprite();

		SpriteInformationObject group = CreateGroup(collection, texture);
		group.AddToDictionary(sprite, atlas);
		group.SetMainAsset();

		Assert.Multiple(() =>
		{
			Assert.That(texture.MainAsset, Is.SameAs(group));
			Assert.That(sprite.MainAsset, Is.SameAs(group));
			// The atlas is shared, so no single group may claim it.
			Assert.That(atlas.MainAsset, Is.Null);
		});
	}

	[Test]
	public void AtlasIsStillReportedAsADependency()
	{
		ProcessedAssetCollection collection = new GameBundle().AddNewProcessedCollection("Collection", Version);

		ISpriteAtlas atlas = collection.CreateSpriteAtlas();
		ITexture2D texture = collection.CreateTexture2D();
		ISprite sprite = collection.CreateSprite();

		SpriteInformationObject group = CreateGroup(collection, texture);
		group.AddToDictionary(sprite, atlas);

		Assert.That(group.FetchDependencies().Any(d => d.Item1.Contains("Value")), Is.True);
	}
}
