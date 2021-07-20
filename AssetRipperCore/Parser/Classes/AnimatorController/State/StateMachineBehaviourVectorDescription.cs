using AssetRipper.Parser.Files.File.Version;
using AssetRipper.Parser.IO.Asset.Reader;
using AssetRipper.Parser.IO.Extensions;
using System.Collections.Generic;

namespace AssetRipper.Parser.Classes.AnimatorController.State
{
	public struct StateMachineBehaviourVectorDescription : IAssetReadable
	{
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool HasStateMachineBehaviourIndices(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
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
