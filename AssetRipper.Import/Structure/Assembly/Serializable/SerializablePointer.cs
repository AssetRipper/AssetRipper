using AssetRipper.Assets;
using AssetRipper.Assets.Export;
using AssetRipper.Assets.Export.Dependencies;
using AssetRipper.Assets.IO;
using AssetRipper.Assets.IO.Reading;
using AssetRipper.Assets.IO.Writing;
using AssetRipper.Assets.Metadata;
using AssetRipper.Yaml;

namespace AssetRipper.Import.Structure.Assembly.Serializable
{
	public sealed class SerializablePointer<T> : IAsset, IDependent, IPPtr<T> where T : IUnityObjectBase
	{
		public SerializablePointer(int fileID, long pathID)
		{
			FileID = fileID;
			PathID = pathID;
		}

		public SerializablePointer()
		{
		}

		/// <summary>
		/// At least version 5.0.0
		/// </summary>
		public static bool IsLongID(UnityVersion version)
		{
			// NOTE: unknown version SerializedFiles.FormatVersion.Unknown_14
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetReader reader)
		{
			FileID = reader.ReadInt32();
			PathID = IsLongID(reader.Version) ? reader.ReadInt64() : reader.ReadInt32();
		}

		public void Write(AssetWriter writer)
		{
			writer.Write(FileID);
			if (IsLongID(writer.Version))
			{
				writer.Write(PathID);
			}
			else
			{
				writer.Write((int)PathID);
			}
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(Pointer, string.Empty);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			return this.ExportYaml(container, 0);
		}

		public SerializablePointer<T> Clone() => new SerializablePointer<T>(FileID, PathID);

		public IEnumerable<(FieldName, PPtr<IUnityObjectBase>)> FetchDependencies(FieldName? parent)
		{
			yield return (FieldName.Empty, Pointer);
		}

		public int FileID { get; set; }
		public long PathID { get; set; }
		public PPtr<T> Pointer => new PPtr<T>(FileID, PathID);
	}
}
