using System.ComponentModel.DataAnnotations;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class LogOnModel : ViewboxModel
	{
		[Required]
		[LocalizedDisplayName("Username")]
		public string UserName { get; set; }

		[DataType(DataType.Password)]
		[LocalizedDisplayName("Password")]
		public string Password { get; set; }

		[LocalizedDisplayName("RememberMe")]
		public bool RememberMe { get; set; }

		public string LabelForgotPassword => Resources.ForgotPassword;

		public string LabelLogon => Resources.Logon;

		public override string LabelCaption => Resources.Logon;
	}
}
