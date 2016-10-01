using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Retranslator.Model
{
    /// <summary>
    /// Словарь для перевода. Содержит по нескольку переводов одной фразы.
    /// </summary>
    public class PhraseBook : IXmlSerializable
    {
        public static PhraseBook Instance { get; set; }

        public SortedDictionary<string, List<string >> Phrases { get; set; }

        static PhraseBook()
        {
            Instance = new PhraseBook();
        }

        private PhraseBook()
        {
            Phrases = new SortedDictionary<string, List<string>>();
        }

        public bool AddTranslation(string source, string translation, int langCode)
        {
            if (string.IsNullOrEmpty(source)) return false;
            if (string.IsNullOrEmpty(translation)) return false;

            if (!Phrases.ContainsKey(source))
                Phrases.Add(source, new List<string>());

            var translations = Phrases[source];
            if (!translations.Any(tr => tr == translation))
            {
                translations.Add(translation);
                Modified = true;
            	return true;
            }

        	return false;
        }

        public bool Modified { get; private set; }
        public string FileName { get; private set; }

        static public void Load(string fileName)
        {
            if (!File.Exists(fileName)) return;

            var backup = Instance;
            try
            {
                using (var file = new FileStream(fileName, FileMode.Open))
                {
                    Instance = new XmlSerializer(Instance.GetType()).Deserialize(file) as PhraseBook;
                    file.Close();
                }
                
                if (Instance == null)
                    throw new Exception("Unable to load a Phrase Book from file for unknown reasons.");

                Instance.FileName = fileName;
            }
            catch (Exception)
            {
                Instance = backup;
                throw;
            }
        }

        static public void Save(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Create))
            {
                new XmlSerializer(Instance.GetType()).Serialize(file, Instance);
                file.Close();
            }
            Instance.FileName = fileName;
        }

        #region Implementation of IXmlSerializable

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                reader.ReadStartElement("Phrase");

                var phrase = reader.ReadElementString("Text");
                var translations = new List<string>();

                reader.ReadStartElement("Translations");
                while (reader.NodeType != XmlNodeType.EndElement)
                    translations.Add(reader.ReadElementString("Translation"));
                reader.ReadEndElement();

                reader.ReadEndElement();
                reader.MoveToContent();

                Phrases.Add(phrase, translations);
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var phrase in Phrases)
            {
                writer.WriteStartElement("Phrase");

                writer.WriteElementString("Text", phrase.Key);
                
                writer.WriteStartElement("Translations");
                foreach (var translation in phrase.Value)
                    writer.WriteElementString("Translation", translation);
                writer.WriteEndElement();
                
                writer.WriteEndElement();
            }
        }

        #endregion

        public IList<string> Guess(string phrase)
        {
        	var result = new List<string>();

			foreach (var storedPhrase in Phrases)
				if (string.Equals(storedPhrase.Key, phrase, StringComparison.OrdinalIgnoreCase))
					foreach (var foundPhrase in storedPhrase.Value)
						if (result.All(res => res != foundPhrase))
							result.Add(foundPhrase);
            return result;
        }
    }
}
