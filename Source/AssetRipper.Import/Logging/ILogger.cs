namespace AssetRipper.Import.Logging;

public interface ILogger
{
	void Log(LogType type, LogCategory category, string message);

	void BlankLine(int numLines);
}
