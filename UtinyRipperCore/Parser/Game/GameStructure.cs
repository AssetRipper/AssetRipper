using System;
using System.Collections.Generic;
using System.Linq;

using Object = UtinyRipper.Classes.Object;

namespace UtinyRipper
{
	public sealed class GameStructure
	{
		private GameStructure()
		{
			FileCollection = new FileCollection(OnRequestDependency, OnRequestAssembly);
		}

		public static GameStructure Load(IEnumerable<string> pathes)
		{
			List<string> toProcess = new List<string>();
			toProcess.AddRange(pathes);
			if (toProcess.Count == 0)
			{
				throw new ArgumentException("No pathes found", nameof(pathes));
			}

			GameStructure structure = new GameStructure();
			structure.Load(toProcess);
			return structure;
		}

		public void Export(string exportPath, Func<Object, bool> filter)
		{
			FileCollection.Exporter.Export(exportPath, FileCollection, FileCollection.FetchAssets().Where(t => filter(t)));
		}
		
		public bool RequestDependency(string dependency)
		{
			if (m_knownFiles.Add(dependency))
			{
				if (PlatformStructure != null)
				{
					if (PlatformStructure.RequestDependency(dependency))
					{
						return true;
					}
				}
				if (MixedStructure != null)
				{
					if (MixedStructure.RequestDependency(dependency))
					{
						return true;
					}
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
			if(m_knownAssemblies.Add(assembly))
			{
				if (PlatformStructure != null)
				{
					if (PlatformStructure.RequestAssembly(assembly))
					{
						return true;
					}
				}
				if (MixedStructure != null)
				{
					if (MixedStructure.RequestAssembly(assembly))
					{
						return true;
					}
				}

				Logger.Instance.Log(LogType.Warning, LogCategory.Import, $"Assembly '{assembly}' hasn't been found");
				return false;
			}
			else
			{
				return true;
			}
		}

		private void Load(List<string> pathes)
		{
			if (CheckPC(pathes)) { }
			else if (CheckAndroid(pathes)) { }
			CheckMixed(pathes);

			if (PlatformStructure != null)
			{
				foreach (KeyValuePair<string, string> file in PlatformStructure.Files)
				{
					if (m_knownFiles.Add(file.Key))
					{
						FileCollection.Load(file.Value);
					}
				}
			}
			if (MixedStructure != null)
			{
				foreach (KeyValuePair<string, string> file in MixedStructure.Files)
				{
					if (m_knownFiles.Add(file.Key))
					{
						FileCollection.Load(file.Value);
					}
				}
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
				MixedStructure = new MixedGameStructure(FileCollection, pathes);
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
		public PlatformGameStructure MixedStructure { get; private set; }

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
	}
}
