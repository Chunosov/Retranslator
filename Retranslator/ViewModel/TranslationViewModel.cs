using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Retranslator.Model;
using Retranslator.Utils;
using Retranslator.View;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Retranslator.ViewModel
{
	public class TranslationViewModel : ViewModelBase
	{
		private readonly Translation _translation;
		private readonly Window _presenter;
		private TranslationSectionViewModel _selectedSection;
		private bool _modified;
		private string _statusMessage;
		private bool _statusMessageShown;
		private System.Threading.Timer _statusMessageTimer;

		public DelegateCommand NavigatePrev { get; set; }
		public DelegateCommand NavigateNext { get; set; }
		public DelegateCommand NavigatePrevUnfinished { get; set; }
		public DelegateCommand NavigateNextUnfinished { get; set; }
		public DelegateCommand ShowProperties { get; set; }
		public DelegateCommand Save { get; set; }
		public DelegateCommand SaveAs { get; set; }
		public DelegateCommand MakePhraseBook { get; set; }
		public DelegateCommand ShowPhraseBook { get; set; }
		public DelegateCommand GrowPhraseBookAuto { get; set; }
		public DelegateCommand GrowPhraseBookManual { get; set; }
		public DelegateCommand Find { get; set; }
		public DelegateCommand ApplyTranslation { get; set; }
		public DelegateCommand ApplyGuess { get; set; }
		public DelegateCommand CopyEntryName { get; set; }
		public DelegateCommand CopyEntrySource { get; set; }
		public DelegateCommand CopyEntryTarget { get; set; }

		public TranslationViewModel(Translation translation, Window presenter)
		{
			_translation = translation;
			_presenter = presenter;

			NavigatePrev = new DelegateCommand(NavigatePrevExecuted);
			NavigateNext = new DelegateCommand(NavigateNextExecuted);
			NavigatePrevUnfinished = new DelegateCommand(NavigatePrevUnfinishedExecuted);
			NavigateNextUnfinished = new DelegateCommand(NavigateNextUnfinishedExecuted);
			ShowProperties = new DelegateCommand(ShowPropertiesExecuted);
			Save = new DelegateCommand(SaveExecuted);
			SaveAs = new DelegateCommand(SaveAsExecuted);
			MakePhraseBook = new DelegateCommand(MakePhraseBookExecuted);
			ShowPhraseBook = new DelegateCommand(ShowPhraseBookExecuted);
			GrowPhraseBookAuto = new DelegateCommand(GrowPhraseBookAutoExecuted);
			GrowPhraseBookManual = new DelegateCommand(GrowPhraseBookManualExecuted);
			Find = new DelegateCommand(FindExecuted);
			ApplyTranslation = new DelegateCommand(ApplyTranslationExecuted);
			ApplyGuess = new DelegateCommand(ApplyGuessExecuted);
			CopyEntryName = new DelegateCommand(CopyEntryNameExecuted);
			CopyEntrySource = new DelegateCommand(CopyEntrySourceExecuted);
			CopyEntryTarget = new DelegateCommand(CopyEntryTargetExecuted);

			Sections = translation.Sections.Select(section => new TranslationSectionViewModel(this, section)).ToList();
		}

		#region View Model Properties

		public IList<TranslationSectionViewModel> Sections { get; private set; }

		public string LangSourceFileName
		{
			get { return _translation.LangSourceFileName; }
		}

		public string TranslationFileName
		{
			get { return _translation.TranslationFileName; }
		}

		public TranslationSectionViewModel SelectedSection
		{
			get { return _selectedSection; }
			set
			{
				if (_selectedSection == value) return;
				_selectedSection = value;

				OnPropertyChanged("SelectedSection");

				_selectedSection.SelectedEntry = _selectedSection.Entries.FirstOrDefault();
			}
		}

		public bool Modified
		{
			get { return _modified; }
			set
			{
				if (_modified == value) return;
				_modified = value;

				OnPropertyChanged("Modified");

				if (!_modified)
					foreach (var section in Sections)
						section.Modified = false;
			}
		}

		public string StatusMessage
		{
			get { return _statusMessage; }
			set
			{
				_statusMessage = value;
				OnPropertyChanged("StatusMessage");
			}
		}

		public bool StatusMessageShown
		{
			get { return _statusMessageShown; }
			set
			{
				_statusMessageShown = value;
				OnPropertyChanged("StatusMessageShown");
			}
		}
		#endregion

		#region Validation

		/// <summary>
		/// Произвести валидацию и выяснить новое состояние перевода в целом.
		/// </summary>
		public void Validate()
		{
			foreach (var section in Sections)
				section.Validate();

			// Общее состояние перевода пока не используется.
		}

		/// <summary>
		/// Переключение проверки акселераторов, т.е. совпадает ли количество амперсандов в исходном и переведённом текстах. 
		/// Если выявлено несовпадение, будет показано сообщение в окне предупреждений.
		/// </summary>
		public bool ValidateAccelerators
		{
			get { return Translation.ValidateAccelerators; }
			set
			{
				if (Translation.ValidateAccelerators == value) return;
				Translation.ValidateAccelerators = value;

				OnPropertyChanged("ValidateAccelerators");

				Validate();
			}
		}

		/// <summary>
		/// Переключение проверки знаков препинания в конце текста. 
		/// Если выявлено несовпадение, будет показано сообщение в окне предупреждений.
		/// </summary>
		public bool ValidateEndingPunctuation
		{
			get { return Translation.ValidateEndingPunctuation; }
			set
			{
				if (Translation.ValidateEndingPunctuation == value) return;
				Translation.ValidateEndingPunctuation = value;

				OnPropertyChanged("ValidateEndingPunctuation");

				Validate();
			}
		}

		/// <summary>
		/// Переключение проверки финальных переводов строк.
		/// Если выявлено несовпадение, будет показано сообщение в окне предупреждений.
		/// </summary>
		public bool ValidateEndingNewline
		{
			get { return Translation.ValidateEndingNewline; }
			set
			{
				if (Translation.ValidateEndingNewline == value) return;
				Translation.ValidateEndingNewline = value;

				OnPropertyChanged("ValidateEndingNewline");

				Validate();
			}
		}

		/// <summary>
		/// Переключение проверки маркеров форматирования, т.е. все ли маркеры исходного текста 
		/// присутствуют в переведённом. (%1, %2, ..., %d, %s, ..., {0}, {1} ...)
		/// Если выявлено несовпадение, будет показано сообщение в окне предупреждений.
		/// </summary>
		public bool ValidatePlaceMarkers
		{
			get { return Translation.ValidatePlaceMarkers; }
			set
			{
				if (Translation.ValidatePlaceMarkers == value) return;
				Translation.ValidatePlaceMarkers = value;

				OnPropertyChanged("ValidatePlaceMarkers");

				Validate();
			}
		}

		#endregion

		#region Navigation

		private void NavigatePrevExecuted(object obj)
		{
			if (SelectedSection == null)
			{
				SelectLast();
				if (SelectedSection != null)
					SelectedSection.SelectedEntry = null;
			}
			while ((SelectedSection == null || !SelectedSection.SelectPrev()) && SelectPrev()) { }
		}

		private void NavigateNextExecuted(object obj)
		{
			if (SelectedSection == null)
			{
				SelectFirst();
				if (SelectedSection != null)
					SelectedSection.SelectedEntry = null;
			}
			while ((SelectedSection == null || !SelectedSection.SelectNext()) && SelectNext()) { }
		}

		private void NavigatePrevUnfinishedExecuted(object obj)
		{
			if (SelectedSection == null)
			{
				SelectLast();
				if (SelectedSection != null)
					SelectedSection.SelectedEntry = null;
			}
			while ((SelectedSection == null || !SelectedSection.SelectPrevUnfinished()) && SelectPrev()) { }
		}

		private void NavigateNextUnfinishedExecuted(object obj)
		{
			if (SelectedSection == null)
			{
				SelectFirst();
				if (SelectedSection != null)
					SelectedSection.SelectedEntry = null;
			}
			while ((SelectedSection == null || !SelectedSection.SelectNextUnfinished()) && SelectNext()) { }
		}

		public bool SelectFirst()
		{
			SelectedSection = Sections.FirstOrDefault();
			return SelectedSection != null;
		}

		public bool SelectLast()
		{
			SelectedSection = Sections.LastOrDefault();
			return SelectedSection != null;
		}

		public bool SelectPrev()
		{
			if (SelectedSection == null)
			{
				SelectedSection = Sections.LastOrDefault();
				return SelectedSection != null;
			}

			var index = Sections.IndexOf(SelectedSection);
			if (index == 0) return false;

			SelectedSection = Sections[index - 1];
			return true;
		}

		/// <summary>
		/// Выделяет следующую секцию.
		/// Возвращает false, если текущая секция была последней и выделять больше нечего.
		/// </summary>
		public bool SelectNext()
		{
			if (SelectedSection == null)
			{
				SelectedSection = Sections.FirstOrDefault();
				return SelectedSection != null;
			}

			var index = Sections.IndexOf(SelectedSection);
			if (index == Sections.Count - 1) return false;

			SelectedSection = Sections[index + 1];
			return true;
		}

		#endregion

		#region Command Handlers

		private void SaveExecuted(object obj)
		{
			if (string.IsNullOrEmpty(_translation.TranslationFileName))
			{
				SaveAs.Execute(null);
			}
			else
			{
				_translation.Save(_translation.TranslationFileName);
				Modified = false;
			}
		}

		private void SaveAsExecuted(object obj)
		{
			using (var dlg = new SaveFileDialog())
			{
				dlg.Title = @"Select a Translation File to Save to";
				dlg.Filter = @"DKLang translation files (*.lng)|*.lng|All files (*.*)|*.*";
				if (dlg.ShowDialog() != DialogResult.OK) return;

				_translation.Save(dlg.FileName);

				OnPropertyChanged("TranslationFileName");
				Modified = false;

				Preferences.Instance.AddMruItem(
					_translation.LangSourceFileName, _translation.TranslationFileName);
			}
		}

		private void ShowPropertiesExecuted(object obj)
		{
			var dlg = new TranslationPropsDialog { Owner = _presenter, DataContext = _translation.Properties.Clone() };
			if (dlg.ShowDialog() == true)
			{
				var newProperties = dlg.DataContext as TranslationProperties;
				if (!_translation.Properties.Equals(newProperties))
				{
					_translation.Properties = newProperties;
					Modified = true;
				}
			}
		}

		private void MakePhraseBookExecuted(object obj)
		{
			var count = _translation.AppendToPhraseBook();
			PhraseBook.Save(Preferences.Instance.PhraseBookPath);
			if (count == 0)
				MessageWindow.Cancel(@"No translations were added. Phrase Book already contains all the translations", _presenter);
			else
				MessageWindow.Ok(@"Phrase Book enriched. Phrases added: " + count, _presenter);
		}

		private void ShowPhraseBookExecuted(object obj)
		{
			var presenter = new PhraseBookWindow { Owner = _presenter };
			var viewModel = new PhraseBookViewModel(PhraseBook.Instance, presenter);
			presenter.DataContext = viewModel;
			presenter.ShowDialog();
		}

		private void GrowPhraseBookManualExecuted(object obj)
		{
			var result = GrowPhraseBook();
			if (result.HasValue)
			{
				if (!result.Value)
					MessageWindow.Cancel(@"Phrase Book already contains this translation", _presenter);
				else
					MessageWindow.Ok(@"Phrase Book enriched", _presenter);
			}
		}

		private void GrowPhraseBookAutoExecuted(object obj)
		{
			var result = GrowPhraseBook();
			if (result.HasValue && result.Value)
				ShowStatusMessage(@"New translation was added to the Phrase Book");
		}

		private void FindExecuted(object obj)
		{
			SearchWindow.Open(_presenter, new SearchParameters(), DoSearch);
		}

		private void ApplyTranslationExecuted(object obj)
		{
			if (SelectedSection != null && SelectedSection.SelectedEntry != null)
			{
				var entry = SelectedSection.SelectedEntry;

				entry.Apply();

				if (Preferences.Instance.AutoGrowPhraseBook)
					PhraseBook.Instance.AddTranslation(entry.Source, entry.Target, _translation.Properties.TargetLang);

				Modified = true;
			}
		}

		private void ApplyGuessExecuted(object obj)
		{
			if (SelectedSection != null && SelectedSection.SelectedEntry != null)
				SelectedSection.SelectedEntry.Target = obj.ToString();
		}

		private void CopyEntryNameExecuted(object obj)
		{
			if (SelectedSection != null && SelectedSection.SelectedEntry != null)
				System.Windows.Clipboard.SetText(SelectedSection.SelectedEntry.Name);
		}

		private void CopyEntrySourceExecuted(object obj)
		{
			if (SelectedSection != null && SelectedSection.SelectedEntry != null)
				System.Windows.Clipboard.SetText(SelectedSection.SelectedEntry.Source);
		}

		private void CopyEntryTargetExecuted(object obj)
		{
			if (SelectedSection != null && SelectedSection.SelectedEntry != null)
				System.Windows.Clipboard.SetText(SelectedSection.SelectedEntry.Target);
		}

		#endregion

		#region Command Helpers

		public bool Close()
		{
			bool haveAutoTranslations =
				_translation.Sections.Any(section => section.Entries.Any(entry => entry.Status == Translation.Status.Unsaved));

			if (!Modified)
			{
				if (haveAutoTranslations)
				{
					switch (MessageBox.Show(@"Translation has auto-translated but not verified items. " +
						"Do you wich to automatically apply them?", Preferences.AppTitle,
						MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
					{
						case DialogResult.Yes:
							Save.Execute(null);
							return true;

						case DialogResult.No:
							return true;

						default:
							return false;
					}
				}
				return true;
			}

			switch (MessageBox.Show(@"Translation has been modified. Save changes?", Preferences.AppTitle,
				MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
			{
				case DialogResult.Yes:
					if (haveAutoTranslations)
					{
						switch (MessageBox.Show(@"Translation has auto-translated but not verified items. Save anyway?", Preferences.AppTitle,
							MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1))
						{
							case DialogResult.No:
								return true;

							case DialogResult.Cancel:
								return false;
						}
					}
					Save.Execute(null);
					return true;

				case DialogResult.No:
					return true;

				default:
					return false;
			}
		}

		public bool DoSearch(SearchParameters @params)
		{
			return true;
		}

		private bool? GrowPhraseBook()
		{
			if (SelectedSection == null || SelectedSection.SelectedEntry == null) return null;
			{
				var phrase = SelectedSection.SelectedEntry.Source;
				var translation = SelectedSection.SelectedEntry.Target;
				if (string.IsNullOrEmpty(phrase) || string.IsNullOrEmpty(translation)) return null;
				return PhraseBook.Instance.AddTranslation(phrase, translation, _translation.Properties.TargetLang);
			}
		}

		private void ShowStatusMessage(string message)
		{
			StatusMessage = message;
			StatusMessageShown = true;

			_statusMessageTimer = new System.Threading.Timer(HideStatusMessage, null, 3000, Timeout.Infinite);
		}

		private void HideStatusMessage(object param)
		{
			_presenter.Dispatcher.Invoke(new Action(HideStatusMessage));
		}

		private void HideStatusMessage()
		{
			StatusMessageShown = false;
		}

		#endregion
	}
}
