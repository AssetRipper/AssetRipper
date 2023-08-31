using ElectronNET.API;
using ElectronNET.API.Entities;

namespace AssetRipper.GUI.Electron;

public static class Program
{
	private static BrowserWindow? _mainWindow;
	public static BrowserWindow MainWindow
	{
		get
		{
			return _mainWindow ?? throw new NullReferenceException();
		}
	}

	public static async Task Main(string[] args)
	{
		WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

		builder.WebHost.UseElectron(args);

		// Add services to the container.
		builder.Services.AddRazorPages();

		WebApplication app = builder.Build();

		// Configure the HTTP request pipeline.
		if (!app.Environment.IsDevelopment())
		{
			app.UseExceptionHandler("/Error");
		}

		app.UseStaticFiles();
		app.UseRouting();
		app.UseAuthorization();
		app.MapRazorPages();

		await app.StartAsync();

		CreateMenu();

		_mainWindow = await ElectronNET.API.Electron.WindowManager.CreateWindowAsync();

		app.WaitForShutdown();
	}

	private static void CreateMenu()
	{
		MenuItem[] fileMenu = new MenuItem[]
		{
			new MenuItem { Role = OperatingSystem.IsMacOS() ? MenuRole.close : MenuRole.quit }
		};

		MenuItem[] editMenu = new MenuItem[]
		{
			new MenuItem { Role = MenuRole.undo, Accelerator = "CmdOrCtrl+Z" },
			new MenuItem { Role = MenuRole.redo, Accelerator = "Shift+CmdOrCtrl+Z" },
			new MenuItem { Type = MenuType.separator },
			new MenuItem { Role = MenuRole.cut, Accelerator = "CmdOrCtrl+X" },
			new MenuItem { Role = MenuRole.copy, Accelerator = "CmdOrCtrl+C" },
			new MenuItem { Role = MenuRole.paste, Accelerator = "CmdOrCtrl+V" },
			new MenuItem { Role = MenuRole.delete },
			new MenuItem { Type = MenuType.separator },
			new MenuItem { Role = MenuRole.selectall, Accelerator = "CmdOrCtrl+A" }
		};

		MenuItem[] viewMenu = new MenuItem[]
		{
			new MenuItem { Role = MenuRole.reload },
			new MenuItem { Role = MenuRole.forcereload },
			new MenuItem { Role = MenuRole.toggledevtools },
			new MenuItem { Type = MenuType.separator },
			new MenuItem { Role = MenuRole.togglefullscreen }
		};

		MenuItem[] windowMenu = new MenuItem[]
		{
			new MenuItem { Role = MenuRole.minimize, Accelerator = "CmdOrCtrl+M" },
			new MenuItem { Role = MenuRole.close, Accelerator = "CmdOrCtrl+W" }
		};

		MenuItem[] menu;
		if (OperatingSystem.IsMacOS())
		{
			MenuItem[] appMenu = new MenuItem[]
			{
				new MenuItem { Role = MenuRole.about },
				new MenuItem { Type = MenuType.separator },
				new MenuItem { Role = MenuRole.services },
				new MenuItem { Type = MenuType.separator },
				new MenuItem { Role = MenuRole.hide },
				new MenuItem { Role = MenuRole.hideothers },
				new MenuItem { Role = MenuRole.unhide },
				new MenuItem { Type = MenuType.separator },
				new MenuItem { Role = MenuRole.quit }
			};

			menu = new MenuItem[]
			{
				new MenuItem { Label = "Electron", Type = MenuType.submenu, Submenu = appMenu },
				new MenuItem { Label = "File", Type = MenuType.submenu, Submenu = fileMenu },
				new MenuItem { Role = MenuRole.editMenu, Submenu = editMenu },
				new MenuItem { Label = "View", Type = MenuType.submenu, Submenu = viewMenu },
				new MenuItem { Role = MenuRole.windowMenu, Submenu = windowMenu },
			};
		}
		else
		{
			menu = new MenuItem[]
			{
				new MenuItem { Label = "File", Type = MenuType.submenu, Submenu = fileMenu },
				new MenuItem { Role = MenuRole.editMenu, Submenu = editMenu },
				new MenuItem { Label = "View", Type = MenuType.submenu, Submenu = viewMenu },
				new MenuItem { Role = MenuRole.windowMenu, Submenu = windowMenu },
			};
		}

		ElectronNET.API.Electron.Menu.SetApplicationMenu(menu);
	}
}
