namespace Retranslator.Model
{
    /// <summary>
    /// Элемент отчета, создавамого при открытии исходного языкового файла (.DKLANG) 
    /// после сравнения его с файлом перевода (.LNG).
    /// </summary>
    public class OpeningReportItem
    {
        /// <summary>
        /// Тип элемента отчета об открытии файла.
        /// </summary>
        public enum ItemType
        {
            /// <summary>
            /// Новый элемент, который есть в исходном файле (.DKLANG), но перевода для него нет в файле перевода (.LNG).
            /// </summary>
            NoTranslation,

            /// <summary>
            /// Перевод найден в файле перевода (.LNG), но элемент с соответствующим идентификатором 
            /// не найден в исходном языковом файле (.DKLANG). 
            /// </summary>
            ExtraTranslation
        }

        /// <summary>
        /// Тип элемента отчета об открытии файла.
        /// </summary>
        public ItemType Type { get; private set; }

        /// <summary>
        /// Название секции, в которой обнаружен этот элемент.
        /// </summary>
        public string Section { get; private set; }

        /// <summary>
        /// Описание элемента. Это текст или перевод элемента, в зависимости от типа.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Элемент отчета, создавамого при открытии исходного языкового файла (.DKLANG) 
        /// после сравнения его с файлом перевода (.LNG).
        /// </summary>
        public OpeningReportItem(ItemType itemType, string section, string id, string text)
        {
            Type = itemType;
            Section = section;

            Description = (Section == TranslationSection.ConstSectionName) ? id : id.TrimStart(new[] { '0' }) + "\t" + text;
        }
    }
}
