using AssetRipper.Assets.Generics;
using AssetRipper.SourceGenerated.Subclasses.BlendTreeConstant;
using AssetRipper.SourceGenerated.Subclasses.StateConstant;

namespace AssetRipper.SourceGenerated.Extensions
{
	public static class StateConstantExtensions
	{
		public static bool IsBlendTree(this IStateConstant stateConstant)
		{
			if (stateConstant.BlendTreeConstantArray.Count == 0)
			{
				return false;
			}
			return stateConstant.GetBlendTree().NodeArray.Count > 1;
		}

		public static IBlendTreeConstant GetBlendTree(this IStateConstant stateConstant)
		{
			return stateConstant.BlendTreeConstantArray[0].Data;
		}

		public static bool GetWriteDefaultValues(this IStateConstant stateConstant)
		{
			return !stateConstant.Has_WriteDefaultValues() || stateConstant.WriteDefaultValues;
		}

		public static uint GetId(this IStateConstant stateConstant)
		{
			if (stateConstant.Has_FullPathID())
			{
				return stateConstant.FullPathID;
			}
			else if (stateConstant.Has_PathID())
			{
				return stateConstant.PathID;
			}
			else
			{
				return stateConstant.ID;
			}
		}

		public static string GetName(this IStateConstant stateConstant, AssetDictionary<uint, Utf8String> tos)
		{
			if (stateConstant.Has_NameID())
			{
				return tos[stateConstant.NameID];
			}

			string statePath = tos[stateConstant.ID].String; // ParentStateMachineName.StateName
			int pathDelimiterPos = statePath.IndexOf('.');
			if (pathDelimiterPos != -1 && pathDelimiterPos + 1 < statePath.Length)
			{
				return statePath[(pathDelimiterPos + 1)..];
			}

			return statePath;
		}
	}
}
