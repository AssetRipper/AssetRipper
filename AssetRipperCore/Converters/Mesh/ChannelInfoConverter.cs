using AssetRipper.Core.Classes.Mesh;
using AssetRipper.Core.Classes.Shader.Enums.ShaderChannel;
using AssetRipper.Core.Classes.Shader.Enums.VertexFormat;
using AssetRipper.Core.Project;

namespace AssetRipper.Core.Converters.Mesh
{
	public static class ChannelInfoConverter
	{
		public static ChannelInfo Convert(IExportContainer container, ChannelInfo origin)
		{
			ChannelInfo instance = origin.Clone();
			if (origin.IsSet())
			{
				if (VertexFormatExtensions.VertexFormat2019Relevant(container.Version))
				{
				}
				else if (ShaderChannelExtensions.ShaderChannel5Relevant(container.Version))
				{
					if (VertexFormatExtensions.VertexFormat2019Relevant(container.ExportVersion))
					{
						instance.Format = origin.GetVertexFormat(container.Version).ToFormat(container.ExportVersion);
					}
				}
				else
				{
					if (container.ExportVersion.IsGreaterEqual(5))
					{
						VertexChannelFormat formatv4 = (VertexChannelFormat)origin.Format;
						instance.Format = formatv4.ToVertexFormat().ToFormat(container.ExportVersion);
						if (formatv4 == VertexChannelFormat.Color)
						{
							// replace Color4b[1] to Color1b[4]
							instance.Dimension = (byte)(instance.GetDataDimension() * 4);
						}
					}
				}
			}
			return instance;
		}
	}
}
