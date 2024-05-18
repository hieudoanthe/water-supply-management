using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _Water_MG
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            navframe.Navigate(new Uri("/Views/StatisticalView.xaml", UriKind.Relative));
        }

        private void sidebar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var selected = sidebar.SelectedItem as NavButton;

            navframe.Navigate(selected.Navlink);

        }

        private void navframe_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {

        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void NavButton_CustomClick(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void NavButton_ToggleWindowState(object sender, MouseButtonEventArgs e)
        {
            /*if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }*/
            WindowState = WindowState.Minimized;
        }

        private void NavButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ToolTipService.SetIsEnabled((DependencyObject)sender, true);
        }

        private void NavButton_MouseLeave(object sender, MouseEventArgs e)
        {
            ToolTipService.SetIsEnabled((DependencyObject)sender, false);
        }
    }
}
