using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly;

namespace uTinyRipper
{
	internal sealed class WebPlayerStructure : PlatformGameStructure
	{
		public WebPlayerStructure(string rootPath)
		{
			if (string.IsNullOrEmpty(rootPath))
			{
				throw new ArgumentNullException(rootPath);
			}
			m_root = new DirectoryInfo(DirectoryUtils.ToLongPath(rootPath));
			if (!m_root.Exists)
			{
				throw new Exception($"Directory '{rootPath}' doesn't exist");
			}

			if(!GetWebPlayerName(m_root, out string name))
			{
				throw new Exception($"Web player asset bundle data hasn't been found");
			}
			Name = name;
			DataPathes = new string[] { rootPath };

			Dictionary<string, string> files = new Dictionary<string, string>();
			string abPath = Path.Combine(m_root.FullName, Name + AssetBundleExtension);
			files.Add(Name, abPath);
			CollectStreamingAssets(m_root, files);
			Files = files;

			Dictionary<string, string> assemblies = new Dictionary<string, string>();
			CollectMainAssemblies(m_root, assemblies);
			Assemblies = assemblies;
		}

		public static bool IsWebPlayerStructure(string path)
		{
			DirectoryInfo dinfo = new DirectoryInfo(DirectoryUtils.ToLongPath(path));
			if (!dinfo.Exists)
			{
				return false;
			}

			return GetWebPlayerName(dinfo, out string _);
		}

		public static bool GetWebPlayerName(DirectoryInfo root, out string name)
		{
			foreach (FileInfo fi in root.EnumerateFiles())
			{
				if (fi.Extension == HtmlExtension)
				{
					name = fi.Name.Substring(0, fi.Name.Length - HtmlExtension.Length);
					string abPath = Path.Combine(root.FullName, name + AssetBundleExtension);
					if (File.Exists(abPath))
					{
						return true;
					}
				}
			}
			name = null;
			return false;
		}

		public override ScriptingBackEnd GetScriptingBackend()
		{
			return ScriptingBackEnd.Mono;
		}

		public override string Name { get; }
		public override IReadOnlyList<string> DataPathes { get; }

		public override IReadOnlyDictionary<string, string> Files { get; }
		public override IReadOnlyDictionary<string, string> Assemblies { get; }

		private const string HtmlExtension = ".html";

		private readonly DirectoryInfo m_root;
	}
}
