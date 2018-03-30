using System.Collections.Generic;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct ControllerConstant : IAssetReadable
	{		
		public void Read(AssetStream stream)
		{
			m_layerArray = stream.ReadArray<OffsetPtr<LayerConstant>>();
			m_stateMachineArray = stream.ReadArray<OffsetPtr<StateMachineConstant>>();
			Values.Read(stream);
			DefaultValues.Read(stream);
		}

		public IReadOnlyList<OffsetPtr<LayerConstant>> LayerArray => m_layerArray;
		public IReadOnlyList<OffsetPtr<StateMachineConstant>> StateMachineArray => m_stateMachineArray;

		public OffsetPtr<ValueArrayConstant> Values;
		public OffsetPtr<ValueArray> DefaultValues;

		/// <summary>
		/// m_HumanLayerArray previously
		/// </summary>
		private OffsetPtr<LayerConstant>[] m_layerArray;
		private OffsetPtr<StateMachineConstant>[] m_stateMachineArray;
	}
}
