using System.Text;
using System.Windows;
using CpuPowerManagement.ViewModels;

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