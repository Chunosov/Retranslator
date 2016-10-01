using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Retranslator.Model
{
    /// <summary>
    /// Перводимая секиця, содержащая список элементов для перевода.
    /// </summary>
    public class TranslationSection
    {
        /// <summary>
        /// Тип переводимой секции.
        /// </summary>
        public enum SectionType
        {
            /// <summary>
            /// Обычная секция, содержащая переводы свойств компонентов.
            /// </summary>
            Component,

            /// <summary>
            /// Секция, содержащая константы.
            /// </summary>
            Constant
        }

        /// <summary>
        /// Предопределенное имя секции, содержащей сонстанты.
        /// </summary>
        public const string ConstSectionName = "$CONSTANTS";

        private readonly Translation _parent;
        private bool _locked;

        /// <summary>
        /// Переводимая секиця, содержащая список элементов для перевода.
        /// </summary>
        public TranslationSection(Translation parent, string line)
        {
            Name = line.Substring(1, line.Length - 2);
            Type = (Name == ConstSectionName) ? SectionType.Constant : SectionType.Component;
            Entries = new List<TranslationEntry>();
            _parent = parent;
        }

        #region Properties

        /// <summary>
        /// Список переводимых элементов, содержащихся в секции.
        /// </summary>
        public List<TranslationEntry> Entries { get; private set; }

        /// <summary>
        /// Имя переводимой секции.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Тип переводимой секции.
        /// </summary>
        public SectionType Type { get; private set; }

        /// <summary>
        /// Состояние перевода секции.
        /// </summary>
        public Translation.Status Status { get; private set; }

        #endregion

        /// <summary>
        /// Создает переводимый элемент из строки dklang-файла.
        /// </summary>
        public void AddEntry(string line)
        {
            switch (Type)
            {
                case SectionType.Component:
                    Entries.Add(MakePropertyEntry(line));
                    break;

                case SectionType.Constant:
                    Entries.Add(MakeConstantEntry(line));
                    break;
            }
        }

        /// <summary>
        /// Создает переводимый элемент из строки dklang-файла.
        /// Формат строки: name=id,text
        /// Например: ToolbarMenu.Caption=00000041,Menu 
        /// </summary>
        private TranslationEntry MakePropertyEntry(string line)
        {
            var index = line.IndexOf('=');
            if (index < 0)
                throw new Exception("Invalid line: " + line);

            var name = line.Substring(0, index);
            var value = line.Substring(index + 1);

            index = value.IndexOf(',');
            if (index < 0)
                throw new Exception("Invalid line: " + line);

            var id = value.Substring(0, index);
            var text = value.Substring(index + 1);

            return new TranslationEntry(this) { Id = id, Name = name, Source = text };
        }

        /// <summary>
        /// Создает переводимый элемент из строки dklang-файла, описывающей константу.
        /// Формат строки: name=text.
        /// </summary>
        private TranslationEntry MakeConstantEntry(string line)
        {
            var index = line.IndexOf('=');
            if (index < 0)
                throw new Exception("Invalid line: " + line);

            var name = line.Substring(0, index);
            var value = line.Substring(index + 1);

            return new TranslationEntry(this) { Id = name, Name = name, Source = value };
        }

        /// <summary>
        /// Число корректно переведенных элементов в секции.
        /// </summary>
        public int TranslatedCount
        {
            get { return Entries.Count(entry => entry.Status == Translation.Status.Ok); }
        }

        /// <summary>
        /// Обновить статусы всех элементов.
        /// </summary>
        public void UpdateStatus()
        {
        	_locked = true;
        	try
        	{
				foreach (var entry in Entries)
					entry.UpdateStatus();
			}
        	finally
        	{
        		_locked = false;
        	}
			StatusUpdated();
        }

        /// <summary>
        /// Вычисление статуса секции на основе статусов элементов.
        /// Что-то вроде уведомления, вызываемого элементом при его обновлении.
        /// </summary>
        public void StatusUpdated()
        {
            if (_locked) return;

            foreach (var entry in Entries)
            {
                switch (entry.Status)
                {
                    case Translation.Status.Warnings:
                        Status = Translation.Status.Warnings;
                        return;

                    case Translation.Status.None:
                        Status = Translation.Status.None;
                        return;

					case Translation.Status.Unsaved:
                		Status = Translation.Status.Unsaved;
                		return;
                }
            }
            Status = Translation.Status.Ok;

            if (_parent != null)
                _parent.StatusUpdated();
        }

        /// <summary>
        /// Запрещает вычисление статуса секции на основе статусов элементов.
        /// Применяется при групповом изменении элементов, например при загрузке.
        /// </summary>
        public void BeginUpdate()
        {
            _locked = true;
        }

        /// <summary>
        /// Разрешает вычисление статуса секции, заблокированное методом BeginUpdate().
        /// </summary>
        public void EndUpdate()
        {
            if (!_locked) return;
            _locked = false;
            StatusUpdated();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Format("[{0}]", Name));
            foreach (var entry in Entries.Where(entry => entry.Status != Translation.Status.None))
            {
                sb.AppendLine(entry.ToString());
            }

            return sb.ToString();
        }
    }
}
