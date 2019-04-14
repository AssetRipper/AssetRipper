using System.Collections.Generic;

namespace uTinyRipper.Classes.AnimatorControllers
{
	public struct StateMachineBehaviourVectorDescription : IAssetReadable
	{
		/// <summary>
		/// 5.0.0f1 and greater
		/// </summary>
		public static bool IsReadStateMachineBehaviourIndices(Version version)
		{
			// unknown version
			return version.IsGreaterEqual(5, 0, 0, VersionType.Final);
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
