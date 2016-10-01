using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Retranslator.ViewModel;

namespace Retranslator.View
{
    public partial class PhraseBookWindow
    {
        public PhraseBookWindow()
        {
            InitializeComponent();
        }

        private void PhraseListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PhraseList.ScrollIntoView(PhraseList.SelectedItem);
        }

		private void PhraseListSizeChanged(object sender, SizeChangedEventArgs e)
		{
			AdjustPhraseListColumns();
		}

		private void AdjustPhraseListColumns()
		{
			if (PhraseList.ItemsSource == null) return;

			var border = VisualTreeHelper.GetChild(PhraseList, 0) as Decorator;
			if (border == null) return;

			var scroller = border.Child as ScrollViewer;
			if (scroller == null) return;

			var presenter = scroller.Content as ItemsPresenter;
			if (presenter == null) return;

			PhraseListColumnName.Width = presenter.ActualWidth - 6 - PhraseListColumnTrans.ActualWidth;
		}

		private void TranslationsListSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (PhraseList.ItemsSource == null) return;

			var border = VisualTreeHelper.GetChild(PhraseList, 0) as Decorator;
			if (border == null) return;

			var scroller = border.Child as ScrollViewer;
			if (scroller == null) return;

			var presenter = scroller.Content as ItemsPresenter;
			if (presenter == null) return;

			TranslationsListColumnTrans.Width = presenter.ActualWidth - 6;
		}

		private void TranslationsListMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			//var viewModel = DataContext as PhraseBookViewModel;
			//if (viewModel != null)
			//    viewModel.EditTranslation.Execute(null);
		}
    }
}
