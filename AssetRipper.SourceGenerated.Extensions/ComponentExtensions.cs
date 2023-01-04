using AssetRipper.Assets.Metadata;
using AssetRipper.SourceGenerated.Classes.ClassID_1;
using AssetRipper.SourceGenerated.Classes.ClassID_2;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class ComponentExtensions
	{
		public static IGameObject GetGameObject(this IComponent component)
		{
			return component.GameObject_C2.GetAsset(component.Collection);
		}

		public static IGameObject GetRoot(this IComponent component)
		{
			return component.GetGameObject().GetRoot();
		}

		public static int GetRootDepth(this IComponent component)
		{
			return component.GetGameObject().GetRootDepth();
		}
	}
}
