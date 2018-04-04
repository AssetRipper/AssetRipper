using System.Collections.Generic;

namespace UtinyRipper.Classes
{
	public static class ObjectUtils
	{
		public static List<Object> CollectDependencies(Object @object, bool isLog = false)
		{
			List<Object> deps = new List<Object>();
			deps.Add(@object);

			for (int i = 0; i < deps.Count; i++)
			{
				Object dep = deps[i];
				foreach (Object newDep in dep.FetchDependencies(dep.File, isLog))
				{
					if (newDep == null)
					{
						continue;
					}

					if (!deps.Contains(newDep))
					{
						deps.Add(newDep);
					}
				}
			}
			
			return deps;
		}
	}
}
