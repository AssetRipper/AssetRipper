namespace uTinyRipper.YAML
{
	public abstract class YAMLNode
	{
		internal virtual void Emit(Emitter emitter)
		{
			bool isWrote = false;
			if(!CustomTag.IsEmpty)
			{
				emitter.Write(CustomTag.ToString()).WriteWhitespace();
				isWrote = true;
			}
			if (Anchor != string.Empty)
			{
				emitter.Write("&").Write(Anchor).WriteWhitespace();
				isWrote = true;
			}
			
			if(isWrote)
			{
				if(IsMultiline)
				{
					emitter.WriteLine();
				}
			}
		}

		public abstract YAMLNodeType NodeType { get; }
		public abstract bool IsMultiline { get; }
		public abstract bool IsIndent { get; }
		
		public string Tag
		{
			get => CustomTag.Content;
			set => CustomTag = new YAMLTag(YAMLWriter.DefaultTagHandle, value);
		}
		public YAMLTag CustomTag { get; set; }
		public string Anchor { get; set; } = string.Empty;
	}
}
