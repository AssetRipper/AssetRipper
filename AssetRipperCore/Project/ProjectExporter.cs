using AssetRipper.Core.Classes;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files.SerializedFiles;
using AssetRipper.Core.Project.Collections;
using AssetRipper.Core.Project.Exporters;
using System;
using System.Collections.Generic;

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

		public ProjectExporter()
		{
			OverrideExporter<UnknownObject>(new UnknownObjectExporter(), false);
			OverrideExporter<UnreadableObject>(new UnreadableObjectExporter(), false);
		}

		/// <inheritdoc/>
		public override void OverrideExporter(Type type, IAssetExporter exporter, bool allowInheritance)
		{
			if (exporter == null)
			{
				throw new ArgumentNullException(nameof(exporter));
			}

			registeredExporters.Add((type, exporter, allowInheritance));
			if (typeMap.Count > 0)//Just in case an exporter gets added after CreateCollection or ToExportType have already been used
			{
				RecalculateTypeMap();
			}
		}

		public override AssetType ToExportType(Type type)
		{
			Stack<IAssetExporter> exporters = GetExporterStack(type);
			foreach (IAssetExporter exporter in exporters)
			{
				if (exporter.ToUnknownExportType(type, out AssetType assetType))
				{
					return assetType;
				}
			}
			throw new NotSupportedException($"There is no exporter that know {nameof(AssetType)} for unknown asset '{type}'");
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
			if (!typeMap.TryGetValue(type, out Stack<IAssetExporter>? exporters))
			{
				exporters = CalculateAssetExporterStack(type);
				typeMap.Add(type, exporters);
			}
			return exporters;
		}

		private void RecalculateTypeMap()
		{
			foreach (Type type in typeMap.Keys)
			{
				typeMap[type] = CalculateAssetExporterStack(type);
			}
		}

		private Stack<IAssetExporter> CalculateAssetExporterStack(Type type)
		{
			Stack<IAssetExporter> result = new Stack<IAssetExporter>();
			foreach ((Type baseType, IAssetExporter exporter, bool allowInheritance) in registeredExporters)
			{
				if (type == baseType || (allowInheritance && type.IsAssignableTo(baseType)))
				{
					result.Push(exporter);
				}
			}
			return result;
		}
	}
}
