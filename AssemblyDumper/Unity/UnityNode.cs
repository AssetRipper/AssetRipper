using System.Text.Json.Serialization;

namespace AssemblyDumper.Unity
{
	public class UnityNode
	{
		/// <summary>
		/// The unique type name used in the <see cref = "SharedState"/> dictionaries
		/// </summary>
		public string TypeName { get => typeName; set => typeName = value ?? ""; }
		/// <summary>
		/// The original type name as obtained from the json file
		/// </summary>
		[JsonIgnore]
		public string OriginalTypeName
		{
			get => string.IsNullOrEmpty(originalTypeName) ? TypeName : originalTypeName;
			set => originalTypeName = value ?? "";
		}
		public string Name { get => name; set => name = value ?? ""; }
		/// <summary>
		/// The original name as obtained from the json file
		/// </summary>
		[JsonIgnore]
		public string OriginalName
		{
			get => string.IsNullOrEmpty(originalName) ? Name : originalName;
			set => originalName = value ?? "";
		}
		public byte Level { get; set; }
		public int ByteSize { get; set; }
		public int Index { get; set; }
		public short Version { get; set; }
		public byte TypeFlags { get; set; }
		public uint MetaFlag { get; set; }
		public List<UnityNode> SubNodes { get => subNodes; set => subNodes = value ?? new(); }

		private string originalTypeName = "";
		private string originalName = "";
		private string typeName = "";
		private string name = "";
		private List<UnityNode> subNodes = new();

		/// <summary>
		/// Deep clones a node and all its subnodes<br/>
		/// Warning: Deep cloning a node with a circular hierarchy will cause an endless loop
		/// </summary>
		/// <returns>The new node</returns>
		public UnityNode DeepClone()
		{
			UnityNode? cloned = new UnityNode();
			cloned.TypeName = CloneString(TypeName);
			cloned.originalTypeName = CloneString(originalTypeName);
			cloned.Name = CloneString(Name);
			cloned.originalName = CloneString(originalName);
			cloned.Level = Level;
			cloned.ByteSize = ByteSize;
			cloned.Index = Index;
			cloned.Version = Version;
			cloned.TypeFlags = TypeFlags;
			cloned.MetaFlag = MetaFlag;
			cloned.SubNodes = SubNodes.ConvertAll(x => x.DeepClone());
			return cloned;
		}

		private static string CloneString(string? @string)
		{
			return @string == null ? "" : new string(@string);
		}
	}
}