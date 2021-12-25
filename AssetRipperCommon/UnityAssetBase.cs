using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Endian;
using AssetRipper.Core.Layout;
using AssetRipper.Core.Parser.Asset;
using AssetRipper.Core.Parser.Files;
using AssetRipper.Core.Parser.Files.SerializedFiles.Parser.TypeTree;
using AssetRipper.Core.Project;
using AssetRipper.Core.YAML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AssetRipper.Core
{
	/// <summary>
	/// The artificial base class for all generated Unity classes
	/// </summary>
	public class UnityAssetBase : IUnityAssetBase
	{
		private const BindingFlags fieldBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
		public UnityVersion AssetUnityVersion { get; set; }
		public EndianType EndianType { get; set; }
		public TransferInstructionFlags TransferInstructionFlags { get; set; }

		public UnityAssetBase() { }
		public UnityAssetBase(LayoutInfo layout)
		{
			AssetUnityVersion = layout.Version;
			TransferInstructionFlags = layout.Flags;
		}

		public virtual void ReadEditor(AssetReader reader) => throw new NotSupportedException();

		public virtual void ReadRelease(AssetReader reader) => throw new NotSupportedException();

		public virtual void Read(AssetReader reader)
		{
			AssetUnityVersion = reader.Version;
			EndianType = reader.EndianType;
			TransferInstructionFlags = reader.Flags;
			if (reader.Flags.IsRelease())
				ReadRelease(reader);
			else
				ReadEditor(reader);
		}

		public virtual void WriteEditor(AssetWriter writer) => throw new NotSupportedException();

		public virtual void WriteRelease(AssetWriter writer) => throw new NotSupportedException();

		public virtual void Write(AssetWriter writer)
		{
			if (writer.Flags.IsRelease())
				WriteRelease(writer);
			else
				WriteEditor(writer);
		}

		public virtual YAMLNode ExportYAMLEditor(IExportContainer container) => throw new NotSupportedException($"Editor yaml export is not supported for {GetType().FullName}");

		public virtual YAMLNode ExportYAMLRelease(IExportContainer container) => throw new NotSupportedException($"Release yaml export is not supported for {GetType().FullName}");

		public virtual YAMLNode ExportYAML(IExportContainer container)
		{
			if (container.ExportFlags.IsRelease())
			//if(this.TransferInstructionFlags.IsRelease())
				return ExportYAMLRelease(container);
			else
				return ExportYAMLEditor(container);
		}

		public virtual IEnumerable<PPtr<IUnityObjectBase>> FetchDependencies(DependencyContext context)
		{
			yield break;
		}

		public virtual List<TypeTreeNode> MakeReleaseTypeTreeNodes(int depth, int startingIndex) => throw new NotSupportedException();

		public virtual List<TypeTreeNode> MakeEditorTypeTreeNodes(int depth, int startingIndex) => throw new NotSupportedException();

		private FieldInfo TryGetFieldInfo(string fieldName) => this.GetType().GetFields(fieldBindingFlags).Where(fieldInfo => fieldInfo.Name == fieldName).FirstOrDefault();

		/// <summary>
		/// Try using reflection to get the value of a field.
		/// </summary>
		/// <param name="fieldName">The name of the field.</param>
		/// <returns>The boxed value of that field if it exists.</returns>
		public object TryGetFieldValue(string fieldName) => TryGetFieldInfo(fieldName)?.GetValue(this);

		/// <summary>
		/// Try using reflection to get the value of a field.
		/// </summary>
		/// <typeparam name="T">The type to cast the boxed value to.</typeparam>
		/// <param name="fieldName">The name of the field.</param>
		/// <returns>The value of that field if it exists and is castable to the specified type.</returns>
		public T TryGetFieldValue<T>(string fieldName)
		{
			object value = TryGetFieldValue(fieldName);
			if (value is T result)
				return result;
			else
				return default;
		}

		/// <summary>
		/// Try using reflection to set the value of a field.
		/// </summary>
		/// <typeparam name="T">The type of the value being assigned.</typeparam>
		/// <param name="fieldName">The name of the field.</param>
		/// <param name="value">The value to assign.</param>
		/// <returns>True if successful, false otherwise</returns>
		public bool TrySetFieldValue<T>(string fieldName, T value)
		{
			FieldInfo fieldInfo = TryGetFieldInfo(fieldName);
			if (fieldInfo == null)
			{
				return false;
			}
			else
			{
				fieldInfo.SetValue(this, value);
				return true;
			}
		}
	}
}
