using AssetRipper.SourceGenerated.Subclasses.ControllerConstant;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;

namespace AssetRipper.SourceGenerated.Extensions;

public static class ControllerConstantExtensions
{
	public static ILayerConstant GetLayerByStateMachineIndex(this IControllerConstant controllerConstant, int index)
	{
		for (int i = 0; i < controllerConstant.LayerArray.Count; i++)
		{
			ILayerConstant layer = controllerConstant.LayerArray[i].Data;
			if (layer.StateMachineIndex == index && layer.StateMachineSynchronizedLayerIndex == 0)
			{
				return layer;
			}
		}
		throw new ArgumentOutOfRangeException(nameof(index));
	}

	public static int GetLayerIndexByStateMachineIndex(this IControllerConstant controllerConstant, int index, out ILayerConstant layer)
	{
		for (int i = 0; i < controllerConstant.LayerArray.Count; i++)
		{
			layer = controllerConstant.LayerArray[i].Data;
			if (layer.StateMachineIndex == index && layer.StateMachineSynchronizedLayerIndex == 0)
			{
				return i;
			}
		}
		throw new ArgumentOutOfRangeException(nameof(index));
	}

	public static int GetLayerIndex(this IControllerConstant controllerConstant, ILayerConstant layer)
	{
		for (int i = 0; i < controllerConstant.LayerArray.Count; i++)
		{
			ILayerConstant checkLayer = controllerConstant.LayerArray[i].Data;
			if (checkLayer.StateMachineIndex == layer.StateMachineIndex && checkLayer.StateMachineSynchronizedLayerIndex == layer.StateMachineSynchronizedLayerIndex)
			{
				return i;
			}
		}
		throw new ArgumentException("Layer hasn't been found", nameof(layer));
	}
}
