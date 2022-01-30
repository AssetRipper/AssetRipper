using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using AssetRipper.Core.Extensions;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.Mesh
{
	public static class StreamInfoConverter
	{
		public static ChannelInfo GenerateChannelInfo(IExportContainer container, StreamInfo[] origin, ShaderChannel channelType)
		{
			return GenerateChannelInfo(container.ExportVersion, origin, channelType);
		}

		public static ChannelInfo GenerateChannelInfo(UnityVersion instanceVersion, StreamInfo[] origin, ShaderChannel channelType)
		{
			ChannelInfo instance = new ChannelInfo();
			ShaderChannel4 channelv4 = channelType.ToShaderChannel4();
			int streamIndex = origin.IndexOf(t => t.IsMatch(channelv4));
			if (streamIndex >= 0)
			{
				byte offset = 0;
				StreamInfo stream = origin[streamIndex];
				for (ShaderChannel4 i = 0; i < channelv4; i++)
				{
					if (stream.IsMatch(i))
					{
						offset += i.ToShaderChannel().GetStride(instanceVersion);
					}
				}

				instance.Stream = (byte)streamIndex;
				instance.Offset = offset;
				instance.Format = channelType.GetVertexFormat(instanceVersion).ToFormat(instanceVersion);
				instance.Dimension = channelType.GetDimention(instanceVersion);
			}
			return instance;
		}
	}
}
