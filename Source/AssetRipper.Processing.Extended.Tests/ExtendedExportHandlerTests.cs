using AssetRipper.Export.Configuration;
using AssetRipper.Export.UnityProjects;
using AssetRipper.Processing.Extended.PathOverrides;

namespace AssetRipper.Processing.Extended.Tests;

public class ExtendedExportHandlerTests
{
	[Test]
	public void IsNotTheBaseTypeSoThePremiumUiGatesUnlock()
	{
		// GameFileLoader.Premium is `ExportHandler.GetType() != typeof(ExportHandler)`.
		ExportHandler handler = new ExtendedExportHandler(new FullConfiguration());

		Assert.That(handler.GetType(), Is.Not.EqualTo(typeof(ExportHandler)));
	}

	[Test]
	public void RegistersThePathOverridesConfigurationSlot()
	{
		FullConfiguration settings = new();

		_ = new ExtendedExportHandler(settings);

		Assert.That(settings.SingletonData.Keys, Contains.Item(ExtendedExportHandler.PathOverridesKey));
	}

	[Test]
	public void PathOverridesStartEmpty()
	{
		FullConfiguration settings = new();

		_ = new ExtendedExportHandler(settings);

		Assert.That(settings.SingletonData.TryGetStoredValue(ExtendedExportHandler.PathOverridesKey, out PathOverrideData? data), Is.True);
		Assert.That(data!.Files, Is.Empty);
	}
}
