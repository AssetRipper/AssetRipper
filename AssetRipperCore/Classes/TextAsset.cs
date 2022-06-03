using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;

using AssetRipper.Yaml;
using AssetRipper.Yaml.Extensions;

namespace AssetRipper.Core.Classes
{
	/// <summary>
	/// Script previously
	/// </summary>
	public class TextAsset : NamedObject, IHasRawData
	{
		public TextAsset(AssetInfo assetInfo) : base(assetInfo) { }

		/// <summary>
		/// Less than 2017.1
		/// </summary>
		public static bool HasPath(UnityVersion version) => version.IsLess(2017);

		public override void Read(AssetReader reader)
		{
			base.Read(reader);

			RawData = reader.ReadByteArray();
			reader.AlignStream();

			if (HasPath(reader.Version))
			{
				PathName = reader.ReadString();
			}
		}

		protected void ReadNamedObject(AssetReader reader)
		{
			base.Read(reader);
		}

		protected YamlMappingNode ExportBaseYamlRoot(IExportContainer container)
		{
			return base.ExportYamlRoot(container);
		}

		protected override YamlMappingNode ExportYamlRoot(IExportContainer container)
		{
			YamlMappingNode node = base.ExportYamlRoot(container);
			node.Add(ScriptName, RawData.ExportYaml());
			return node;
		}

		/// <summary>
		/// NOTE: originally, it is a string, but since binary files are serialized as TextAsset, we have to store its content as byte array
		/// </summary>
		public byte[] RawData { get; protected set; }

		public byte[] Script => RawData ?? Array.Empty<byte>();
		public string PathName { get; protected set; } = string.Empty;

		public const string ScriptName = "m_Script";
	}
}
