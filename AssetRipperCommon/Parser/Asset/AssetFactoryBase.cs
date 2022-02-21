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
using AssetRipper.Core.Math.Vectors;
using AssetRipper.Core.Structure.Assembly.Mono;
using System;

namespace AssetRipper.Core.Parser.Asset
{
	public abstract class AssetFactoryBase
	{
		public abstract IUnityObjectBase CreateAsset(AssetInfo assetInfo);

		public virtual IAsset CreateEngineAsset(string name)
		{
			return name switch
			{
				MonoUtils.Vector2Name => new Vector2f(),
				MonoUtils.Vector2IntName => new Vector2i(),
				MonoUtils.Vector3Name => new Vector3f(),
				MonoUtils.Vector3IntName => new Vector3i(),
				MonoUtils.Vector4Name => new Vector4f(),
				MonoUtils.RectName => new Rectf(),
				MonoUtils.BoundsName => new AABB(),
				MonoUtils.BoundsIntName => new AABBi(),
				MonoUtils.QuaternionName => new Quaternionf(),
				MonoUtils.Matrix4x4Name => new Matrix4x4f(),
				MonoUtils.ColorName => new ColorRGBAf(),
				MonoUtils.Color32Name => new ColorRGBA32(),
				MonoUtils.LayerMaskName => new LayerMask(),
				MonoUtils.AnimationCurveName => new AnimationCurveTpl<Float>(),
				MonoUtils.GradientName => new Gradient(),
				MonoUtils.RectOffsetName => new RectOffset(),
				MonoUtils.GUIStyleName => new GUIStyle(),
				MonoUtils.PropertyNameName => new PropertyName(),
				_ => throw new NotImplementedException(name),
			};
		}
	}
}
