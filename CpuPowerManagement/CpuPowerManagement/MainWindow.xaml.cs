using System.Text;
using System.Windows;
using CpuPowerManagement.ViewModel;

namespace CpuPowerManagement
{
  public partial class MainWindow : Window
  {
    private MainViewModel ViewModel => (MainViewModel)DataContext;
    public MainWindow()
    {
      InitializeComponent();
    }
  }
}