using System;
using System.Collections.Generic;
using System.Linq;

namespace Retranslator.Model
{
    public class OpeningParameters
    {
        public string LangSourceFile { get; set; }
        public string TranslationFile { get; set; }
        
        public List<string> LangSourcesMru { get; private set; }
        public List<string> TranslationsMru { get; private set; }

        public bool CreateNewTranslation { get; set; }

        public OpeningParameters()
        {
            CreateNewTranslation = true;
            // Distinct() нужен потому, что один и тот же DKLANG-файл может несколько раз встречаться в MRU-списке в паре 
            // с разными LNG-файлами. Дублирование LNG-файлов - ситуация трудно представимая, но на всякий случай тоже Distinct.
            LangSourcesMru = Preferences.Instance.Mru.Select(item => item.LangSourceFile).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            TranslationsMru = Preferences.Instance.Mru.Select(item => item.TranslationFile).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
