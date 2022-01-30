using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IVertexData : IAsset
	{
		/// <summary>
		/// Less than 2018.1
		/// </summary>
		uint CurrentChannels { get; set; }
		uint VertexCount { get; set; }
		/// <summary>
		/// 4.0.0 and greater
		/// </summary>
		//IChannelInfo[] Channels { get; }
		/// <summary>
		/// Less than 5.0.0
		/// </summary>
		//IStreamInfo[] Streams { get; }
		/// <summary>
		/// Actually called m_DataSize
		/// </summary>
		byte[] Data { get; set; }
	}
}
