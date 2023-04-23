using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace AssetRipper.GUI.Components
{
	public abstract class BaseConfigurationDropdown<T> : UserControlWithPropChange where T : struct, Enum
	{
		private ItemWrapper? _selectedValue;

		public static readonly StyledProperty<List<ItemWrapper>> ValuesProperty =
			AvaloniaProperty.Register<BaseConfigurationDropdown<T>, List<ItemWrapper>>(nameof(Values));

		public static DirectProperty<BaseConfigurationDropdown<T>, ItemWrapper?> RawSelectedValueProperty =
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, ItemWrapper?>(nameof(RawSelectedValue), obj => obj.RawSelectedValue, (obj, val) => obj.RawSelectedValue = val);

		public static readonly DirectProperty<BaseConfigurationDropdown<T>, T> SelectedValueProperty =
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, T>(nameof(SelectedValue), obj => obj.SelectedValue, (obj, val) => obj.SelectedValue = val);

		public static readonly StyledProperty<string> OptionTitleProperty =
			AvaloniaProperty.Register<BaseConfigurationDropdown<T>, string>(nameof(OptionTitle), defaultValue: $"<Missing Title##{typeof(T).Name}>");

		public static readonly StyledProperty<string?> SelectedValueDescriptionProperty =
			AvaloniaProperty.Register<BaseConfigurationDropdown<T>, string?>(nameof(SelectedValueDescription));

		public List<ItemWrapper> Values
		{
			get => GetValue(ValuesProperty);
			init => SetValue(ValuesProperty, value);
		}

		public string OptionTitle
		{
			get => GetValue(OptionTitleProperty);
			set => SetValue(OptionTitleProperty, value);
		}

		public ItemWrapper? RawSelectedValue
		{
			get => _selectedValue!;
			set
			{
				BaseConfigurationDropdown<T>.ItemWrapper? oldValue = _selectedValue;
				_selectedValue = value;
				OnPropertyChanged();
				RaisePropertyChanged(RawSelectedValueProperty, oldValue, value);
				RaisePropertyChanged(SelectedValueProperty, oldValue?.Item ?? default, value?.Item ?? default);
				SelectedValueDescription = GetValueDescription(value?.Item ?? default);
			}
		}

		public string? SelectedValueDescription
		{
			get => GetValue(SelectedValueDescriptionProperty);
			set => SetValue(SelectedValueDescriptionProperty, value);
		}

		public T SelectedValue
		{
			get => RawSelectedValue?.Item ?? default;
			set => RawSelectedValue = new(value, GetValueDisplayName(value));
		}

		public BaseConfigurationDropdown()
		{
			Values = Enum.GetValues<T>().Select(e => new ItemWrapper(e, GetValueDisplayName(e))).ToList();
			InitializeComponent();

			MainWindow.Instance.LocalizationManager.OnLanguageChanged += () =>
			{
				//Reload all localized strings
				Values.ForEach(v => v.DisplayName = GetValueDisplayName(v.Item));
				base.OnPropertyChanged(nameof(BaseConfigurationDropdown<T>.Values));

				T selectedValue = RawSelectedValue?.Item ?? default;
				RawSelectedValue = new(selectedValue, GetValueDisplayName(selectedValue));
			};
		}

		private void InitializeComponent()
		{
			Content = AvaloniaXamlLoader.Load(new Uri("avares://AssetRipper/Components/BaseConfigurationDropdown.axaml"));
			((UserControl)Content).DataContext = this;
		}

		protected virtual string GetValueDisplayName(T value) => Enum.GetName(typeof(T), value) ?? throw new($"Value {value} is not defined in the enum {typeof(T)}, but has ended up in a dropdown?");

		protected virtual string? GetValueDescription(T value) => null;

		public sealed record class ItemWrapper
		{
			public T Item { get; set; }
			public string DisplayName { get; set; }

			public ItemWrapper(T item, string displayName)
			{
				Item = item;
				DisplayName = displayName;
			}

			public override string ToString() => DisplayName;
		}
	}
}
