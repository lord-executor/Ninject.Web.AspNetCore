namespace Ninject.Web.AspNetCore
{
	public class BindingIndex
	{
		public int Count { get; private set; }

		public Item Next()
		{
			return new Item(this, ++Count);
		}

		public class Item
		{
			private readonly BindingIndex _root;

			public int Index { get; }
			public bool IsLatest => Index == _root.Count;

			public Item(BindingIndex root, int index)
			{
				_root = root;
				Index = index;
			}
		}
	}
}
