using System.Configuration;

namespace Utils
{
	public class PasswordPolicy : ConfigurationElement
	{
		public bool HistoryEnabled => PasswordHistoryCount > 0;

		[ConfigurationProperty("passwordHistoryCount")]
		public int PasswordHistoryCount
		{
			get
			{
				int.TryParse(base["passwordHistoryCount"].ToString(), out var result);
				return result;
			}
			set
			{
				base["passwordHistoryCount"] = value;
			}
		}

		[ConfigurationProperty("validityPeriod")]
		public int ValidityPeriod
		{
			get
			{
				int.TryParse(base["validityPeriod"].ToString(), out var result);
				return result;
			}
			set
			{
				base["validityPeriod"] = value;
			}
		}

		[ConfigurationProperty("passwordPattern")]
		public string PasswordPattern
		{
			get
			{
				return base["passwordPattern"].ToString();
			}
			set
			{
				base["passwordPattern"] = value;
			}
		}

		[ConfigurationProperty("passwordDescription_EN")]
		public string PasswordDescription_EN
		{
			get
			{
				return base["passwordDescription_EN"].ToString();
			}
			set
			{
				base["passwordDescription_EN"] = value;
			}
		}

		[ConfigurationProperty("passwordDescription_DE")]
		public string PasswordDescription_DE
		{
			get
			{
				return base["passwordDescription_DE"].ToString();
			}
			set
			{
				base["passwordDescription_DE"] = value;
			}
		}

		[ConfigurationProperty("passwordDescription_FR")]
		public string PasswordDescription_FR
		{
			get
			{
				return base["passwordDescription_FR"].ToString();
			}
			set
			{
				base["passwordDescription_FR"] = value;
			}
		}

		[ConfigurationProperty("passwordDescription_ES")]
		public string PasswordDescription_ES
		{
			get
			{
				return base["passwordDescription_ES"].ToString();
			}
			set
			{
				base["passwordDescription_ES"] = value;
			}
		}

		[ConfigurationProperty("passwordDescription_IT")]
		public string PasswordDescription_IT
		{
			get
			{
				return base["passwordDescription_IT"].ToString();
			}
			set
			{
				base["passwordDescription_IT"] = value;
			}
		}

		[ConfigurationProperty("enablePolicy")]
		public bool EnablePolicy
		{
			get
			{
				bool.TryParse(base["enablePolicy"].ToString(), out var result);
				return result;
			}
			set
			{
				base["enablePolicy"] = value;
			}
		}
	}
}
