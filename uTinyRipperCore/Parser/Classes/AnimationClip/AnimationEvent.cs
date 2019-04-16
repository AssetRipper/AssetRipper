using System.Collections.Generic;
using uTinyRipper.AssetExporters;
using uTinyRipper.YAML;
using uTinyRipper.SerializedFiles;

namespace uTinyRipper.Classes.AnimationClips
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

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();

			FunctionName = reader.ReadString();
			StringParameter = reader.ReadString();
			if (IsReadObjectReferenceParameter(reader.Version))
			{
				ObjectReferenceParameter.Read(reader);
				FloatParameter = reader.ReadSingle();
			}
			if (IsReadIntParameter(reader.Version))
			{
				IntParameter = reader.ReadInt32();
			}
			MessageOptions = reader.ReadInt32();
		}

		public IEnumerable<Object> FetchDependencies(ISerializedFile file, bool isLog = false)
		{
			yield return ObjectReferenceParameter.FetchDependency(file, isLog, () => nameof(AnimationEvent), ObjectReferenceParameterName);
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			YAMLMappingNode node = new YAMLMappingNode();
			node.Add(TimeName, Time);
			node.Add(FunctionNameName, FunctionName);
			node.Add(DataName, StringParameter);
			node.Add(ObjectReferenceParameterName, ObjectReferenceParameter.ExportYAML(container));
			node.Add(FloatParameterName, FloatParameter);
			node.Add(IntParameterName, IntParameter);
			node.Add(MessageOptionsName, MessageOptions);
			return node;
		}

		public float Time { get; private set; }
		public string FunctionName { get; private set; }
		/// <summary>
		/// Data
		/// </summary>
		public string StringParameter { get; private set; }
		public float FloatParameter { get; private set; }
		public int IntParameter { get; private set; }
		public int MessageOptions { get; private set; }

		public const string TimeName = "time";
		public const string FunctionNameName = "functionName";
		public const string DataName = "data";
		public const string ObjectReferenceParameterName = "objectReferenceParameter";
		public const string FloatParameterName = "floatParameter";
		public const string IntParameterName = "intParameter";
		public const string MessageOptionsName = "messageOptions";

		public PPtr<Object> ObjectReferenceParameter;
	}
}
