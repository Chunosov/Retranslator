using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Retranslator.ViewModel;

namespace Retranslator.Model
{
    /// <summary>
    /// Настройки приложения.
    /// </summary>
    public class Preferences : ViewModelBase, ICloneable
    {
    	public const string AppTitle = "Retranslator";

        public class MruItem
        {
            public string LangSourceFile { get; set; }
            public string TranslationFile { get; set; }

            public MruItem()
            {
            }

            public MruItem(string langSourceFile, string translationFile)
            {
                LangSourceFile = langSourceFile;
                TranslationFile = translationFile;
            }
        }

        public static Preferences Instance { get; set; }

        static Preferences()
        {
            Instance = new Preferences();
        }

        private Preferences()
        {
            Mru = new List<MruItem>();
            //Mru.Add(new MruItem { LangSourceFile = "File1.dklang", TranslationFile = "File1.lng" });
            //Mru.Add(new MruItem { LangSourceFile = "File2.dklang", TranslationFile = "File2.lng" });
            //Mru.Add(new MruItem { LangSourceFile = "File3.dklang", TranslationFile = "File3.lng" });
            //Mru.Add(new MruItem { LangSourceFile = "File4.dklang", TranslationFile = "File4.lng" });

            ShowOpeningReport = true;
            UsePhraseBook = true;
        }

        public List<MruItem> Mru { get; set; }

        public bool IsMruEmpty
        {
            get { return Mru.Count > 0; }
        }

        public bool ShowOpeningReport { get; set; }

        public bool UsePhraseBook { get; set; }

		public bool AutoTranslate { get; set; }

		public bool AutoGrowPhraseBook { get; set; }

		public bool ValidateMarkersCpp { get; set; }
		public bool ValidateMarkersCSharp { get; set; }
		public bool ValidateMarkersQt { get; set; }

        [XmlIgnore]
        public static bool IsPortable { get; private set; }

        private const string PhraseBookFileName = "PhraseBook.xml";

        public string PhraseBookPath
        {
            get
            {
                var path = IsPortable ? Utils.Utils.GetLocalPath() : Utils.Utils.GetSpecialPath(Environment.SpecialFolder.MyDocuments, "Retranslator");
                return path + PhraseBookFileName;
            }
        }

        #region Load\Save

        /// <summary>
        /// Имя файла, в котором хранятся настройки.
        /// </summary>
        private const string ConfigFileName = "Retranslator.xml";


        /// <summary>
        /// Полный путь к файлу, в котором хранятся настройки.
        /// </summary>
        static private string ConfigFilePath
        {
            get
            {
                // Если есть конфиг рядом с екзешником, то берем его. Для портабельности.
                var path = Utils.Utils.GetLocalPath() + ConfigFileName; 
                if (File.Exists(path))
                {
                    IsPortable = true;
                    return path; 
                }

                return Utils.Utils.GetSpecialPath(Environment.SpecialFolder.CommonApplicationData, "Retranslator") + ConfigFileName;
            }
        }

        public static void Save()
        {
            using (var file = new FileStream(ConfigFilePath, FileMode.Create))
            {
                new XmlSerializer(Instance.GetType()).Serialize(file, Instance);
                file.Close();
            }
        }

        public static void Load()
        {
            var fileName = ConfigFilePath;
            if (!File.Exists(fileName)) return;

            var backup = Instance;
            try
            {
                using (var file = new FileStream(fileName, FileMode.Open))
                {
                    Instance = new XmlSerializer(Instance.GetType()).Deserialize(file) as Preferences;
                    file.Close();
                }
            }
            catch (Exception)
            {
                // Ошибки из-за неверного формата файла игнорируем.
                // Просто возвращаем дефолтовые настройки.
                Instance = backup;
            }
        }

        #endregion

        public void AddMruItem(string langSourceFile, string translationFile)
        {
            Mru.Remove(Mru.FirstOrDefault(item =>
                string.Compare(item.LangSourceFile, langSourceFile, StringComparison.OrdinalIgnoreCase) == 0 &&
				string.Compare(item.TranslationFile, translationFile, StringComparison.OrdinalIgnoreCase) == 0));
            Mru.Insert(0, new MruItem(langSourceFile, translationFile));

            // Чтобы MenuRecent осознал, что это уже другой список
            Mru = new List<MruItem>(Mru);

            OnPropertyChanged("Mru");
            OnPropertyChanged("IsMruEmpty");
        }

        #region ICloneable

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
