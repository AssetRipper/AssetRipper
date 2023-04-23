using AssetRipper.Import.Logging;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;

namespace AssetRipper.GUI.Utils
{
	public static class PermissionValidator
	{
		public static void RestartAsAdministrator()
		{
			if (OperatingSystem.IsWindows())
			{
				WindowsIdentity identity = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				// is run as administrator?
				if (principal.IsInRole(WindowsBuiltInRole.Administrator))
				{
					return;
				}
			}

			// try run as admin
			Process proc = new Process();
			string[] args = Environment.GetCommandLineArgs();
			proc.StartInfo.FileName = args[0];
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
					Logger.Log(LogType.Error, LogCategory.General, $"You can't execute desired action without Administrator permission");
				}
				else
				{
					Logger.Log(LogType.Error, LogCategory.General, $"You have to restart application as Administator in order execute desired action");
				}
			}
		}

		public static bool CheckAccess(string path)
		{
			if (!OperatingSystem.IsWindows())
			{
				return true;
			}

			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			bool isInRoleWithAccess = true;
			try
			{
				DirectoryInfo di = new(path);
				DirectorySecurity ds = di.GetAccessControl();
				AuthorizationRuleCollection rules = ds.GetAccessRules(true, true, typeof(NTAccount));

				foreach (AuthorizationRule rule in rules)
				{
					if (rule is not FileSystemAccessRule fsAccessRule)
					{
						continue;
					}

					if ((fsAccessRule.FileSystemRights & FileSystemRights.CreateDirectories) == 0 && (fsAccessRule.FileSystemRights & FileSystemRights.DeleteSubdirectoriesAndFiles) == 0)
					{
						continue;
					}

					if (rule.IdentityReference is not NTAccount ntAccount)
					{
						continue;
					}

					if (!principal.IsInRole(ntAccount.Value))
					{
						continue;
					}

					if (fsAccessRule.AccessControlType == AccessControlType.Deny)
					{
						isInRoleWithAccess = false;
						break;
					}
				}
			}
			catch (UnauthorizedAccessException)
			{
				isInRoleWithAccess = false;
			}
			return isInRoleWithAccess;
		}
	}
}
