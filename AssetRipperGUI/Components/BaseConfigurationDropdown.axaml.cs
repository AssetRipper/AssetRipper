using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.GUI.Components
{
	public abstract class BaseConfigurationDropdown<T> : UserControlWithPropChange where T : struct, Enum
	{
		private string _optionTitle = $"<Missing Title##{typeof(T).Name}>";
		private ItemWrapper? _selectedValue;
		private string? _selectedValueDesc;

		public static DirectProperty<BaseConfigurationDropdown<T>, List<ItemWrapper>> ValuesProperty =
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, List<ItemWrapper>>(nameof(Values), obj => obj.Values);

		public static DirectProperty<BaseConfigurationDropdown<T>, ItemWrapper> RawSelectedValueProperty =
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, ItemWrapper>(nameof(RawSelectedValue), obj => obj.RawSelectedValue, (obj, val) => obj.RawSelectedValue = val);

		public static DirectProperty<BaseConfigurationDropdown<T>, T> SelectedValueProperty =
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, T>(nameof(SelectedValue), obj => obj.SelectedValue, (obj, val) => obj.SelectedValue = val);

		public static DirectProperty<BaseConfigurationDropdown<T>, string> OptionTitleProperty =
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, string>(nameof(OptionTitle), obj => obj.OptionTitle, (obj, val) => obj.OptionTitle = val);

		public static DirectProperty<BaseConfigurationDropdown<T>, string?> SelectedValueDescriptionProperty =
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, string?>(nameof(SelectedValueDescription), obj => obj.SelectedValueDescription, (obj, val) => obj.SelectedValueDescription = val);

		public List<ItemWrapper> Values { get; }

		public string OptionTitle
		{
			get => _optionTitle;
			set
			{
				string oldValue = _optionTitle;
				_optionTitle = value;
				OnPropertyChanged();
				RaisePropertyChanged(OptionTitleProperty, oldValue, value);
			}
		}

		public ItemWrapper RawSelectedValue
		{
			get => _selectedValue!;
			set
			{
				BaseConfigurationDropdown<T>.ItemWrapper? oldValue = _selectedValue;
				_selectedValue = value;
				OnPropertyChanged();
				RaisePropertyChanged(RawSelectedValueProperty, oldValue, value);
				RaisePropertyChanged(SelectedValueProperty, oldValue == null ? Optional<T>.Empty : oldValue.Item, value.Item);
				SelectedValueDescription = GetValueDescription(value.Item);
			}
		}

		public string? SelectedValueDescription
		{
			get => _selectedValueDesc;
			set
			{
				string? oldValue = _selectedValueDesc;
				_selectedValueDesc = value;
				OnPropertyChanged();
				RaisePropertyChanged(SelectedValueDescriptionProperty, oldValue, value);
			}
		}

		public T SelectedValue
		{
			get => RawSelectedValue.Item;
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
				RaisePropertyChanged(ValuesProperty, Optional<List<ItemWrapper>>.Empty, Values);

				RawSelectedValue = new(RawSelectedValue.Item, GetValueDisplayName(RawSelectedValue.Item));
			};
		}

		private void InitializeComponent()
		{
			Content = AvaloniaXamlLoader.Load(new Uri("avares://AssetRipper/Components/BaseConfigurationDropdown.axaml"));
			((UserControl)Content).DataContext = this;
		}

		protected virtual string GetValueDisplayName(T value) => Enum.GetName(typeof(T), value) ?? throw new($"Value {value} is not defined in the enum {typeof(T)}, but has ended up in a dropdown?");

		protected virtual string? GetValueDescription(T value) => null;

		public class ItemWrapper
		{
			public T Item;
			public string DisplayName;

			public ItemWrapper(T item, string displayName)
			{
				Item = item;
				DisplayName = displayName;
			}

			public override string ToString() => DisplayName;

			protected bool Equals(ItemWrapper other)
			{
				return Item.Equals(other.Item) && DisplayName == other.DisplayName;
			}

			public override bool Equals(object? obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				if (obj.GetType() != this.GetType())
				{
					return false;
				}

				return Equals((ItemWrapper)obj);
			}

			public override int GetHashCode()
			{
				return HashCode.Combine(Item, DisplayName);
			}
		}
	}
}