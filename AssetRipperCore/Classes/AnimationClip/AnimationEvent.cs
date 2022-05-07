﻿using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Project;
using AssetRipper.Yaml;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip
{
	public sealed class AnimationEvent : IAssetReadable, IYamlExportable, IDependent
	{
		/// <summary>
		/// 2.6.0 and greater
		/// </summary>
		public static bool HasObjectReferenceParameter(UnityVersion version) => version.IsGreaterEqual(2, 6);
		/// <summary>
		/// 3.0.0 and greater
		/// </summary>
		public static bool HasIntParameter(UnityVersion version) => version.IsGreaterEqual(3);

		public void Read(AssetReader reader)
		{
			Time = reader.ReadSingle();

			FunctionName = reader.ReadString();
			StringParameter = reader.ReadString();
			if (HasObjectReferenceParameter(reader.Version))
			{
				ObjectReferenceParameter.Read(reader);
				FloatParameter = reader.ReadSingle();
			}
			if (HasIntParameter(reader.Version))
			{
				IntParameter = reader.ReadInt32();
			}
			MessageOptions = reader.ReadInt32();
		}

		public IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield return context.FetchDependency(ObjectReferenceParameter, ObjectReferenceParameterName);
		}

		public YamlNode ExportYaml(IExportContainer container)
		{
			YamlMappingNode node = new YamlMappingNode();
			node.Add(TimeName, Time);
			node.Add(FunctionNameName, FunctionName);
			node.Add(DataName, StringParameter);
			node.Add(ObjectReferenceParameterName, ObjectReferenceParameter.ExportYaml(container));
			node.Add(FloatParameterName, FloatParameter);
			node.Add(IntParameterName, IntParameter);
			node.Add(MessageOptionsName, MessageOptions);
			return node;
		}

		public float Time { get; set; }
		public string FunctionName { get; set; }
		/// <summary>
		/// Data
		/// </summary>
		public string StringParameter { get; set; }
		public float FloatParameter { get; set; }
		public int IntParameter { get; set; }
		public int MessageOptions { get; set; }

		public const string TimeName = "time";
		public const string FunctionNameName = "functionName";
		public const string DataName = "data";
		public const string ObjectReferenceParameterName = "objectReferenceParameter";
		public const string FloatParameterName = "floatParameter";
		public const string IntParameterName = "intParameter";
		public const string MessageOptionsName = "messageOptions";

		public PPtr<Object.Object> ObjectReferenceParameter = new();
	}
}
