namespace Viewbox.Models
{
	public class ErrorModel : ViewboxModel
	{
		public string Message { get; internal set; }

		public override string LabelCaption => "Viewbox";
	}
}
