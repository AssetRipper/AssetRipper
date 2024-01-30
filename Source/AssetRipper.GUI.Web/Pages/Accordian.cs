namespace AssetRipper.GUI.Web.Pages;

internal static class Accordian
{
	public static void Write(TextWriter writer, ReadOnlySpan<AccordianItem> items, Guid parentId = default)
	{
		string parentIdString = (parentId == default ? Guid.NewGuid() : parentId).ToString();

		using (new Div(writer).WithClass("accordion").WithId(parentIdString).End())
		{
			for (int i = 0; i < items.Length; i++)
			{
				AccordianItem item = items[i];
				using (new Div(writer).WithClass("accordion-item").End())
				{
					using (new H2(writer).WithClass("accordion-header").End())
					{
						new Button(writer).WithClass("accordion-button collapsed")
							.WithType("button")
							.WithCustomAttribute("data-bs-toggle", "collapse")
							.WithCustomAttribute("data-bs-target", $"#collapse-{parentIdString}-{i}")
							.WithCustomAttribute("aria-expanded", "false")
							.WithCustomAttribute("aria-controls", $"collapse-{parentIdString}-{i}")
							.Close(item.Name ?? (i + 1).ToString());
					}
					using (new Div(writer).WithId($"collapse-{parentIdString}-{i}")
						.WithClass("accordion-collapse collapse")
						.WithCustomAttribute("data-bs-parent", $"#{parentIdString}")
						.End())
					{
						using (new Div(writer).WithClass("accordion-body").End())
						{
							item.Write(writer);
						}
					}
				}
			}
		}
	}
}
