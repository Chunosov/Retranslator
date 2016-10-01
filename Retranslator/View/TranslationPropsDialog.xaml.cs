using System.Windows;

namespace Retranslator.View
{
    public partial class TranslationPropsDialog
    {
        public TranslationPropsDialog()
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
