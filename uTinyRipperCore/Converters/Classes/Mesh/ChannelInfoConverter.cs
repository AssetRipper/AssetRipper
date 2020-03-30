using uTinyRipper.Classes.Meshes;
using uTinyRipper.Classes.Shaders;

namespace uTinyRipper.Converters.Meshes
{
	public static class ChannelInfoConverter
	{
		public static ChannelInfo Convert(IExportContainer container, ChannelInfo origin)
		{
			ChannelInfo instance = origin;
			if (origin.IsSet)
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
							instance.RawDimension = (byte)(instance.Dimension * 4);
						}
					}
				}
			}
			return instance;
		}
	}
}
