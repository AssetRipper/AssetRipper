using AssetRipper.Export.UnityProjects;
using AssetRipper.GUI.Localizations;
using AssetRipper.Import.Logging;
using ElectronNET.API;
using ElectronNET.API.Entities;
using ElectronAPI = ElectronNET.API.Electron;

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

	public static Ripper Ripper { get; } = new();

	public static async Task Main(string[] args)
	{
		Logger.Add(new ConsoleLogger());
		Pages.ConsoleModel.RegisterLogger();
		Logger.LogSystemInformation("AssetRipper");

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

		LocalHost.Initialize();

		_mainWindow = await ElectronAPI.WindowManager.CreateWindowAsync();

		app.WaitForShutdown();
	}

	private static void CreateMenu()
	{
		MenuItem[] fileMenu = new MenuItem[]
		{
			new MenuItem
			{
				Label = "Load File(s)",
				Type = MenuType.normal,
				Click = async () =>
				{
					OpenDialogOptions options = new()
					{
						Properties = new OpenDialogProperty[] { OpenDialogProperty.openFile, OpenDialogProperty.multiSelections }
					};
					string[] result = await ElectronAPI.Dialog.ShowOpenDialogAsync(MainWindow, options);
					if (result.Length == 0)
					{
						return;
					}
					try
					{
						Ripper.Load(result);
					}
					catch (Exception ex)
					{
						Logger.Error(LogCategory.Export, null, ex);
						Ripper.ResetData();
						await ElectronAPI.Dialog.ShowMessageBoxAsync(MainWindow, new MessageBoxOptions(ex.Message)
						{
							Title = "Error",
							Type = MessageBoxType.error
						});
					}
					MainWindow.LoadURL(LocalHost.BaseUrl);
				}
			},
			new MenuItem
			{
				Label = "Load Folder(s)",
				Type = MenuType.normal,
				Click = async () =>
				{
					OpenDialogOptions options = new()
					{
						Properties = new OpenDialogProperty[] { OpenDialogProperty.openDirectory, OpenDialogProperty.multiSelections }
					};
					string[] result = await ElectronAPI.Dialog.ShowOpenDialogAsync(MainWindow, options);
					if (result.Length == 0)
					{
						return;
					}
					try
					{
						Ripper.Load(result);
					}
					catch (Exception ex)
					{
						Logger.Error(LogCategory.Export, null, ex);
						Ripper.ResetData();
						await ElectronAPI.Dialog.ShowMessageBoxAsync(MainWindow, new MessageBoxOptions(ex.Message)
						{
							Title = "Error",
							Type = MessageBoxType.error
						});
					}
					MainWindow.LoadURL(LocalHost.BaseUrl);
				}
			},
			new MenuItem
			{
				Label = "Reset",
				Type = MenuType.normal,
				Click = () =>
				{
					Ripper.ResetData();
					MainWindow.LoadURL(LocalHost.BaseUrl);
				}
			},
			new MenuItem { Type = MenuType.separator },
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

		MenuItem[] exportMenu = new MenuItem[]
		{
			new MenuItem
			{
				Label = "Export All",
				Type = MenuType.normal,
				Click = async () =>
				{
					if (!Ripper.IsLoaded)
					{
						await ElectronAPI.Dialog.ShowMessageBoxAsync(MainWindow, new MessageBoxOptions("No files loaded")
						{
							Title = "Error",
							Type = MessageBoxType.error
						});
						return;
					}
					OpenDialogOptions options = new()
					{
						Properties = new OpenDialogProperty[] { OpenDialogProperty.openDirectory }
					};
					string[] result = await ElectronAPI.Dialog.ShowOpenDialogAsync(MainWindow, options);
					if (result.Length == 0)
					{
						return;
					}
					if (result.Length > 1)
					{
						await ElectronAPI.Dialog.ShowMessageBoxAsync(MainWindow, new MessageBoxOptions("Only one directory can be selected")
						{
							Title = "Error",
							Type = MessageBoxType.error
						});
						return;
					}
					if (!Directory.Exists(result[0]))
					{
						await ElectronAPI.Dialog.ShowMessageBoxAsync(MainWindow, new MessageBoxOptions("Directory does not exist")
						{
							Title = "Error",
							Type = MessageBoxType.error
						});
						return;
					}
					
					//if directory is not empty
					if (Directory.EnumerateFileSystemEntries(result[0]).Any())
					{
						MessageBoxResult confirmation = await ElectronAPI.Dialog.ShowMessageBoxAsync(MainWindow, new MessageBoxOptions("Directory is not empty. Continue?")
						{
							Title = "Warning",
							Type = MessageBoxType.warning,
							Buttons = new string[] { "Yes", "No" }
						});
						if (confirmation.Response == 1)
						{
							return;
						}
						else
						{
							Directory.Delete(result[0], true);
						}
					}

					try
					{
						Ripper.ExportProject(result[0]);
					}
					catch (Exception ex)
					{
						Logger.Error(LogCategory.Export, null, ex);
						Ripper.GameStructure.FileCollection.ClearTemporaryBundles();
						await ElectronAPI.Dialog.ShowMessageBoxAsync(MainWindow, new MessageBoxOptions(ex.Message)
						{
							Title = "Error",
							Type = MessageBoxType.error
						});
					}
					MainWindow.LoadURL(LocalHost.BaseUrl);
				}
			},
		};

		MenuItem[] languageMenu = LocalizationLoader.LanguageNameDictionary.Select(x => new MenuItem
		{
			Label = x.Value,
			Type = MenuType.normal,
			Click = () =>
			{
				Logger.Info(LogCategory.System, $"Loading locale {x.Key}.json");
				Localization.LoadLanguage(x.Key);
				MainWindow.Reload();
			}
		}).ToArray();

		MenuItem[] windowMenu = new MenuItem[]
		{
#if DEBUG
			new MenuItem
			{
				Label = "Launch Debugger",
				Type = MenuType.normal,
				Click = () => System.Diagnostics.Debugger.Launch(),
			},
#endif
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
				new MenuItem { Label = "Export", Type = MenuType.submenu, Submenu = exportMenu },
				new MenuItem { Label = "Language", Type = MenuType.submenu, Submenu = languageMenu },
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
				new MenuItem { Label = "Export", Type = MenuType.submenu, Submenu = exportMenu },
				new MenuItem { Label = "Language", Type = MenuType.submenu, Submenu = languageMenu },
				new MenuItem { Role = MenuRole.windowMenu, Submenu = windowMenu },
			};
		}

		ElectronAPI.Menu.SetApplicationMenu(menu);
	}
}
