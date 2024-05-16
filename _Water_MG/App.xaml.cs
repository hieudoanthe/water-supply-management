using System.Configuration;
using System.Data;
using System.Windows;
using _Water_MG.Views;

namespace _Water_MG
{
    public partial class App : Application
    {
        protected void AppOpen(object sender, StartupEventArgs e)
        {
            var openLogin = new LoginView();
            openLogin.Show();
            openLogin.IsVisibleChanged += OpenLogin_IsVisibleChanged;
        }

        private void OpenLogin_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var openLogin = sender as LoginView;
            if (openLogin != null && !openLogin.IsVisible && openLogin.IsLoaded)
            {
                openLogin.IsVisibleChanged -= OpenLogin_IsVisibleChanged;
                var mainWindowView = new MainWindow();
                mainWindowView.Show();
                openLogin.Dispatcher.BeginInvoke(new Action(() =>
                {
                    openLogin.Close();
                }));
            }
        }
    }
}
