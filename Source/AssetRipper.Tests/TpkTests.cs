using AssetRipper.SourceGenerated;
using AssetRipper.Tpk;
using AssetRipper.Tpk.EngineAssets;
using AssetRipper.Tpk.TypeTrees;

namespace AssetRipper.Tests;

internal class TpkTests
{
	[Test]
	public async Task EngineAssetsTpkSuccessfullyExtracts()
	{
		TpkFile tpk = TpkFile.FromStream(EngineAssetsTpk.GetStream());
		TpkEngineAssetsBlob blob = (TpkEngineAssetsBlob)tpk.GetDataBlob();
		await Assert.That(blob.Data).IsNotEmpty();
	}

	[Test]
	public async Task TypeTreeTpkSuccessfullyExtracts()
	{
		TpkFile tpk = TpkFile.FromStream(SourceTpk.GetStream());
		TpkTypeTreeBlob blob = (TpkTypeTreeBlob)tpk.GetDataBlob();
		await Assert.That(blob.ClassInformation).IsNotEmpty();
	}
}