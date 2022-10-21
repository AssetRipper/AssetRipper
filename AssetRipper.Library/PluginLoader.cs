using AssetRipper.Core.Logging;
using AssetRipper.Core.Utils;
using AssetRipper.Library.Attributes;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AssetRipper.Library
{
	internal static class PluginLoader
	{
		internal static void LoadPlugins(Ripper ripper)
		{
			Logger.Info(LogCategory.Plugin, "Loading plugins...");
			string pluginsDirectory = ExecutingDirectory.Combine("Plugins");
			Directory.CreateDirectory(pluginsDirectory);
			List<Type> pluginTypes = new();
			foreach (string dllFile in Directory.GetFiles(pluginsDirectory, "*.dll"))
			{
				try
				{
					Logger.Info(LogCategory.Plugin, $"Found assembly at {dllFile}");
#pragma warning disable IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
					Assembly assembly = Assembly.LoadFile(dllFile);
#pragma warning restore IL2026 // Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code
					foreach (RegisterPluginAttribute pluginAttr in assembly.GetCustomAttributes<RegisterPluginAttribute>())
					{
						pluginTypes.Add(pluginAttr.PluginType);
					}
				}
				catch (Exception ex)
				{
					Logger.Error(LogCategory.Plugin, $"Exception thrown while loading plugin assembly: {dllFile}", ex);
				}
			}
			foreach (Type type in pluginTypes)
			{
				try
				{
#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
					PluginBase? plugin = Activator.CreateInstance(type) as PluginBase;
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
					if (plugin is not null)
					{
						plugin.CurrentRipper = ripper;
						plugin.Initialize();
						Logger.Info(LogCategory.Plugin, $"Initialized plugin: {plugin.Name}");
					}
					else
					{
						Logger.Warning(LogCategory.Plugin, $"Could not create plugin: {ToFullName(type)}");
					}
				}
				catch (Exception ex)
				{
					Logger.Error(LogCategory.Plugin, $"Exception thrown while initializing plugin: {ToFullName(type)}", ex);
				}
			}
			Logger.Info(LogCategory.Plugin, "Finished loading plugins.");
		}

		private static string ToFullName(Type type)
		{
			return type?.FullName ?? "<null>";
		}
	}
}
