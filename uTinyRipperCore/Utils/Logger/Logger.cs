namespace uTinyRipper
{
	public static class Logger
	{
		public static void Log(LogType type, LogCategory category, string message)
        {
            Instance?.Log(type, category, message);
        }

		public static ILogger Instance { get; set; }
	}
}
