using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Parser.Asset
{
	public sealed class DependencyContext
	{
		public DependencyContext(LayoutInfo layout, bool log)
		{
			Info = layout;
			IsLog = log;
			m_hierarchy = log ? new Stack<string>() : null;
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependenciesFromDependent<T>(T dependent, string name) where T : IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
			foreach (PPtr<IUnityObjectBase> pointer in dependent.FetchDependencies(this))
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

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependenciesFromArray<T>(IEnumerable<T> dependents, string name) where T : IDependent
		{
			if (IsLog)
			{
				m_hierarchy.Push(name);
			}
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
			if (IsLog)
			{
				m_hierarchy.Pop();
			}
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
			foreach (PPtr<T> pointer in pointers)
			{
				if (!pointer.IsNull)
				{
					yield return FetchDependency(pointer, name);
				}
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies<T1, T2>(IDictionary<T1, PPtr<T2>> pointers, string name) where T2 : IUnityObjectBase
		{
			foreach (KeyValuePair<T1, PPtr<T2>> pointerPair in pointers)
			{
				if (!pointerPair.Value.IsNull)
				{
					yield return FetchDependency(pointerPair.Value, name);
				}
			}
		}

		public PPtr<IUnityObjectBase> FetchDependency<T>(PPtr<T> pointer, string name) where T : IUnityObjectBase
		{
			if (IsLog)
			{
				PointerName = name;
			}
			return pointer.CastTo<IUnityObjectBase>();
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

		public LayoutInfo Info { get; }
		public UnityVersion Version => Info.Version;
		public Platform Platform => Info.Platform;
		public TransferInstructionFlags Flags => Info.Flags;
		public bool IsLog { get; }
		public string PointerName { get; private set; }

		private readonly Stack<string> m_hierarchy;
	}
}
