using AssetRipper.Core.IO.Asset;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IChannelInfo : IAsset
	{
		/// <summary>
		/// Stream index
		/// BinaryData:[Stream0][Align][Stream1][Align]...
		/// </summary>
		byte Stream { get; set; }
		/// <summary>
		/// Offset inside stream
		/// Stream:[FirstVertex: VertexOffset,NormalOffset,TangentOffset...][SecondVertex: VertexOffset,NormalOffset,TangentOffset...]...
		/// </summary>
		byte Offset { get; set; }
		/// <summary>
		/// Data format: float, int, byte
		/// </summary>
		byte Format { get; set; }
		/// <summary>
		/// An unprocessed byte value containing the data dimension
		/// </summary>
		byte Dimension { get; set; }
	}

	public static class ChannelInfoExtensions
	{
		public static bool IsSet(this IChannelInfo channelInfo) => channelInfo.Dimension > 0;

		/// <summary>
		/// Data dimention: Vector3, Vector2, Vector1
		/// </summary>
		public static byte GetDataDimension(this IChannelInfo channelInfo)
		{
			return (byte)(channelInfo.Dimension & 0b00001111);
		}

		/// <summary>
		/// Data dimention: Vector3, Vector2, Vector1
		/// </summary>
		public static void SetDataDimension(this IChannelInfo channelInfo, byte value)
		{
			channelInfo.Dimension = (byte)((channelInfo.Dimension & 0b11110000) | (value & 0b00001111));
		}
	}
}
