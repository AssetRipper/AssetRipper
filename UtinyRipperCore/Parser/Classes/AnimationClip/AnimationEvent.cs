using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;
using UtinyRipper.SerializedFiles;

namespace UtinyRipper.Classes.AnimationClips
{
	public struct AnimationEvent : IAssetReadable, IYAMLExportable, IDependent
	{
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool IsReadObjectReferenceParameter(Version version)
		{
			return version.IsGreaterEqual(2, 6);
		}
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool IsReadIntParameter(Version version)
		{
			return version.IsGreaterEqual(3);
		}

		public void Read(AssetStream stream)
		{
			Time = stream.ReadSingle();

			FunctionName = stream.ReadStringAligned();
			StringParameter = stream.ReadStringAligned();
			if (IsReadObjectReferenceParameter(stream.Version))
			{
				ObjectReferenceParameter.Read(stream);
				FloatParameter = stream.ReadSingle();
			}
			if (IsReadIntParameter(stream.Version))
			{
				IntParameter = stream.ReadInt32();
			}
			MessageOptions = stream.ReadInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add("time", Time);
			node.Add("functionName", FunctionName);
			node.Add("data", StringParameter);
			node.Add("objectReferenceParameter", ObjectReferenceParameter.ExportYAML(container));
			node.Add("floatParameter", FloatParameter);
			node.Add("intParameter", IntParameter);
			node.Add("messageOptions", MessageOptions);
			return node;
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return ObjectReferenceParameter.FetchDependency(file, isLog, () => nameof(AnimationEvent), "objectReferenceParameter");
		}

		public float Time { get; private set; }
		public string FunctionName { get; private set; }
		public string StringParameter { get; private set; }
		public float FloatParameter { get; private set; }
		public int IntParameter { get; private set; }
		public int MessageOptions { get; private set; }

		public PPtr<Object> ObjectReferenceParameter;
	}
}
