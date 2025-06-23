using AssetRipper.Assets;
using System.Diagnostics;

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
	public static long GetRandomExportId(IUnityObjectBase asset, Func<long, bool> duplicateChecker)
	{
		ArgumentNullException.ThrowIfNull(asset);
		ArgumentNullException.ThrowIfNull(duplicateChecker);

#warning TODO: depending on the export version exportID should has random or ordered value

		long exportID;
		if (asset.ClassID > MaxPrefixedClassId)
		{
			do
			{
				//Checked for StreamingController on 2018.2.5f1
				//Small class id's use the below format
				//Whereas this uses random id's
				exportID = GetInternalId();
			}
			while (duplicateChecker(exportID));
		}
		else
		{
			long prefix = asset.ClassID * TenToTheFifthteenth;
			ulong persistentValue = 0;
			do
			{
				ulong value = unchecked((ulong)GetInternalId());
				persistentValue = unchecked(persistentValue + value);
				exportID = prefix + (long)(persistentValue % TenToTheFifthteenth);
			}
			while (duplicateChecker(exportID));
		}

		return exportID;
	}

	/// <summary>
	/// Generate a random internal id.
	/// </summary>
	/// <returns>A random <see cref="long"/> between <see cref="long.MinValue"/> and <see cref="long.MaxValue"/>.</returns>
	public static long GetInternalId()
	{
		Span<byte> buffer = stackalloc byte[8];
		Random.Shared.NextBytes(buffer);
		return BitConverter.ToInt64(buffer);
	}
}
