using AssetRipper.SerializationLogic;
using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

public interface ITypeResolver
{
	bool TryGetSerializableType(
		ScriptIdentifier scriptID,
		UnityVersion version,
		[NotNullWhen(true)] out SerializableType? scriptType,
		[NotNullWhen(false)] out string? failureReason);

	public static ITypeResolver Null { get; } = new NullResolver();

	private sealed class NullResolver : ITypeResolver
	{
		public bool TryGetSerializableType(
			ScriptIdentifier scriptID,
			UnityVersion version,
			[NotNullWhen(true)] out SerializableType? scriptType,
			[NotNullWhen(false)] out string? failureReason)
		{
			scriptType = null;
			failureReason = "No resolver provided";
			return false;
		}
	}
}
