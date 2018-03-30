using System.Text;

namespace UtinyRipper
{
	public static class StringBuilderExtensions
	{
		public static StringBuilder AppendIntent(this StringBuilder _this, int count)
		{
			for(int i = 0; i < count; i++)
			{
				_this.Append('\t');
			}
			return _this;
		}
	}
}
