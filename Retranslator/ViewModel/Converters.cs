using System;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Retranslator.Model;

namespace Retranslator.ViewModel
{
    public class OneWayConverter
    {
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class OneToMultiLineConverter : IValueConverter
    {
        public static string Convert(string value)
        {
            return value.Replace("\\n", Environment.NewLine);
        }

        public static string ConvertBack(string value)
        {
            return value.Replace(Environment.NewLine, "\\n");
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : Convert((string) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : ConvertBack((string) value);
        }
    }


    [ValueConversion(typeof(string), typeof(string))]
    public class IdStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                int id;
                return int.TryParse((string) value, out id) ? id.ToString() : value;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

	#region Translation.Status Converters

	/// <summary>
    /// Преобразует состояние перевода элемента в соответствующую иконку.
    /// </summary>
    [ValueConversion(typeof(Translation.Status), typeof(BitmapImage))]
    public class StatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            switch((Translation.Status) value)
            {
                case Translation.Status.None:
                    return new BitmapImage(new Uri("Images/StateNone.png", UriKind.Relative));

                case Translation.Status.Ok:
                    return new BitmapImage(new Uri("Images/StateOk.png", UriKind.Relative));

                case Translation.Status.Warnings:
                    return new BitmapImage(new Uri("Images/StateWarning.png", UriKind.Relative));
			
				case Translation.Status.Unsaved:
					return new BitmapImage(new Uri("Images/StateAuto.png", UriKind.Relative));
			}
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    /// <summary>
    /// Преобразует состояние перевода элемента в соответствующую иконку.
    /// </summary>
    [ValueConversion(typeof(Translation.Status), typeof(String))]
    public class StatusToStringConverter : OneWayConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            switch ((Translation.Status)value)
            {
                case Translation.Status.None:
                    return "Missed translation";

                case Translation.Status.Warnings:
                    return "May be inconsistent translation";
            }
            return null;
        }
    }

	/// <summary>
	/// Определяет можно активировать команду "Применить перевод".
	/// </summary>
	[ValueConversion(typeof(Translation.Status), typeof(bool))]
	public class StatusToCanApplyConverter : OneWayConverter, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			return ((Translation.Status) value) != Translation.Status.Ok;
		}
	}

	#endregion

	/// <summary>
    /// Возвращает описание состояния перевода.
    /// </summary>
    [ValueConversion(typeof(TranslationEntry.Warning), typeof(string))]
    public class WarningToStringConverter : OneWayConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;
            switch ((TranslationEntry.Warning) value)
            {
                case TranslationEntry.Warning.EndingPunctuation:
                    return "Translation does not end with the same punctuation as the source text.";
                    // RU: "Перевод не заканчивается тем же знаком препинания, что и исходный текст."

				case TranslationEntry.Warning.EndingNewlineSource:
					return "Trailing linebreakes (\\n) possibly superfluous in translation.";
					// RU: "Лишние переводы строк (\\n) в конце текста перевода."

				case TranslationEntry.Warning.EndingNewlineTranslation:
					return "Trailing linebreakes (\\n) possibly missing in translation.";
					// RU: "Недостающие переводы строк (\\n) строки в конце текста перевода."

            		/*
                 *   Accelerator possibly superfluous in translation.
                 *   Возможно, лишний акселератор в переводе.
                 * 
                 *   Accelerator possibly missing in translation.
                 *   Возможно, пропущен акселератор в переводе.
                 *   
                 *   Translation does not refer to the same place markers as in the source text.
                 *   Перевод не содержит тех же маркеров форматирования, что и исходный текст.
                 */
            }
            return null;
        }
    }

    [ValueConversion(typeof(TranslationEntry.Warning), typeof(BitmapImage))]
    public class WarningToImageConverter : OneWayConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new BitmapImage(new Uri("Images/StateWarning.png", UriKind.Relative));
        }
    }

	#region File Path Converters

    [ValueConversion(typeof(string), typeof(string))]
    public class PathToFileNameConverter : OneWayConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : Path.GetFileName((string)value);
        }
    }

    [ValueConversion(typeof(string), typeof(string))]
    public class PathToDirectoryConverter : OneWayConverter, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : Path.GetDirectoryName((string)value) + "\\";
        }
    }

    #endregion

	/// <summary>
	/// Преобразует состояние перевода элемента в соответствующую иконку.
	/// </summary>
	[ValueConversion(typeof(MessageWindow.Type), typeof(BitmapImage))]
	public class MessageBoxTypeToImageConverter : OneWayConverter, IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null) return null;
			switch ((MessageWindow.Type)value)
			{
				case MessageWindow.Type.Cancel:
					return new BitmapImage(new Uri("/Retranslator;component/Images/IconCancel48.png", UriKind.RelativeOrAbsolute));

				case MessageWindow.Type.Ok:
					return new BitmapImage(new Uri("/Retranslator;component/Images/IconOk48.png", UriKind.RelativeOrAbsolute));
			}
			return null;
		}
	}
}