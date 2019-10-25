using System;
using System.Linq;
using uTinyRipper.Classes.TerrainDatas;

namespace uTinyRipper.Converters.TerrainDatas
{
	public static class HeightmapConverter
	{
		public static Heightmap Convert(IExportContainer container, ref Heightmap origin)
		{
			Heightmap instance = new Heightmap();
			instance.Heights = origin.Heights.ToArray();
			if (Heightmap.HasShifts(container.ExportVersion))
			{
				instance.Shifts = GetShifts(container, ref origin);
			}
			instance.PrecomputedError = origin.PrecomputedError.ToArray();
			instance.MinMaxPatchHeights = origin.MinMaxPatchHeights.ToArray();
			if (Heightmap.HasDefaultPhysicMaterial(container.ExportVersion))
			{
				instance.DefaultPhysicMaterial = origin.DefaultPhysicMaterial;
			}
			instance.Width = origin.Width;
			instance.Height = origin.Height;
			if (Heightmap.HasThickness(container.ExportVersion))
			{
				instance.Thickness = GetThickness(container, ref origin);
			}
			instance.Levels = origin.Levels;
			instance.Scale = origin.Scale;
			return instance;
		}

		private static Shift[] GetShifts(IExportContainer container, ref Heightmap origin)
		{
			return Heightmap.HasShifts(container.Version) ? origin.Shifts : Array.Empty<Shift>();
		}

		private static float GetThickness(IExportContainer container, ref Heightmap origin)
		{
			return Heightmap.HasThickness(container.Version) ? origin.Thickness : 1.0f;
		}
	}
}
