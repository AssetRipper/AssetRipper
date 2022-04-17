namespace AssetRipper.Core.Importers
{
	public static class ImporterVersionHandler
	{
		private static IAssetImporterFactory legacyImporter;

		public static void SetLegacyImporter(IAssetImporterFactory importerFactory)
		{
			legacyImporter = importerFactory;
		}

		public static IAssetImporterFactory GetImporterFactory(UnityVersion version)
		{
			return legacyImporter;
		}
	}
}
