using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DNSChanger.Services
{
    public static class DnsInterop
    {
        private const string DllName = "dns_core.dll";
        private static bool _dllChecked = false;

        static DnsInterop()
        {
            CheckDllExists();
        }

        private static void CheckDllExists()
        {
            if (_dllChecked) return;
            
            string dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DllName);
            if (!File.Exists(dllPath))
            {
                System.Diagnostics.Debug.WriteLine($"DLL not found at: {dllPath}");
                System.Windows.MessageBox.Show(
                    $"dns_core.dll not found!\n\nExpected location: {dllPath}\n\nPlease ensure the DLL is in the same directory as DNSChanger.exe",
                    "DLL Not Found",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"DLL found at: {dllPath}");
            }
            _dllChecked = true;
        }

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "list_networks_json")]
        public static extern IntPtr list_networks_json();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "list_dns_servers_json")]
        public static extern IntPtr list_dns_servers_json();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "test_dns_speed_json")]
        public static extern IntPtr test_dns_speed_json();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "set_dns")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool set_dns([MarshalAs(UnmanagedType.LPStr)] string interfaceName,
                                          [MarshalAs(UnmanagedType.LPStr)] string dns1,
                                          [MarshalAs(UnmanagedType.LPStr)] string dns2);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "restore_default_dns")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool restore_default_dns([MarshalAs(UnmanagedType.LPStr)] string interfaceName);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "free_dns_string")]
        public static extern void free_dns_string(IntPtr str);

        public static string GetJsonString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
            {
                System.Diagnostics.Debug.WriteLine("GetJsonString: Received null pointer");
                return "[]";
            }
            
            try
            {
                string result = Marshal.PtrToStringAnsi(ptr) ?? "[]";
                System.Diagnostics.Debug.WriteLine($"GetJsonString: Retrieved {result.Length} characters");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetJsonString error: {ex.Message}");
                return "[]";
            }
        }
    }
}
