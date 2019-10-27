using System.Collections.Generic;
using uTinyRipper.Classes;

namespace uTinyRipper
{
	public sealed class DependencyContext : IDependencyContext
	{
		public DependencyContext(bool log)
		{
			IsLog = log;
			m_hierarchy = log ? new Stack<string>() : null;
		}

		public IEnumerable<Object> FetchDependencies<T>(T dependent, string name)
			where T: IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
			foreach (Object asset in dependent.FetchDependencies(this))
			{
				if (asset != null)
				{
					yield return asset;
				}
			}
			if (IsLog)
			{
				m_hierarchy.Pop();
			}
		}

		public IEnumerable<Object> FetchDependencies<T>(T[] dependents, string name)
			where T : IDependent
		{
			return FetchDependencies((IEnumerable<T>)dependents, name);
		}

		public IEnumerable<Object> FetchDependencies<T>(IReadOnlyList<T> dependents, string name)
			where T : IDependent
		{
			return FetchDependencies((IEnumerable<T>)dependents, name);
		}

		public IEnumerable<Object> FetchDependencies<T>(IEnumerable<T> dependents, string name)
			where T : IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
			foreach (T dependent in dependents)
			{
				foreach (Object asset in dependent.FetchDependencies(this))
				{
					if (asset != null)
					{
						yield return asset;
					}
				}
			}
			if (IsLog)
			{
				m_hierarchy.Pop();
			}
		}

		public IEnumerable<Object> FetchDependencies<T>(PPtr<T>[] pointers, string name)
			where T : Object
		{
			return FetchDependencies((IEnumerable<PPtr<T>>)pointers, name);
		}

		public IEnumerable<Object> FetchDependencies<T>(IReadOnlyList<PPtr<T>> pointers, string name)
			where T : Object
		{
			return FetchDependencies((IEnumerable<PPtr<T>>)pointers, name);
		}

		public IEnumerable<Object> FetchDependencies<T>(IEnumerable<PPtr<T>> pointers, string name)
			where T : Object
		{
			foreach (PPtr<T> pointer in pointers)
			{
				Object asset = FetchDependency(pointer, name);
				if (asset != null)
				{
					yield return asset;
				}
			}
		}

		public Object FetchDependency<T>(PPtr<T> pointer, string name)
			where T : Object
		{
			if (pointer.IsNull)
			{
				return null;
			}

			T obj = pointer.FindAsset(File);
			if (obj == null)
			{
				if (IsLog)
				{
					Logger.Log(LogType.Warning, LogCategory.General, $"{GetHierarchy()}'s dependency {name} = {pointer.ToLogString(File)} wasn't found");
				}
			}
			return obj;
		}

		private string GetHierarchy()
		{
			string hierarchy = $"[{File.Name}]" + File.GetAssetLogString(PathID);
			foreach (string sub in m_hierarchy)
			{
				hierarchy += "." + sub;
			}
			return hierarchy;
		}

		public Version Version => File.Version;
		public Platform Platform => File.Platform;
		public TransferInstructionFlags Flags => File.Flags;

		public bool IsLog { get; }
		public IAssetContainer File { get; set; }
		public long PathID { get; set; }

		private readonly Stack<string> m_hierarchy;
	}
}
