using System;
using System.Windows;

namespace ViewBuilder.Windows
{
	public abstract class DlgWindowBase : Window
	{
		protected DlgWindowBase()
		{
			this.Initialized += OnInitialized;
		}

		private void OnInitialized(object sender, EventArgs eventArgs)
		{
			CenterWindowOnScreen();
		}

		private void CenterWindowOnScreen()
		{
			double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
			double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
			double windowWidth = this.Width;
			double windowHeight = this.Height;
			this.Left = (screenWidth / 2) - (windowWidth / 2);
			this.Top = (screenHeight / 2) - (windowHeight / 2);
		}
	
		
	}	
}
