using System.Collections.Generic;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct StateMachineBehaviourVectorDescription : IAssetReadable
	{
		/// <summary>
		/// Greater than 5.0.0b1
		/// </summary>
		public static bool IsReadStateMachineBehaviourIndices(Version version)
		{
#warning unknown
			return version.IsGreater(5, 0, 0, VersionType.Beta);
		}

		public void Read(AssetReader reader)
		{
			m_stateMachineBehaviourRanges = new Dictionary<StateKey, StateRange>();

			m_stateMachineBehaviourRanges.Read(reader);
			if (IsReadStateMachineBehaviourIndices(reader.Version))
			{
				m_stateMachineBehaviourIndices = reader.ReadUInt32Array();
			}
		}
		
		/// <summary>
		/// m_BehavioursRangeInfo previously
		/// </summary>
		public IReadOnlyDictionary<StateKey, StateRange> StateMachineBehaviourRanges => m_stateMachineBehaviourRanges;
		public IReadOnlyList<uint> StateMachineBehaviourIndices => m_stateMachineBehaviourIndices;

		private Dictionary<StateKey, StateRange> m_stateMachineBehaviourRanges;
		private uint[] m_stateMachineBehaviourIndices;
	}
}
