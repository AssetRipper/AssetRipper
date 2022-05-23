﻿using AssetRipper.SourceGenerated.Subclasses.ControllerConstant;
using AssetRipper.SourceGenerated.Subclasses.LayerConstant;
using System;

namespace AssetRipper.Core.SourceGenExtensions
{
	public static class ControllerConstantExtensions
	{
		public static ILayerConstant GetLayerByStateMachineIndex(this IControllerConstant controllerConstant, int index)
		{
			for (int i = 0; i < controllerConstant.LayerArray.Count; i++)
			{
				ILayerConstant layer = controllerConstant.LayerArray[i].Data;
				if (layer.StateMachineIndex == index && layer.StateMachineMotionSetIndex == 0)
				{
					return layer;
				}
			}
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		public static int GetLayerIndexByStateMachineIndex(this IControllerConstant controllerConstant, int index)
		{
			for (int i = 0; i < controllerConstant.LayerArray.Count; i++)
			{
				ILayerConstant layer = controllerConstant.LayerArray[i].Data;
				if (layer.StateMachineIndex == index && layer.StateMachineMotionSetIndex == 0)
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
				if (checkLayer.StateMachineIndex == layer.StateMachineIndex && checkLayer.StateMachineMotionSetIndex == layer.StateMachineMotionSetIndex)
				{
					return i;
				}
			}
			throw new ArgumentException("Layer hasn't been found", nameof(layer));
		}
	}
}
