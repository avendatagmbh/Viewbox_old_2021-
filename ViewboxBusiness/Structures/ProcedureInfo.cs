using Utils;

namespace ViewboxBusiness.Structures
{
	public class ProcedureInfo : NotifyPropertyChangedBase
	{
		private bool _executeCreateTempTables;

		private bool _executeStoredProcedure;

		private bool _onlyMetadata;

		public bool ExecuteCreateTempTables
		{
			get
			{
				return _executeCreateTempTables;
			}
			set
			{
				if (_executeCreateTempTables != value)
				{
					_executeCreateTempTables = value;
					OnPropertyChanged("ExecuteCreateTempTables");
					if (_executeCreateTempTables)
					{
						OnlyMetadata = false;
					}
				}
			}
		}

		public bool ExecuteStoredProcedure
		{
			get
			{
				return _executeStoredProcedure;
			}
			set
			{
				if (_executeStoredProcedure != value)
				{
					_executeStoredProcedure = value;
					OnPropertyChanged("ExecuteStoredProcedure");
				}
			}
		}

		public bool OnlyMetadata
		{
			get
			{
				return _onlyMetadata;
			}
			set
			{
				if (_onlyMetadata != value)
				{
					_onlyMetadata = value;
					OnPropertyChanged("OnlyMetadata");
					if (_onlyMetadata)
					{
						ExecuteStoredProcedure = false;
					}
				}
			}
		}

		public ProcedureInfo()
		{
			ExecuteCreateTempTables = true;
			ExecuteStoredProcedure = true;
			OnlyMetadata = false;
		}
	}
}
