namespace Ninject.Web.AspNetCore.Test.Fakes
{
	public class TestObject
	{
		private int _value;

		public TestObject(int value)
		{
			this._value = value;
		}

		public override int GetHashCode()
		{
			return _value;
		}

		public override bool Equals(object obj)
		{
			var other = obj as TestObject;
			if (other != null)
			{
				return other._value.Equals(_value);
			}

			return _value.Equals(obj);
		}

		public void ChangeHashCode(int i)
		{
			_value = i;
		}
	}
}
