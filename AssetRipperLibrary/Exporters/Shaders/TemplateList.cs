using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetRipper.Library.Exporters.Shaders
{
	public static class TemplateList
	{
		public static List<TemplateShader> Templates { get; }

		private static List<T> ListFromSingle<T>(T element) => new List<T>(new[] { element });

		static TemplateList()
		{
			Templates = new List<TemplateShader>();
			Templates.Add(new TemplateShader()
			{
				ShaderText =
					"\t\tsampler2D _MainTex;\n" +
					"\t\tstruct Input\n" +
					"\t\t{\n" +
					"\t\t\tfloat2 uv_MainTex;\n" +
					"\t\t};\n" +
					"\t\tvoid surf(Input IN, inout SurfaceOutputStandard o)\n" +
					"\t\t{\n" +
					"\t\t\tfixed4 c = tex2D(_MainTex, IN.uv_MainTex);\n" +
					"\t\t\to.Albedo = c.rgb;\n" +
					"\t\t}\n"
			});
			Templates.Add(new TemplateShader()
			{
				RequiredProperties = ListFromSingle(new RequiredProperty("_MainTex", PropertyType.Texture)),
				ShaderText = 
					"\t\tsampler2D _MainTex;\n" +
					"\t\tstruct Input\n" +
					"\t\t{\n" +
					"\t\t\tfloat2 uv_MainTex;\n" +
					"\t\t};\n" +
					"\t\tvoid surf(Input IN, inout SurfaceOutputStandard o)\n" +
					"\t\t{\n" +
					"\t\t\tfixed4 c = tex2D(_MainTex, IN.uv_MainTex);\n" +
					"\t\t\to.Albedo = c.rgb;\n" +
					"\t\t}\n",
			});
		}
	}
}
