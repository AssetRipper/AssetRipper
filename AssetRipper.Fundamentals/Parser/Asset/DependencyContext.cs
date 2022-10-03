using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Core.Parser.Asset
{
	public sealed class DependencyContext
	{
		public DependencyContext(bool log)
		{
			m_hierarchy = log ? new Stack<string>() : null;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependenciesFromDependent<T>(T dependent, string name) where T : IDependent
		{
			m_hierarchy?.Push(name);
			foreach (PPtr<IUnityObjectBase> pointer in dependent.FetchDependencies(this))
			{
				if (!pointer.IsNull)
				{
					yield return pointer;
				}
			}
			m_hierarchy?.Pop();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependenciesFromArray<T>(IEnumerable<T> dependents, string name) where T : IDependent
		{
			m_hierarchy?.Push(name);
			foreach (T dependent in dependents)
			{
				foreach (PPtr<IUnityObjectBase> pointer in dependent.FetchDependencies(this))
				{
					if (!pointer.IsNull)
					{
						yield return pointer;
					}
				}
			}
			m_hierarchy?.Pop();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependenciesFromArrayArray<T>(IEnumerable<T> dependents, string name) where T : IEnumerable<IDependent>
		{
			//Logging not required here because FetchDependenciesFromArray handles it
			foreach (T subArray in dependents)
			{
				foreach (PPtr<IUnityObjectBase> pointer in this.FetchDependenciesFromArray(subArray, name))
				{
					yield return pointer; //this pointer is not null because FetchDependenciesFromArray already checks for that 
				}
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies<T>(IEnumerable<PPtr<T>> pointers, string name) where T : IUnityObjectBase
		{
			return pointers.Where(pointer => !pointer.IsNull).Select(pointer => FetchDependency(pointer, name));
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies<T1, T2>(IDictionary<T1, PPtr<T2>> pointers, string name) where T2 : IUnityObjectBase
		{
			return pointers.Where(pointerPair => !pointerPair.Value.IsNull).Select(pointerPair => FetchDependency(pointerPair.Value, name));
		}

		public PPtr<IUnityObjectBase> FetchDependency<T>(PPtr<T> pointer, string name) where T : IUnityObjectBase
		{
			if (m_hierarchy is not null)
			{
				PointerName = name;
			}
			return pointer.CastTo<IUnityObjectBase>();
		}

		public string GetPointerPath()
		{
			if (m_hierarchy is null || m_hierarchy.Count == 0)
			{
				return string.Empty;
			}

			return string.Join('.', m_hierarchy.Reverse());
		}

		public string PointerName { get; private set; } = "";

		private readonly Stack<string>? m_hierarchy;
	}
}
