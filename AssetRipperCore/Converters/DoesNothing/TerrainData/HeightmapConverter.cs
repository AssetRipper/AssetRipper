using AssetRipper.Core.Classes.TerrainData;
using AssetRipper.Core.Project;
using System;
using System.Linq;

namespace AssetRipper.Core.Converters.TerrainData
{
	public static class HeightmapConverter
	{
		public static Heightmap Convert(IExportContainer container, Heightmap origin)
		{
			Heightmap instance = new Heightmap();
			instance.Heights = origin.Heights.ToArray();
			if (Heightmap.HasHoles(container.ExportVersion))
			{
				instance.Holes = GetHoles(container, origin);
				instance.HolesLOD = GetHolesLOD(container, origin);
				instance.EnableHolesTextureCompression = GetEnableHolesTextureCompression(container, origin);
			}
			if (Heightmap.HasShifts(container.ExportVersion))
			{
				instance.Shifts = GetShifts(container, origin);
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
				instance.Thickness = GetThickness(container, origin);
			}
			instance.Levels = origin.Levels;
			instance.m_Scale = origin.m_Scale;
			return instance;
		}

		private static byte[] GetHoles(IExportContainer container, Heightmap origin)
		{
			return Heightmap.HasHoles(container.Version) ? origin.Holes.ToArray() : Array.Empty<byte>();
		}

		private static byte[] GetHolesLOD(IExportContainer container, Heightmap origin)
		{
			return Heightmap.HasHoles(container.Version) ? origin.HolesLOD.ToArray() : Array.Empty<byte>();
		}

		private static bool GetEnableHolesTextureCompression(IExportContainer container, Heightmap origin)
		{
			return Heightmap.HasHoles(container.Version) ? origin.EnableHolesTextureCompression : true;
		}

		private static Shift[] GetShifts(IExportContainer container, Heightmap origin)
		{
			return Heightmap.HasShifts(container.Version) ? origin.Shifts.ToArray() : Array.Empty<Shift>();
		}

		private static float GetThickness(IExportContainer container, Heightmap origin)
		{
			return Heightmap.HasThickness(container.Version) ? origin.Thickness : 1.0f;
		}
	}
}
