namespace UtinyRipper.Classes.PhysicMaterials
{
	public struct JointSpring : IAssetReadable
	{
		public void Read(AssetStream stream)
		{
			Spring = stream.ReadSingle();
			Damper = stream.ReadSingle();
			TargetPosition = stream.ReadSingle();
		}

		public float Spring { get; private set; }
		public float Damper { get; private set; }
		public float TargetPosition { get; private set; }
	}
}
