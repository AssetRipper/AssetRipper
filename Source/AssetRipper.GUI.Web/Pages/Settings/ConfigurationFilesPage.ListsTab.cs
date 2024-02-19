using AssetRipper.GUI.Web.Paths;
using AssetRipper.Import.Configuration;

namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class ConfigurationFilesPage
{
	private sealed class ListsTab : DataStorageTab
	{
		public static ListsTab Instance { get; } = new();

		public override string DisplayName => Localization.ConfigurationFilesLists;

		protected override IEnumerable<HtmlTab> GetTabs()
		{
			ListDataStorage dataStorage = GameFileLoader.Settings.ListData;
			foreach (string key in dataStorage.Keys)
			{
				yield return new FileListTab(key, dataStorage[key]);
			}
		}

		private sealed class FileListTab(string key, DataSet? list) : HtmlTab
		{
			public override string DisplayName => key;
			public override void Write(TextWriter writer)
			{
				if (list is null or { Count: 0 })
				{
					using (new Div(writer).WithTextCenter().End())
					{
						new P(writer).WithClass("p-2").Close(Localization.NoDataHasBeenLoadedForThisKey);
						using (new Form(writer).WithAction("/ConfigurationFiles/List/Add").WithMethod("post").End())
						{
							new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
							new Input(writer).WithType("submit").WithClass("btn btn-primary").WithValue(Localization.Load.ToHtml()).Close();
						}
					}
				}
				else
				{
					FileAccordianItem[] items = new FileAccordianItem[list.Count];
					for (int i = 0; i < list.Count; i++)
					{
						items[i] = new FileAccordianItem(key, i, list.Strings[i]);
					}
					Accordian.Write(writer, items);
					using (new Div(writer).WithTextCenter().End())
					{
						using (new Form(writer).WithAction("/ConfigurationFiles/List/Add").WithMethod("post").End())
						{
							new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
							new Input(writer).WithType("submit").WithClass("btn btn-secondary").WithValue("+").Close();
						}
					}
				}
			}

			private sealed class FileAccordianItem(string key, int index, string text) : AccordianItem
			{
				public override void Write(TextWriter writer)
				{
					new Pre(writer).WithClass("bg-dark-subtle rounded-3 p-2").Close(text);
					using (new Div(writer).WithClass("row text-center").End())
					{
						using (new Div(writer).WithClass("col").End())
						{
							using (new Form(writer).WithAction("/ConfigurationFiles/List/Replace").WithMethod("post").End())
							{
								new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
								new Input(writer).WithType("hidden").WithName("Index").WithValue(index.ToString()).Close();
								new Input(writer).WithType("submit").WithClass("btn btn-primary mx-1").WithValue(Localization.Replace.ToHtml()).Close();
							}
						}
						using (new Div(writer).WithClass("col").End())
						{
							using (new Form(writer).WithAction("/ConfigurationFiles/List/Remove").WithMethod("post").End())
							{
								new Input(writer).WithType("hidden").WithName("Key").WithValue(key.ToHtml()).Close();
								new Input(writer).WithType("hidden").WithName("Index").WithValue(index.ToString()).Close();
								new Input(writer).WithType("submit").WithClass("btn btn-danger mx-1").WithValue(Localization.Remove.ToHtml()).Close();
							}
						}
					}
				}
			}
		}
	}
}
