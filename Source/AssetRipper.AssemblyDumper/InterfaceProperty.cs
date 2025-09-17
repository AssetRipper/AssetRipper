using AssetRipper.Numerics;

namespace AssetRipper.AssemblyDumper;

internal class InterfaceProperty : PropertyBase
{
	private readonly List<ClassProperty> implementations = new();
	private DiscontinuousRange<UnityVersion>? presentRange;
	private DiscontinuousRange<UnityVersion>? absentRange;
	private DiscontinuousRange<UnityVersion>? releaseOnlyRange;
	private DiscontinuousRange<UnityVersion>? editorOnlyRange;
	private DiscontinuousRange<UnityVersion>? notReleaseOnlyRange;
	private DiscontinuousRange<UnityVersion>? notEditorOnlyRange;

	public InterfaceProperty(PropertyDefinition definition, ClassGroupBase group) : base(definition)
	{
		Group = group;
	}

	public DiscontinuousRange<UnityVersion> PresentRange
	{
		get
		{
			presentRange ??= CalculateRange(p => p.IsPresent);
			return presentRange.Value;
		}
	}

	public DiscontinuousRange<UnityVersion> AbsentRange
	{
		get
		{
			absentRange ??= CalculateRange(p => p.IsAbsent);
			return absentRange.Value;
		}
	}

	public IReadOnlyList<ClassProperty> Implementations => implementations;

	public ClassGroupBase Group { get; }

	public DiscontinuousRange<UnityVersion> ReleaseOnlyRange
	{
		get
		{
			releaseOnlyRange ??= CalculateRange(p => p.IsReleaseOnly);
			return releaseOnlyRange.Value;
		}
	}

	public DiscontinuousRange<UnityVersion> EditorOnlyRange
	{
		get
		{
			editorOnlyRange ??= CalculateRange(p => p.IsEditorOnly);
			return editorOnlyRange.Value;
		}
	}

	public DiscontinuousRange<UnityVersion> NotReleaseOnlyRange
	{
		get
		{
			notReleaseOnlyRange ??= CalculateRange(p => !p.IsReleaseOnly);
			return notReleaseOnlyRange.Value;
		}
	}

	public DiscontinuousRange<UnityVersion> NotEditorOnlyRange
	{
		get
		{
			notEditorOnlyRange ??= CalculateRange(p => !p.IsEditorOnly);
			return notEditorOnlyRange.Value;
		}
	}

	internal void AddImplementation(ClassProperty implementation)
	{
		implementations.Add(implementation);
		presentRange = null;
		absentRange = null;
		releaseOnlyRange = null;
		editorOnlyRange = null;
		notReleaseOnlyRange = null;
		notEditorOnlyRange = null;
	}

	private DiscontinuousRange<UnityVersion> CalculateRange(Func<ClassProperty, bool> predicate)
	{
		return new DiscontinuousRange<UnityVersion>(implementations.Where(predicate).Select(static p => p.Class.VersionRange));
	}
}
