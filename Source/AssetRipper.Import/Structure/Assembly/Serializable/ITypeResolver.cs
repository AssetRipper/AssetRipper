using AssetRipper.SerializationLogic;

namespace AssetRipper.Import.Structure.Assembly.Serializable;

/// <summary>
/// Provides the layout of the types referenced by an asset's [SerializeReference] fields.
/// </summary>
/// <remarks>
/// The layout can come from the type trees of the file the asset was read from, or from the assemblies of the game.
/// </remarks>
public interface ITypeResolver
{
	bool TryGetSerializableType(
		ScriptIdentifier scriptID,
		UnityVersion version,
		[NotNullWhen(true)] out SerializableType? scriptType,
		[NotNullWhen(false)] out string? failureReason);

	/// <summary>
	/// A resolver for assets that cannot have [SerializeReference] fields, such as the engine assets created from the embedded type tree package.
	/// </summary>
	public static ITypeResolver Null { get; } = new NullTypeResolver();

	private sealed class NullTypeResolver : ITypeResolver
	{
		public bool TryGetSerializableType(
			ScriptIdentifier scriptID,
			UnityVersion version,
			[NotNullWhen(true)] out SerializableType? scriptType,
			[NotNullWhen(false)] out string? failureReason)
		{
			scriptType = null;
			failureReason = "No type resolver was provided.";
			return false;
		}
	}
}
