using System;
using System.IO;
using System.Reflection;

namespace Retranslator.Utils
{
    /// <summary>
    /// Набор различных вспомогательных методов.
    /// </summary>
    static public class Utils
    {
        /// <summary>
        /// Возвращает путь, по которому запущен исполняемый файл приложения.
        /// </summary>
        /// <returns></returns>
        static public string GetLocalPath()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\";
        }

        /// <summary>
        /// Возвращает (и создает, если не было) путь до указанной папки внутри одной из специальных системных папок.
        /// </summary>
        static public string GetSpecialPath(Environment.SpecialFolder specialFolder, string subFolder)
        {
            var path = Environment.GetFolderPath(specialFolder) + "\\" + subFolder + "\\";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}
