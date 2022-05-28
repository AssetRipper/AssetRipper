using AssetRipper.Core.Logging;

namespace AssetRipper.Library
{
	public abstract class PluginBase
	{
		public Ripper CurrentRipper { get; internal set; }
		public virtual void Initialize() { }
		public abstract string Name { get; }
		public void Info(string message) => Logger.Info(LogCategory.Plugin, $"[{Name}] {message}");
		public void Warning(string message) => Logger.Warning(LogCategory.Plugin, $"[{Name}] {message}");
		public void Error(string message) => Logger.Error(LogCategory.Plugin, $"[{Name}] {message}");
		public void Error(string message, Exception exception) => Logger.Error(LogCategory.Plugin, $"[{Name}] {message}", exception);
		public void Error(Exception exception) => Logger.Error(LogCategory.Plugin, $"[{Name}] Exception thrown", exception);
		public void Verbose(string message) => Logger.Verbose(LogCategory.Plugin, $"[{Name}] {message}");
	}
}
