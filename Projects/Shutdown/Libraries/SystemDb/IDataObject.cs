using System.Collections.Generic;
using System.ComponentModel;

namespace SystemDb
{
	public interface IDataObject : INotifyPropertyChanged
	{
		int Id { get; }

		string Name { get; }

		DataObjectType Type { get; }

		int Ordinal { get; set; }

		bool UserDefined { get; }

		Dictionary<string, object> Properties { get; }

		ILocalizedTextCollection Descriptions { get; }

		ILocalizedTextCollection ObjectTypes { get; }

		event NameChangedHandler NameChanged;

		event OrdinalChangedHandler OrdinalChanged;

		event ObjectRemovedHandler ObjectRemoved;

		void SetDescription(string description, ILanguage language);
	}
}
