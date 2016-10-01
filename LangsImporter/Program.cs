using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace LangsImporter
{
    public class Language
    {
        public string Name { get; set; }
        public int Code { get; set; }
    }

    class Program
    {
        static void Main()
        {
            try
            {
                var langs = new List<Language>();

                using (var file = new StreamReader("langs.txt", Encoding.Unicode))
                {
                    while (!file.EndOfStream)
                    {
                        var line = file.ReadLine();
                        if (!string.IsNullOrEmpty(line))
                        {
                            var parts = line.Split('\t');
                            if (parts.Length == 2)
                            {
                                langs.Add(new Language { Name = parts[0], Code = int.Parse(parts[1]) });
                            }
                        }
                    }
                    file.Close();
                }

                ToXaml(langs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void ToXml(List<Language> langs)
        {
            using (var file = new FileStream("langs.xml", FileMode.Create))
            {
                new XmlSerializer(langs.GetType()).Serialize(file, langs);
                file.Close();
            }
        }

        static void ToXaml(IEnumerable<Language> langs)
        {
            using (var file = new StreamWriter("langs.xaml"))
            {
                foreach (var lang in langs)
                {
                    file.WriteLine("<local:Language Title=\"{0}\" Code=\"{1}\"/>", lang.Name, lang.Code);
                }
                file.Close();
            }
        }
    }
}
