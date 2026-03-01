using System.Windows;
using SpecTrace.Views;

namespace SpecTrace
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            new MainWindow().Show();
        }
    }
}
