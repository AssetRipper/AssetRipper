#if DEBUG
#define DEBUG_PROGRAM
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using UtinyRipper.Classes;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper
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
			Config.IsAdvancedLog = true;
			Config.IsGenerateGUIDByContent = false;
			Config.IsExportDependencies = false;
			
			if (args.Length == 0)
			{
				Console.WriteLine("No arguments");
				Console.ReadKey();
				return;
			}

			foreach (string arg in args)
			{
				if (FileMultiStream.Exists(arg))
				{
					continue;
				}
				if(DirectoryUtils.Exists(arg))
				{
					continue;
				}
				Console.WriteLine(FileMultiStream.IsMultiFile(arg) ?
					$"File '{arg}' doesn't has all parts for combining" :
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
				GameStructure.Load(args);
				Validate();

				string exportPath = Path.Combine("Ripped", GameStructure.Name);
				PrepareExportDirectory(exportPath);

				GameStructure.Export(exportPath, AssetSelector);
				Logger.Instance.Log(LogType.Info, LogCategory.General, "Finished");
			}
#if !DEBUG_PROGRAM
			catch(Exception ex)
			{
				Logger.Instance.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
#endif
		}
		
		private void Validate()
		{
			Version[] versions = GameStructure.FileCollection.Files.Select(t => t.Version).Distinct().ToArray();
			if (versions.Count() > 1)
			{
				Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Asset collection has versions probably incompatible with each other. Here they are:");
				foreach (Version version in versions)
				{
					Logger.Instance.Log(LogType.Warning, LogCategory.Import, version.ToString());
				}
			}
		}
		
		private static void PrepareExportDirectory(string path)
		{
			if (!RunetimeUtils.IsRunningOnMono)
			{
				string directory = Directory.GetCurrentDirectory();
				CheckWritePermission(directory);
			}
			
			if (DirectoryUtils.Exists(path))
			{
				DirectoryUtils.Delete(path, true);
			}
		}

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
						Logger.Instance.Log(LogType.Error, LogCategory.General, $"You can't export to folder {path} without Administrator permission");
						Console.ReadKey();
					}
					else
					{
						Logger.Instance.Log(LogType.Error, LogCategory.General, $"You have to restart application as Administator in order to export to folder {path}");
						Console.ReadKey();
					}
				}
			}
		}

		private GameStructure GameStructure { get; } = new GameStructure();
	}
}
