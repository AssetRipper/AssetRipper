#if DEBUG
#define DEBUG_PROGRAM
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !NET_CORE
using System.ComponentModel;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
#endif
using uTinyRipper;
using uTinyRipper.Converters;
using Object = uTinyRipper.Classes.Object;

namespace uTinyRipperConsole
{
	public class Program
	{
		public static bool AssetSelector(Object asset)
		{
			return true;
		}

		public static void Main(string[] args)
		{
			Logger.Instance = ConsoleLogger.Instance;

			if (args.Length == 0)
			{
				Console.WriteLine("No arguments");
				Console.ReadKey();
				return;
			}

			foreach (string arg in args)
			{
				if (MultiFileStream.Exists(arg))
				{
					continue;
				}
				if(DirectoryUtils.Exists(arg))
				{
					continue;
				}
				Console.WriteLine(MultiFileStream.IsMultiFile(arg) ?
					$"File '{arg}' doesn't have all parts for combining" :
					$"Neither file nor directory with path '{arg}' exists");
				Console.ReadKey();
				return;
			}

			Program program = new Program();
			program.Load(args);
			Console.ReadKey();
		}

		public void Load(IReadOnlyList<string> args)
		{
#if !DEBUG_PROGRAM
			try
#endif
			{
				GameStructure = GameStructure.Load(args);

				string exportPath = Path.Combine("Ripped", GameStructure.Name);
				PrepareExportDirectory(exportPath);

				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.TextAsset, new TextAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, new FontAssetExporter());
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MovieTexture, new MovieTextureAssetExporter());

#if DEBUG
				EngineAssetExporter engineExporter = new EngineAssetExporter();
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Material, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Texture2D, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Mesh, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Shader, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Font, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.Sprite, engineExporter);
				GameStructure.FileCollection.Exporter.OverrideExporter(ClassIDType.MonoBehaviour, engineExporter);
#endif

				GameStructure.Export(exportPath, AssetSelector);
				Logger.Log(LogType.Info, LogCategory.General, "Finished");
			}
#if !DEBUG_PROGRAM
			catch(Exception ex)
			{
				Logger.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
#endif
		}
		
		private static void PrepareExportDirectory(string path)
		{
#if !NET_CORE
			if (!RunetimeUtils.IsRunningOnMono)
			{
				string directory = Directory.GetCurrentDirectory();
				CheckWritePermission(directory);
			}
#endif
			
			if (DirectoryUtils.Exists(path))
			{
				DirectoryUtils.Delete(path, true);
			}
		}

#if !NET_CORE
		private static void CheckWritePermission(string path)
		{
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			bool isInRoleWithAccess = false;
			try
			{
				DirectoryInfo di = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
				DirectorySecurity ds = di.GetAccessControl();
				AuthorizationRuleCollection rules = ds.GetAccessRules(true, true, typeof(NTAccount));

				foreach (AuthorizationRule rule in rules)
				{
					FileSystemAccessRule fsAccessRule = rule as FileSystemAccessRule;
					if (fsAccessRule == null)
					{
						continue;
					}

					if ((fsAccessRule.FileSystemRights & FileSystemRights.Write) != 0)
					{
						NTAccount ntAccount = rule.IdentityReference as NTAccount;
						if (ntAccount == null)
						{
							continue;
						}

						if (principal.IsInRole(ntAccount.Value))
						{
							if (fsAccessRule.AccessControlType == AccessControlType.Deny)
							{
								isInRoleWithAccess = false;
								break;
							}
							isInRoleWithAccess = true;
						}
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
			}

			if (!isInRoleWithAccess)
			{
				// is run as administrator?
				if (principal.IsInRole(WindowsBuiltInRole.Administrator))
				{
					return;
				}

				// try run as admin
				Process proc = new Process();
				string[] args = Environment.GetCommandLineArgs();
				proc.StartInfo.FileName = args[0];
				proc.StartInfo.Arguments = string.Join(" ", args.Skip(1).Select(t => $"\"{t}\""));
				proc.StartInfo.UseShellExecute = true;
				proc.StartInfo.Verb = "runas";

				try
				{
					proc.Start();
					Environment.Exit(0);
				}
				catch (Win32Exception ex)
				{
					//The operation was canceled by the user.
					const int ERROR_CANCELLED = 1223;
					if (ex.NativeErrorCode == ERROR_CANCELLED)
					{
						Logger.Log(LogType.Error, LogCategory.General, $"You can't export to folder {path} without Administrator permission");
						Console.ReadKey();
					}
					else
					{
						Logger.Log(LogType.Error, LogCategory.General, $"You have to restart application as Administator in order to export to folder {path}");
						Console.ReadKey();
					}
				}
			}
		}
#endif

		private GameStructure GameStructure { get; set; }
	}
}
