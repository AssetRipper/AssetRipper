using AssetRipper.Core.Classes.Misc;
using AssetRipper.Core.Classes.Misc.Serializable;
using AssetRipper.Core.Classes.Misc.Serializable.AnimationCurveTpl;
using AssetRipper.Core.Classes.Misc.Serializable.Boundaries;
using AssetRipper.Core.Classes.Misc.Serializable.Gradient;
using AssetRipper.Core.Classes.Misc.Serializable.GUIStyle;
using AssetRipper.Core.Interfaces;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Math;
using AssetRipper.Core.Math.Colors;
using AssetRipper.Core.Structure.Assembly.Mono;
using System;

namespace AssetRipper.Core.Parser.Asset
{
	public abstract class AssetFactoryBase
	{
		public abstract IUnityObjectBase CreateAsset(AssetInfo assetInfo);

		public virtual IAsset CreateEngineAsset(string name)
		{
			switch (name)
			{
				case MonoUtils.Vector2Name:
					return new Vector2f();
				case MonoUtils.Vector2IntName:
					return new Vector2i();
				case MonoUtils.Vector3Name:
					return new Vector3f();
				case MonoUtils.Vector3IntName:
					return new Vector3i();
				case MonoUtils.Vector4Name:
					return new Vector4f();
				case MonoUtils.RectName:
					return new Rectf();
				case MonoUtils.BoundsName:
					return new AABB();
				case MonoUtils.BoundsIntName:
					return new AABBi();
				case MonoUtils.QuaternionName:
					return new Quaternionf();
				case MonoUtils.Matrix4x4Name:
					return new Matrix4x4f();
				case MonoUtils.ColorName:
					return new ColorRGBAf();
				case MonoUtils.Color32Name:
					return new ColorRGBA32();
				case MonoUtils.LayerMaskName:
					return new LayerMask();
				case MonoUtils.AnimationCurveName:
					return new AnimationCurveTpl<Float>();
				case MonoUtils.GradientName:
					return new Gradient();
				case MonoUtils.RectOffsetName:
					return new RectOffset();
				case MonoUtils.GUIStyleName:
					return new GUIStyle();

				case MonoUtils.PropertyNameName:
					return new PropertyName();

				default:
					throw new NotImplementedException(name);
			}
		}
	}
}
