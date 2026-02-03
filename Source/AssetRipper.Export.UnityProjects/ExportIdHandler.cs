using AssetRipper.Assets;
using System.Diagnostics;
using System.IO.Hashing;

namespace AssetRipper.Export.UnityProjects;

public static class ExportIdHandler
{
	/// <summary>
	/// One quadrillion, ie 10^15
	/// </summary>
	private const long TenToTheFifthteenth = 1_000_000_000_000_000L;

	/// <summary>
	/// The maximum class id that can be used as a prefix for export ids.
	/// </summary>
	/// <remarks>
	/// 9223
	/// </remarks>
	private const int MaxPrefixedClassId = (int)(long.MaxValue / TenToTheFifthteenth);

	public static long GetMainExportID(IUnityObjectBase asset)
	{
		return GetMainExportID(asset.ClassID, 0);
	}

	public static long GetMainExportID(int classID)
	{
		return GetMainExportID(classID, 0);
	}

	public static long GetMainExportID(IUnityObjectBase asset, uint value)
	{
		return GetMainExportID(asset.ClassID, value);
	}

	public static long GetMainExportID(int classID, uint value)
	{
		if (classID > 100100)
		{
			if (value != 0)
			{
				throw new ArgumentException("Unique asset type with non unique modifier", nameof(value));
			}
			return classID;
		}

		Debug.Assert(value < 100000, $"Value {value} for main export ID must have no more than 5 digits");
		return classID * 100000L + value;
	}

	/// <summary>
	/// Generate a random export id.
	/// </summary>
	/// <param name="asset"></param>
	/// <param name="duplicateChecker"></param>
	/// <returns></returns>
	public static long GetPseudoRandomExportId(IUnityObjectBase asset, long seed)
	{
		ArgumentNullException.ThrowIfNull(asset);

		// Depending on the export version, exportID should has random or ordered value.
		// However, I don't have any idea where the version threshold is, and it has never seemed to matter.

		// We don't bother checking for duplicates here, as the probability of a collision is extremely low.

		long exportID;
		if (asset.ClassID > MaxPrefixedClassId)
		{
			//Checked for StreamingController on 2018.2.5f1
			//Small class id's use the below format
			//Whereas this uses random id's
			exportID = GetPseudoRandomValue(seed);
		}
		else
		{
			long prefix = asset.ClassID * TenToTheFifthteenth;
			ulong value = unchecked((ulong)GetPseudoRandomValue(seed));
			exportID = prefix + (long)(value % TenToTheFifthteenth);
		}

		return exportID;
	}

	/// <summary>
	/// Generate a pseudo random internal id.
	/// </summary>
	/// <remarks>
	/// This uses the XxHash64 algorithm to generate a random-looking <see cref="long"/> from a <see cref="long"/> seed.
	/// </remarks>
	/// <returns>A random-looking <see cref="long"/> between <see cref="long.MinValue"/> and <see cref="long.MaxValue"/>.</returns>
	public static long GetPseudoRandomValue(long seed)
	{
		return unchecked((long)XxHash64.HashToUInt64([], seed));
	}
}
