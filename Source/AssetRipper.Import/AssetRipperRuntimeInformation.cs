using AssetRipper.Import.Utils;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace AssetRipper.Import;

public static partial class AssetRipperRuntimeInformation
{
	public static class Build
	{
		public static bool Debug
		{
			get
			{
#if DEBUG
				return true;
#else
				return false;
#endif
			}
		}

		/// <summary>
		/// Either "Debug" or "Release"
		/// </summary>
		public static string Configuration => Debug ? "Debug" : "Release";

		/// <summary>
		/// Either "Compiled" or "Published"
		/// </summary>
		public static string Type => File.Exists(ExecutingDirectory.Combine("AssetRipper.Assets.dll")) ? "Compiled" : "Published";

		public static string? Version => typeof(AssetRipperRuntimeInformation).Assembly.GetName().Version?.ToString();
	}

	public static string ProcessArchitecture
	{
		get
		{
			if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
			{
				return "x64";
			}
			else if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
			{
				return "x86";
			}
			else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm)
			{
				return "Arm";
			}
			else if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64)
			{
				return "Arm64";
			}
			else
			{
				return "Unknown";
			}
		}
	}

	/// <summary>
	/// Get the current time.
	/// </summary>
	/// <remarks>
	/// This format matches the format used in <see cref="CompileTime"/>
	/// </remarks>
	/// <returns>A string like "Thu Nov 24 18:39:37 UTC 2022"</returns>
	public static string CurrentTime
	{
		get
		{
			DateTime now = DateTime.UtcNow;
			StringBuilder sb = new();
			sb.Append(now.DayOfWeek switch
			{
				DayOfWeek.Sunday => "Sun",
				DayOfWeek.Monday => "Mon",
				DayOfWeek.Tuesday => "Tue",
				DayOfWeek.Wednesday => "Wed",
				DayOfWeek.Thursday => "Thu",
				DayOfWeek.Friday => "Fri",
				DayOfWeek.Saturday => "Sat",
				_ => throw new NotSupportedException(),
			});
			sb.Append(' ');
			sb.Append(now.Month switch
			{
				1 => "Jan",
				2 => "Feb",
				3 => "Mar",
				4 => "Apr",
				5 => "May",
				6 => "Jun",
				7 => "Jul",
				8 => "Aug",
				9 => "Sep",
				10 => "Oct",
				11 => "Nov",
				12 => "Dec",
				_ => throw new NotSupportedException(),
			});
			sb.Append(' ');
			sb.Append($"{now.Day,2}");
			sb.Append(' ');
			sb.Append(now.TimeOfDay.Hours.ToString("00", CultureInfo.InvariantCulture));
			sb.Append(':');
			sb.Append(now.TimeOfDay.Minutes.ToString("00", CultureInfo.InvariantCulture));
			sb.Append(':');
			sb.Append(now.TimeOfDay.Seconds.ToString("00", CultureInfo.InvariantCulture));
			sb.Append(" UTC ");
			sb.Append(now.Year);
			return sb.ToString();
		}
	}

	/// <summary>
	/// Get the time the application was compiled.
	/// </summary>
	/// <remarks>
	/// This format matches the format used in <see cref="CurrentTime"/>
	/// </remarks>
	/// <returns>A string like "Thu Nov 24 18:39:37 UTC 2022"</returns>
	public static string CompileTime
	{
		get
		{
			string path = ExecutingDirectory.Combine("compile_time.txt");
			if (File.Exists(path))
			{
				return File.ReadAllText(path).Trim();
			}
			else
			{
				return "Unknown";
			}
		}
	}

	public static class OS
	{
		public static string Name
		{
			get
			{
				if (OperatingSystem.IsWindows())
				{
					return "Windows";
				}
				else if (OperatingSystem.IsLinux())
				{
					return "Linux";
				}
				else if (OperatingSystem.IsMacOS())
				{
					return "MacOS";
				}
				else if (OperatingSystem.IsBrowser())
				{
					return "Browser";
				}
				else if (OperatingSystem.IsAndroid())
				{
					return "Android";
				}
				else if (OperatingSystem.IsIOS())
				{
					return "iOS";
				}
				else if (OperatingSystem.IsFreeBSD())
				{
					return "FreeBSD";
				}
				else
				{
					return "Other";
				}
			}
		}

		public static string Version => Environment.OSVersion.VersionString;
	}

	public static string RamQuantity
	{
		get
		{
			if (TryGetSystemMemory(out long totalMemoryInKilobytes))
			{
				return $"{totalMemoryInKilobytes / 1024 / 1024} GB";
			}
			else
			{
				return "Unknown GB";
			}
		}
	}

	private static bool TryGetSystemMemory(out long totalMemoryInKilobytes)
	{
		if (OperatingSystem.IsWindows())
		{
			return TryGetSystemMemoryWindows(out totalMemoryInKilobytes);
		}
		else if (OperatingSystem.IsLinux())
		{
			return TryGetSystemMemoryLinux(out totalMemoryInKilobytes);
		}
		else if (OperatingSystem.IsMacOS())
		{
			return TryGetSystemMemoryMacOS(out totalMemoryInKilobytes);
		}
		else
		{
			totalMemoryInKilobytes = default;
			return false;
		}
	}

	[SupportedOSPlatform("windows")]
	[LibraryImport("kernel32.dll", EntryPoint = "GetPhysicallyInstalledSystemMemory")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool TryGetSystemMemoryWindows(out long totalMemoryInKilobytes);

	[SupportedOSPlatform("macos")]
	private static bool TryGetSystemMemoryMacOS(out long totalMemoryInKilobytes)
	{
		try
		{
			using Process process = new()
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "/usr/sbin/sysctl",
					Arguments = "hw.memsize",
					RedirectStandardOutput = true,
					UseShellExecute = false,
					CreateNoWindow = true
				}
			};
			process.Start();
			string output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();

			// Output is in format: hw.memsize: 17179869184
			string[] parts = output.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length == 2 && long.TryParse(parts[1], out long memoryInBytes))
			{
				totalMemoryInKilobytes = memoryInBytes / 1024;
				return true;
			}
		}
		catch
		{
		}
		totalMemoryInKilobytes = default;
		return false;
	}

	[SupportedOSPlatform("linux")]
	private static bool TryGetSystemMemoryLinux(out long totalMemoryInKilobytes)
	{
		totalMemoryInKilobytes = 0;
		try
		{
			using StreamReader reader = new("/proc/meminfo");
			string? line;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.StartsWith("MemTotal:", StringComparison.Ordinal))
				{
					string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length == 3 && long.TryParse(parts[1], out long memoryInKilobytes))
					{
						totalMemoryInKilobytes = memoryInKilobytes;
						return true;
					}
				}
			}
		}
		catch
		{
		}
		return false;
	}
}
