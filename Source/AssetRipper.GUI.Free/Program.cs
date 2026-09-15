using AssetRipper.GUI.Web;
using AssetRipper.Processing.Extended;

GameFileLoader.ExportHandler = new ExtendedExportHandler(GameFileLoader.Settings);

WebApplicationLauncher.Launch(args);
