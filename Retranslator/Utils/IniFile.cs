using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Retranslator.Utils
{
    /// <summary>
    /// Класс для работы с INI-файлами.
    /// </summary>
    public class IniFile
    {
        private readonly string _fileName;
        private readonly Dictionary<string, Dictionary<string, string>> _sections = 
            new Dictionary<string, Dictionary<string, string>>();

        public IniFile()
        {
        }

        /// <summary>
        /// Загружает INI-файл, если задан путь.
        /// Если файл по указанному пути не найден, то ничего не делает.
        /// </summary>
        public IniFile(string fileName)
        {
            _fileName = fileName;

            if (!string.IsNullOrEmpty(_fileName) && File.Exists(_fileName))
                Load();
        }

        /// <summary>
        /// Загружает соержимое INI-файла в память.
        /// </summary>
        private void Load()
        {
            Dictionary<string, string> section = null;
            using (var file = new StreamReader(_fileName))
            {
                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();
                    if (string.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("["))
                    {
                        var sectionName = line.Substring(1, line.Length - 2);
                        if (_sections.ContainsKey(sectionName)) continue;
                        section = new Dictionary<string, string>();
                        _sections.Add(sectionName, section);
                    }
                    else
                    {
                        var index = line.IndexOf('=');
                        if (index == -1) continue;
                        var name = line.Substring(0, index).Trim();

                        if (section == null)
                        {
                            section = new Dictionary<string, string>();
                            _sections.Add(string.Empty, section);
                        }

                        if (section.ContainsKey(name)) continue;
                        section.Add(name, line.Substring(index+1));
                    }
                }
            }
        }

        /// <summary>
        /// Возвращает значение заданного ключа в указанной секции.
        /// Если такого ключа в секции нет, то возвращает пустую строку.
        /// </summary>
        public string GetEntry(string section, string name)
        {
            if (_sections.ContainsKey(section))
            {
                var entries = _sections[section];
                if (entries.ContainsKey(name))
                    return entries[name];
            }
            return string.Empty;
        }

        /// <summary>
        /// Возвращает значение заданного ключа в указанной секции, преобразуя его в целое число.
        /// Если такого ключа в секции нет или он не является целым числом, то возвращает дефолтное значение.
        /// </summary>
        public int GetInteger(string section, string name, int @default)
        {
            int result;
            return int.TryParse(GetEntry(section, name), out result) ? result : @default;
        }

        /// <summary>
        /// Проверяет, содержится ли указанная секция в INI-файле.
        /// </summary>
        public bool HasSection(string section)
        {
            return _sections.ContainsKey(section);
        }

        /// <summary>
        /// Проверяет, имеется ли ключ с заданным именем в указанной секции.
        /// </summary>
        public bool HasEntry(string section, string name)
        {
            return _sections.ContainsKey(section) && _sections[section].ContainsKey(name);
        }

        /// <summary>
        /// Удаляет заданный ключ из указанной секции.
        /// </summary>
        public void RemoveEntry(string section, string name)
        {
            if (HasEntry(section, name))
            {
                _sections[section].Remove(name);
                if (_sections[section].Count == 0)
                    _sections.Remove(section);
            }
        }

        /// <summary>
        /// Список названий всех секций.
        /// </summary>
        public IList<string> Sections
        {
            get { return _sections.Keys.ToList(); }
        }

        /// <summary>
        /// Возвращает список всех ключей в заданной секции.
        /// </summary>
        public IList<string> GetSectionKeys(string section)
        {
            return HasSection(section)? _sections[section].Keys.ToList(): new List<string>();
        }
    }
}
