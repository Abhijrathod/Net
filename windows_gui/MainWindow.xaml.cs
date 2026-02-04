using System.Windows;
using System.Windows.Controls;
using DNSChanger.ViewModels;

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
    }
}
