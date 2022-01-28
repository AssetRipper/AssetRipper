using AssetRipper.Core.Classes.AnimatorController.Constants;
using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimationClip.Clip
{
	public sealed class Clip : IAssetReadable
	{
		/// <summary>
		/// 4.3.0 and greater
		/// </summary>
		public static bool HasConstantClip(UnityVersion version) => version.IsGreaterEqual(4, 3);
		/// <summary>
		/// Less than 2018.3
		/// </summary>
		public static bool HasBinding(UnityVersion version) => version.IsLess(2018, 3);

		public void Read(AssetReader reader)
		{
			StreamedClip.Read(reader);
			DenseClip.Read(reader);
			if (HasConstantClip(reader.Version))
			{
				ConstantClip.Read(reader);
			}
			if (HasBinding(reader.Version))
			{
				Binding.Read(reader);
			}
		}

		public bool IsSet(UnityVersion version)
		{
			if (StreamedClip.IsSet)
			{
				return true;
			}
			if (DenseClip.IsSet)
			{
				return true;
			}
			if (HasConstantClip(version))
			{
				if (ConstantClip.IsSet)
				{
					return true;
				}
			}
			return false;
		}

		public AnimationClipBindingConstant ConvertValueArrayToGenericBinding()
		{
			var bindings = new AnimationClipBindingConstant();
			var genericBindings = new List<GenericBinding.GenericBinding>();
			var values = Binding;
			for (int i = 0; i < values.ValueArray.Length;)
			{
				var curveID = values.ValueArray[i].ID;
				var curveTypeID = values.ValueArray[i].TypeID;
				var binding = new GenericBinding.GenericBinding();
				genericBindings.Add(binding);
				if (curveTypeID == 4174552735) //CRC(PositionX))
				{
					binding.Path = curveID;
					binding.Attribute = 1; //kBindTransformPosition
					binding.ClassID = ClassIDType.Transform;
					i += 3;
				}
				else if (curveTypeID == 2211994246) //CRC(QuaternionX))
				{
					binding.Path = curveID;
					binding.Attribute = 2; //kBindTransformRotation
					binding.ClassID = ClassIDType.Transform;
					i += 4;
				}
				else if (curveTypeID == 1512518241) //CRC(ScaleX))
				{
					binding.Path = curveID;
					binding.Attribute = 3; //kBindTransformScale
					binding.ClassID = ClassIDType.Transform;
					i += 3;
				}
				else
				{
					binding.ClassID = ClassIDType.Animator;
					binding.Path = 0;
					binding.Attribute = curveID;
					i++;
				}
			}
			bindings.GenericBindings = genericBindings.ToArray();
			return bindings;
		}

		public StreamedClip StreamedClip = new();
		public DenseClip DenseClip = new();
		public ConstantClip ConstantClip = new();
		public ValueArrayConstant Binding = new();
	}
}
