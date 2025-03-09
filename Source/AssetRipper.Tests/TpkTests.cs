using AssetRipper.SourceGenerated;
using AssetRipper.Tpk;
using AssetRipper.Tpk.EngineAssets;
using AssetRipper.Tpk.TypeTrees;

namespace AssetRipper.Tests;

internal class TpkTests
{
	[Test]
	public void EngineAssetsTpkSuccessfullyExtracts()
	{
		TpkFile tpk = TpkFile.FromStream(EngineAssetsTpk.GetStream());
		TpkEngineAssetsBlob blob = (TpkEngineAssetsBlob)tpk.GetDataBlob();
		Assert.That(blob.Data, Is.Not.Empty);
	}

	[Test]
	public void TypeTreeTpkSuccessfullyExtracts()
	{
		TpkFile tpk = TpkFile.FromStream(SourceTpk.GetStream());
		TpkTypeTreeBlob blob = (TpkTypeTreeBlob)tpk.GetDataBlob();
		Assert.That(blob.ClassInformation, Is.Not.Empty);
	}
}
