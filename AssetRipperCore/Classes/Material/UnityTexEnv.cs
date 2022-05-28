using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.Material
{
	public sealed class UnityTexEnv : IAssetReadable, IYamlExportable, IDependent
	{
		/// <summary>
		/// Less than 2.1.0
		/// </summary>
		private static bool IsVector3f(UnityVersion version) => version.IsLess(2, 1);

		public void Read(AssetReader reader)
		{
			Texture.Read(reader);
			if (IsVector3f(reader.Version))
			{
				Scale3.Read(reader);
				Offset3.Read(reader);
			}
			else
			{
				Scale = reader.ReadAsset<Vector2f>();
				Offset = reader.ReadAsset<Vector2f>();
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Texture, TextureName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TextureName, Texture.ExportYaml(container));
			node.Add(ScaleName, Scale.ExportYaml(container));
			node.Add(OffsetName, Offset.ExportYaml(container));
			return node;
		}


		public Vector2f Scale
		{
			get => (Vector2f)Scale3;
			set => Scale3 = value;
		}
		public Vector2f Offset
		{
			get => (Vector2f)Offset3;
			set => Offset3 = value;
		}

		public const string TextureName = "m_Texture";
		public const string ScaleName = "m_Scale";
		public const string OffsetName = "m_Offset";

		public PPtr<Texture> Texture = new();
		public Vector3f Scale3 = new();
		public Vector3f Offset3 = new();
	}
}
