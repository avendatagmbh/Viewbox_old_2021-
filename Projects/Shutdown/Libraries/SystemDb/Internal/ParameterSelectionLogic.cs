using System;
using System.Collections.Generic;
using DbAccess.Attributes;

namespace SystemDb.Internal
{
	[DbTable("parameter_selection_logics", ForceInnoDb = true)]
	public class ParameterSelectionLogic : ICloneable
	{
		private Dictionary<string, ParameterSelectionLogicText> textCollection;

		[DbColumn("id", AutoIncrement = true)]
		[DbPrimaryKey]
		public int Id { get; set; }

		[DbColumn("issue_id")]
		public int IssueId { get; set; }

		[DbColumn("logic", Length = 1024)]
		public string Logic { get; set; }

		public void SetText(ParameterSelectionLogicText text)
		{
			if (textCollection == null)
			{
				textCollection = new Dictionary<string, ParameterSelectionLogicText>();
			}
			foreach (KeyValuePair<string, ParameterSelectionLogicText> item in textCollection)
			{
				if (item.Value != text)
				{
					textCollection.Add(text.CountryCode, text);
				}
			}
		}

		public void SetTexts(List<ParameterSelectionLogicText> texts)
		{
			if (textCollection == null)
			{
				textCollection = new Dictionary<string, ParameterSelectionLogicText>();
			}
			foreach (ParameterSelectionLogicText t in texts)
			{
				textCollection.Add(t.CountryCode, t);
			}
		}

		public ParameterSelectionLogicText GetWarningTextObject(string countryCode = "de")
		{
			ParameterSelectionLogicText textObject = textCollection[countryCode];
			if (textObject != null)
			{
				return textObject;
			}
			return null;
		}

		public string GetWarningText(string countryCode = "de")
		{
			ParameterSelectionLogicText textObject = textCollection[countryCode];
			if (textObject != null)
			{
				return textObject.Text;
			}
			return "";
		}

		public object Clone()
		{
			return new ParameterSelectionLogic
			{
				Id = Id,
				IssueId = IssueId,
				Logic = Logic,
				textCollection = textCollection
			};
		}
	}
}
