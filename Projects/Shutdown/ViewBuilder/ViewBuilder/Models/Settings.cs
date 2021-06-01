using System;
using ViewBuilderBusiness.Structures.Config;

namespace ViewBuilder.Models
{
	static class Settings
	{
		private static ProfileConfig _currentProfileConfig;
		public static ProfileConfig CurrentProfileConfig
		{
			get
			{
				if(_currentProfileConfig == null) 
					throw new MissingFieldException("ProfileConfig has not been set!");
				return _currentProfileConfig;
			}
			set { _currentProfileConfig = value; }
		}
	}
}
