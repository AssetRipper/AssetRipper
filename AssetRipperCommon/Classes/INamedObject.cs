using AssetRipper.Core.Classes.Shader;
using AssetRipper.Core.Interfaces;

namespace AssetRipper.Core.Classes
{
	public interface INamedObject : IHasName, IEditorExtension
	{
	}

	public static class NamedObjectExtensions
	{
		public static string GetNameNotEmpty(this INamedObject named)
		{
			return string.IsNullOrEmpty(named.Name) ? named.GetType().Name : named.Name;
		}

		public static string GetValidName(this INamedObject named)
		{
			if(named is IShader shader)
			{
				return shader.ValidName;
			}
			else
			{
				return named.GetNameNotEmpty();
			}
		}
	}
}