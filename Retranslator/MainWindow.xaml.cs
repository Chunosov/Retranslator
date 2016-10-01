using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Retranslator.Model;
using Retranslator.View;
using Retranslator.ViewModel;

using Cursors = System.Windows.Input.Cursors;
using KeyEventHandler = System.Windows.Input.KeyEventHandler;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Retranslator
{
    public partial class MainWindow
    {
    	private bool _phraseBookLoaded;

        private TranslationViewModel _translationViewModel;

        public static readonly RoutedCommand OpenMruItem = new RoutedCommand("OpenMruItem", typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            /*SectionList.AddHandler(ItemsSourceChangedBehavior.ItemsSourceChangedEvent,
                new RoutedEventHandler(SectionListItemsSourceChanged));
            EntryList.AddHandler(ItemsSourceChangedBehavior.ItemsSourceChangedEvent, 
                new RoutedEventHandler(EntryListItemsSourceChanged));*/

            Preferences.Load();

            MenuRecent.DataContext = Preferences.Instance;

			TranslationTextBox.AddHandler(Keyboard.KeyDownEvent, (KeyEventHandler)TranslationTextBoxKeyDown);
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            if (Preferences.Instance.UsePhraseBook && PhraseBook.Instance.Modified)
                PhraseBook.Save(Preferences.Instance.PhraseBookPath);

			Preferences.Save();
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = _translationViewModel != null && !_translationViewModel.Close();
        }

        /// <summary>
        /// Обработчик команды открытия файла. Используется как для кнопки/меню, 
        /// так и для MRU-элемента, отличается только параметром команды.
        /// </summary>
        public void OpenTranslationExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (_translationViewModel != null && !_translationViewModel.Close()) return;

                string langSourceFile;
                string translationFile;
                bool newTranslation = false;

                var mruItem = e.Parameter as Preferences.MruItem;
                if (mruItem == null)
                {
                    // Если параметр команды не является MRU-элементом, то отображаем диалог открытия файлов.
                    var openParams = new OpeningParameters();
                    var dlg = new OpenDialog { Owner = this, DataContext = openParams };
                    if (dlg.ShowDialog() != true) return;

                    langSourceFile = openParams.LangSourceFile;
                    translationFile = openParams.TranslationFile;
                    newTranslation = openParams.CreateNewTranslation;
                }
                else
                {
                    langSourceFile = mruItem.LangSourceFile;
                    translationFile = mruItem.TranslationFile;
                }

                if (!File.Exists(langSourceFile))
                {
                    MessageBox.Show(string.Format("Language source file {0} does not exist.", langSourceFile),
                                    Preferences.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!File.Exists(translationFile) && !newTranslation)
                {
                    MessageBox.Show(string.Format("Translation file {0} does not exist", translationFile),
									Preferences.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

				LoadPhraseBook();

                Translation translation;
            	IList<OpeningReportItem> report;
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    translation = new Translation(langSourceFile, newTranslation ? null : translationFile);
					report = translation.Load(Preferences.Instance.AutoTranslate && _phraseBookLoaded);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

				_translationViewModel = new TranslationViewModel(translation, this);
				DataContext = _translationViewModel;
				
				if (report != null && report.Count > 0 && Preferences.Instance.ShowOpeningReport)
                {
					if (report.Any(item => item.Type == OpeningReportItem.ItemType.ExtraTranslation))
						_translationViewModel.Modified = true;

                    var dlg = new OpeningReportWindow { Owner = this, DataContext = report };
                    dlg.ShowDialog();
                }

                AdjustSectionListColumns();

                if (newTranslation)
                    _translationViewModel.ShowProperties.Execute(null);

				if (!newTranslation)
					Preferences.Instance.AddMruItem(langSourceFile, translationFile);

            	Title = Preferences.AppTitle + " - " + _translationViewModel.LangSourceFileName;

            	// Сортировка осуществляется при загрузке файла: во-первых, полезно для сохранения,
            	// а во-вторых, если сортировать ListView, то порядок перехода по командам
            	// SelectNext/SelectPrev не совпадает с видимым порядком отображения.
            	//var view = CollectionViewSource.GetDefaultView(SectionList.ItemsSource);
            	//view.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            }
            catch (Exception ex)
            {
				MessageBox.Show(ex.ToString(), Preferences.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

		private void LoadPhraseBook()
		{
			if (Preferences.Instance.UsePhraseBook && !_phraseBookLoaded)
			{
				Mouse.OverrideCursor = Cursors.Wait;
				try
				{
					PhraseBook.Load(Preferences.Instance.PhraseBookPath);
					_phraseBookLoaded = true;
				}
				catch (Exception ex)
				{
					MessageBox.Show(
						@"Error during loading of Phrase Book:\n\n" + ex.Message, 
						Preferences.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
					Preferences.Instance.UsePhraseBook = false;
				}
				finally
				{
					Mouse.OverrideCursor = null;
				}
			}
		}

    	private void SectionsFilterChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext == null) return;
            var view = CollectionViewSource.GetDefaultView(SectionList.ItemsSource);
            var pattern = SectionsFilter.Text.Trim().ToLower();
            view.Filter = string.IsNullOrEmpty(pattern)? null: new Predicate<object>(
                item => ((TranslationSectionViewModel)item).Name.ToLower().Contains(pattern));
        }

        #region Lists Adjustment

        private void SectionListSizeChanged(object sender, SizeChangedEventArgs e)
        {
             AdjustSectionListColumns();
        }

        //private void SectionListItemsSourceChanged(object sender, RoutedEventArgs e)
        //{
        //    AdjustSectionListColumns();
        //}

        private void EntryListSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustEntryListColumns();
        }

        //private void EntryListItemsSourceChanged(object sender, RoutedEventArgs e)
        //{
        //    AdjustEntryListColumns();
        //}

        private void AdjustSectionListColumns()
        {
            if (SectionList.ItemsSource == null) return;

            var border = VisualTreeHelper.GetChild(SectionList, 0) as Decorator;
            if (border == null) return;

            var scroller = border.Child as ScrollViewer;
            if (scroller == null) return;

            var presenter = scroller.Content as ItemsPresenter;
            if (presenter == null) return;

            SectionListColumnName.Width = presenter.ActualWidth - 6 -
                SectionListColumnState.ActualWidth - SectionListColumnEntries.ActualWidth;
        }

        private void AdjustEntryListColumns()
        {
            if (EntryList.ItemsSource == null) return;

            /*
             * TODO выставление авто-размера колонки не приводит к тому что ее ActualWidth немедленно пересчитывается
             * Когда-то позже он оказывается правильным, но в этом методе еще рассчитан и ширины последних двух колонок
             * из-за этого определяются неправильно. Поэтому пока задаем размеры первых колонок вручную.
             * 
            EntryListColumnId.Width = 0;
            EntryListColumnId.Width = Double.NaN;

            EntryListColumnName.Width = 0;
            EntryListColumnName.Width = Double.NaN;
            */

            var border = VisualTreeHelper.GetChild(EntryList, 0) as Decorator;
            if (border == null) return;

            var scroller = border.Child as ScrollViewer;
            if (scroller == null) return;

            var presenter = scroller.Content as ItemsPresenter;
            if (presenter == null) return;

            var freeWidth = presenter.ActualWidth - 6 -
                            EntryListColumnState.ActualWidth -
                            EntryListColumnId.ActualWidth -
                            EntryListColumnName.ActualWidth;

            EntryListColumnSource.Width = freeWidth * 0.5;
            EntryListColumnTranslation.Width = freeWidth * 0.5;
        }

        #endregion

        private void SectionListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SectionList.ScrollIntoView(SectionList.SelectedItem);
            AdjustEntryListColumns();
        }

        private void EntryListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EntryList.ScrollIntoView(EntryList.SelectedItem);
        }

        private void EntryListKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                TranslationTextBox.Focus();
                TranslationTextBox.SelectAll();
				e.Handled = true;
			}
		}

        private void TranslationTextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
			if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
			{
				EntryList.Focus();
				// Список получает фокус, выделен текущий элемент, но при нажатии стрелки 
				// вниз или вверх выделение перскакивает на первый элемент списка.
			}
		}

        private void PreferencesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new PreferencesDialog {Owner = this, DataContext = Preferences.Instance.Clone()};
            if (dlg.ShowDialog() == true)
            {
                Preferences.Instance = dlg.DataContext as Preferences;
				Preferences.Save();
				// Как-то оповестить об изменении, когда это будет нужно.
            }
        }

        private void ApplicationCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

		private void GuessListMouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			_translationViewModel.ApplyGuess.Execute(GuessList.SelectedItem);
		}

		private void GuessListSizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (GuessList.ItemsSource == null) return;

			var border = VisualTreeHelper.GetChild(GuessList, 0) as Decorator;
			if (border == null) return;

			var scroller = border.Child as ScrollViewer;
			if (scroller == null) return;

			var presenter = scroller.Content as ItemsPresenter;
			if (presenter == null) return;

			GuessListColumn.Width = presenter.ActualWidth - 6;
		}

		private void TranslationLostFocus(object sender, object e)
		{
			// Не добавляем перевод в справочник на каждый чих
			// Только при нажатти кнопки "Применить перевод"
			//if (Preferences.Instance.AutoGrowPhraseBook)
				//_translationViewModel.GrowPhraseBookAuto.Execute(null);
		}

		private void ToolBarLoaded(object sender, RoutedEventArgs e)
		{
			var toolBar = (System.Windows.Controls.ToolBar) sender;
			var overflowGrid = toolBar.Template.FindName("OverflowGrid", toolBar) as FrameworkElement;
			if (overflowGrid != null)
				overflowGrid.Visibility = Visibility.Collapsed;
		}
    }
}
