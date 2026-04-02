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
	/// One hundred thousand, ie 10^5
	/// </summary>
	private const long TenToTheFifth = 100_000L;

	/// <summary>
	/// The maximum class id that can be used as a prefix for export ids, on versions of Unity that use 64-bit export ids.
	/// </summary>
	/// <remarks>
	/// 9223
	/// </remarks>
	private const int MaxPrefixedClassId_64bit = (int)(long.MaxValue / TenToTheFifthteenth);

	/// <summary>
	/// The maximum class id that can be used as a prefix for export ids, on versions of Unity that use 32-bit export ids.
	/// </summary>
	/// <remarks>
	/// 21474
	/// </remarks>
	private const int MaxPrefixedClassId_32bit = (int)(int.MaxValue / TenToTheFifth);

	// Note: the disparity between the 64-bit and 32-bit max prefixed class ids is largely irrelevant since there's no class ids between them.

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
		if (classID > MaxPrefixedClassId_32bit)
		{
			if (value != 0)
			{
				throw new ArgumentException("Unique asset type with non unique modifier", nameof(value));
			}
			return classID;
		}

		Debug.Assert(value < TenToTheFifth, $"Value {value} for main export ID must have no more than 5 digits");
		return classID * TenToTheFifth + value;
	}

	/// <summary>
	/// Generate a random export id.
	/// </summary>
	/// <param name="asset"></param>
	/// <param name="duplicateChecker"></param>
	/// <returns></returns>
	public static long GetPseudoRandomExportId(IUnityObjectBase asset, int seed)
	{
		ArgumentNullException.ThrowIfNull(asset);

		// We don't bother checking for duplicates here, as the probability of a collision is extremely low.

		long exportID;
		if (asset.Collection.Version.GreaterThanOrEquals(5, 5))
		{
			if (asset.ClassID > MaxPrefixedClassId_64bit)
			{
				//Checked for StreamingController on 2018.2.5f1
				//Small class id's use the below format
				//Whereas this uses random id's
				exportID = GetPseudoRandomValue64(seed);
			}
			else
			{
				long prefix = asset.ClassID * TenToTheFifthteenth;
				ulong value = unchecked((ulong)GetPseudoRandomValue64(seed));
				exportID = prefix + (long)(value % TenToTheFifthteenth);
			}
		}
		else
		{
			if (asset.ClassID > MaxPrefixedClassId_32bit)
			{
				exportID = GetPseudoRandomValue32(seed);
			}
			else
			{
				long prefix = asset.ClassID * TenToTheFifth;
				uint value = unchecked((uint)GetPseudoRandomValue32(seed));
				exportID = prefix + value % TenToTheFifth;
			}
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
	public static long GetPseudoRandomValue64(long seed)
	{
		return unchecked((long)XxHash64.HashToUInt64([], seed));
	}

	/// <summary>
	/// Generate a pseudo random internal id.
	/// </summary>
	/// <remarks>
	/// This uses the XxHash32 algorithm to generate a random-looking <see cref="int"/> from a <see cref="int"/> seed.
	/// </remarks>
	/// <returns>A random-looking <see cref="long"/> between <see cref="int.MinValue"/> and <see cref="int.MaxValue"/>.</returns>
	public static int GetPseudoRandomValue32(int seed)
	{
		return unchecked((int)XxHash32.HashToUInt32([], seed));
	}
}
