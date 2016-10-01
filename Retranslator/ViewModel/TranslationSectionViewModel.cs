using System.Collections.Generic;
using System.Linq;
using Retranslator.Model;

namespace Retranslator.ViewModel
{
    public class TranslationSectionViewModel : ViewModelBase
    {
        private readonly TranslationSection _section;
        private readonly TranslationViewModel _parent;
        private TranslationEntryViewModel _selectedEntry;
        private string _countInfo = null;
        private bool _modified;

        public TranslationSectionViewModel(TranslationViewModel parent, TranslationSection section)
        {
            _section = section;
            _parent = parent;

            Entries = section.Entries.Select(entry => new TranslationEntryViewModel(this, entry)).ToList();
        }

        #region Model Properties

        /// <summary>
        /// Название секции.
        /// </summary>
        public string Name { get { return _section.Name; } }

        public Translation.Status Status { get { return _section.Status; } }

        #endregion

        #region View Model Properties

        /// <summary>
        /// Элементы секции.
        /// </summary>
        public IList<TranslationEntryViewModel> Entries { get; private set; }

        public IList<string> TranslationGuesses { get; private set; }

        /// <summary>
        /// Выделенный элемент секции.
        /// </summary>
        public TranslationEntryViewModel SelectedEntry
        {
            get { return _selectedEntry; }
            set
            {
                if (_selectedEntry == value) return;

                _selectedEntry = value;
                OnPropertyChanged("SelectedEntry");

				if (_selectedEntry != null)
				{
					TranslationGuesses = Preferences.Instance.UsePhraseBook ? PhraseBook.Instance.Guess(_selectedEntry.Source) : null;
					OnPropertyChanged("TranslationGuesses");
				}
            }
        }

        /// <summary>
        /// Отображаемая информация об общем количестве 
        /// и количестве корректно переведенных элементов в секции.
        /// </summary>
        public string CountInfo
        {
            get
            {
                if (string.IsNullOrEmpty(_countInfo))
                    _countInfo = string.Format("{0}/{1}", _section.TranslatedCount, _section.Entries.Count);
                return _countInfo;
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

                if (_modified)
                    _parent.Modified = true;
                else
                    foreach (var entry in Entries)
                        entry.Modified = false;
            }
        }
        #endregion

        public void OnStatusChanged()
        {
            _countInfo = null;

            OnPropertyChanged("Status");
            OnPropertyChanged("CountInfo");
        }

        /// <summary>
        /// Произвести валидацию и выяснить новое состояни перевода секции.
        /// </summary>
        public void Validate()
        {
            var oldStatus = Status;

            foreach (var entry in Entries)
                entry.Validate();

            if (oldStatus != Status)
                OnStatusChanged();
        }

        #region Navigation

        public bool SelectPrev()
        {
            if (SelectedEntry == null)
            {
                SelectedEntry = Entries.LastOrDefault();
                return SelectedEntry != null;
            }
            
            var index = Entries.IndexOf(SelectedEntry);
            if (index == 0) return false;

            SelectedEntry = Entries[index - 1];
            return true;
        }

        public bool SelectNext()
        {
            if (SelectedEntry == null)
            {
                SelectedEntry = Entries.FirstOrDefault();
                return SelectedEntry != null;
            }

            var index = Entries.IndexOf(SelectedEntry);
            if (index == Entries.Count - 1) return false;

            SelectedEntry = Entries[index + 1];
            return true;
        }

        public bool SelectPrevUnfinished()
        {
            var selectedIndex = (SelectedEntry == null) ? Entries.Count : Entries.IndexOf(SelectedEntry);

            for (var i = selectedIndex-1; i >= 0 ; i--)
                if (Entries[i].Status != Translation.Status.Ok)
                {
                    SelectedEntry = Entries[i];
                    return true;
                }
            return false;
        }

        public bool SelectNextUnfinished()
        {
            var selectedIndex = (SelectedEntry == null) ? -1 : Entries.IndexOf(SelectedEntry);

            for (var i = selectedIndex+1; i < Entries.Count; i++)
                if (Entries[i].Status != Translation.Status.Ok)
                {
                    SelectedEntry = Entries[i];
                    return true;
                }
            return false;
        }

        #endregion
    }
}
