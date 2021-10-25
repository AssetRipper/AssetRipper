using AssetRipper.Core.Parser.Files;
using System;
using System.Collections.Generic;

namespace AssetRipper.Core.Layout
{
	public sealed class AssetLayout
	{
		static AssetLayout()
		{
			ClassNames = InitializeClassNames();
		}

		public AssetLayout(LayoutInfo info)
		{
			Info = info;
		}

		private static Dictionary<ClassIDType, string> InitializeClassNames()
		{
			Dictionary<ClassIDType, string> names = new Dictionary<ClassIDType, string>();
			ClassIDType[] classTypes = (ClassIDType[])Enum.GetValues(typeof(ClassIDType));
			foreach (ClassIDType classType in classTypes)
			{
				names[classType] = classType.ToString();
			}
			return names;
		}

		public string GetClassName(ClassIDType classID)
		{
			if (classID == ClassIDType.PrefabInstance)
				return GetPrefabClassName(Info.Version);
			else if (ClassNames.TryGetValue(classID, out string name))
				return name;
			else
				return null;
		}

		public LayoutInfo Info { get; }

		private static IReadOnlyDictionary<ClassIDType, string> ClassNames { get; }

		private static string GetPrefabClassName(UnityVersion version)
		{
			if (version.IsGreaterEqual(2018, 3))
			{
				return nameof(ClassIDType.PrefabInstance);
			}
			else if (version.IsGreaterEqual(3, 5))
			{
				return nameof(ClassIDType.Prefab);
			}
			else
			{
				return nameof(ClassIDType.DataTemplate);
			}
		}


		public const string TypelessdataName = "_typelessdata";
	}
}
