using System;
using System.Collections.Generic;
using UtinyRipper.AssetExporters;
using UtinyRipper.Exporter.YAML;

namespace UtinyRipper.Classes.AnimatorControllers
{
	public struct StateMachineConstant : IAssetReadable, IYAMLExportable
	{
		/// <summary>
		/// 5.0.0 and greater
		/// </summary>
		public static bool IsReadConstantArray(Version version)
		{
			return version.IsGreaterEqual(5);
		}

		public void Read(AssetStream stream)
		{
			m_stateConstantArray = stream.ReadArray<OffsetPtr<StateConstant>>();
			m_anyStateTransitionConstantArray = stream.ReadArray<OffsetPtr<TransitionConstant>>();
			if(IsReadConstantArray(stream.Version))
			{
				m_selectorStateConstantArray = stream.ReadArray<OffsetPtr<SelectorStateConstant>>();
			}

			DefaultState = stream.ReadUInt32();
			MotionSetCount = stream.ReadUInt32();
		}

		public YAMLNode ExportYAML(IExportContainer container)
		{
			throw new NotSupportedException();
		}

		public IReadOnlyList<OffsetPtr<StateConstant>> StateConstantArray => m_stateConstantArray;
		public IReadOnlyList<OffsetPtr<TransitionConstant>> AnyStateTransitionConstantArray => m_anyStateTransitionConstantArray;
		public IReadOnlyList<OffsetPtr<SelectorStateConstant>> SelectorStateConstantArray => m_selectorStateConstantArray;
		public uint DefaultState { get; private set; }
		public uint MotionSetCount { get; private set; }

		private OffsetPtr<StateConstant>[] m_stateConstantArray;
		private OffsetPtr<TransitionConstant>[] m_anyStateTransitionConstantArray;
		private OffsetPtr<SelectorStateConstant>[] m_selectorStateConstantArray;
	}
}
