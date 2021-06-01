namespace DbAccess
{
	public sealed class HistoryValues
	{
		private int _id;

		private int _userId;

		private int _parameterId;

		private string _value;

		private int _selectionType;

		private bool _userDefined;

		private int _ordinal;

		public int Id => _id;

		public int UserId => _userId;

		public int ParameterId => _parameterId;

		public string Value => _value;

		public int SelectionType => _selectionType;

		public int Ordinal => _ordinal;

		public bool UserDefined => _userDefined;

		public HistoryValues(int id, int userId, int parameterId, string value, int selectionType, int ordinal, bool userDefined)
		{
			_id = id;
			_userId = userId;
			_parameterId = parameterId;
			_value = value;
			_selectionType = selectionType;
			_ordinal = ordinal;
			_userDefined = userDefined;
		}
	}
}
