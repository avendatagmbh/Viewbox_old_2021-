using System;
using System.Collections.Generic;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class NotEnoughFreeSpaceModel : ViewboxModel
	{
		private string _driveName;

		public string DriveName
		{
			get
			{
				return _driveName;
			}
			set
			{
				_driveName = value;
			}
		}

		public new DialogModel Dialog { get; set; }

		public override string LabelCaption
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public NotEnoughFreeSpaceModel(string driveName)
		{
			DriveName = driveName;
			if (DriveName != string.Empty)
			{
				Dialog = new DialogModel
				{
					Title = Resources.hardDriveFullWarningTitle,
					Content = Resources.hardDriveFullWarningText,
					DialogType = DialogModel.Type.Warning,
					Buttons = new List<DialogModel.Button>
					{
						new DialogModel.Button
						{
							Caption = "OK"
						}
					}
				};
			}
		}
	}
}
