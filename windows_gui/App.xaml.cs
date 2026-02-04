using System.Windows;
using DNSChanger.Services;

namespace DNSChanger
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Check if running as administrator
            AdminService.CheckAdminAndWarn();
        }
    }
}
