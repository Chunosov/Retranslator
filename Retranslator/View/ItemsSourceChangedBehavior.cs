using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Retranslator.View
{
    /// <summary>
    /// ItemsSourceChanged Behavior uses an Attached Dependency Property
    /// to add and raise a rotued event whenever an ItemsControl's ItemsSource property
    /// changes. Also looks for INotifyCollectionChanged on the ItemsSource and raises the
    /// event on every collection changed event
    /// </summary>
    /// <remarks>
    /// http://www.theabsentmindedcoder.com/2009/08/itemssourcechanged-event-using-attached.html
    /// </remarks>
    public class ItemsSourceChangedBehavior
    {
        #region ItemsSourceChanged Property

        /// <summary>
        /// ItemsSourceChanged Attached Dependency Property with Callback method
        /// </summary>
        public static readonly DependencyProperty ItemsSourceChangedProperty = 
            DependencyProperty.RegisterAttached(
                "ItemsSourceChanged", typeof(bool), typeof(ItemsSourceChangedBehavior), 
                new FrameworkPropertyMetadata(false, OnItemsSourceChanged));

        /// <summary>
        /// Static Get method allowing easy Xaml usage and to simplify the GetValue process
        /// </summary>
        public static bool GetItemsSourceChanged(DependencyObject obj)
        {
            return (bool) obj.GetValue(ItemsSourceChangedProperty);
        }

        /// <summary>
        /// Static Set method allowing easy Xaml usage and to simplify the Setvalue process
        /// </summary>
        public static void SetItemsSourceChanged(DependencyObject obj, bool value)
        {
            obj.SetValue(ItemsSourceChangedProperty, value);
        }

        /// <summary>
        /// Dependency Property Changed Call Back method. This will be called anytime
        /// the ItemsSourceChangedProperty value changes on a Dependency Object
        /// </summary>
        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = obj as ItemsControl;
            if (itemsControl == null) return;

            var oldValue = (bool) e.OldValue;
            var newValue = (bool) e.NewValue;

            if (!oldValue && newValue) // If changed from false to true
            {
                // Create a binding to the ItemsSourceProperty on the ItemsControl
                var b = new Binding {Source = itemsControl, Path = new PropertyPath(ItemsControl.ItemsSourceProperty)};

                // Since the ItemsSourceListenerProperty is now bound to the
                // ItemsSourceProperty on the ItemsControl, whenever the 
                // ItemsSourceProperty changes the ItemsSourceListenerProperty
                // callback method will execute
                itemsControl.SetBinding(ItemsSourceListenerProperty, b);
            }
            else if (oldValue && !newValue) // If changed from true to false
            {
                // Clear Binding on the ItemsSourceListenerProperty
                BindingOperations.ClearBinding(itemsControl, ItemsSourceListenerProperty);
            }
        }

        #endregion

        #region Items Source Listener Property

        /// <summary>
        /// The ItemsSourceListener Attached Dependency Property is a private property
        /// the ItemsSourceChangedBehavior will use silently to bind to the ItemsControl
        /// ItemsSourceProperty.
        /// Once bound, the callback method will execute anytime the ItemsSource property changes
        /// </summary>
        private static readonly DependencyProperty ItemsSourceListenerProperty = 
            DependencyProperty.RegisterAttached(
                "ItemsSourceListener", typeof(object), typeof(ItemsSourceChangedBehavior), 
                new FrameworkPropertyMetadata(null, OnItemsSourceListenerChanged));

        /// <summary>
        /// Dependency Property Changed Call Back method. This will be called anytime
        /// the ItemsSourceListenerProperty value changes on a Dependency Object
        /// </summary>
        private static void OnItemsSourceListenerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = obj as ItemsControl;
            if (itemsControl == null) return;

            var collection = e.NewValue as INotifyCollectionChanged;
            if (collection != null)
            {
                collection.CollectionChanged += delegate
                {
                    itemsControl.RaiseEvent(new RoutedEventArgs(ItemsSourceChangedEvent));
                };
            }

            if (GetItemsSourceChanged(itemsControl))
                itemsControl.RaiseEvent(new RoutedEventArgs(ItemsSourceChangedEvent));
        }

        #endregion

        #region Items Source Changed Event

        /// <summary>
        /// Routed Event to raise whenever the ItemsSource changes on an ItemsControl
        /// </summary>
        public static readonly RoutedEvent ItemsSourceChangedEvent = 
            EventManager.RegisterRoutedEvent("ItemsSourceChanged", 
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ItemsControl));

        #endregion
    }
}
