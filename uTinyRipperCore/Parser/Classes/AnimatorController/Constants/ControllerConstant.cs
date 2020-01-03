using System;
using uTinyRipper.Classes.Misc;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct ControllerConstant : IAssetReadable
	{		
		public void Read(AssetReader reader)
		{
			LayerArray = reader.ReadAssetArray<OffsetPtr<LayerConstant>>();
			StateMachineArray = reader.ReadAssetArray<OffsetPtr<StateMachineConstant>>();
			Values.Read(reader);
			DefaultValues.Read(reader);
		}

		public LayerConstant GetLayerByStateMachineIndex(int index)
		{
			for (int i = 0; i < LayerArray.Length; i++)
			{
				ref LayerConstant layer = ref LayerArray[i].Instance;
				if (layer.StateMachineIndex == index && layer.StateMachineMotionSetIndex == 0)
				{
					return layer;
				}
			}
			throw new ArgumentOutOfRangeException(nameof(index));
		}
		
		public int GetLayerIndexByStateMachineIndex(int index)
		{
			for (int i = 0; i < LayerArray.Length; i++)
			{
				ref LayerConstant layer = ref LayerArray[i].Instance;
				if (layer.StateMachineIndex == index && layer.StateMachineMotionSetIndex == 0)
				{
					return i;
				}
			}
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		public int GetLayerIndex(LayerConstant layer)
		{
			for(int i = 0; i < LayerArray.Length; i++)
			{
				LayerConstant checkLayer = LayerArray[i].Instance;
				if(checkLayer.StateMachineIndex == layer.StateMachineIndex && checkLayer.StateMachineMotionSetIndex == layer.StateMachineMotionSetIndex)
				{
					return i;
				}
			}
			throw new ArgumentException("Layer hasn't been found", nameof(layer));
		}

		/// <summary>
		/// HumanLayerArray previously
		/// </summary>
		public OffsetPtr<LayerConstant>[] LayerArray { get; set; }
		public OffsetPtr<StateMachineConstant>[] StateMachineArray { get; set; }

		public OffsetPtr<ValueArrayConstant> Values;
		public OffsetPtr<ValueArray> DefaultValues;
	}
}
