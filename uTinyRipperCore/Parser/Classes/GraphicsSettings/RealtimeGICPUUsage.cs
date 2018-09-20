namespace uTinyRipper.Classes.GraphicsSettingss
{
	/// <summary>
	/// How much CPU usage to assign to the final lighting calculations at runtime.
	/// </summary>
	public enum RealtimeGICPUUsage
	{
		/// <summary>
		/// 25% of the allowed CPU threads are used as worker threads.
		/// </summary>
		Low			= 25,
		/// <summary>
		/// 50% of the allowed CPU threads are used as worker threads.
		/// </summary>
		Medium		= 50,
		/// <summary>
		/// 75% of the allowed CPU threads are used as worker threads.
		/// </summary>
		High		= 75,
		/// <summary>
		/// 100% of the allowed CPU threads are used as worker threads.
		/// </summary>
		Unlimited	= 100
	}
}
