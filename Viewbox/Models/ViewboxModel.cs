using SystemDb;

namespace Viewbox.Models
{
	public abstract class ViewboxModel : BaseModel
	{
		public IUser User => ViewboxSession.User;

		public BoxHeaderModel Header { get; private set; }

		public abstract string LabelCaption { get; }

		public DialogModel Dialog { get; set; }

		public ILanguage Language => ViewboxSession.Language;

		public ViewboxModel()
		{
			Header = new BoxHeaderModel(this);
		}

		protected string GetParameterDescription(IParameter param, ILanguage language)
		{
			if (param == null)
			{
				return string.Empty;
			}
			string parName = param.GetDescription(language).Trim();
			if (parName.EndsWith("="))
			{
				parName = parName.Substring(0, parName.Length - 1);
			}
			return parName;
		}

		protected string GetParameterDescription(IParameter param)
		{
			return GetParameterDescription(param, Language);
		}
	}
}
