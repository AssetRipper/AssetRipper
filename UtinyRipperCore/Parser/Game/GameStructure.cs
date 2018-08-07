using System;
using System.Collections.Generic;
using System.Linq;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper
{
	public sealed class GameStructure : IGameStructure
	{
		public GameStructure()
		{
			FileCollection = new FileCollection(OnRequestDependency, OnRequestAssembly);
			m_mixedStructure = new MixedGameStructure(FileCollection);
		}

		public void Load(IEnumerable<string> pathes)
		{
			List<string> toProcess = new List<string>();
			toProcess.AddRange(pathes);
			if(CheckPC(toProcess))
			{ }
			else if(CheckAndroid(toProcess))
			{ }
			CheckMixed(toProcess);

			if(PlatformStructure != null)
			{
				foreach (string filePath in PlatformStructure.FetchFiles())
				{
					string fileName = FileMultiStream.GetFileName(filePath);
					if (m_knownFiles.Add(fileName))
					{
						FileCollection.Load(filePath);
					}
				}
			}
			foreach (string filePath in MixedStructure.FetchFiles())
			{
				string fileName = FileMultiStream.GetFileName(filePath);
				if (m_knownFiles.Add(fileName))
				{
					FileCollection.Load(filePath);
				}
			}
		}

		public void Export(string exportPath, Func<Object, bool> selector)
		{
			FileCollection.Exporter.Export(exportPath, FileCollection.FetchAssets().Where(t => selector(t)));
		}
		
		public IEnumerable<string> FetchFiles()
		{
			if(PlatformStructure != null)
			{
				foreach (string assemblyPath in PlatformStructure.FetchFiles())
				{
					yield return assemblyPath;
				}
			}
			foreach (string assemblyPath in MixedStructure.FetchFiles())
			{
				yield return assemblyPath;
			}
		}

		public IEnumerable<string> FetchAssemblies()
		{
			if (PlatformStructure != null)
			{
				foreach (string assemblyPath in PlatformStructure.FetchAssemblies())
				{
					yield return assemblyPath;
				}
			}
			foreach (string assemblyPath in MixedStructure.FetchAssemblies())
			{
				yield return assemblyPath;
			}
		}

		public bool RequestDependency(string dependency)
		{
			if (m_knownFiles.Add(dependency))
			{
				if (PlatformStructure != null)
				{
					if (PlatformStructure.RequestDependency(dependency))
					{
						Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Dependency '{dependency}' has been loaded");
						return true;
					}
				}
				if (MixedStructure.RequestDependency(dependency))
				{
					Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Dependency '{dependency}' has been loaded");
					return true;
				}

				Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Dependency '{dependency}' hasn't been found");
				return false;
			}
			else
			{
				return true;
			}
		}

		public bool RequestAssembly(string assembly)
		{
			if (m_knownAssemblies.Add(assembly))
			{
				if (PlatformStructure != null)
				{
					if (PlatformStructure.RequestAssembly(assembly))
					{
						Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");
						return true;
					}
				}
				if (MixedStructure.RequestAssembly(assembly))
				{
					Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Assembly '{assembly}' has been loaded");
					return true;
				}

				Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Assembly '{assembly}' hasn't been found");
				return false;
			}
			else
			{
				return true;
			}
		}

		private bool CheckPC(List<string> pathes)
		{
			foreach(string path in pathes)
			{
				if(PCGameStructure.IsPCStructure(path))
				{
					if(PlatformStructure == null)
					{
						PlatformStructure = new PCGameStructure(FileCollection, path);
						pathes.Remove(path);
						Logger.Instance.Log(LogType.Info, LogCategory.Import, $"PC game structure has been found at '{path}'");
						return true;
					}
					else
					{
						throw new Exception("Platform structure already exists");
					}
				}
			}
			return false;
		}

		private bool CheckAndroid(List<string> pathes)
		{
			string androidStructure = null;
			string obbStructure = null;
			foreach (string path in pathes)
			{
				if (AndroidGameStructure.IsAndroidStructure(path))
				{
					if(androidStructure == null)
					{
						androidStructure = path;
					}
					else
					{
						throw new Exception("2 Android game stuctures has been found");
					}
				}
				else if(AndroidGameStructure.IsAndroidObbStructure(path))
				{
					if (obbStructure == null)
					{
						obbStructure = path;
					}
					else
					{
						throw new Exception("2 Android obb game stuctures has been found");
					}
				}
			}

			if(androidStructure != null)
			{
				if (PlatformStructure == null)
				{
					PlatformStructure = new AndroidGameStructure(FileCollection, androidStructure, obbStructure);
					pathes.Remove(androidStructure);
					Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Android game structure has been found at '{androidStructure}'");
					if(obbStructure != null)
					{
						pathes.Remove(obbStructure);
						Logger.Instance.Log(LogType.Info, LogCategory.Import, $"Android obb game structure has been found at '{obbStructure}'");
					}
					return true;
				}
				else
				{
					throw new Exception("Platform structure already exists");
				}
			}

			return false;
		}

		private void CheckMixed(List<string> pathes)
		{
			if(pathes.Count > 0)
			{
				m_mixedStructure.AddFiles(pathes);
				pathes.Clear();
			}
		}

		private void OnRequestDependency(string dependency)
		{
			RequestDependency(dependency);
		}

		private void OnRequestAssembly(string assembly)
		{
			RequestAssembly(assembly);
		}

		public FileCollection FileCollection { get; }
		public PlatformGameStructure PlatformStructure { get; private set; }
		public IGameStructure MixedStructure => m_mixedStructure;

		public string Name
		{
			get
			{
				if(PlatformStructure == null)
				{
					return MixedStructure.Name;
				}
				else
				{
					return PlatformStructure.Name;
				}
			}
		}

		private readonly HashSet<string> m_knownFiles = new HashSet<string>();
		private readonly HashSet<string> m_knownAssemblies = new HashSet<string>();

		private readonly MixedGameStructure m_mixedStructure;
	}
}
