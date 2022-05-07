//using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface INamedObject : IHasNameString, IUnityObjectBase
	{
	}

	public static class NamedObjectExtensions
	{
		public static string GetValidName(this INamedObject named)
		{
			//if (named is IShader shader)
			//{
			//	return shader.GetValidShaderName();
			//}
			//else
			//{
				return named.GetNameNotEmpty();
			//}
		}
	}
}
