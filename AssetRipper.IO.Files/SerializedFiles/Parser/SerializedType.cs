using AssetRipper.IO.Files.SerializedFiles.IO;

namespace AssetRipper.IO.Files.SerializedFiles.Parser
{
	public sealed class SerializedType : SerializedTypeBase
	{
		public int[] TypeDependencies { get; set; } = Array.Empty<int>();

		private static bool HasScriptID(FormatVersion generation, int typeID)
		{
			//Temporary solution to #296
			return typeID == 114;//MonoBehaviour
								 //Previous code:
								 //(generation < FormatVersion.RefactoredClassId && typeID < 0)
								 //|| (generation >= FormatVersion.RefactoredClassId && typeID == ClassIDType.MonoBehaviour);
		}

		public override void Read(SerializedReader reader, bool hasTypeTree)
		{
			base.Read(reader, hasTypeTree);

			if (HasHash(reader.Generation))
			{
				if (HasScriptID(reader.Generation, TypeID))
				{
					ScriptID = reader.ReadBytes(16);
				}
				OldTypeHash = reader.ReadBytes(16);
			}

			if (hasTypeTree)
			{
				OldType.Read(reader);
				if (HasTypeDependencies(reader.Generation))
				{
					TypeDependencies = reader.ReadInt32Array();
				}
			}
		}

		public override void Write(SerializedWriter writer, bool hasTypeTree)
		{
			base.Write(writer, hasTypeTree);

			if (HasHash(writer.Generation))
			{
				if (HasScriptID(writer.Generation, TypeID))
				{
					writer.Write(ScriptID);
				}
				writer.Write(OldTypeHash);
			}

			if (hasTypeTree)
			{
				OldType.Write(writer);
				if (HasTypeDependencies(writer.Generation))
				{
					writer.WriteArray(TypeDependencies);
				}
			}
		}
	}
}
