using System.Windows;

namespace Retranslator.View
{
    public partial class OpenDialog
    {
        public OpenDialog()
        {
            InitializeComponent();
        }

        private static string OpenFileDialog(string title, string filter)
        {
            using (var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = title;
                dlg.Filter = filter;
                dlg.CheckFileExists = true;
                return (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK) ? dlg.FileName : null;
            }
        }

        private void ButtonBrowseLangSourceClick(object sender, RoutedEventArgs e)
        {
            var fileName = OpenFileDialog(
                "Select a Language Source File",
                "DKLang language source files (*.dklang)|*.dklang|All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(fileName))
                ComboBoxLangSource.Text = fileName;
        }

        private void ButtonBrowseTranslationClick(object sender, RoutedEventArgs e)
        {
            var fileName = OpenFileDialog(
                "Select a Translation File",
                "DKLang translation files (*.lng)|*.lng|All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(fileName))
                ComboBoxTranslation.Text = fileName;
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
