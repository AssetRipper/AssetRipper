namespace AssetRipper.Export.Modules.Shaders.ShaderBlob;

/// <summary>
/// SerializedBindChannels
/// </summary>
public sealed class ParserBindChannels
{
	public ParserBindChannels() { }

	public ParserBindChannels(ShaderBindChannel[] channels, int sourceMap)
	{
		Channels = channels;
		SourceMap = sourceMap;
	}

	public ShaderBindChannel[] Channels { get; set; } = [];
	/// <summary>
	/// m_FullChannelMask
	/// </summary>
	public int SourceMap { get; set; }
}
