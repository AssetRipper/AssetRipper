using AssetRipper.Core.Classes.Shader;
using System.Collections.Generic;
using System.Linq;

namespace AssetRipper.Library.Exporters.Shaders
{
	public class TemplateShader
	{
		public string TemplateName { get; set; }
		public List<RequiredProperty> RequiredProperties { get; set; }
		public string ShaderText { get; set; }


		public bool IsMatch(IShader shader)
		{
			if (RequiredProperties == null)
				throw new System.NullReferenceException("requiredProperties cannot be null");
			if (RequiredProperties.Count == 0)
				return true;
			var properties = shader.ParsedForm.PropInfo.Props;
			if (properties == null || properties.Length == 0)
				return false;
			foreach (var reqProp in RequiredProperties)
			{
				int matches = properties.Where(prop => reqProp.IsMatch(prop)).Count();
				if (matches == 0)
					return false;
			}
			return true;
		}
	}
}
