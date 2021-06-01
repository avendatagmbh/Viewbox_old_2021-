using SystemDb;
using Viewbox.Properties;

namespace Viewbox.Models
{
	public class FileSystemModel : ViewboxModel
	{
		public IFileObjectCollection Files { get; set; }

		public IDirectoryObjectCollection Directories { get; set; }

		public int SelectedDirectoryId { get; set; }

		public override string LabelCaption => Resources.FileSystem;
	}
}
