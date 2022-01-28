using AssetRipper.Core.IO.Asset;
using AssetRipper.Core.IO.Extensions;
using AssetRipper.Core.Parser.Files;
using System.Collections.Generic;

namespace AssetRipper.Core.Classes.AnimatorController.State
{
	public sealed class StateMachineBehaviourVectorDescription : IAssetReadable
	{
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasStateMachineBehaviourIndices(UnityVersion version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, UnityVersionType.Final);
		}

		public void Read(AssetReader reader)
		{
			StateMachineBehaviourRanges = new Dictionary<StateKey, StateRange>();

			StateMachineBehaviourRanges.Read(reader);
			if (HasStateMachineBehaviourIndices(reader.Version))
			{
				StateMachineBehaviourIndices = reader.ReadUInt32Array();
			}
		}

		/// <summary>
		/// m_BehavioursRangeInfo previously
		/// </summary>
		public Dictionary<StateKey, StateRange> StateMachineBehaviourRanges { get; set; }
		public uint[] StateMachineBehaviourIndices { get; set; }
	}
}
