using AssetRipper.Core.Converters.TrailRenderer;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;

namespace AssetRipper.Core.Classes.TrailRenderer
{
	public sealed class Gradient : IAsset
	{
		public Misc.Serializable.Gradient.Gradient GenerateGragient(IExportContainer container)
		{
			return GradientConverter.GenerateGradient(container, this);
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
			node.Add(Color0Name, Color0.ExportYAML(container));
			node.Add(Color1Name, Color1.ExportYAML(container));
			node.Add(Color2Name, Color2.ExportYAML(container));
			node.Add(Color3Name, Color3.ExportYAML(container));
			node.Add(Color4Name, Color4.ExportYAML(container));
			return node;
		}

		public const string Color0Name = "m_Color0";
		public const string Color1Name = "m_Color1";
		public const string Color2Name = "m_Color2";
		public const string Color3Name = "m_Color3";
		public const string Color4Name = "m_Color4";

		public ColorRGBA32 Color0 = new();
		public ColorRGBA32 Color1 = new();
		public ColorRGBA32 Color2 = new();
		public ColorRGBA32 Color3 = new();
		public ColorRGBA32 Color4 = new();
	}
}
