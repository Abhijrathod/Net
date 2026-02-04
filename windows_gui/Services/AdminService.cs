using System;
using System.Security.Principal;
using System.Windows;

namespace DNSChanger.Services
{
    public static class AdminService
    {
        public static bool IsRunningAsAdministrator()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        public static void CheckAdminAndWarn()
        {
            if (!IsRunningAsAdministrator())
            {
                MessageBox.Show(
                    "This application requires Administrator privileges to change DNS settings.\n\n" +
                    "Please restart the application as Administrator:\n" +
                    "1. Right-click DNSChanger.exe\n" +
                    "2. Select 'Run as administrator'\n\n" +
                    "You can still view network adapters and DNS servers, but DNS changes will fail.",
                    "Administrator Rights Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
