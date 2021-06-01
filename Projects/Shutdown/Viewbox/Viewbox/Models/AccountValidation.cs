using System.Web.Security;

namespace Viewbox.Models
{
	public static class AccountValidation
	{
		public static string ErrorCodeToString(MembershipCreateStatus createStatus)
		{
			return createStatus switch
			{
				MembershipCreateStatus.DuplicateUserName => "Der Benutzername ist bereits vorhanden. Geben Sie einen anderen Benutzernamen ein.", 
				MembershipCreateStatus.DuplicateEmail => "Für diese E-Mail-Adresse ist bereits ein Benutzername vorhanden. Geben Sie eine andere E-Mail-Adresse ein.", 
				MembershipCreateStatus.InvalidPassword => "Das angegebene Kennwort ist ungültig. Geben Sie einen gültigen Kennwortwert ein.", 
				MembershipCreateStatus.InvalidEmail => "Die angegebene E-Mail-Adresse ist ungültig. Überprüfen Sie den Wert, und wiederholen Sie den Vorgang.", 
				MembershipCreateStatus.InvalidAnswer => "Die angegebene Kennwortabrufantwort ist ungültig. Überprüfen Sie den Wert, und wiederholen Sie den Vorgang.", 
				MembershipCreateStatus.InvalidQuestion => "Die angegebene Kennwortabruffrage ist ungültig. Überprüfen Sie den Wert, und wiederholen Sie den Vorgang.", 
				MembershipCreateStatus.InvalidUserName => "Der angegebene Benutzername ist ungültig. Überprüfen Sie den Wert, und wiederholen Sie den Vorgang.", 
				MembershipCreateStatus.ProviderError => "Vom Authentifizierungsanbieter wurde ein Fehler zurückgegeben. Überprüfen Sie die Eingabe, und wiederholen Sie den Vorgang. Sollte das Problem weiterhin bestehen, wenden Sie sich an den zuständigen Systemadministrator.", 
				MembershipCreateStatus.UserRejected => "Die Benutzererstellungsanforderung wurde abgebrochen. Überprüfen Sie die Eingabe, und wiederholen Sie den Vorgang. Sollte das Problem weiterhin bestehen, wenden Sie sich an den zuständigen Systemadministrator.", 
				_ => "Unbekannter Fehler. Überprüfen Sie die Eingabe, und wiederholen Sie den Vorgang. Sollte das Problem weiterhin bestehen, wenden Sie sich an den zuständigen Systemadministrator.", 
			};
		}
	}
}
