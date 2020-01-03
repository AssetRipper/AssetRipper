using System.Collections.Generic;
using uTinyRipper.Classes;
using uTinyRipper.Layout;

namespace uTinyRipper
{
	public sealed class DependencyContext
	{
		public DependencyContext(AssetLayout layout, bool log)
		{
			Layout = layout;
			IsLog = log;
			m_hierarchy = log ? new Stack<string>() : null;
		}

		public IEnumerable<PPtr<Object>> FetchDependencies<T>(T dependent, string name)
			where T: IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
			foreach (PPtr<Object> pointer in dependent.FetchDependencies(this))
			{
				if (!pointer.IsNull)
				{
					yield return pointer;
				}
			}
			if (IsLog)
			{
				m_hierarchy.Pop();
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies<T>(T[] dependents, string name)
			where T : IDependent
		{
			return FetchDependencies((IEnumerable<T>)dependents, name);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies<T>(IReadOnlyList<T> dependents, string name)
			where T : IDependent
		{
			return FetchDependencies((IEnumerable<T>)dependents, name);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies<T>(IEnumerable<T> dependents, string name)
			where T : IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
			foreach (T dependent in dependents)
			{
				foreach (PPtr<Object> pointer in dependent.FetchDependencies(this))
				{
					if (!pointer.IsNull)
					{
						yield return pointer;
					}
				}
			}
			if (IsLog)
			{
				m_hierarchy.Pop();
			}
		}

		public IEnumerable<PPtr<Object>> FetchDependencies<T>(PPtr<T>[] pointers, string name)
			where T : Object
		{
			return FetchDependencies((IEnumerable<PPtr<T>>)pointers, name);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies<T>(IReadOnlyList<PPtr<T>> pointers, string name)
			where T : Object
		{
			return FetchDependencies((IEnumerable<PPtr<T>>)pointers, name);
		}

		public IEnumerable<PPtr<Object>> FetchDependencies<T>(IEnumerable<PPtr<T>> pointers, string name)
			where T : Object
		{
			foreach (PPtr<T> pointer in pointers)
			{
				if (!pointer.IsNull)
				{
					yield return FetchDependency(pointer, name);
				}
			}
		}

		public PPtr<Object> FetchDependency<T>(PPtr<T> pointer, string name)
			where T : Object
		{
			if (IsLog)
			{
				PointerName = name;
			}
			return pointer.CastTo<Object>();
		}

		public string GetPointerPath()
		{
			if (m_hierarchy.Count == 0)
			{
				return string.Empty;
			}

			string hierarchy = string.Empty;
			int i = 0;
			foreach (string sub in m_hierarchy)
			{
				if (i == 0)
				{
					hierarchy = sub;
				}
				else
				{
					hierarchy = sub + "." + hierarchy;
				}
				i++;
			}
			return hierarchy;
		}

		public AssetLayout Layout { get; }
		public Version Version => Layout.Info.Version;
		public Platform Platform => Layout.Info.Platform;
		public TransferInstructionFlags Flags => Layout.Info.Flags;
		public bool IsLog { get; }
		public string PointerName { get; private set; }

		private readonly Stack<string> m_hierarchy;
	}
}
