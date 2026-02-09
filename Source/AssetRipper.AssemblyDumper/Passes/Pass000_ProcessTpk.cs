using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.Tpk;
using AssetRipper.Tpk.TypeTrees;

namespace AssetRipper.AssemblyDumper.Passes;

internal static class Pass000_ProcessTpk
{
	private static UnityVersion MinimumVersion { get; } = new UnityVersion(3, 5, 0);

	public static void IntitializeSharedState(string tpkPath)
	{
		TpkTypeTreeBlob blob = ReadAndProcessTpkFile(tpkPath);
		Console.WriteLine($"\tCreation time: {blob.CreationTime.ToLocalTime()}");
		Dictionary<int, VersionedList<UniversalClass>> classes = new();
		foreach (TpkClassInformation classInfo in blob.ClassInformation)
		{
			int id = classInfo.ID;

			if (id is 129) // PlayerSettings
			{
				continue;
			}

			VersionedList<UniversalClass> classList = new();
			classes.Add(id, classList);
			for (int i = 0; i < classInfo.Classes.Count; i++)
			{
				KeyValuePair<UnityVersion, TpkUnityClass?> pair = classInfo.Classes[i];
				if (pair.Value is not null)
				{
					UniversalClass universalClass = UniversalClass.FromTpkUnityClass(pair.Value, id, blob.StringBuffer, blob.NodeBuffer);
					classList.Add(pair.Key, universalClass);
				}
				else
				{
					classList.Add(pair.Key, null);
				}
			}
		}
		UniversalCommonString commonString = UniversalCommonString.FromBlob(blob);
		UnityVersion[] usedVersions = blob.Versions.Where(v => v >= MinimumVersion).ToArray();
		SharedState.Initialize(usedVersions, classes, commonString, WriteTpkFile(blob));
	}

	private static TpkTypeTreeBlob ReadAndProcessTpkFile(string tpkPath)
	{
		TpkTypeTreeBlob blob = ReadTpkFile(tpkPath);
		Dictionary<UnityVersion, UnityVersion> versionRedirectDictionary = MakeVersionRedirectDictionary(blob.Versions);
		for (int i = blob.ClassInformation.Count - 1; i >= 0; i--)
		{
			TpkClassInformation classInfo = blob.ClassInformation[i];

			if (IsUnacceptable(classInfo.ID) || HasNoDataAfterMinimumVersion(classInfo))
			{
				blob.ClassInformation.RemoveAt(i);
			}
		}
		foreach (TpkClassInformation classInfo in blob.ClassInformation)
		{
			int i = 0;
			while (i < classInfo.Classes.Count)
			{
				KeyValuePair<UnityVersion, TpkUnityClass?> pair = classInfo.Classes[i];
				UnityVersion version = versionRedirectDictionary[pair.Key];
				if (version == MinimumVersion && i < classInfo.Classes.Count - 1 && versionRedirectDictionary[classInfo.Classes[i + 1].Key] == MinimumVersion)
				{
					//Delete. This TpkUnityClass conflicts with the next one because they're both redirected to the minimum version.
					classInfo.Classes.RemoveAt(i);
				}
				else
				{
					classInfo.Classes[i] = new(version, pair.Value);
					i++;
				}
			}

			while (classInfo.Classes[0].Value is null)
			{
				classInfo.Classes.RemoveAt(0);
			}
		}
		return blob;

		static bool IsUnacceptable(int typeId) => typeId is >= 100000 and <= 100011;

		static bool HasNoDataAfterMinimumVersion(TpkClassInformation info)
		{
			KeyValuePair<UnityVersion, TpkUnityClass?> lastPair = info.Classes[info.Classes.Count - 1];
			return lastPair.Key < MinimumVersion && lastPair.Value is null;
		}
	}

	private static TpkTypeTreeBlob ReadTpkFile(string path)
	{
		TpkDataBlob blob = TpkFile.FromFile(path).GetDataBlob();
		return blob is TpkTypeTreeBlob typeTreeBlob
			? typeTreeBlob
			: throw new NotSupportedException($"Blob cannot be type {blob.GetType()}");
	}

	private static byte[] WriteTpkFile(TpkTypeTreeBlob blob)
	{
		return TpkFile.FromBlob(blob, TpkCompressionType.Brotli).WriteToMemory();
	}

	private static Dictionary<UnityVersion, UnityVersion> MakeVersionRedirectDictionary(List<UnityVersion> list)
	{
		Dictionary<UnityVersion, UnityVersion> dict = new();

		UnityVersion first = list.First(v => v >= MinimumVersion);
		dict.Add(first, first.StripType());

		int firstIndex = list.IndexOf(first);
		for (int i = 0; i < firstIndex; i++)
		{
			dict.Add(list[i], MinimumVersion);
		}
		for (int i = firstIndex + 1; i < list.Count; i++)
		{
			UnityVersion previous = list[i - 1];
			UnityVersion current = list[i];
			if (current.Major != previous.Major)
			{
				dict.Add(current, current.StripMinor());
			}
			else if (current.Minor != previous.Minor)
			{
				dict.Add(current, current.StripBuild());
			}
			else if (current.Build != previous.Build)
			{
				dict.Add(current, current.StripType());
			}
			else if (current.Type != previous.Type)
			{
				dict.Add(current, current.StripTypeNumber());
			}
			else
			{
				dict.Add(current, current);
			}
		}
		return dict;
	}
}
