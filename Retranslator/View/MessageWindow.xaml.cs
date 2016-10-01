using System.Windows;

namespace Retranslator.View
{
	public partial class MessageWindow
	{
		public MessageWindow()
		{
			InitializeComponent();
		}

		private void ButtonOkClick(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
