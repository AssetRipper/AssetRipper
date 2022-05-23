using AssetRipper.SourceGenerated.Classes.ClassID_21;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class MaterialExtensions
	{
		public static string? FindPropertyNameByCRC28(this IMaterial material, uint crc)
		{
			return material.SavedProperties_C21.FindPropertyNameByCRC28(crc);
		}
	}
}
