namespace UtinyRipper
{
	public interface ILogger
	{
		void Log(LogType type, LogCategory category, string message);
	}
}
