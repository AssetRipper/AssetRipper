//#define DEBUG_PROGRAM

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper
{
	public class Program
	{
		public static IEnumerable<Object> FetchExportObjects(FileCollection collection)
		{
			//yield break;
			return collection.FetchAssets();
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
				return;
			}
			foreach (string arg in args)
			{
				if (!FileMultiStream.Exists(arg))
				{
					Console.WriteLine(FileMultiStream.IsMultiFile(arg) ?
						$"File {arg} doen't has all parts for combining" :
						$"File {arg} doesn't exist", arg);
					return;
				}
			}

			Program program = new Program();
			program.Load(args);
			Console.ReadKey();
		}

		public Program()
		{
			m_collection = new FileCollection();
			m_collection.EventRequestDependency += OnRequestDependency;
		}

		public void Load(IReadOnlyList<string> args)
		{
#if !DEBUG_PROGRAM
			try
#endif
			{
				string name = Path.GetFileNameWithoutExtension(args.First());
				string exportPath = Path.Combine("Ripped", name);

				Prepare(exportPath, args);
				LoadFiles(args);
				Validate();

				m_collection.Exporter.Export(exportPath, FetchExportObjects(m_collection));
				Logger.Instance.Log(LogType.Info, LogCategory.General, "Finished");
			}
#if !DEBUG_PROGRAM
			catch(Exception ex)
			{
				Logger.Instance.Log(LogType.Error, LogCategory.General, ex.ToString());
			}
#endif
		}

		private void Prepare(string exportPath, IEnumerable<string> filePathes)
		{
			PrepareExportDirectory(exportPath);

			foreach (string filePath in filePathes)
			{
				string dirPath = Path.GetDirectoryName(filePath);
				m_knownDirectories.Add(dirPath);
			}
		}
		
		private void LoadFiles(IEnumerable<string> filePathes)
		{
			foreach (string filePath in filePathes)
			{
				string fileName = FileMultiStream.GetFileName(filePath);
				LoadFile(filePath, fileName);
			}
		}

		private void LoadFile(string fullFilePath, string originalFileName)
		{
			if (m_knownFiles.Add(originalFileName))
			{
				string filePath = FileMultiStream.GetFilePath(fullFilePath);
				using (Stream stream = FileMultiStream.OpenRead(filePath))
				{
					m_collection.Read(stream, filePath, originalFileName);
				}
			}
		}

		private void Validate()
		{
			Version[] versions = m_collection.Files.Select(t => t.Version).Distinct().ToArray();
			if (versions.Count() > 1)
			{
				Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Asset collection has versions probably incompatible with each other. Here they are:");
				foreach (Version version in versions)
				{
					Logger.Instance.Log(LogType.Warning, LogCategory.Import, version.ToString());
				}
			}
		}

		private void LoadDependency(string fileName)
		{
			foreach (string loadName in FetchNameVariants(fileName))
			{
				bool found = TryLoadDependency(loadName, fileName);
				if (found)
				{
					return;
				}
			}

			Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Dependency '{fileName}' wasn't found");
		}

		private bool TryLoadDependency(string loadName, string originalName)
		{
			foreach (string dirPath in m_knownDirectories)
			{
				string path = Path.Combine(dirPath, loadName);
				if (FileMultiStream.Exists(path))
				{
					LoadFile(path, originalName);
					Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Dependency '{path}' was loaded");
					return true;
				}
			}
			return false;
		}

		private void OnRequestDependency(string dependency)
		{
			if(m_knownFiles.Contains(dependency))
			{
				return;
			}

			LoadDependency(dependency);
		}
		
		private static void PrepareExportDirectory(string path)
		{
			string directory = Directory.GetCurrentDirectory();
			CheckWritePermission(directory);

			if (Directory.Exists(path))
			{
				Directory.Delete(path, true);
			}
		}

		private static void CheckWritePermission(string path)
		{
			WindowsIdentity identity = WindowsIdentity.GetCurrent();
			WindowsPrincipal principal = new WindowsPrincipal(identity);
			bool isInRoleWithAccess = false;
			try
			{
				DirectoryInfo di = new DirectoryInfo(path);
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

		private static IEnumerable<string> FetchNameVariants(string name)
		{
			yield return name;

			const string libraryFolder = "library";
			if (name.ToLower().StartsWith(libraryFolder))
			{
				string fixedName = name.Substring(libraryFolder.Length + 1);
				yield return fixedName;

				fixedName = Path.Combine("Resources", fixedName);
				yield return fixedName;
			}
		}

		private readonly HashSet<string> m_knownDirectories = new HashSet<string>();
		private readonly HashSet<string> m_knownFiles = new HashSet<string>();

		private readonly FileCollection m_collection;
	}
}
