using AssetRipper.Core.IO.Asset;
using System;
using System.Linq;

namespace AssetRipper.Core.Classes.Mesh
{
	public interface IVariableBoneCountWeights : IAsset
	{
		uint[] Data { get; set; }
	}

	public static class VariableBoneCountWeightsExtensions
	{
		public static void CopyValues(this IVariableBoneCountWeights destination, IVariableBoneCountWeights source)
		{
			destination.Data = source.Data?.ToArray() ?? Array.Empty<uint>();
		}
	}
}
