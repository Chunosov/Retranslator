using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Retranslator.Utils;

namespace Retranslator.Model
{
    public class Translation
    {
        #region Translation.Status

        /// <summary>
        /// Состояния перевода элемента.
        /// </summary>
        public enum Status
        {
            /// <summary>
            /// Элемент не переведен.
            /// </summary>
            None,

            /// <summary>
            /// Элемент переведен.
            /// </summary>
            Ok,

            /// <summary>
            /// Элемент переведен, но есть несоответствия с исходником.
            /// </summary>
            Warnings,

			/// <summary>
			/// Элемент переведен автоматически
			/// </summary>
			Unsaved,
        }

        #endregion

		#region Constructors

		static Translation()
        {
            ValidateAccelerators = false;
            ValidateEndingPunctuation = true;
            ValidateEndingNewline = true;
            ValidatePlaceMarkers = false;
        }

        public Translation(string langSourceFile, string translationFile)
        {
        	Sections = new List<TranslationSection>();
        	LangSourceFileName = langSourceFile;
            TranslationFileName = translationFile;

            Properties = new TranslationProperties();

            LoadLangSource(langSourceFile);
        }

		#endregion

		#region Privates

		#endregion

		#region Loading

		/// <summary>
        /// Сопоставление исходного DKLANG-файла с файлом перевода (.LNG)
        /// и формирование отчета о добавленных и удаленных элементах в переводе.
        /// </summary>
        public IList<OpeningReportItem> Load(bool autoTranslate)
        {
            var report = new List<OpeningReportItem>();
            var langFile = new IniFile(TranslationFileName);

            if (langFile.HasSection(string.Empty))
            {
                Properties.Author = langFile.GetEntry(string.Empty, "Author");
                Properties.TargetApp = langFile.GetEntry(string.Empty, "TargetApplication");
                Properties.SourceLang = langFile.GetInteger(string.Empty, "SourceLANGID", TranslationProperties.DefaultSourceLang);
                Properties.TargetLang = langFile.GetInteger(string.Empty, "LANGID", TranslationProperties.DefaultTargetLang);
            }

            foreach (var section in Sections)
            {
                section.BeginUpdate();
                try
                {
                    foreach (var entry in section.Entries)
                    {
                        if (!langFile.HasEntry(section.Name, entry.Id))
                        {
                            report.Add(new OpeningReportItem(
								OpeningReportItem.ItemType.NoTranslation, section.Name, entry.Id, entry.Name));
							if (autoTranslate)
								entry.Autotranslate();
                        }
                        else
                        {
                            entry.Load(langFile.GetEntry(section.Name, entry.Id));
                            langFile.RemoveEntry(section.Name, entry.Id);
                        }
                    }
                }
                finally
                {
                    section.EndUpdate();
                }
            }

            // Лишние секции и элементы в файле перевода
			// Секция с пустым именем соответствет секции с настройками LNG-файла
            foreach (var section in langFile.Sections.Where(s => s != string.Empty))
            {
                var sectionName = section;
                if (!Sections.Any(s => s.Name == sectionName))
                {
                    report.Add(new OpeningReportItem(OpeningReportItem.ItemType.ExtraTranslation, section, "(The whole section)", ""));
                    continue;
                }

                report.AddRange(langFile.GetSectionKeys(section).Select(entry => new OpeningReportItem(
					OpeningReportItem.ItemType.ExtraTranslation, sectionName, entry, langFile.GetEntry(sectionName, entry))));
            }

            // Секции сортируем по алфавиту, а элементы секций по их идентификаторам.
            Sections.Sort((s1, s2) => string.Compare(s1.Name, s2.Name, StringComparison.Ordinal));
            foreach(var section in Sections)
                section.Entries.Sort((e1, e2) => string.Compare(e1.Id, e2.Id, StringComparison.Ordinal));

            return report;
        }

        /// <summary>
        /// Загрузка исходного языкового файла.
        /// </summary>
        private void LoadLangSource(string fileName)
        {
            TranslationSection section = null;

            using (var file = new StreamReader(fileName))
            {
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
					if (!string.IsNullOrEmpty(line))
					{
						if (line.StartsWith("["))
						{
							if (section != null)
								Sections.Add(section);

							section = new TranslationSection(this, line);
						}
						else if (section != null)
							section.AddEntry(line);
					}
                }
            }
            if (section != null)
                Sections.Add(section);
        }

        /// <summary>
        /// Сохранение файла перевода.
        /// </summary>
        public void Save(string fileName)
        {
            using (var file = new StreamWriter(fileName, false, Encoding.Unicode))
            {
                file.WriteLine(Properties.ToString());

                foreach (var section in Sections.Where(s => s.Type == TranslationSection.SectionType.Component))
                    file.WriteLine(section.ToString());

                foreach (var section in Sections.Where(s => s.Type == TranslationSection.SectionType.Constant))
                    file.WriteLine(section.ToString());

                file.Close();
            }
            
            TranslationFileName = fileName;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Имя исходного языкового файла (.DKLANG)
        /// </summary>
        public string LangSourceFileName { get; private set; }

        /// <summary>
        /// Имя текущего файла перевода (.LNG)
        /// </summary>
        public string TranslationFileName { get; private set; }

    	/// <summary>
    	/// Список переводимых секций.
    	/// </summary>
    	public List<TranslationSection> Sections { get; private set; }

    	/// <summary>
        /// Свойства перевода.
        /// </summary>
        public TranslationProperties Properties { get; set; }

        #endregion

        #region Validation

        /// <summary>
        /// Переключение проверки акселераторов. 
        /// </summary>
        public static bool ValidateAccelerators { get; set; }

        /// <summary>
        /// Переключение проверки знаков препинания в конце текста. 
        /// </summary>
        public static bool ValidateEndingPunctuation { get; set; }

        /// <summary>
        /// Переключение проверки финальных переводов строк.
        /// </summary>
        public static bool ValidateEndingNewline { get; set; }

        /// <summary>
        /// Переключение проверки маркеров форматирования/
        /// </summary>
        public static bool ValidatePlaceMarkers { get; set; }

        #endregion

        public void UpdateStatus()
        {
			foreach (var section in Sections)
				section.UpdateStatus();
        }

        public void StatusUpdated()
        {
        }
    }
}
