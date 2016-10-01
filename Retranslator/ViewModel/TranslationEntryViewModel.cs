using System.Collections.Generic;
using System.Linq;
using Retranslator.Model;
using Retranslator.Utils;

namespace Retranslator.ViewModel
{
    public class TranslationEntryViewModel : ViewModelBase
    {
        private readonly TranslationEntry _entry;
        private readonly TranslationSectionViewModel _parent;
        private bool _modified;

        public TranslationEntryViewModel(TranslationSectionViewModel parent, TranslationEntry entry)
        {
            _parent = parent;
            _entry = entry;
        }

        #region Model Properties

        /// <summary>
        /// Идентификатор переводимого элемента.
        /// </summary>
        public string Id { get { return _entry.Id; } }

        /// <summary>
        /// Название переводимого элемента.
        /// </summary>
        public string Name { get { return _entry.Name; } }

        /// <summary>
        /// Исходное значение элемента.
        /// </summary>
        public string Source { get { return _entry.Source; } }

        /// <summary>
        /// Перевод элемента.
        /// </summary>
        public string Target
        {
            get { return _entry.Target; }
            set
            {
                if (_entry.Target == value) return;

                var oldStatus = _entry.Status;
            	var oldWarnings = _entry.Warnings;

                _entry.Target = value;

            	if (_entry.Status != oldStatus)
            		OnStatusChanged();
            	if (_entry.Warnings.DifferFrom(oldWarnings))
					OnPropertyChanged("Warnings");

				OnPropertyChanged("Target");
                Modified = true;
            }
        }

        /// <summary>
        /// Состояние перевода элемента.
        /// </summary>
        public Translation.Status Status { get { return _entry.Status; } }

        /// <summary>
        /// Предупреждения о несогласованном переводе.
        /// </summary>
        public IList<TranslationEntry.Warning> Warnings { get { return _entry.Warnings; } }

        #endregion

        #region View Model Properties

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
            }
        }

        #endregion

        /// <summary>
        /// Произвести валидацию и выяснить новое состояни перевода элемента.
        /// </summary>
        public void Validate()
        {
            var oldStatus = Status;
        	var oldWarnings = Warnings;

        	_entry.Validate();
            _entry.UpdateStatus();

        	if (oldStatus != Status)
        		OnStatusChanged();
        	if (oldWarnings.DifferFrom(Warnings))
				OnPropertyChanged("Warnings");
        }

		public void Apply()
		{
			_entry.Apply();
			OnStatusChanged();
		}

		private void OnStatusChanged()
		{
			OnPropertyChanged("Status");
			_parent.OnStatusChanged();
		}
    }
}
