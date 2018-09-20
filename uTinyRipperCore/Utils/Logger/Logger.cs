namespace uTinyRipper
{
	public static class Logger
	{
		internal static void Log(LogType type, LogCategory category, string message)
		{
			if(Instance != null)
			{
				Instance.Log(type, category, message);
			}
		}

		public static ILogger Instance { get; set; }
	}
}
