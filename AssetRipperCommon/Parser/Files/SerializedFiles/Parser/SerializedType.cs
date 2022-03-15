using AssetRipper.Core.Parser.Files.SerializedFiles.IO;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.Parser
{
	public sealed class SerializedType : SerializedTypeBase
	{
		public int[] TypeDependencies { get; set; }

		private static bool HasScriptID(FormatVersion generation, ClassIDType typeID)
		{
			//Temporary solution to #296
			return typeID == ClassIDType.MonoBehaviour;
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
					ScriptID.Read(reader);
				}
				OldTypeHash.Read(reader);
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
					ScriptID.Write(writer);
				}
				OldTypeHash.Write(writer);
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
