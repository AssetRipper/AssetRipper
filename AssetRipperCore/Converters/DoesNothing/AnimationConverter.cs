using AssetRipper.Core.Classes.Animation;
using AssetRipper.Core.Project;
using System.Linq;

namespace AssetRipper.Core.Converters
{
	public static class AnimationConverter
	{
		public static Animation Convert(IExportContainer container, Animation origin)
		{
			Animation instance = new Animation(container.ExportLayout);
			BehaviourConverter.Convert(container, origin, instance);
			instance.Animations = origin.Animations.ToArray();
			instance.WrapMode = origin.WrapMode;
			instance.PlayAutomatically = origin.PlayAutomatically;
			instance.AnimatePhysics = origin.AnimatePhysics;
			if (Animation.HasAnimateOnlyIfVisible(container.ExportVersion) && Animation.HasAnimateOnlyIfVisible(container.Version))
			{
				instance.AnimateOnlyIfVisible = origin.AnimateOnlyIfVisible;
			}
			if (Animation.HasCullingType(container.ExportVersion) && Animation.HasCullingTypeInvariant(container.Version))
			{
				instance.CullingType = origin.CullingType;
			}
			if (Animation.HasUserAABB(container.ExportVersion) && Animation.HasUserAABB(container.Version))
			{
				instance.UserAABB = origin.UserAABB;
			}
			return instance;
		}
	}
}
