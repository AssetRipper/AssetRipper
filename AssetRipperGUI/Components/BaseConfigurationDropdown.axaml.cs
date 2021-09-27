using AssetRipper.Core.Logging;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.GUI.Components
{
	public abstract class BaseConfigurationDropdown<T> : UserControlWithPropChange where T : struct, Enum
	{
		private string _optionTitle = $"<Missing Title##{typeof(T).Name}>";
		private ItemWrapper _selectedValue;
		private string? _selectedValueDesc;

		public static DirectProperty<BaseConfigurationDropdown<T>, T> SelectedValueProperty = 
			AvaloniaProperty.RegisterDirect<BaseConfigurationDropdown<T>, T>(nameof(SelectedValue), obj => obj.SelectedValue, (obj, val) => obj.SelectedValue = val);

		public List<ItemWrapper> Values { get; }

		public string OptionTitle
		{
			get => _optionTitle;
			set
			{
				_optionTitle = value;
				OnPropertyChanged();
			}
		}

		public ItemWrapper RawSelectedValue
		{
			get => _selectedValue;
			set
			{
				var oldValue = _selectedValue?.Item;
				_selectedValue = value;
				OnPropertyChanged();
				RaisePropertyChanged(SelectedValueProperty, oldValue.HasValue ? new(oldValue.Value) : Optional<T>.Empty, value.Item);
				SelectedValueDescription = GetValueDescription(value.Item);
			}
		}

		public string? SelectedValueDescription
		{
			get => _selectedValueDesc;
			set
			{
				_selectedValueDesc = value;
				OnPropertyChanged();
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
		}

		private void InitializeComponent()
		{
			Content = AvaloniaXamlLoader.Load(new Uri("avares://AssetRipper/Components/BaseConfigurationDropdown.axaml"));
			((UserControl) Content).DataContext = this;
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