using System.Collections.Generic;
using System.Threading.Tasks;

namespace AssetRipper.Core
{
	public static class TaskManager
	{
		private static readonly List<Task> tasks = new List<Task>();

		public static void AddTask(Task task) => tasks.Add(task);

		public static void WaitUntilAllCompleted()
		{
			foreach (Task task in tasks)
			{
				task.Wait();
			}
			tasks.Clear();
		}
	}
}
