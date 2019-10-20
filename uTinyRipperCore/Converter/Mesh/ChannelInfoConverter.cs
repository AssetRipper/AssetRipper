using uTinyRipper.AssetExporters;
using uTinyRipper.Classes.Meshes;

namespace uTinyRipper.Converters.Meshes
{
	public static class ChannelInfoConverter
	{
		public static ChannelInfo Convert(IExportContainer container, ChannelInfo origin)
		{
			ChannelInfo instance = origin;
			if (origin.IsSet)
			{
				if (container.Version.IsLess(5))
				{
					if (container.ExportVersion.IsGreaterEqual(5))
					{
						VertexChannelFormat formatv4 = (VertexChannelFormat)origin.Format;
						if (formatv4 == VertexChannelFormat.Color)
						{
							// replace Color[1] to Byte[4]
							instance.Format = VertexFormat.Byte.ToFormat(container.ExportVersion);
							instance.RawDimension = (byte)((instance.RawDimension & 0xF0) | (instance.Dimension * 4));
						}
						else
						{
							instance.Format = formatv4.ToVertexFormat().ToFormat(container.ExportVersion);
						}
					}
				}
				else if (container.Version.IsLess(2019))
				{
					if (container.ExportVersion.IsGreaterEqual(2019))
					{
						instance.Format = origin.GetVertexFormat(container.Version).ToFormat(container.ExportVersion);
					}
				}
				else // Version >= 2019
				{
					// TEMP: downgrade
					if (container.ExportVersion.IsLess(2019))
					{
						instance.Format = origin.GetVertexFormat(container.Version).ToFormat(container.ExportVersion);
					}
				}
			}
			return instance;
		}
	}
}
