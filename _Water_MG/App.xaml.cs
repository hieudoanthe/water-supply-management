using System.Configuration;
using System.Data;
using System.Windows;
using _Water_MG.Views;

namespace _Water_MG
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected void AppOpen(object sender, StartupEventArgs e)
        {
            var openLogin = new LoginView();
            openLogin.Show();
            openLogin.IsVisibleChanged += (s, ev) =>
            {
                if (openLogin.IsVisible == false && openLogin.IsLoaded)
                {
                    var MainWindowView = new MainWindowView();
                    MainWindowView.Show();
                    openLogin.Close();
                }
            };
        }
    }

}
