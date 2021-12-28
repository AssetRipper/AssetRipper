﻿using AssetRipper.Core.Classes;
using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Shader;
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
using AssetRipper.Core.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using YamlDotNet.RepresentationModel;
using Version = AssetRipper.Core.Parser.Files.UnityVersion;

namespace AssetRipper.Library.Utils
{
	public class Util
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
			if (!Directory.Exists(path)) return;
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
			using (MD5 md5 = MD5.Create())
			{
				byte[] hashBytes = md5.ComputeHash(inputBytes);
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					sb.Append(hashBytes[i].ToString("X2"));
				}
				return sb.ToString();
			}
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
			var text = File.ReadAllText(filePath);
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
			var lines = File.ReadAllLines(filePath).ToList();
			lines.Insert(index, replacement);
			File.WriteAllLines(filePath, lines);
		}

		public static List<string> GetManifestDependencies(string filePath)
		{
			var yaml = new YamlStream();
			YamlMappingNode mapping = null;
			using (var fs = File.OpenText(filePath))
			{
				yaml.Load(fs);
				mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
			}
			var dependencies = (YamlSequenceNode)mapping.Children[new YamlScalarNode("Dependencies")];
			var results = dependencies
				.Select(node => Path.GetFileName(((YamlScalarNode)node).Value))
				.ToList();
			return results;
		}

		internal static List<string> GetManifestAssets(string filePath)
		{
			var lines = File.ReadAllLines(filePath).ToList();
			bool isAtDependencies = false;
			var results = new List<string>();
			foreach (var line in lines)
			{
				if (isAtDependencies)
				{
					if (line.StartsWith("- "))
					{
						var dep = line.Replace("- ", "");
						results.Add(Path.GetFileName(dep));
					}
					else
					{
						break;
					}
				}
				else
				{
					if (line.StartsWith("Assets:")) isAtDependencies = true;
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
			var shaderBundle = fileCollection.GameFiles.Values.FirstOrDefault(f => f is SerializedFile sf && Path.GetFileName(sf.FilePath) == "shaders");
			if (shaderBundle != null)
			{
				foreach (var asset in shaderBundle.FetchAssets())
				{
					if (asset is Shader shader)
					{
						using (MD5 md5 = MD5.Create())
						{
							byte[] md5Hash = md5.ComputeHash(Encoding.ASCII.GetBytes(shader.ValidName));
							asset.AssetInfo.GUID = new UnityGUID(md5Hash);
						}
					}
				}
			}
		}
		public static string GetName(IUnityObjectBase asset)
		{
			if (asset is INamedObject no)
			{
				return no.GetValidName();
			}
			if (asset is IMonoBehaviour mb && mb.IsScriptableObject())
			{
				return mb.Name;
			}
			var nameProp = asset.GetType().GetProperty("Name");
			if (nameProp != null)
			{
				return (string)nameProp.GetValue(asset);
			}
			return "Unnamed";
		}
		public static void RandomizeAssetGuid(IEnumerable<AssetRipper.Core.Classes.Object.Object> assets)
		{
			foreach (var asset in assets)
			{
				asset.AssetInfo.GUID = new UnityGUID(Guid.NewGuid());
			}
		}
		public static void FixScript(IMonoScript script)
		{
			using (MD5 md5 = MD5.Create())
			{
				var fullName = $"{script.GetAssemblyNameFixed()}.{script.Namespace}.{script.ClassName}";
				var data = md5.ComputeHash(Encoding.UTF8.GetBytes(fullName));
				SetGUID(script, data);
			}
		}
		public static void SetGUID(IUnityObjectBase asset, byte[] guid)
		{
			var swapped = new byte[guid.Length];
			for (int i = 0; i < guid.Length; i++)
			{
				var x = guid[i];
				swapped[i] = (byte)((x & 0x0F) << 4 | (x & 0xF0) >> 4);
			}
			asset.AssetInfo.GUID = new UnityGUID(swapped);
		}
		static T CreateInstance<T>(params object[] parameters)
		{
			var instance = typeof(T)
				.GetConstructors(AllBindingFlags)
				.Single(c => c.GetParameters().Length == parameters.Length)
				.Invoke(parameters);
			return (T)instance;
		}
		public static GameCollection CreateGameCollection()
		{
			var layoutInfo = new LayoutInfo(new Version(), Platform.StandaloneWin64Player, TransferInstructionFlags.NoTransferInstructionFlags);
			var gameCollection = new GameCollection(layoutInfo);
			return gameCollection;
		}
		public static object LoadFile(string filepath)
		{
			var scheme = SchemeReader.LoadScheme(filepath, Path.GetFileName(filepath));
			object file = LoadScheme(scheme);
			scheme.Dispose();
			return file;
		}
		private static void AddScheme(FileList fileList, FileSchemeList list)
		{
			foreach (var scheme in list.Schemes)
			{
				AddScheme(fileList, scheme);
			}
		}
		private static void AddScheme(FileList fileList, FileScheme scheme)
		{
			var file = LoadScheme(scheme);
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
			object file = null;
			if (scheme is SerializedFileScheme serializedFileScheme)
			{
				var platform = serializedFileScheme.Metadata != null &&
					serializedFileScheme.Metadata.TargetPlatform != 0 ?
					serializedFileScheme.Metadata.TargetPlatform
					: Platform.StandaloneWin64Player;
				var version = serializedFileScheme.Metadata != null ?
					serializedFileScheme.Metadata.UnityVersion
					: new Version();
				var layoutInfo = new LayoutInfo(version, platform, serializedFileScheme.Flags);
				var collection = new GameCollection(layoutInfo);
				collection.AssemblyManager = new Core.Structure.Assembly.Managers.BaseManager(layoutInfo, new Action<string>(str => str.GetType()));
				file = Util.CreateInstance<SerializedFile>(collection, scheme);
				typeof(SerializedFile).GetMethod("ReadData", AllBindingFlags)
					.Invoke(file, new object[] { serializedFileScheme.Stream });
			}
			if (scheme is BundleFileScheme bundleFileScheme)
			{
				file = Util.CreateInstance<BundleFile>(scheme);
				AddScheme((BundleFile)file, bundleFileScheme);
			}
			if (scheme is ArchiveFileScheme archiveFileScheme)
			{
				file = Util.CreateInstance<ArchiveFile>(scheme);
				AddScheme((ArchiveFile)file, archiveFileScheme);
			}
			if (scheme is WebFileScheme webFileScheme)
			{
				file = Util.CreateInstance<WebFile>(scheme);
				AddScheme((WebFile)file, webFileScheme);
			}
			if (scheme is ResourceFileScheme resourceFileScheme)
			{
				file = Util.CreateInstance<ResourceFile>(scheme);
			}
			return file;
		}
	}
}