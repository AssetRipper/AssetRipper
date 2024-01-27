namespace AssetRipper.GUI.Web.Pages;

internal abstract class HtmlTab
{
	private string? guid;
	public abstract string DisplayName { get; }
	public virtual string HtmlName
	{
		get
		{
			return guid ??= Guid.NewGuid().ToString();
		}
	}
	public virtual bool Enabled => true;
	public abstract void Write(TextWriter writer);

	/// <summary>
	/// Write a tabbed navigation bar.
	/// </summary>
	/// <remarks>
	/// This should be used in conjunction with <see cref="WriteContent(TextWriter, ReadOnlySpan{HtmlTab})"/>.
	/// </remarks>
	/// <param name="writer">The text writer.</param>
	/// <param name="tabs">The tabs to write.</param>
	/// <param name="navClass">The list of classes to be used on the nav element.</param>
	public static void WriteNavigation(TextWriter writer, ReadOnlySpan<HtmlTab> tabs, string navClass = "nav nav-tabs")
	{
		using (new Nav(writer).WithClass(navClass).WithId("nav-tab").WithRole("tablist").End())
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				HtmlTab tab = tabs[i];
				Button button = new Button(writer)
					.WithClass(GetTabClassSet(tab, i))
					.WithId($"nav-{tab.HtmlName}-tab")
					.WithCustomAttribute("data-bs-toggle", "tab")
					.WithCustomAttribute("data-bs-target", $"#nav-{tab.HtmlName}")
					.WithType("button")
					.WithRole("tab")
					.WithCustomAttribute("aria-controls", $"nav-{tab.HtmlName}")
					.WithCustomAttribute("aria-selected", "true");

				if (!tab.Enabled)
				{
					button.WithCustomAttribute("aria-disabled", "true");
				}

				button.Close(tab.DisplayName);
			}
		}

		static string GetTabClassSet(HtmlTab tab, int index)
		{
			const string DefaultTabClassSet = "nav-link";
			if (tab.Enabled)
			{
				return index == 0 ? DefaultTabClassSet + " active" : DefaultTabClassSet;
			}
			return DefaultTabClassSet + " disabled";
		}
	}

	/// <summary>
	/// Write the content for a tabbed navigation bar.
	/// </summary>
	/// <remarks>
	/// This should be used in conjunction with <see cref="WriteNavigation(TextWriter, ReadOnlySpan{HtmlTab}, string)"/>.
	/// </remarks>
	/// <param name="writer">The text writer.</param>
	/// <param name="tabs">The tabs to write.</param>
	public static void WriteContent(TextWriter writer, ReadOnlySpan<HtmlTab> tabs)
	{
		using (new Div(writer).WithClass("tab-content").End())
		{
			for (int i = 0; i < tabs.Length; i++)
			{
				HtmlTab tab = tabs[i];
				using (new Div(writer)
					.WithClass(i == 0 && tab.Enabled ? "tab-pane fade show active" : "tab-pane fade")
					.WithId($"nav-{tab.HtmlName}")
					.WithRole("tabpanel")
					.WithCustomAttribute("aria-labelledby", $"nav-{tab.HtmlName}-tab")
					.End())
				{
					if (tab.Enabled)
					{
						tab.Write(writer);
					}
				}
			}
		}
	}
}
