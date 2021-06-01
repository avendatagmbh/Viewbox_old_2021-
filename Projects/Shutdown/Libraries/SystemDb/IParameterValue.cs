namespace SystemDb
{
	public interface IParameterValue
	{
		string Value { get; }

		ILocalizedTextCollection Descriptions { get; }

		void SetDescription(string description, ILanguage language);
	}
}
