using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace AssetRipper.GUI
{
	public class LocalizeExtension : MarkupExtension
	{
		public LocalizeExtension(string key)
		{
			this.Key = key;
		}

		public string Key { get; set; }

		public string? Context { get; set; }

		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			string keyToUse = Key;
			if (!string.IsNullOrWhiteSpace(Context))
			{
				keyToUse = $"{Context}/{Key}";
			}

			ReflectionBindingExtension binding = new ReflectionBindingExtension($"[{keyToUse}]")
			{
				Mode = BindingMode.OneWay,
				Source = MainWindow.Instance.LocalizationManager,
			};

			return binding.ProvideValue(serviceProvider);
		}
	}
}