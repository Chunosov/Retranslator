using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Retranslator.Model;
using Retranslator.View;

namespace Retranslator.ViewModel
{
    public class PhraseBookViewModel : ViewModelBase
    {
		public class PhraseViewModel
		{
			public string Phrase { get; set; }
			public int TranslationsCount { get; set; }
		}

		public class PhraseTranslationViewModel
		{
			public string Phrase { get; set; }
			public string Translation { get; set; }
		}

        private readonly Window _presenter;

		private PhraseViewModel _selectedPhrase;

        public DelegateCommand Find { get; set; }
		public DelegateCommand EditTranslation { get; set; }

        public PhraseBookViewModel(PhraseBook phraseBook, Window presenter)
        {
            Find = new DelegateCommand(FindExecuted);
			EditTranslation = new DelegateCommand(EditTranslationExecuted);

            PhraseBook = phraseBook;
            _presenter = presenter;

        	TotalTranslations = 0;
			Phrases = new List<PhraseViewModel>();
			foreach (var phrase in PhraseBook.Phrases)
			{
				var phraseModel = new PhraseViewModel();
				phraseModel.Phrase = phrase.Key;
				phraseModel.TranslationsCount = phrase.Value.Count;
				TotalTranslations += phraseModel.TranslationsCount;
				Phrases.Add(phraseModel);
			}
        }

    	private void EditTranslationExecuted(object obj)
    	{
			if (SelectedPhrase == null || SelectedTranslation == null) return;

    		var translationModel = new PhraseTranslationViewModel();
    		translationModel.Phrase = SelectedPhrase.Phrase;
    		translationModel.Translation = SelectedTranslation;

			var presenter = new TranslationEditorWindow { Owner = _presenter };
    		presenter.DataContext = translationModel;
    		var result = presenter.ShowDialog();
			if (result.HasValue && result.Value)
			{
			}
    	}

    	#region View Model Properties

        public PhraseBook PhraseBook { get; private set; }

		/// <summary>
		/// Все фразы, содержащиеся в книге
		/// </summary>
		public List<PhraseViewModel> Phrases { get; private set; }

		/// <summary>
		/// Переводы веделенной фразы
		/// </summary>
        public List<string> Translations { get; private set; }

        public int SelectedIndex { get; set; }

		public PhraseViewModel SelectedPhrase
        {
            get { return _selectedPhrase; }
            set
            {
                if (_selectedPhrase == value) return;
                _selectedPhrase = value;

                Translations = PhraseBook.Phrases.ContainsKey(value.Phrase) ? PhraseBook.Phrases[value.Phrase] : null;

            	int a = Phrases.Count();

                OnPropertyChanged("Translations");
            }
			
        }

		public string SelectedTranslation { get; set; }

        public int TotalTranslations { get; private set; }

        #endregion

        private void FindExecuted(object obj)
        {
            var @params = new SearchParameters {ThroughIdsEnabled = false};
            SearchWindow.Open(_presenter, @params, DoSearch);
        }

        private bool IsMatch(string phrase, string pattern, SearchParameters @params)
        {
          /*  var text = @params.CaseSensetive ? phrase : phrase.ToLower();

            bool result;
            if (@params.WholeWord)
            {
                result = text == pattern;
            }
            else
            {
                result = text.Contains(pattern);
            }
            if (result)
            {
                SelectedPhrase = phrase;
                OnPropertyChanged("SelectedPhrase");
                //((PhraseBookWindow)_presenter).PhraseList.ScrollIntoView(SelectedPhrase);
            }
            return result;*/
        	return false;
        }

        private bool DoSearch(SearchParameters @params)
        {
          /*  var pattern = @params.CaseSensetive ? @params.Pattern : @params.Pattern.ToLower();

            if (@params.Backward)
            {
                for (var i = SelectedIndex - 1; i >= 0; i--)
                    if (IsMatch(Phrases[i], pattern, @params)) return true;
            }
            else
            {
                for (var i = SelectedIndex + 1; i < Phrases.Count; i++)
                    if (IsMatch(Phrases[i], pattern, @params)) return true;
            }*/
            return false;
        }
    }
}
