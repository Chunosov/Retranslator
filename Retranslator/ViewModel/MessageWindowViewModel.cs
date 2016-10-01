using System.Windows;
using Retranslator.Model;

namespace Retranslator.ViewModel
{
	public class MessageWindow
	{
		public enum Type
		{
			Cancel,
			Ok
		}

		static public void Show(string text, Type type, Window owner = null)
		{
			var viewModel = new MessageWindowViewModel();
			viewModel.MessageType = type;
			viewModel.MessageText = text;
			viewModel.CaptionText = Preferences.AppTitle;
			var presenter = new View.MessageWindow();
			presenter.Owner = owner;
			presenter.DataContext = viewModel;
			presenter.ShowDialog();
		}

		public static void Ok(string text, Window owner = null)
		{
			Show(text, Type.Ok, owner);
		}

		public static void Cancel(string text, Window owner = null)
		{
			Show(text, Type.Cancel, owner);
		}
	}

	public class MessageWindowViewModel
	{
		public MessageWindow.Type MessageType { get; set; }

		public string MessageText { get; set; }

		public string CaptionText { get; set; }
	}
}
