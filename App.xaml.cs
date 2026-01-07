using System.Windows;
using TopBarDock.Views;

namespace TopBarDock
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(
                    ex.Exception.Message,
                    "Unhandled Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                ex.Handled = true;
            };

            base.OnStartup(e);

            new Views.TopBarWindow().Show();
        }
    }
}
