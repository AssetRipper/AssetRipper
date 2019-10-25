using uTinyRipper.Converters;
using uTinyRipper.Converters.TrailRenderers;
using uTinyRipper.YAML;

namespace uTinyRipper.Classes.TrailRenderers
{
	public struct Gradient : IAsset
	{
		public ParticleSystems.Gradient GenerateGragient(IExportContainer container)
		{
			return GradientConverter.GenerateGradient(container, ref this);
		}

		public void Read(AssetReader reader)
		{
			Color0.Read(reader);
			Color1.Read(reader);
			Color2.Read(reader);
			Color3.Read(reader);
			Color4.Read(reader);
		}

		public void Write(AssetWriter writer)
		{
			Color0.Write(writer);
			Color1.Write(writer);
			Color2.Write(writer);
			Color3.Write(writer);
			Color4.Write(writer);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(ColorName + "0", Color0.ExportYAML(container));
			node.Add(ColorName + "1", Color1.ExportYAML(container));
			node.Add(ColorName + "2", Color2.ExportYAML(container));
			node.Add(ColorName + "3", Color3.ExportYAML(container));
			node.Add(ColorName + "4", Color4.ExportYAML(container));
			return node;
		}

		public const string ColorName = "m_Color";

		public ColorRGBA32 Color0;
		public ColorRGBA32 Color1;
		public ColorRGBA32 Color2;
		public ColorRGBA32 Color3;
		public ColorRGBA32 Color4;
	}
}
