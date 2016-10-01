using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using Retranslator.Model;

namespace Retranslator.ViewModel
{
    /// <summary>
    /// Окно поиска
    /// </summary>
    public partial class SearchWindow
    {
        static private readonly Dictionary<Window, SearchWindow> Windows = new Dictionary<Window, SearchWindow>();

        private FindDelegate _findDelegate;

        /// <summary>
        /// Открывает новое или уже существующее окно поиска.
        /// </summary>
        static public void Open(Window owner, SearchParameters @params, FindDelegate findDelegate)
        {
            SearchWindow window;
            if (!Windows.ContainsKey(owner))
            {
                window = new SearchWindow {Owner = owner, DataContext = @params, _findDelegate = findDelegate};

                Windows.Add(owner, window);
            }
            else
                window = Windows[owner];

            window.Show();
        }

        private SearchWindow()
        {
            InitializeComponent();

            PatternInput.Focus();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            _findDelegate(null);
            Windows.Remove(Owner);
        }

        private void FindNextClick(object sender, RoutedEventArgs e)
        {
            if (!_findDelegate(DataContext as SearchParameters))
				System.Windows.Forms.MessageBox.Show(@"No more occurences founded", 
					Preferences.AppTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
