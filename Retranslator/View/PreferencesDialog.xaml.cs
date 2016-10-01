using System.Windows;

namespace Retranslator.View
{
    public partial class PreferencesDialog
    {
        public PreferencesDialog()
        {
            InitializeComponent();
        }

        private void ButtonOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
