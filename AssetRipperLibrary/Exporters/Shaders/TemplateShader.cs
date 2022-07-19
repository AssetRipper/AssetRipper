using AssetRipper.Core.IO;
using AssetRipper.SourceGenerated.Classes.ClassID_48;
using AssetRipper.SourceGenerated.Subclasses.SerializedProperty;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Library.Exporters.Shaders
{
	public class TemplateShader
	{
		public string TemplateName { get; set; } = string.Empty;
		public List<RequiredProperty> RequiredProperties { get; set; } = new();
		public string ShaderText { get; set; } = string.Empty;


		public bool IsMatch(IShader shader)
		{
			if (RequiredProperties == null)
			{
				throw new System.NullReferenceException("requiredProperties cannot be null");
			}

			if (RequiredProperties.Count == 0)
			{
				return true;
			}

			AccessListBase<ISerializedProperty>? properties = shader.ParsedForm_C48?.PropInfo.Props;
			if (properties is null || properties.Count == 0)
			{
				return false;
			}

			foreach (RequiredProperty? reqProp in RequiredProperties)
			{
				int matches = properties.Where(prop => reqProp.IsMatch(prop)).Count();
				if (matches == 0)
				{
					return false;
				}
			}
			return true;
		}
	}
}
