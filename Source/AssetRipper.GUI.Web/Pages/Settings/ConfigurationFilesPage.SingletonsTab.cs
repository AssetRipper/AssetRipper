using AssetRipper.Export.UnityProjects.Configuration;
using AssetRipper.GUI.Web.Paths;

namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class ConfigurationFilesPage
{
	private sealed class SingletonsTab : DataStorageTab
	{
		public static SingletonsTab Instance { get; } = new();

		public override string DisplayName => "Singletons";

		protected override IEnumerable<HtmlTab> GetTabs()
		{
			SingletonDataStorage dataStorage = GameFileLoader.Settings.SingletonData;
			foreach (string key in dataStorage.KnownKeys)
			{
				yield return new FileTab(key, dataStorage[key]);
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
						new P(writer).WithClass("p-2").Close("No data has been loaded for this key.");
						using (new Form(writer).WithAction("/ConfigurationFiles/Singleton/Add").WithMethod("post").End())
						{
							new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
							new Input(writer).WithType("submit").WithClass("btn btn-primary mx-1").WithValue("Load").Close();
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
								new Input(writer).WithType("submit").WithClass("btn btn-primary mx-1").WithValue("Replace").Close();
							}
						}
						using (new Div(writer).WithClass("col").End())
						{
							using (new Form(writer).WithAction("/ConfigurationFiles/Singleton/Remove").WithMethod("post").End())
							{
								new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
								new Input(writer).WithType("submit").WithClass("btn btn-danger mx-1").WithValue("Remove").Close();
							}
						}
					}
				}
			}
		}
	}
}
