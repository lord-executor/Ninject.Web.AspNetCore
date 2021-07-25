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
			private static int _globalCount = 0;
			private readonly BindingIndex _root;
			private readonly int _globalIndex;

			public int Index { get; }
			public bool IsLatest => Index == _root.Count;
			public int Precedence => _globalCount - _globalIndex + 1;

			public Item(BindingIndex root, int index)
			{
				_root = root;
				_globalIndex = ++_globalCount;
				Index = index;
			}
		}
	}
}
