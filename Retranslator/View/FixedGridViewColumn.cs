using System.Windows;
using System.Windows.Controls;

namespace Retranslator.View
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// http://blogs.msdn.com/b/atc_avalon_team/archive/2006/04/11/573037.aspx
    /// </remarks>
    public class FixedGridViewColumn : GridViewColumn
    {
        public static readonly DependencyProperty FixedWidthProperty = DependencyProperty.Register(
            "FixedWidth", typeof(double), typeof(FixedGridViewColumn), 
            new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnFixedWidthChanged)));

        static FixedGridViewColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixedGridViewColumn),
                new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceWidth)));
        }

        public double FixedWidth
        {
            get { return (double) GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        private static object OnCoerceWidth(DependencyObject sender, object baseValue)
        {
            var column = sender as FixedGridViewColumn;
            return column == null ? baseValue : column.FixedWidth;
        }

        private static void OnFixedWidthChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var fwc = sender as FixedGridViewColumn;
            if (fwc != null)
                fwc.CoerceValue(WidthProperty);
        }
    }
}
