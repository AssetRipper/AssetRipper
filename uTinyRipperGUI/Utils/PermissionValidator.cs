using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using uTinyRipper;

namespace uTinyRipperGUI
{
	public static class PermissionValidator
	{
		public static void RestartAsAdministrator(string arguments)
		{
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			// is run as administrator?
			if (principal.IsInRole(WindowsBuiltInRole.Administrator))
			{
				return;
			}

			// try run as admin
			Process proc = new Process();
			string[] args = Environment.GetCommandLineArgs();
			proc.StartInfo.FileName = args[0];
			proc.StartInfo.Arguments = arguments;
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
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			bool isInRoleWithAccess = true;
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

					if ((fsAccessRule.FileSystemRights & FileSystemRights.CreateDirectories) != 0 ||
						(fsAccessRule.FileSystemRights & FileSystemRights.DeleteSubdirectoriesAndFiles) != 0)
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
						}
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
