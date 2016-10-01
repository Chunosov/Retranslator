using System;
using System.Collections.Generic;

namespace Retranslator.Model
{
    /// <summary>
    /// Единичный переводимый элемент.
    /// </summary>
    public class TranslationEntry
    {
        #region Warnings

        /// <summary>
        /// Предупреждения о несогласованном переводе.
        /// </summary>
        public enum Warning
        {
            /// <summary>
            /// Перевод не заканчивается тем же знаком препинания, что и исходный текст.
            /// </summary>
            EndingPunctuation,

            /// <summary>
            /// Возможно, пропущен акселератор в переводе.
            /// </summary>
            AcceleratorMissed,

            /// <summary>
            /// Возмжно, лишний акселератор в переводе.
            /// </summary>
            AcceleratorExtra,

            /// <summary>
            /// Перевод не содержит тех же маркеров форматирования, что и исходный текст.
            /// </summary>
            FormatMarker,

			/// <summary>
			/// Лишние пустые строки в конце текста перевода.
			/// </summary>
			EndingNewlineSource,

			/// <summary>
			/// Недостающие пустые строки в конце текста паревода.
			/// </summary>
			EndingNewlineTranslation,
        }

        #endregion

        private readonly TranslationSection _parent;

        public TranslationEntry(TranslationSection parent)
        {
            Source = string.Empty;
            _target = string.Empty;
            Status = Translation.Status.None;
            _parent = parent;
        }

        private string _target;

        #region Properties

        /// <summary>
        /// Идентификатор переводимого элемента, который дается ему DKLangManager'ом.
        /// Для констант совпадает с именем Name.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Название переводимого элемента. Соответствует имени компонента 
        /// на форме (например ToolbarMenu.Caption) или имени константы.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Исходное значение элемента, взятое из DKLANG-файла.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Перевод элемента, сохраняемый в LNG-файл.
        /// Свойство должно устанавливаться только при ручном переврде.
        /// При автопереводе использовать Autotranslate(), 
        /// при загрузке из файла использовать Load().
        /// </summary>
        public string Target
        {
            get { return _target; }
            set
            {
                _target = value;
				Status = string.IsNullOrEmpty(_target)? Translation.Status.None: Translation.Status.Unsaved;
				Validate();
                UpdateStatus();
            }
        }

        /// <summary>
        /// Состояние перевода элемента.
        /// </summary>
        public Translation.Status Status { get; private set; }

        /// <summary>
        /// Предупрежедения о несогласованном переводе.
        /// </summary>
        public IList<Warning> Warnings { get; private set; }

        #endregion

		public void Load(string value)
		{
			Status = Translation.Status.Ok;
			_target = value;
		}

		public void Apply()
		{
			Status = Translation.Status.Ok;
			UpdateStatus();
		}

		public void Autotranslate()
		{
			var guesses = PhraseBook.Instance.Guess(Source);
			if (guesses != null && guesses.Count > 0)
			{
				_target = guesses[0];
				Status = Translation.Status.Unsaved;
				Validate();
				UpdateStatus();
			}
		}

		/// <summary>
		/// Обновить родительский статус в соответствии с собственным.
		/// </summary>
		public void UpdateStatus()
		{
			if (_parent != null)
				_parent.StatusUpdated();
		}

		#region Validation

        public void Validate()
        {
			Warnings = null;
			var oldStatus = Status;
            var ok = true;
            if (Translation.ValidateEndingPunctuation && !ValidateEndingPunctuation())
            {
				AddValidationWarning(Warning.EndingPunctuation);
                ok = false;
            }
			if (Translation.ValidateEndingNewline && !ValidateEndingNewLine())
			{
				ok = false;
			}

        	if (ok)
        	{
				if (oldStatus == Translation.Status.Warnings)
					Status = Translation.Status.Unsaved;
        	}
			else
        	{
				Status = Translation.Status.Warnings;
            }
        }

		private void AddValidationWarning(Warning warning)
		{
			if (Warnings == null)
				Warnings = new List<Warning>();
			Warnings.Add(warning);
		}

    	/// <summary>
        /// Проверяет, заканчивается ли перевод тем же знаком препинания, что и исходный текст.
        /// </summary>
        private bool ValidateEndingPunctuation()
        {
			var source = string.IsNullOrEmpty(Source) ? string.Empty : TrimEnd(Source);
			var target = string.IsNullOrEmpty(Target) ? string.Empty : TrimEnd(Target);

            var sourceCh = string.IsNullOrEmpty(source) ? ' ' : source[source.Length - 1];
            var targetCh = string.IsNullOrEmpty(target) ? ' ' : target[target.Length - 1];

            var isSourcePunct = IsPunctuation(sourceCh);
            var isTargetPunct = IsPunctuation(targetCh);

            if (isSourcePunct && isTargetPunct)
            {
                if (source.EndsWith("..."))
                    return target.EndsWith("...");

                return sourceCh == targetCh;
            }
            return isSourcePunct == isTargetPunct;
        }

		private bool ValidateEndingNewLine()
		{
			if (!IsMatchEndingNewLine(Source, Target))
			{
				AddValidationWarning(Warning.EndingNewlineTranslation);
				return false;
			}
			if (!IsMatchEndingNewLine(Target, Source))
			{
				AddValidationWarning(Warning.EndingNewlineSource);
				return false;
			}
			return true;
		}

		private static bool IsMatchEndingNewLine(string s1, string s2)
		{
			var str1 = s1;
			var str2 = s2;
			while (str1.EndsWith("\\n"))
			{
				if (!str2.EndsWith("\\n")) return false;
				str1 = str1.Substring(0, str1.Length - 2);
				str2 = str2.Substring(0, str2.Length - 2);
			}
			return true;
		}

		private static string TrimEnd(string s)
		{
			var result = s.Trim();
			while (result.EndsWith("\\n"))
				result = result.Substring(0, result.Length - 2);
			return result;
		}

    	/// <summary>
        /// Является ли символом знаком пунктуации.
        /// Обрабатываем меньше значений, чем в стандартном методе char.IsPunctuation()
        /// т.к. тот считает пунктуацией и скобки, и решетку, и т.п., а это нам не надо.
        /// </summary>
        private static bool IsPunctuation(char ch)
        {
            return (ch == '.') || (ch == ',') || (ch == ';') || (ch == ':') || (ch == '!') || (ch == '?');
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0}={1}", Id, Target);
        }
    }
}
