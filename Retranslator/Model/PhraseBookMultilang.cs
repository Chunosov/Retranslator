using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Retranslator.Utils;

namespace Retranslator.Model
{
    /// <summary>
    /// Словарь для перевода. Содержит по нескольку переводов одной фразы.
    /// Может содержать перводы фразы на несколько языков. Недоделано.
    /// </summary>
    public class PhraseBookMultilang : IXmlSerializable
    {
        public class Translations : Dictionary<int, List<string>>
        {
            public void AddTranslation(int langCode, string text)
            {
                if (ContainsKey(langCode))
                {
                    var translations = this[langCode];
                    if (!translations.Any(s => s == text))
                        translations.Add(text);
                }
                else
                {
                    Add(langCode, new List<string> { text });
                }
            }

            public void ReadXml(XmlReader reader)
            {

            }

            public void WriteXml(XmlWriter writer)
            {
                if (Count == 0) return;

                writer.WriteStartElement("Translations");

                foreach (var key in Keys)
                {
                    writer.WriteStartElement("Language");
                    writer.WriteAttributeString("Code", key.ToString());
                    foreach (var translation in this[key])
                        writer.WriteElementString("Translation", translation);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }
        }

        public static PhraseBookMultilang Instance { get; set; }

        static PhraseBookMultilang()
        {
            Instance = new PhraseBookMultilang();
        }

        private PhraseBookMultilang()
        {
            Phrases = new SerializableDictionary<string, Translations>();
        }

        [XmlIgnore]
        public SerializableDictionary<string, Translations> Phrases { get; set; }

        public void AddTranslation(string source, string translation, int langCode)
        {
            if (string.IsNullOrEmpty(source)) return;
            if (string.IsNullOrEmpty(translation)) return;

            if (!Phrases.ContainsKey(source))
                Phrases.Add(source, new Translations());

            Phrases[source].AddTranslation(langCode, translation);
        }

        public void Load(string fileName)
        {
            
        }

        public void Save(string fileName)
        {
            using (var file = new FileStream(fileName, FileMode.Create))
            {
                new XmlSerializer(Instance.GetType()).Serialize(file, Instance);
                file.Close();
            }
        }

        #region Implementation of IXmlSerializable

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
        }

        public void WriteXml(XmlWriter writer)
        {
            foreach (var phrase in Phrases)
            {
                writer.WriteStartElement("Phrase");
                writer.WriteElementString("Text", phrase.Key);
                phrase.Value.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        #endregion
    }
}
