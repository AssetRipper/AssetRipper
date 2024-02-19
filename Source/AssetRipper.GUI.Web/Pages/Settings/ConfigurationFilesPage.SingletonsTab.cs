using AssetRipper.GUI.Web.Paths;
using AssetRipper.Import.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class ConfigurationFilesPage
{
	private sealed class SingletonsTab : DataStorageTab
	{
		public static SingletonsTab Instance { get; } = new();

		public override string DisplayName => Localization.ConfigurationFilesSingletons;

		protected override IEnumerable<HtmlTab> GetTabs()
		{
			SingletonDataStorage dataStorage = GameFileLoader.Settings.SingletonData;
			foreach (string key in dataStorage.Keys)
			{
				yield return new FileTab(key, dataStorage[key]?.Text);
			}
		}

		private sealed class FileTab(string key, string? text) : HtmlTab
		{
			public override string DisplayName => key;
			public override void Write(TextWriter writer)
			{
				if (string.IsNullOrEmpty(text))
				{
					using (new Div(writer).WithTextCenter().End())
					{
						new P(writer).WithClass("p-2").Close(Localization.NoDataHasBeenLoadedForThisKey);
						using (new Form(writer).WithAction("/ConfigurationFiles/Singleton/Add").WithMethod("post").End())
						{
							new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
							new Input(writer).WithType("submit").WithClass("btn btn-primary mx-1").WithValue(Localization.Load.ToHtml()).Close();
						}
					}
				}
				else
				{
					new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(text);
					using (new Div(writer).WithClass("row text-center").End())
					{
						using (new Div(writer).WithClass("col").End())
						{
							using (new Form(writer).WithAction("/ConfigurationFiles/Singleton/Add").WithMethod("post").End())
							{
								new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
								new Input(writer).WithType("submit").WithClass("btn btn-primary mx-1").WithValue(Localization.Replace.ToHtml()).Close();
							}
						}
						using (new Div(writer).WithClass("col").End())
						{
							using (new Form(writer).WithAction("/ConfigurationFiles/Singleton/Remove").WithMethod("post").End())
							{
								new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
								new Input(writer).WithType("submit").WithClass("btn btn-danger mx-1").WithValue(Localization.Remove.ToHtml()).Close();
							}
						}
					}
				}
			}
		}
	}
}
