using AssetRipper.Core.Parser.Files.SerializedFiles.IO;

namespace AssetRipper.Core.Parser.Files.SerializedFiles.Parser
{
	public sealed class SerializedTypeReference : SerializedTypeBase
	{
		public string ClassName { get; set; }
		public string NameSpace { get; set; }
		public string AsmName { get; set; }

		public override void Read(SerializedReader reader, bool hasTypeTree)
		{
			base.Read(reader, hasTypeTree);

			if (HasHash(reader.Generation))
			{
				if (ScriptTypeIndex >= 0)
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
					ClassName = reader.ReadStringZeroTerm();
					NameSpace = reader.ReadStringZeroTerm();
					AsmName = reader.ReadStringZeroTerm();
				}
			}
		}

		public override void Write(SerializedWriter writer, bool hasTypeTree)
		{
			base.Write(writer, hasTypeTree);

			if (HasHash(writer.Generation))
			{
				if (ScriptTypeIndex >= 0)
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
					writer.WriteStringZeroTerm(ClassName);
					writer.WriteStringZeroTerm(NameSpace);
					writer.WriteStringZeroTerm(AsmName);
				}
			}
		}
	}
}
