using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssetRipper.Core.Project
{
	public class ProjectExporter : ProjectExporterBase
	{
		/// <summary>
		/// Exact type to the exporters that handle that type
		/// </summary>
		private readonly Dictionary<Type, Stack<IAssetExporter>> typeMap = new Dictionary<Type, Stack<IAssetExporter>>();
		/// <summary>
		/// List of type-exporter-allow pairs<br/>
		/// Type: the asset type<br/>
		/// IAssetExporter: the exporter that can handle that asset type<br/>
		/// Bool: allow the exporter to apply on inherited asset types?
		/// </summary>
		private readonly List<(Type, IAssetExporter, bool)> registeredExporters = new List<(Type, IAssetExporter, bool)>();
		/// <summary>
		/// All the unity types from before the 2.0 update
		/// </summary>
		private static readonly Type[] unityTypes = GetAllUnityTypesSafe();

		public ProjectExporter()
		{
			OverrideExporter<UnknownObject>(new UnknownObjectExporter(), false);
			OverrideExporter<UnreadableObject>(new UnreadableObjectExporter(), false);
		}

		/// <inheritdoc/>
		public override void OverrideExporter(ClassIDType classType, IAssetExporter exporter)
		{
			OverrideExporter(GetTypeForID(classType), exporter, false);
		}

		/// <inheritdoc/>
		public override void OverrideExporter(Type type, IAssetExporter exporter, bool allowInheritance)
		{
			if (exporter == null)
				throw new ArgumentNullException(nameof(exporter));
			registeredExporters.Add((type, exporter, allowInheritance));
			if (typeMap.Count > 0)//Just in case an exporter gets added after CreateCollection or ToExportType have already been used
				RecalculateTypeMap();
		}

		public override AssetType ToExportType(ClassIDType classID)
		{
			switch (classID)
			{
				// abstract objects
				case ClassIDType.Object:
					return AssetType.Meta;
				case ClassIDType.Renderer:
					return AssetType.Serialized;
				case ClassIDType.Texture:
					classID = ClassIDType.Texture2D;
					break;
				case ClassIDType.RuntimeAnimatorController:
					classID = ClassIDType.AnimatorController;
					break;
				case ClassIDType.Motion:
					return AssetType.Serialized;

				// not implemented yet
				case ClassIDType.AudioMixerGroup:
					return AssetType.Serialized;
				case ClassIDType.EditorExtension:
					return AssetType.Serialized;
			}

			Stack<IAssetExporter> exporters = GetExporterStack(GetTypeForID(classID));
			foreach (IAssetExporter exporter in exporters)
			{
				if (exporter.ToUnknownExportType(classID, out AssetType assetType))
				{
					return assetType;
				}
			}
			throw new NotSupportedException($"There is no exporter that know {nameof(AssetType)} for unknown asset '{classID}'");
		}

		protected override IExportCollection CreateCollection(VirtualSerializedFile file, IUnityObjectBase asset)
		{
			Stack<IAssetExporter> exporters = GetExporterStack(asset);
			foreach (IAssetExporter exporter in exporters)
			{
				if (exporter.IsHandle(asset))
				{
					return exporter.CreateCollection(file, asset);
				}
			}
			throw new Exception($"There is no exporter that can handle '{asset}'");
		}

		private Stack<IAssetExporter> GetExporterStack(IUnityObjectBase asset) => GetExporterStack(asset.GetType());
		private Stack<IAssetExporter> GetExporterStack(Type type)
		{
			if (!typeMap.TryGetValue(type, out Stack<IAssetExporter> exporters))
			{
				exporters = CalculateAssetExporterStack(type);
				typeMap.Add(type, exporters);
			}
			return exporters;
		}

		private void RecalculateTypeMap()
		{
			foreach (var type in typeMap.Keys)
			{
				typeMap[type] = CalculateAssetExporterStack(type);
			}
		}

		private Stack<IAssetExporter> CalculateAssetExporterStack(Type type)
		{
			var result = new Stack<IAssetExporter>();
			foreach ((Type baseType, IAssetExporter exporter, bool allowInheritance) in registeredExporters)
			{
				if (type == baseType || (allowInheritance && type.IsAssignableTo(baseType)))
					result.Push(exporter);
			}
			return result;
		}

		private static Type GetTypeForID(ClassIDType classID)
		{
			return TryGetTypeForID(classID, out Type type) ? type : throw new NotSupportedException($"The {classID} ClassIDType does not have an associated Type.");
		}
		private static bool TryGetTypeForID(ClassIDType classID, out Type type)
		{
			if (classID == ClassIDType.AvatarMaskOld)
				classID = ClassIDType.AvatarMask;
			else if (classID == ClassIDType.VideoClipOld)
				classID = ClassIDType.VideoClip;

			if (classID == ClassIDType.UnknownType)
			{
				type = typeof(UnknownObject);
				return true;
			}

			string className = classID.ToString();
			type = unityTypes.FirstOrDefault(t => t.Name == className);
			return type != null;
		}

		private static ClassIDType GetIDForType(Type type)
		{
			return TryGetIDForType(type, out ClassIDType result) ? result : throw new NotSupportedException($"The {type.Name} Type does not have an associated ClassIDType.");
		}
		private static bool TryGetIDForType(Type type, out ClassIDType classID)
		{
			if (type == typeof(UnknownObject))
			{
				classID = ClassIDType.UnknownType;
				return true;
			}
			return Enum.TryParse<ClassIDType>(type.Name, out classID);
		}

		private static Type[] GetAllUnityTypesSafe()
		{
			Type[] types;
			Type objectType = typeof(AssetRipper.Core.Classes.Object.Object);
			try
			{
				types = objectType.Assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException re)
			{
				types = re.Types.Where(t => t != null).ToArray();
			}
			return types.Where(type => type.IsAssignableTo(objectType)).ToArray();
		}
	}
}
