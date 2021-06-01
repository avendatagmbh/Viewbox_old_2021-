using System.ComponentModel;
using Viewbox.Properties;

namespace Viewbox
{
	public class LocalizedDisplayNameAttribute : DisplayNameAttribute
	{
		private readonly string resourceName;

		public override string DisplayName => Resources.ResourceManager.GetString(resourceName);

		public LocalizedDisplayNameAttribute(string resourceName)
		{
			this.resourceName = resourceName;
		}
	}
}
