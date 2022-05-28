using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.CSharp.Transforms;
using Attribute = ICSharpCode.Decompiler.CSharp.Syntax.Attribute;

namespace AssetRipper.Library.Exporters.Scripts.Transforms
{
	/// <summary>
	/// Fixes compiler generated accessors from IL2CPP.
	/// <para>
	/// Replaces:
	/// <code>PropertyType PropertyName
	/// {
	///     [CompilerGenerated]
	///     get
	///     {
	///			return default(PropertyType);
	///     }
	///     [CompilerGenerated]
	///     set
	///     {
	///     }
	/// }</code>
	/// </para>
	/// <para>
	/// With:
	/// <code>PropertyType PropertyName
	/// {
	///		get;
	///		set;
	/// }</code>
	/// </para>
	/// </summary>
	internal class FixCompilerGeneratedAccessorsTransform : DepthFirstAstVisitor, IAstTransform
	{
		public override void VisitAccessor(Accessor accessor)
		{
			bool compilerGenerated = false;
			foreach (AttributeSection attributeSection in accessor.Attributes)
			{
				foreach (Attribute attribute in attributeSection.Attributes)
				{
					if (attribute.Type is SimpleType type && type.Identifier == "CompilerGenerated")
					{
						compilerGenerated = true;
						attribute.Remove();
					}
				}

				if (attributeSection.Attributes.Count == 0)
				{
					attributeSection.Remove();
				}
			}

			if (compilerGenerated && !accessor.Body.IsNull)
			{
				accessor.Body = BlockStatement.Null;
			}
		}

		public void Run(AstNode rootNode, TransformContext context)
		{
			rootNode.AcceptVisitor(this);
		}
	}
}
