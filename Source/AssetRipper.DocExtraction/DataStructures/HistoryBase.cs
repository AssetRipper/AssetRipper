using AssetRipper.AssemblyDumper.Utils;
using AssetRipper.DocExtraction.MetaData;
using AssetRipper.Primitives;
using System.Text.Json.Serialization;

namespace AssetRipper.DocExtraction.DataStructures;

public abstract class HistoryBase
{
	public string Name { get; set; } = "";
	public VersionedList<bool> Exists { get; set; } = new();
	public VersionedList<string> NativeName { get; set; } = new();
	public VersionedList<string> ObsoleteMessage { get; set; } = new();
	public VersionedList<string> DocumentationString { get; set; } = new();
	[JsonIgnore]
	public string? InjectedDocumentation { get; set; }
	[JsonIgnore]
	public UnityVersion MinimumVersion
	{
		get
		{
			return Exists.Count > 0 ? Exists[0].Key : UnityVersion.MinVersion;
		}
	}
	public override string ToString() => Name;

	public virtual void Initialize(UnityVersion version, DocumentationBase first)
	{
		Name = first.Name;
		ObsoleteMessage.Add(version, first.ObsoleteMessage);
		DocumentationString.Add(version, first.DocumentationString);
		NativeName.Add(version, first.NativeName);
		Exists.Add(version, true);
	}

	public void Add(UnityVersion version, DocumentationBase? next)
	{
		if (next is null)
		{
			if (Exists.GetLastValue() is not false)
			{
				Exists.Add(version, false);
			}
		}
		else
		{
			if (Exists.GetLastValue() is not true)
			{
				Exists.Add(version, true);
			}
			AddNotNull(version, next);
		}
	}

	protected virtual void AddNotNull(UnityVersion version, DocumentationBase next)
	{
		AddIfNotEqual(NativeName, version, next.NativeName);
		AddIfNotEqual(ObsoleteMessage, version, next.ObsoleteMessage);
		AddIfNotEqual(DocumentationString, version, next.DocumentationString, false);
	}

	protected static void AddIfNotEqual<T>(VersionedList<T> versionedList, UnityVersion version, T? element)
	{
		if (!EqualityComparer<T>.Default.Equals(versionedList.GetLastValue(), element))
		{
			versionedList.Add(version, element);
		}
	}

	protected static void AddIfNotEqual(VersionedList<string> versionedList, UnityVersion version, string? element)
	{
		AddIfNotEqual(versionedList, version, element, true);
	}

	protected static void AddIfNotEqual(VersionedList<string> versionedList, UnityVersion version, string? element, bool allowNullReplacement)
	{
		string? lastValue = versionedList.GetLastValue();
		if (element is null)
		{
			if (allowNullReplacement && lastValue is not null)
			{
				versionedList.Add(version, null);
			}
		}
		else if (lastValue != element && (lastValue is null || lastValue.ToLowerInvariant() != element.ToLowerInvariant()))
		{
			versionedList.Add(version, element);
		}
	}

	public bool ExistsOnVersion(UnityVersion version)
	{
		return Exists.GetItemForVersion(version);
	}
}
