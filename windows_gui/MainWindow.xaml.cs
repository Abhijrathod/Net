using System.Windows;
using System.Windows.Controls;
using DNSChanger.ViewModels;
using System;

namespace DNSChanger
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is MainViewModel vm && sender is DataGrid dg)
            {
                vm.SelectedDns = dg.SelectedItem as Models.DnsModel;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnClosed(e);
        }
    }
}
