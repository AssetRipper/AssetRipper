using Avalonia.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AssetRipper.GUI.Components
{
	public abstract class UserControlWithPropChange : UserControl, INotifyPropertyChanged
	{
		public new event PropertyChangedEventHandler? PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			PropertyChanged?.Invoke(this, new(propertyName));
		}
	}
}
