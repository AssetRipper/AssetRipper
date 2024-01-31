namespace AssetRipper.GUI.Web.Pages.Settings;

public sealed partial class ConfigurationFilesPage
{
	private abstract class DataStorageTab : HtmlTab
	{
		public sealed override void Write(TextWriter writer)
		{
			HtmlTab[] tabs = GetTabs().ToArray();
			using (new Div(writer).WithClass("container").End())
			{
				if (tabs.Length == 0)
				{
					new P(writer).WithClass("text-center p-2").Close(Localization.ThereAreNoKeysForThisDataType);
				}
				else
				{
					using (new Div(writer).WithClass("row").End())
					{
						using (new Div(writer).WithClass("col-3 text-center").End())
						{
							WriteNavigation(writer, tabs, "nav nav-pills flex-column");
						}
						using (new Div(writer).WithClass("col-9").End())
						{
							WriteContent(writer, tabs);
						}
					}
				}
			}
		}

		protected abstract IEnumerable<HtmlTab> GetTabs();
	}
}
