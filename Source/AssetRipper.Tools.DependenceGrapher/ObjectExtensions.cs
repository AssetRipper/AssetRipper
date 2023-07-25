using AssetRipper.Assets;
using AssetRipper.Assets.Interfaces;
using AssetRipper.SourceGenerated.Classes.ClassID_2;

namespace AssetRipper.Tools.DependenceGrapher
{
	internal static class ObjectExtensions
	{
		public static string? GetName(this IUnityObjectBase _this)
		{
			string? gameObjectName = (_this as IComponent)?.GameObject_C2P?.Name;
			if (!string.IsNullOrEmpty(gameObjectName))
			{
				return gameObjectName;
			}
			else if (_this is IHasNameString named)
			{
				return named.NameString;
			}
			else
			{
				return null;
			}
		}
	}
}
