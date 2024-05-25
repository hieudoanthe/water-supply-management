using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace _Water_MG
{
    public class NavButton : ListBoxItem
    {
        static NavButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NavButton), new FrameworkPropertyMetadata(typeof(NavButton)));
        }

        public Uri Navlink
        {
            get { return (Uri)GetValue(NavlinkProperty); }
            set { SetValue(NavlinkProperty, value); }
        }
        public static readonly DependencyProperty NavlinkProperty =
            DependencyProperty.Register("Navlink", typeof(Uri), typeof(NavButton), new PropertyMetadata(null));

        public Geometry Icon
        {
            get { return (Geometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(Geometry), typeof(NavButton), new PropertyMetadata(null));

        public Brush IconColor
        {
            get { return (Brush)GetValue(IconColorProperty); }
            set { SetValue(IconColorProperty, value); }
        }
        public static readonly DependencyProperty IconColorProperty =
            DependencyProperty.Register("IconColor", typeof(Brush), typeof(NavButton), new PropertyMetadata(Brushes.Black));

        // Tạo và đăng ký sự kiện tùy chỉnh
        public static readonly RoutedEvent CustomClickEvent =
            EventManager.RegisterRoutedEvent("CustomClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NavButton));

        // Expose the CustomClick event as a public event
        public event RoutedEventHandler CustomClick
        {
            add { AddHandler(CustomClickEvent, value); }
            remove { RemoveHandler(CustomClickEvent, value); }
        }

        // Raise the CustomClick event
        protected virtual void OnCustomClick()
        {
            RoutedEventArgs args = new(CustomClickEvent);
            RaiseEvent(args);
        }

        // Override the OnMouseLeftButtonUp method to raise the CustomClick event
        protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            OnCustomClick();
        }

    }
}
