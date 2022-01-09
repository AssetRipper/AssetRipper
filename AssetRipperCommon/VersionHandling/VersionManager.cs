using AssetRipper.Core.Attributes;
using AssetRipper.Core.Logging;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetRipper.Core.VersionHandling
{
	public static class VersionManager
	{
		private static readonly Dictionary<UnityVersion, UnityHandlerBase> handlers = new(); 
		public static UnityHandlerBase LegacyHandler { get; set; }
		public static UnityHandlerBase SpecialHandler { get; set; }

		static VersionManager()
		{
			LoadHandlers();
		}

		public static UnityHandlerBase GetHandler(UnityVersion version)
		{
			if(SpecialHandler != null)
			{
				return SpecialHandler;
			}
			else if(handlers.TryGetValue(version, out UnityHandlerBase handler))
			{
				return handler;
			}
			else if(LegacyHandler != null)
			{
				return LegacyHandler;
			}
			else
			{
				throw new NotSupportedException($"No handler for {version}");
			}
		}

		private static void LoadHandlers()
		{
			Logger.Info(LogCategory.VersionHandler, "Loading version handlers...");
			string handlerDirectory = ExecutingDirectory.Combine("VersionHandlerAssemblies");
			if (!Directory.Exists(handlerDirectory))
			{
				Directory.CreateDirectory(handlerDirectory);
				Logger.Info(LogCategory.VersionHandler, "Finished loading version handlers.");
				return;
			}

			List<Type> handlerTypes = new();
			foreach (string filePath in Directory.GetFiles(handlerDirectory, "*.dll"))
			{
				try
				{
					Logger.Info(LogCategory.VersionHandler, $"Found assembly at {filePath}");
					Assembly assembly = Assembly.LoadFile(filePath);
					foreach (RegisterVersionHandlerAttribute handlerAttr in assembly.GetCustomAttributes<RegisterVersionHandlerAttribute>())
					{
						handlerTypes.Add(handlerAttr.HandlerType);
					}
				}
				catch (Exception ex)
				{
					Logger.Error(LogCategory.VersionHandler, $"Exception thrown while loading version handler assembly: {filePath}", ex);
				}
			}
			foreach (Type type in handlerTypes)
			{
				try
				{
					UnityHandlerBase versionHandler = (UnityHandlerBase)Activator.CreateInstance(type);
					handlers.Add(versionHandler.UnityVersion, versionHandler);
					Logger.Info(LogCategory.VersionHandler, $"Found version handler: {versionHandler.UnityVersion}");
				}
				catch (Exception ex)
				{
					Logger.Error(LogCategory.VersionHandler, $"Exception thrown while initializing version handler: {type?.FullName ?? "<null>"}", ex);
				}
			}
			Logger.Info(LogCategory.VersionHandler, "Finished loading version handlers.");
		}
	}
}
