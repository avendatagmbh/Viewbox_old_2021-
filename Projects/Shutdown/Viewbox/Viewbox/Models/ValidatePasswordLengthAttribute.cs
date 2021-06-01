using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace Viewbox.Models
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
	public sealed class ValidatePasswordLengthAttribute : ValidationAttribute, IClientValidatable
	{
		private const string _defaultErrorMessage = "'{0}' muss mindestens {1} Zeichen lang sein.";

		private readonly int _minCharacters = Membership.Provider.MinRequiredPasswordLength;

		public ValidatePasswordLengthAttribute()
			: base("'{0}' muss mindestens {1} Zeichen lang sein.")
		{
		}

		public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
		{
			return new ModelClientValidationStringLengthRule[1]
			{
				new ModelClientValidationStringLengthRule(FormatErrorMessage(metadata.GetDisplayName()), _minCharacters, int.MaxValue)
			};
		}

		public override string FormatErrorMessage(string name)
		{
			return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, new object[2] { name, _minCharacters });
		}

		public override bool IsValid(object value)
		{
			string valueAsString = value as string;
			return valueAsString != null && valueAsString.Length >= _minCharacters;
		}
	}
}
