using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;

namespace AssetRipperGuiNew
{
    public partial class MainWindow : Window
    {
	    private MainWindowViewModel VM => (MainWindowViewModel) DataContext!;
	    
	    public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
	        
	        AddHandler(DragDrop.DropEvent, (sender, args) => VM.OnFileDropped(args));
        }

	    protected override void OnDataContextChanged(EventArgs e)
	    {
		    base.OnDataContextChanged(e);
		    
		    VM.HasFile = false;
		    VM.HasLoaded = false;
	    }

	    private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}