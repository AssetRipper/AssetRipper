using System.Collections.Generic;

namespace AssetRipper.Core.Layout
{
	public static class ClassNameHandler
	{
		private static IReadOnlyDictionary<ClassIDType, string> ClassNames { get; } = InitializeClassNames();

		public const string TypelessdataName = "_typelessdata";

		private static Dictionary<ClassIDType, string> InitializeClassNames()
		{
			Dictionary<ClassIDType, string> names = new Dictionary<ClassIDType, string>();
			ClassIDType[] classTypes = (ClassIDType[])System.Enum.GetValues(typeof(ClassIDType));
			foreach (ClassIDType classType in classTypes)
			{
				names[classType] = classType.ToString();
			}
			return names;
		}

		public static string? GetClassName(this LayoutInfo layout, ClassIDType classID)
		{
			if (classID == ClassIDType.PrefabInstance)
			{
				return GetPrefabClassName(layout.Version);
			}
			else if (ClassNames.TryGetValue(classID, out string? name))
			{
				return name;
			}
			else
			{
				return null;
			}
		}

		private static string GetPrefabClassName(UnityVersion version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return "PrefabInstance";
			}
			else if (version.IsGreaterEqual(3, 5))
			{
				return "Prefab";
			}
			else
			{
				return "DataTemplate";
			}
		}
	}
}
