using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.ArchiveFiles;
using AssetRipper.Core.Parser.Files.BundleFile;
using AssetRipper.Core.Parser.Files.Entries;
using AssetRipper.Core.Parser.Files.ResourceFiles;
using AssetRipper.Core.Parser.Files.Schemes;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Parser.Files.WebFiles;
using AssetRipper.Core.SourceGenExtensions;
using AssetRipper.Core.Structure;
using AssetRipper.SourceGenerated.Classes.ClassID_114;
using AssetRipper.SourceGenerated.Classes.ClassID_115;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using YamlDotNet.RepresentationModel;

namespace AssetRipper.Library.Utils
{
	/// <summary>
	/// Needs removed at some point
	/// </summary>
	internal class Util
	{
		public static BindingFlags AllBindingFlags = BindingFlags.Instance
			| BindingFlags.Static
			| BindingFlags.Public
			| BindingFlags.NonPublic
			| BindingFlags.GetField
			| BindingFlags.SetField
			| BindingFlags.GetProperty
			| BindingFlags.SetProperty;
		public static void PrepareExportDirectory(string path)
		{
			DeleteDirectory(path);
		}
		public static void DeleteDirectory(string path)
		{
			if (!Directory.Exists(path))
			{
				return;
			}

			foreach (string directory in Directory.GetDirectories(path))
			{
				Thread.Sleep(1);
				DeleteDir(directory);
			}
			DeleteDir(path);
		}

		private static void DeleteDir(string dir)
		{
			try
			{
				Thread.Sleep(1);
				Directory.Delete(dir, true);
			}
			catch (IOException)
			{
				DeleteDir(dir);
			}
			catch (UnauthorizedAccessException)
			{
				DeleteDir(dir);
			}
		}
		public static string HashBytes(byte[] inputBytes)
		{
			using MD5 md5 = MD5.Create();
			byte[] hashBytes = md5.ComputeHash(inputBytes);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hashBytes.Length; i++)
			{
				sb.Append(hashBytes[i].ToString("X2"));
			}
			return sb.ToString();
		}
		public static string GetRelativePath(string filePath, string folder)
		{
			Uri pathUri = new Uri(filePath);
			if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				folder += Path.DirectorySeparatorChar;
			}
			Uri folderUri = new Uri(folder);
			return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
		}
		public static string NormalizePath(string path)
		{
			return Path.GetFullPath(new Uri(path).LocalPath)
						.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
						.ToUpperInvariant();
		}
		public static ISerializedFile FindFile(GameCollection fileCollection, string path)
		{
			return fileCollection.GameFiles.Values.First(f => f is SerializedFile sf && NormalizePath(sf.FilePath) == NormalizePath(path));
		}
		public static IEnumerable<SerializedFile> GetSerializedFiles(GameCollection fileCollection)
		{
			return fileCollection.GameFiles.Values;
		}
		public static void ReplaceInFile(string filePath, string source, string replacement)
		{
			if (!File.Exists(filePath))
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Could not perform line replace on {filePath}, file does not exist");
				return;
			}
			string? text = File.ReadAllText(filePath);
			text = text.Replace(source, replacement);
			File.WriteAllText(filePath, text);
		}
		public static void InsertInFile(string filePath, int index, string replacement)
		{
			if (!File.Exists(filePath))
			{
				Logger.Log(LogType.Warning, LogCategory.Export, $"Could not perform line insert on {filePath}, file does not exist");
				return;
			}
			List<string>? lines = File.ReadAllLines(filePath).ToList();
			lines.Insert(index, replacement);
			File.WriteAllLines(filePath, lines);
		}

		public static List<string> GetManifestDependencies(string filePath)
		{
			YamlStream? yaml = new YamlStream();
			YamlMappingNode? mapping = null;
			using (StreamReader? fs = File.OpenText(filePath))
			{
				yaml.Load(fs);
				mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
			}
			YamlSequenceNode? dependencies = (YamlSequenceNode)mapping.Children[new YamlScalarNode("Dependencies")];
			List<string> results = dependencies
				.Select(node => Path.GetFileName(((YamlScalarNode)node).Value ?? ""))
				.ToList();
			return results;
		}

		internal static List<string> GetManifestAssets(string filePath)
		{
			List<string>? lines = File.ReadAllLines(filePath).ToList();
			bool isAtDependencies = false;
			List<string>? results = new List<string>();
			foreach (string? line in lines)
			{
				if (isAtDependencies)
				{
					if (line.StartsWith("- "))
					{
						string? dep = line.Replace("- ", "");
						results.Add(Path.GetFileName(dep));
					}
					else
					{
						break;
					}
				}
				else
				{
					if (line.StartsWith("Assets:"))
					{
						isAtDependencies = true;
					}
				}
			}
			return results;
		}
		public static string FormatTime(TimeSpan obj)
		{
			StringBuilder sb = new StringBuilder();
			if (obj.Hours != 0)
			{
				sb.Append(obj.Hours);
				sb.Append(" ");
				sb.Append("hours");
				sb.Append(" ");
			}
			if (obj.Minutes != 0 || sb.Length != 0)
			{
				sb.Append(obj.Minutes);
				sb.Append(" ");
				sb.Append("minutes");
				sb.Append(" ");
			}
			if (obj.Seconds != 0 || sb.Length != 0)
			{
				sb.Append(obj.Seconds);
				sb.Append(" ");
				sb.Append("seconds");
				sb.Append(" ");
			}
			if (obj.Milliseconds != 0 || sb.Length != 0)
			{
				sb.Append(obj.Milliseconds);
				sb.Append(" ");
				sb.Append("Milliseconds");
				sb.Append(" ");
			}
			if (sb.Length == 0)
			{
				sb.Append(0);
				sb.Append(" ");
				sb.Append("Milliseconds");
			}
			return sb.ToString();
		}

		public static void FixShaderBundle(GameCollection fileCollection)
		{
			SerializedFile? shaderBundle = fileCollection.GameFiles.Values.FirstOrDefault(f => f is SerializedFile sf && Path.GetFileName(sf.FilePath) == "shaders");
			if (shaderBundle != null)
			{
				foreach (IUnityObjectBase? asset in shaderBundle.FetchAssets())
				{
					if (asset is IShader shader)
					{
						using MD5 md5 = MD5.Create();
						byte[] md5Hash = md5.ComputeHash(shader.Name.Data);
						asset.GUID = new UnityGUID(md5Hash);
					}
				}
			}
		}
		public static string GetName(IUnityObjectBase asset)
		{
			if (asset is IHasName no)
			{
				return no.GetNameNotEmpty();
			}
			if (asset is IMonoBehaviour mb && mb.IsScriptableObject())
			{
				return mb.NameString;
			}
			return "Unnamed";
		}
		public static void FixScript(IMonoScript script)
		{
			using MD5 md5 = MD5.Create();
			string fullName = $"{script.GetAssemblyNameFixed()}.{script.Namespace_C115}.{script.ClassName_C115}";
			byte[] data = md5.ComputeHash(Encoding.UTF8.GetBytes(fullName));
			SetGUID(script, data);
		}
		public static void SetGUID(IUnityObjectBase asset, byte[] guid)
		{
			byte[]? swapped = new byte[guid.Length];
			for (int i = 0; i < guid.Length; i++)
			{
				byte x = guid[i];
				swapped[i] = (byte)((x & 0x0F) << 4 | (x & 0xF0) >> 4);
			}
			asset.GUID = new UnityGUID(swapped);
		}
		static T CreateInstance<T>(params object[] parameters)
		{
			object instance = typeof(T)
				.GetConstructors(AllBindingFlags)
				.Single(c => c.GetParameters().Length == parameters.Length)
				.Invoke(parameters);
			return (T)instance;
		}
		public static GameCollection CreateGameCollection()
		{
			LayoutInfo? layoutInfo = new LayoutInfo(new UnityVersion(), BuildTarget.StandaloneWin64Player, TransferInstructionFlags.NoTransferInstructionFlags);
			GameCollection? gameCollection = new GameCollection(layoutInfo);
			return gameCollection;
		}
		public static object LoadFile(string filepath)
		{
			FileScheme? scheme = SchemeReader.LoadScheme(filepath, Path.GetFileName(filepath));
			object file = LoadScheme(scheme);
			scheme.Dispose();
			return file;
		}
		private static void AddScheme(FileList fileList, FileSchemeList list)
		{
			foreach (FileScheme? scheme in list.Schemes)
			{
				AddScheme(fileList, scheme);
			}
		}
		private static void AddScheme(FileList fileList, FileScheme scheme)
		{
			object? file = LoadScheme(scheme);
			switch (scheme.SchemeType)
			{
				case FileEntryType.Serialized:
					fileList.AddSerializedFile((SerializedFile)file);
					break;
				case FileEntryType.Bundle:
				case FileEntryType.Archive:
				case FileEntryType.Web:
					fileList.AddSerializedFile((SerializedFile)file);
					break;
				case FileEntryType.Resource:
					fileList.AddResourceFile((ResourceFile)file);
					break;
			}
		}
		private static object LoadScheme(FileScheme scheme)
		{
			object? file = null;
			if (scheme is SerializedFileScheme serializedFileScheme)
			{
				BuildTarget platform = serializedFileScheme.Metadata != null &&
					serializedFileScheme.Metadata.TargetPlatform != 0 ?
					serializedFileScheme.Metadata.TargetPlatform
					: BuildTarget.StandaloneWin64Player;
				UnityVersion version = serializedFileScheme.Metadata != null ?
					serializedFileScheme.Metadata.UnityVersion
					: new UnityVersion();
				LayoutInfo? layoutInfo = new LayoutInfo(version, platform, serializedFileScheme.Flags);
				GameCollection? collection = new GameCollection(layoutInfo);
				collection.AssemblyManager = new Core.Structure.Assembly.Managers.BaseManager(layoutInfo, new Action<string>(str => str.GetType()));
				file = CreateInstance<SerializedFile>(collection, scheme);
				typeof(SerializedFile).GetMethod("ReadData", AllBindingFlags)?
					.Invoke(file, new object[] { serializedFileScheme.Stream });
			}
			if (scheme is BundleFileScheme bundleFileScheme)
			{
				file = CreateInstance<BundleFile>(scheme);
				AddScheme((BundleFile)file, bundleFileScheme);
			}
			if (scheme is ArchiveFileScheme archiveFileScheme)
			{
				file = CreateInstance<ArchiveFile>(scheme);
				AddScheme((ArchiveFile)file, archiveFileScheme);
			}
			if (scheme is WebFileScheme webFileScheme)
			{
				file = CreateInstance<WebFile>(scheme);
				AddScheme((WebFile)file, webFileScheme);
			}
			if (scheme is ResourceFileScheme resourceFileScheme)
			{
				file = CreateInstance<ResourceFile>(scheme);
			}
			return file ?? throw new Exception();
		}
	}
}
